namespace ParagonRobotics.DiamondScout.Common

[<Struct>]
type District = District of string

type FrcEvent =
    { Name: string
      Matches: MatchId list }

    static member Create name = { Name = name; Matches = [] }

[<RequireQualifiedAccess>]
module FrcEvent =
    let create name matches = { Name = name; Matches = matches }

    let add matchId (event: FrcEvent) =
        { event with
            Matches = matchId :: event.Matches }

    type Event =
        | CreateEvent of eventId: FrcEventId * event: FrcEvent
        | AddMatch of eventId: FrcEventId * matchId: MatchId
        | RemoveMatch of eventId: FrcEventId * matchId: MatchId
        | RemoveEvent of eventId: FrcEventId
