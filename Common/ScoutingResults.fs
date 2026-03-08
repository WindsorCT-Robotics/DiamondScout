namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type Alliance =
    | Red
    | Blue

    member this.Match(redAction: Action, blueAction: Action) =
        match this with
        | Red -> redAction
        | Blue -> blueAction

[<Struct>]
[<RequireQualifiedAccess>]
type EndgameResult =
    | Success of ScoringTier
    | Failure
    | NotAttempted

    member this.Match(successAction: Action<ScoringTier>, failureAction: Action, notAttemptedAction: Action) =
        match this with
        | Success tier -> successAction.Invoke tier
        | Failure -> failureAction.Invoke()
        | NotAttempted -> notAttemptedAction.Invoke()

[<Struct>]
type BotStrategy =
    | Offense
    | Defense
    | Both

    member this.Match(offenseAction: Action, defenseAction: Action, bothAction: Action) =
        match this with
        | Offense -> offenseAction.Invoke()
        | Defense -> defenseAction.Invoke()
        | Both -> bothAction.Invoke()

type Endgame =
    { Capable: EndgameCapable
      Result: EndgameResult }

[<Struct>]
type Breakdown =
    | AutoEmergency
    | Emergency
    | Malfunction

    member this.Match(autoEmergencyAction: Action, emergencyAction: Action, malfunctionAction: Action) =
        match this with
        | AutoEmergency -> autoEmergencyAction.Invoke()
        | Emergency -> emergencyAction.Invoke()
        | Malfunction -> malfunctionAction.Invoke()

type ScoreRecord =
    { GamePiece: GamePiece
      Tier: ScoringTier
      Phase: SubPhaseId }

    static member create gamePiece tier phase =
        { GamePiece = gamePiece
          Tier = tier
          Phase = phase }

type ScoutingResults =
    { Game: GameId
      Match: MatchId
      Team: TeamId
      Alliance: Alliance
      Scores: ScoreRecord list
      Endgame: Endgame
      Breakdowns: Breakdown list
      Infractions: InfractionId list
      GamePoints: Points option
      RankingPoints: RankingPoints option
      Notes: NoteId list }

[<RequireQualifiedAccess>]
module ScoutingResults =
    let createScoutingResult game frcMatch team alliance endgameCapability =
        { Game = game
          Match = frcMatch
          Team = team
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
            ScoutingResults.Endgame.Result = endgameData }

    let tally victoryReward winningAlliance (matchData: ScoutingResults) : ScoutingResults =
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
            ScoutingResults.Notes = note :: matchData.Notes }

    type Event =
        | Created of resultId: ScoutingResultsId * scoutingResults: ScoutingResults
        | Scored of resultId: ScoutingResultsId * scoreRecord: ScoreRecord
        | BrokenDown of resultId: ScoutingResultsId * breakdown: Breakdown
        | Fouled of resultId: ScoutingResultsId * infraction: InfractionId
        | MatchEnded of resultId: ScoutingResultsId * endgame: Endgame * victor: Alliance * victoryReward: RankingPoints
        | NoteAdded of resultId: ScoutingResultsId * note: NoteId
        | Deleted of resultId: ScoutingResultsId
