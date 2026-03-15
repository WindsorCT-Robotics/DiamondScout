namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic
open System.Linq

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
    { 
      Team: TeamId
      Alliance: Alliance
      Scores: ScoreRecord IReadOnlyList
      Endgame: Endgame
      Breakdowns: Breakdown IReadOnlyList
      Infractions: Infractions
      Notes: Notes }

[<RequireQualifiedAccess>]
module ScoutingResults =
    let create team alliance endgameCapability =
        { 
          Team = team
          Alliance = alliance
          Scores = []
          Endgame =
            { Capable = endgameCapability
              Result = EndgameResult.NotAttempted }
          Breakdowns = []
          Infractions = Infractions.Empty
          Notes = Notes.Empty }

    let recordScore phase gamePiece tier matchData =
        { matchData with
            Scores =
                let newScore = ScoreRecord.create gamePiece tier phase
                match matchData.Scores with
                | :? list<ScoreRecord> as scores -> scores @ [ newScore ]
                | scores -> scores |> List.ofSeq |> List.append [ newScore ] }

    let recordBreakdown breakdownData matchData =
        { matchData with
            Breakdowns =
                match matchData.Breakdowns with
                | :? list<Breakdown> as breakdowns -> breakdowns @ [ breakdownData ]
                | breakdowns -> breakdowns |> List.ofSeq |> List.append [ breakdownData ] }

    let recordInfraction infractionId infractionData matchData =
        { matchData with
            Infractions = matchData.Infractions.Add (infractionId, infractionData) }

    let recordEndgame endgameData matchData =
        { matchData with
            ScoutingResults.Endgame.Result = endgameData }

    let addNote noteId note matchData =
        { matchData with
              ScoutingResults.Notes = matchData.Notes.Add (noteId, note) }

type ScoutingResults with
   member this.HasBreakdown breakdown =
        match this.Breakdowns with
        | :? list<Breakdown> as breakdowns -> breakdowns |> List.exists ((=) breakdown)
        | breakdowns -> breakdowns.Contains breakdown
    (* let tally victoryReward winningAlliance (matchData: ScoutingResults) : ScoutingResults =
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
            ScoutingResults.Notes = note :: matchData.Notes } *)
