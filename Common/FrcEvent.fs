namespace ParagonRobotics.DiamondScout.Common

open System.Collections.Generic

[<Struct>]
type FrcEventName = FrcEventName of string

type FrcEvent =
    { Name: FrcEventName
      Matches: MatchId IReadOnlyCollection }

    static member Create name = { Name = name; Matches = [] }

[<RequireQualifiedAccess>]
module FrcEvent =
    let create name matches = { Name = name; Matches = matches }

    let add matchId (event: FrcEvent) =
        { event with
            Matches = List [ matchId; yield! event.Matches ] }
