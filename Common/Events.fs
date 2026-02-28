namespace ParagonRobotics.DiamondScout.Common

type Event =
    { Name: string
      Matches: MatchId list }

module Event =
    let create name matches = { Name = name; Matches = matches }
    let add matchId (event: Event) = { event with Matches = matchId :: event.Matches }