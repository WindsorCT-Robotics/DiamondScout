namespace ParagonRobotics.DiamondScout.Common.Matches

open System

[<Struct>]
type DistrictCode = DistrictCode of string

[<Struct>]
type DistrictName = DistrictName of string

type District =
    { Code: DistrictCode
      Name: DistrictName }

[<Struct>]
type EventCode = EventCode of string

[<Struct>]
type EventName = EventName of string

type FrcEvent = 
    { Code: EventCode
      Name: EventName
      District: District voption }

[<Struct>]
type MatchNumber = MatchNumber of uint

[<Struct>]
[<RequireQualifiedAccess>]
type TournamentLevel =
    | None
    | Practice
    | Qualification
    | Playoff

[<Struct>]
type MatchId =
    private | MatchId of Guid
    static member Zero = MatchId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> MatchId
    member this.Value = let (MatchId guid) = this in guid

type MatchEventDetails =
    { Event: FrcEvent
      TournamentLevel: TournamentLevel
      MatchNumber: MatchNumber }

[<Struct>]
type Alliance =
    | Red
    | Blue

    member this.Match(redAction: Action, blueAction: Action) =
        match this with
        | Red -> redAction.Invoke()
        | Blue -> blueAction.Invoke()

[<AutoOpen>]
module Functional =
    module Match =
        let dummy = ()
