namespace ParagonRobotics.DiamondScout.Common.Aggregates

open System.Collections.Generic
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common

type Evolve<'state, 'event> = 'state -> 'event -> 'state
type Decide<'state, 'command, 'event, 'error> = 'command -> 'state -> Validation<'event list, 'error>

type Aggregate<'state, 'command, 'event, 'error> =
    private
        { Init: 'state
          Evolve: Evolve<'state, 'event>
          Decide: Decide<'state, 'command, 'event, 'error> }

[<AutoOpen>]
module Functional=
    [<RequireQualifiedAccess>]
    module Aggregate =
        let create initialState evolver decider =
            { Init = initialState
              Evolve = evolver
              Decide = decider }

        let fold aggregate = List.fold aggregate.Evolve
        let rehydrate aggregate = fold aggregate aggregate.Init
        let decide aggregate command state = aggregate.Decide command state
        let evolve aggregate event = aggregate.Evolve event
        let init aggregate = aggregate.Init

type Aggregate<'state, 'command, 'event, 'error> with
    static member Create
        (
            initialState: 'state,
            evolver: System.Func<'state, 'event, 'state>,
            decider: System.Func<'command, 'state, Validation<'event IReadOnlyList, 'error>>
        ) =
        Aggregate.create initialState (fun s e -> evolver.Invoke(s, e)) (fun c s ->
            decider.Invoke(c, s) |> Validation.map _.FromReadOnlyList())

    static member Rehydrate(state: Aggregate<'state, 'command, 'event, 'error>, events: IReadOnlyList<'event>) =
        events |> _.FromReadOnlyList() |> Aggregate.rehydrate state

    static member Append
        (state: Aggregate<'state, 'command, 'event, 'error>, startingState: 'state, events: IReadOnlyList<'event>)
        =
        events |> _.FromReadOnlyList() |> Aggregate.fold state startingState
