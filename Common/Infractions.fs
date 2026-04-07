namespace ParagonRobotics.DiamondScout.Common.Functional

open System
open System.Collections.Generic
open System.Linq
open FsToolkit.ErrorHandling

[<Struct>]
type InfractionName = InfractionName of string

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
    private
        { Name: InfractionName
          Severity: Foul option
          Card: Card option }

[<RequireQualifiedAccess>]
module Infraction =
    type Error =
        | InfractionNameEmpty
        
    module private OnlyIf =
        let nameNotEmpty (InfractionName name as infractionName) =
            match System.String.IsNullOrWhiteSpace name with
            | true -> InfractionNameEmpty |> Validation.error
            | false -> infractionName |> Validation.ok
        
    let create card severity name = validation {
        let! name = OnlyIf.nameNotEmpty name
        
        return
            { Name = name
              Severity = severity
              Card = card }
    }

    let changeName infraction name = validation {
        let! name = OnlyIf.nameNotEmpty name
        
        return
            { infraction with
                Infraction.Name = name }
    }

    let changeSeverity infraction severity = { infraction with Severity = severity }
    let changeCard infraction card = { infraction with Card = card }

type Infraction with
    static member Create name severity card = Infraction.create card severity name

    member this.ChangeName name = Infraction.changeName this name
    member this.ChangeSeverity severity = Infraction.changeSeverity this severity
    member this.ChangeCard card = Infraction.changeCard this card
