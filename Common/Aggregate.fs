namespace ParagonRobotics.DiamondScout.Common

open System.Collections.Generic
open FsToolkit.ErrorHandling

type internal Evolve<'state, 'event> = 'state -> 'event -> 'state
type internal Decide<'state, 'command, 'event, 'error> = 'command -> 'state -> Validation<'event list, 'error>

type Aggregate<'state, 'command, 'event, 'error> =
    private
        { Init: 'state
          Evolve: Evolve<'state, 'event>
          Decide: Decide<'state, 'command, 'event, 'error> }

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Aggregate =
        let create initialState evolver decider =
            { Init = initialState
              Evolve = evolver
              Decide = decider }

        let fold state = List.fold state.Evolve
        let rehydrate state = fold state state.Init

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
