namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic
open System.Linq

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

type Infraction with
    static member Create name severity card = Infraction.create card severity name

    member this.ChangeName name = Infraction.changeName this name
    member this.ChangeSeverity severity = Infraction.changeSeverity this severity
    member this.ChangeCard card = Infraction.changeCard this card
    
[<RequireQualifiedAccess>]
[<Struct>]
type Infractions =
    | Infractions of infractions: IReadOnlyDictionary<InfractionId, Infraction>
    
    static member Empty = Map.empty<InfractionId, Infraction> :> IReadOnlyDictionary<InfractionId, Infraction> |> Infractions
    static member internal AsMap (infractions: IReadOnlyDictionary<InfractionId, Infraction>) =
        match infractions with
        | :? Map<InfractionId, Infraction> as map -> map
        | map -> map.Select (|KeyValue|) |> Map.ofSeq
        
    member internal this.AsMap () =
        let (Infractions notes) = this
        Infractions.AsMap notes
        
    // Store incoming type as a Map internally, regardless of the underlying input type
    static member Create (infractions: IReadOnlyDictionary<InfractionId, Infraction>) = Infractions.AsMap infractions :> IReadOnlyDictionary<InfractionId, Infraction> |> Infractions
    
    member this.Add (id, infraction) =
        this.AsMap() |> Map.add id infraction |> Infractions.Create
        
    member this.Remove id =
        this.AsMap() |> Map.remove id |> Infractions.Create

    member this.ContainsKey id =
        let (Infractions notes) = this in notes.ContainsKey id
