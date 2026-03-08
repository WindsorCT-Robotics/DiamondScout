namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type Foul =
    | Minor of Penalty: uint
    | Major of Penalty: uint

    member this.Match(minorAction: Action<uint>, majorAction: Action<uint>) =
        match this with
        | Minor penalty -> minorAction.Invoke penalty
        | Major penalty -> majorAction.Invoke penalty

[<Struct>]
type Card =
    | Yellow
    | Red

    member this.Match(yellowAction: Action<unit>, redAction: Action<unit>) =
        match this with
        | Yellow -> yellowAction.Invoke()
        | Red -> redAction.Invoke()

type Infraction =
    { Name: string
      Severity: Foul option
      Card: Card option }

    static member Create name severity card =
        { Name = name
          Severity = severity
          Card = card }

    member this.ChangeName name = { this with Name = name }
    member this.ChangeSeverity severity = { this with Severity = severity }
    member this.ChangeCard card = { this with Card = card }

[<RequireQualifiedAccess>]
module Infraction =
    let create card severity name =
        { Name = name
          Severity = severity
          Card = card }

    let changeName infraction name =
        { infraction with
            Infraction.Name = name }

    let changeSeverity infraction severity = { infraction with Severity = severity }
    let changeCard infraction card = { infraction with Card = card }

    type Event =
        | InfractionDefined of infractionId: InfractionId * infraction: Infraction
        | NameChanged of infractionId: InfractionId * name: string
        | SeverityChanged of infractionId: InfractionId * severity: Foul option
        | CardChanged of infractionId: InfractionId * card: Card option
        | InfractionRemoved of infractionId: InfractionId
