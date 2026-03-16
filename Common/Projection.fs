namespace ParagonRobotics.DiamondScout.Common

type Apply<'state, 'event> = 'state -> 'event -> 'state

type Projection<'state, 'event> =
    internal
        { Init: 'state
          Apply: Apply<'state, 'event> }

[<RequireQualifiedAccess>]
module Projection =
    let internal create initialState apply =
        { Init = initialState
          Apply = apply }

    let foldEvents projection =
        List.fold projection.Apply

    let rebuild projection =
        foldEvents projection projection.Init

type Projection<'state, 'event> with
    static member Project projection 