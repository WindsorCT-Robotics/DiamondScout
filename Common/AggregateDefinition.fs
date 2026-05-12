namespace ParagonRobotics.DiamondScout.Common

open System.Collections.Generic
open FsToolkit.ErrorHandling

type internal Evolve<'state, 'event> = 'state -> 'event -> 'state
type internal Decide<'state, 'command, 'event, 'error> = 'command -> 'state -> Validation<'event list, 'error>

type AggregateDefinition<'state, 'command, 'event, 'error> =
    internal
        { Init: 'state
          Evolve: Evolve<'state, 'event>
          Decide: Decide<'state, 'command, 'event, 'error> }

[<AutoOpen>]
module Functional =
    [<AutoOpen>]
    module AggregateDefinition =
        let internal create initialState evolver decider =
            { Init = initialState
              Evolve = evolver
              Decide = decider }

        let foldEvents aggregate = List.fold aggregate.Evolve
        let rehydrate aggregate = foldEvents aggregate aggregate.Init

type AggregateDefinition<'state, 'command, 'event, 'error> with
    static member Create
        (
            initialState: 'state,
            evolver: System.Func<'state, 'event, 'state>,
            decider: System.Func<'command, 'state, Validation<'event IReadOnlyList, 'error>>
        ) =
        create initialState (fun s e -> evolver.Invoke(s, e)) (fun c s ->
            decider.Invoke(c, s) |> Validation.map _.FromReadOnlyList())

    static member Rehydrate
        (aggregate: AggregateDefinition<'state, 'command, 'event, 'error>, events: IReadOnlyList<'event>)
        =
        events |> _.FromReadOnlyList() |> rehydrate aggregate

    static member Append
        (
            aggregate: AggregateDefinition<'state, 'command, 'event, 'error>,
            startingState: 'state,
            events: IReadOnlyList<'event>
        ) =
        events |> _.FromReadOnlyList() |> foldEvents aggregate startingState
