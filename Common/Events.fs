module ParagonRobotics.DiamondScout.Common.Events

open ParagonRobotics.DiamondScout.Common.Identifiers

type Event =
    { Name: string
      Matches: MatchId list }

let create name matches = { Name = name; Matches = matches }