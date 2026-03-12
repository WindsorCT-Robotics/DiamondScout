namespace ParagonRobotics.DiamondScout.Common.DomainEvents

open FsToolkit.ErrorHandling

type Evolve<'state, 'event> = 'state -> 'event -> 'state
type Decide<'state, 'command, 'event, 'error> = 'command -> 'state -> Validation<'event list, 'error>

type AggregateDefinition<'state, 'command, 'event, 'error> =
    { Init: 'state
      Evolve: Evolve<'state, 'event>
      Decide: Decide<'state, 'command, 'event, 'error> }

[<RequireQualifiedAccess>]
module AggregateDefinition =
    let create initialState evolver decider =
        { Init = initialState
          Evolve = evolver
          Decide = decider }

    let foldEvents aggregate = List.fold aggregate.Evolve
    let rehydrate aggregate = foldEvents aggregate aggregate.Init
