namespace ParagonRobotics.DiamondScout.Common.Functional

open ParagonRobotics.DiamondScout.Common

[<Struct>]
type FrcEventName = FrcEventName of string

type FrcEvent =
    { Name: FrcEventName
      Matches: Map<MatchId, Match> }

[<RequireQualifiedAccess>]
module FrcEvent =
    let createWithMatches name matches = { Name = name; Matches = matches }
    let create name = createWithMatches name Map.empty

    let addMatch matchId matchData (event: FrcEvent) =
        { event with
            Matches = event.Matches |> Map.add matchId matchData }
        
    let removeMatch matchId event =
        { event with
              Matches = event.Matches |> Map.remove matchId }

    let changeName name frcEvent =
        { frcEvent with
              FrcEvent.Name = name }