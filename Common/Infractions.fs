namespace ParagonRobotics.DiamondScout.Common.Infractions

open System
open FsToolkit.ErrorHandling

[<Struct>]
type InfractionId =
    private
    | InfractionId of Guid

    static member Zero = InfractionId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> InfractionId
    member this.Value = let (InfractionId guid) = this in guid

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
          Severity: Foul voption
          Card: Card voption }

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Infraction =
        type Error = | InfractionNameEmpty

        module private OnlyIf =
            let nameNotEmpty (InfractionName name as infractionName) =
                match String.IsNullOrWhiteSpace name with
                | true -> InfractionNameEmpty |> Validation.error
                | false -> infractionName |> Validation.ok

        let create card severity name =
            validation {
                let! name = OnlyIf.nameNotEmpty name

                return
                    { Name = name
                      Severity = severity
                      Card = card }
            }

        let changeName infraction name =
            validation {
                let! name = OnlyIf.nameNotEmpty name

                return
                    { infraction with
                        Infraction.Name = name }
            }

        let changeSeverity infraction severity = { infraction with Severity = severity }
        let changeCard infraction card = { infraction with Card = card }
