namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type MatchNumber = MatchNumber of uint

[<Struct>]
type Alliance =
    | Red
    | Blue
    member this.Match (redAction: Action, blueAction: Action) =
        match this with
        | Red -> redAction
        | Blue -> blueAction

[<Struct>]
[<RequireQualifiedAccess>]
type EndgameResult =
    | Success of ScoringTier
    | Failure
    | NotAttempted
    member this.Match (successAction: Action<ScoringTier>, failureAction: Action, notAttemptedAction: Action) =
        match this with
        | Success tier -> successAction.Invoke tier
        | Failure -> failureAction.Invoke()
        | NotAttempted -> notAttemptedAction.Invoke()

[<Struct>]
type BotStrategy =
    | Offense
    | Defense
    | Both
    member this.Match (offenseAction: Action, defenseAction: Action, bothAction: Action) =
        match this with
        | Offense -> offenseAction.Invoke()
        | Defense -> defenseAction.Invoke()
        | Both -> bothAction.Invoke()

type Endgame =
    { Capable: EndgameCapable
      Result: EndgameResult }

    static member NotCapable =
        { Capable = NotCapable
          Result = EndgameResult.NotAttempted }
    static member Create (capable) = { Capable = capable; Result = EndgameResult.NotAttempted }
    member this.Success (tier) = { this with Result = EndgameResult.Success tier }
    member this.Failure() = { this with Result = EndgameResult.Failure }
    member this.NotAttempted() = { this with Result = EndgameResult.NotAttempted }

[<Struct>]
type Breakdown =
    | AutoEmergency
    | Emergency
    | Malfunction
    member this.Match (autoEmergencyAction: Action, emergencyAction: Action, malfunctionAction: Action) =
        match this with
        | AutoEmergency -> autoEmergencyAction.Invoke()
        | Emergency -> emergencyAction.Invoke()
        | Malfunction -> malfunctionAction.Invoke()

type ScoreRecord =
    { GamePiece: GamePiece
      Tier: ScoringTier
      Phase: SubPhaseId }
    static member create gamePiece tier phase = { GamePiece = gamePiece; Tier = tier; Phase = phase }

type MatchScoutResult =
    { Team: Team
      Alliance: Alliance
      Scores: ScoreRecord list
      Endgame: Endgame
      Breakdowns: Breakdown list
      Infractions: InfractionId list
      GamePoints: Points option
      RankingPoints: RankingPoints option
      Notes: Note list }
    static member create team alliance endgame = {
        Team = team
        Alliance = alliance
        Scores = []
        Endgame = endgame
        Breakdowns = []
        Infractions = []
        GamePoints = None
        RankingPoints = None
        Notes = []
    }

type Match =
    { MatchNumber: MatchNumber
      MatchScoutResults: MatchScoutResult list
      Winner: Alliance option }
    static member create matchNumber matchScoutResults = {
        MatchNumber = matchNumber
        MatchScoutResults = matchScoutResults
        Winner = None
    }

module Match =
    let createMatch matchNumber matchScoutResults =
        { MatchNumber = matchNumber
          MatchScoutResults = matchScoutResults
          Winner = None }

    let createMatchResult team alliance endgameCapability =
        { Team = team
          Alliance = alliance
          Scores = []
          Endgame =
            { Capable = endgameCapability
              Result = EndgameResult.NotAttempted }
          Breakdowns = []
          Infractions = []
          GamePoints = None
          RankingPoints = None
          Notes = [] }

    let addMatchResult matchScoutResult matchData =
        { matchData with
            MatchScoutResults = matchScoutResult :: matchData.MatchScoutResults }

    let setWinner winner matchData = { matchData with Winner = Some winner }

    let score phase gamePiece tier matchData =
        { matchData with
            Scores =
                { GamePiece = gamePiece
                  Tier = tier
                  Phase = phase }
                :: matchData.Scores }

    let breakdown breakdownData matchData =
        { matchData with
            Breakdowns = breakdownData :: matchData.Breakdowns }

    let foul infractionData matchData =
        { matchData with
            Infractions = infractionData :: matchData.Infractions }

    let endgame endgameData matchData =
        { matchData with
            Endgame.Result = endgameData }

    let tally victoryReward winningAlliance (matchData: MatchScoutResult) : MatchScoutResult =
        let gamePieceScores = matchData.Scores |> List.groupBy _.GamePiece |> Map.ofList

        let gamePiecePoints =
            gamePieceScores
            |> Map.map (fun gamePiece scores ->
                scores
                |> List.sumBy (fun scoreRecord ->
                    gamePiece.PhaseScore
                    |> Map.tryFind scoreRecord.Phase
                    |> function
                        | None ->
                            invalidOp
                                $"Could not find score value for Phase {scoreRecord.Phase} for GamePiece {gamePiece}."
                        | Some scoreValue -> Score.compile >> Score.getPoints scoreRecord.Tier <| scoreValue))

        let totalGamePoints = gamePiecePoints.Values |> Seq.sum

        let victoryRankingPoints =
            match matchData.Alliance = winningAlliance with
            | true -> victoryReward
            | false -> RankingPoints.Zero

        let totalRankingPoints =
            gamePieceScores
            |> Map.map (fun gamePiece scores ->
                gamePiece.RankPoints
                |> List.sumBy (fun grant ->
                    match grant.Threshold with
                    | PointsThreshold points ->
                        match gamePiecePoints[gamePiece] >= points with
                        | true -> grant.Value
                        | false -> RankingPoints.Zero
                    | ScoreThreshold score ->
                        match scores |> List.length |> uint >= score with
                        | true -> grant.Value
                        | false -> RankingPoints.Zero))
            |> Map.values
            |> Seq.sum
            |> (+) victoryRankingPoints

        { matchData with
            GamePoints = Some totalGamePoints
            RankingPoints = Some totalRankingPoints }

    let addNote note matchData =
        { matchData with
            MatchScoutResult.Notes = note :: matchData.Notes }
