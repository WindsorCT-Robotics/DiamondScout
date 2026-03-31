namespace ParagonRobotics.DiamondScout.Common.Functional


[<Struct>]
type MatchNumber = MatchNumber of uint

[<Struct>]
type TournamentLevel =
    | None
    | Practice
    | Qualification
    | Playoff

type WinningAlliance =
    | Undecided
    | Winner of winningAlliance: Alliance

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

type Match =
    { MatchNumber: MatchNumber
      TournamentLevel: TournamentLevel
      MatchScoutResults: MatchScoutingResults
      Winner: WinningAlliance }

[<RequireQualifiedAccess>]
module Match =
    let createMatch tournamentLevel matchNumber matchScoutResults =
        { MatchNumber = matchNumber
          TournamentLevel = tournamentLevel
          MatchScoutResults = matchScoutResults
          Winner = Undecided }

    let scoutAllianceTeam (AllianceTeam(alliance, team)) results matchData =
        match (alliance, team) with
        | Red, Team1 ->
            { matchData with
                MatchScoutResults.RedAlliance.Team1 = results }
        | Red, Team2 ->
            { matchData with
                MatchScoutResults.RedAlliance.Team2 = results }
        | Red, Team3 ->
            { matchData with
                MatchScoutResults.RedAlliance.Team3 = results }
        | Blue, Team1 ->
            { matchData with
                MatchScoutResults.BlueAlliance.Team1 = results }
        | Blue, Team2 ->
            { matchData with
                MatchScoutResults.BlueAlliance.Team2 = results }
        | Blue, Team3 ->
            { matchData with
                MatchScoutResults.BlueAlliance.Team3 = results }

    let setWinner winner matchData =
        { matchData with
            Winner = WinningAlliance.Winner winner }
