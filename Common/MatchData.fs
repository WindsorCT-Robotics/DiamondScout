namespace ParagonRobotics.DiamondScout.Common

[<Struct>]
type MatchNumber = MatchNumber of uint

type AllianceScoutingResults =
    { Team1: ScoutingResultsId option
      Team2: ScoutingResultsId option
      Team3: ScoutingResultsId option }

type MatchScoutingResults =
    { RedAlliance: AllianceScoutingResults
      BlueAlliance: AllianceScoutingResults }

type Match =
    { MatchNumber: MatchNumber
      MatchScoutResults: MatchScoutingResults
      Winner: Alliance option }

[<RequireQualifiedAccess>]
module Match =
    let createMatch matchNumber matchScoutResults =
        { MatchNumber = matchNumber
          MatchScoutResults = matchScoutResults
          Winner = None }

    let updateBlue1ScoutResults scoutingResultsId matchData =
        { matchData with
            MatchScoutResults.BlueAlliance.Team1 = scoutingResultsId }

    let updateBlue2ScoutResults scoutingResultsId matchData =
        { matchData with
            MatchScoutResults.BlueAlliance.Team2 = scoutingResultsId }

    let updateBlue3ScoutResults scoutingResultsId matchData =
        { matchData with
            MatchScoutResults.BlueAlliance.Team3 = scoutingResultsId }

    let updateRed1ScoutResults scoutingResultsId matchData =
        { matchData with
            MatchScoutResults.RedAlliance.Team1 = scoutingResultsId }

    let updateRed2ScoutResults scoutingResultsId matchData =
        { matchData with
            MatchScoutResults.RedAlliance.Team2 = scoutingResultsId }

    let updateRed3ScoutResults scoutingResultsId matchData =
        { matchData with
            MatchScoutResults.RedAlliance.Team3 = scoutingResultsId }

    let setWinner winner matchData = { matchData with Winner = Some winner }

    type Event =
        | MatchAdded of frcMatchId: MatchId * frcMatch: Match
        | Blue1Scouted of frcMatchId: MatchId * scoutingResultsId: ScoutingResultsId
        | Blue2Scouted of frcMatchId: MatchId * scoutingResultsId: ScoutingResultsId
        | Blue3Scouted of frcMatchId: MatchId * scoutingResultsId: ScoutingResultsId
        | Red1Scouted of frcMatchId: MatchId * scoutingResultsId: ScoutingResultsId
        | Red2Scouted of frcMatchId: MatchId * scoutingResultsId: ScoutingResultsId
        | Red3Scouted of frcMatchId: MatchId * scoutingResultsId: ScoutingResultsId
        | MatchEnded of frcMatchId: MatchId * victor: Alliance
