namespace ParagonRobotics.DiamondScout.Common.Functional

open FsToolkit.ErrorHandling

[<Struct>]
type MatchNumber = MatchNumber of uint

[<Struct>]
[<RequireQualifiedAccess>]
type TournamentLevel =
    | None
    | Practice
    | Qualification
    | Playoff

type WinningAlliance =
    | Undecided
    | Winner of winningAlliance: Alliance
    | Tie

type AllianceScoutingResults =
    { Team1: ScoutingResult
      Team2: ScoutingResult
      Team3: ScoutingResult }

type MatchScoutingResults =
    { RedAlliance: AllianceScoutingResults
      BlueAlliance: AllianceScoutingResults }

    static member NotStarted =
        { RedAlliance =
            { Team1 = ScoutingResult.NotStarted
              Team2 = ScoutingResult.NotStarted
              Team3 = ScoutingResult.NotStarted }
          BlueAlliance =
            { Team1 = ScoutingResult.NotStarted
              Team2 = ScoutingResult.NotStarted
              Team3 = ScoutingResult.NotStarted } }

type AllianceTeamNumber =
    | Team1
    | Team2
    | Team3

type AllianceTeam = AllianceTeam of Alliance * AllianceTeamNumber

[<Struct>]
type RedAllianceFinalScore = RedAllianceFinalScore of uint

[<Struct>]
type BlueAllianceFinalScore = BlueAllianceFinalScore of uint

[<Struct>]
type RedAllianceFinalRankingPoints = RedAllianceFinalRankingPoints of uint

type FinalScore =
    { RedAlliance: RedAllianceFinalScore
      BlueAlliance: BlueAllianceFinalScore }
    
type RankingPointsEarned =
    { RedAlliance: RedAllianceFinalRankingPoints
      BlueAlliance: RedAllianceFinalRankingPoints }

type Match =
    { MatchNumber: MatchNumber
      TournamentLevel: TournamentLevel
      MatchScoutResults: MatchScoutingResults
      FinalScore: FinalScore option
      RankingPoints: RankingPointsEarned option
      Winner: WinningAlliance }

[<RequireQualifiedAccess>]
module Match =
    [<RequireQualifiedAccess>]
    type Error =
        | MatchNotFinalized of unfinalizedMatchResult: AllianceTeam

    module private OnlyIf =
        let matchIsFinalized (matchData: Match) =
            let results = matchData.MatchScoutResults

            let validateTeam alliance teamNumber scoutingResult =
                ScoutingResult.mustBeFinalized scoutingResult
                |> Validation.mapError (fun _ -> Error.MatchNotFinalized(AllianceTeam(alliance, teamNumber)))

            validation {
                let! _ = validateTeam Red Team1 results.RedAlliance.Team1
                and! _ = validateTeam Red Team2 results.RedAlliance.Team2
                and! _ = validateTeam Red Team3 results.RedAlliance.Team3
                and! _ = validateTeam Blue Team1 results.BlueAlliance.Team1
                and! _ = validateTeam Blue Team2 results.BlueAlliance.Team2
                and! _ = validateTeam Blue Team3 results.BlueAlliance.Team3
                return matchData
            }

    let createMatch tournamentLevel matchNumber matchScoutResults =
        { MatchNumber = matchNumber
          TournamentLevel = tournamentLevel
          MatchScoutResults = matchScoutResults
          FinalScore = None
          RankingPoints = None
          Winner = Undecided }

    let getScoutingResult alliance team matchData =
        match (alliance, team) with
        | Red, Team1 -> matchData.MatchScoutResults.RedAlliance.Team1
        | Red, Team2 -> matchData.MatchScoutResults.RedAlliance.Team2
        | Red, Team3 -> matchData.MatchScoutResults.RedAlliance.Team3
        | Blue, Team1 -> matchData.MatchScoutResults.BlueAlliance.Team1
        | Blue, Team2 -> matchData.MatchScoutResults.BlueAlliance.Team2
        | Blue, Team3 -> matchData.MatchScoutResults.BlueAlliance.Team3

    let withScoutingResult alliance team matchData newScoutingResult =
        match (alliance, team) with
        | Red, Team1 ->
            { matchData with
                MatchScoutResults.RedAlliance.Team1 = newScoutingResult }
        | Red, Team2 ->
            { matchData with
                MatchScoutResults.RedAlliance.Team2 = newScoutingResult }
        | Red, Team3 ->
            { matchData with
                MatchScoutResults.RedAlliance.Team3 = newScoutingResult }
        | Blue, Team1 ->
            { matchData with
                MatchScoutResults.BlueAlliance.Team1 = newScoutingResult }
        | Blue, Team2 ->
            { matchData with
                MatchScoutResults.BlueAlliance.Team2 = newScoutingResult }
        | Blue, Team3 ->
            { matchData with
                MatchScoutResults.BlueAlliance.Team3 = newScoutingResult }
            
    let withFinalScore matchData finalScore =
        { matchData with FinalScore = finalScore }
        
    let withRankingPoints matchData rankingPoints =
        { matchData with RankingPoints = rankingPoints }
