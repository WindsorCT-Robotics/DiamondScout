namespace ParagonRobotics.DiamondScout.Common

[<Struct>]
type MatchNumber = MatchNumber of uint

[<Struct>]
type Alliance =
    | Red
    | Blue

[<Struct>]
[<RequireQualifiedAccess>]
type EndgameResult =
    | Success of ScoringTier
    | Failure
    | NotAttempted

[<Struct>]
type BotStrategy =
    | Offense
    | Defense
    | Both

type Endgame =
    { Capable: EndgameCapable
      Result: EndgameResult }

    static member NotCapable =
        { Capable = NotCapable
          Result = EndgameResult.NotAttempted }

[<Struct>]
type Breakdown =
    | AutoEmergency
    | Emergency
    | Malfunction

type ScoreRecord = {
    GamePiece: GamePiece
    Tier: ScoringTier
    Phase: SubPhaseId
}

type MatchScoutResult =
    { Team: Team
      Alliance: Alliance
      Scores: ScoreRecord list
      Endgame: Endgame
      Breakdowns: Breakdown list
      Infractions: InfractionId list
      GamePoints: Points option
      RankingPoints: RankingPoints option }

type Match =
    { MatchNumber: MatchNumber
      MatchScoutResults: MatchScoutResult list
      Winner: Alliance option }

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
          RankingPoints = None }

    let addMatchResult matchScoutResult matchData =
        { matchData with
            MatchScoutResults = matchScoutResult :: matchData.MatchScoutResults }

    let score phase gamePiece tier matchData =
        { matchData with Scores = { GamePiece = gamePiece; Tier = tier; Phase = phase } :: matchData.Scores }

    let breakdown breakdownData matchData =
        { matchData with
            Breakdowns = breakdownData :: matchData.Breakdowns }

    let foul infractionData matchData =
        { matchData with
            Infractions = infractionData :: matchData.Infractions }

    let endgame endgameData matchData =
        { matchData with
            Endgame.Result = endgameData }

    let tally (matchData: MatchScoutResult) : MatchScoutResult =
        let gamePieceScores =
            // Calculate score values for each game piece
            matchData.Scores
            |> List.groupBy _.GamePiece
            |> Map.ofList


        // Calculate total game points from all score records
        let gamePiecePoints =
            gamePieceScores
            |> Map.map (fun gamePiece scores ->
                scores
                |> List.sumBy (fun scoreRecord ->
                    gamePiece.Value
                    |> Map.find scoreRecord.Phase
                    |> function
                        | None -> Points.Zero
                        | Some scoreValue ->
                            Score.compile >> Score.getPoints scoreRecord.Tier
                            <| scoreValue))

        let totalGamePoints = gamePiecePoints.Values |> Seq.sum

        // Calculate ranking points based on thresholds
        let totalRankingPoints =
            gamePieceScores
            |> Map.map (fun gamePiece scores ->
                gamePiece.RankPoints
                |> List.sumBy (fun grant ->
                    match grant.Threshold with
                    | PointsThreshold points ->
                        match gamePiecePoints.[gamePiece] >= points with
                        | true -> grant.Value
                        | false -> RankingPoints.Zero
                    | ScoreThreshold score ->
                        match scores |> List.length |> uint >= score with
                        | true -> grant.Value
                        | false -> RankingPoints.Zero))
            |> Map.values
            |> Seq.sum

        { matchData with
            GamePoints = Some totalGamePoints
            RankingPoints = Some totalRankingPoints }
