namespace ParagonRobotics.DiamondScout.Common.Projections

open System.Collections.Generic

type Apply<'state, 'event> = 'state -> 'event -> 'state

type Projection<'state, 'event> =
    internal
        { Init: 'state
          Apply: Apply<'state, 'event> }

[<AutoOpen>]
module Functional=
    [<RequireQualifiedAccess>]
    module Projection =
        let create initialState apply = { Init = initialState; Apply = apply }

        let fold projection = List.fold projection.Apply

        let rebuild projection = fold projection projection.Init

type Projection<'state, 'event> with
    static member Project(projection, events: IReadOnlyList<'event>) =
        events |> List.ofSeq |> Projection.rebuild projection

    static member Update(projection, events: IReadOnlyList<'event>) =
        events |> List.ofSeq |> Projection.fold projection
