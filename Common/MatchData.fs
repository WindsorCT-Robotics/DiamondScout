namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type MatchNumber = MatchNumber of uint

type Match =
    { MatchNumber: MatchNumber
      MatchScoutResults: ScoutingResultsId list
      Winner: Alliance option }

[<RequireQualifiedAccess>]
module Match =
    let createMatch matchNumber matchScoutResults =
        { MatchNumber = matchNumber
          MatchScoutResults = matchScoutResults
          Winner = None }

    let addMatchResult matchScoutResult matchData =
        { matchData with
            MatchScoutResults = matchScoutResult :: matchData.MatchScoutResults }

    let setWinner winner matchData = { matchData with Winner = Some winner }

    type Event =
        | MatchAdded of frcMatchId: MatchId * frcMatch: Match
        | MatchScouted of frcMatchId: MatchId * frcMatchResult: ScoutingResultsId
        | MatchEnded of frcMatchId: MatchId * victor: Alliance
