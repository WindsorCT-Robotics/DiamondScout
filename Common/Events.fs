module ParagonRobotics.DiamondScout.Common.Events

open ParagonRobotics.DiamondScout.Common.MatchData

[<Struct>]
type MatchNumber = MatchNumber of int

type Event =
    { Name: string
      Matches: Map<MatchNumber, MatchScoutResult> }
