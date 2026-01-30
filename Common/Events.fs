module ParagonRobotics.DiamondScout.Common.Events

open ParagonRobotics.DiamondScout.Common.Identifiers

type Event =
    { Name: string
      Matches: MatchId list }
