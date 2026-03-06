namespace ParagonRobotics.DiamondScout.Common.DomainEvents

open FsToolkit.ErrorHandling

type Evolve<'state, 'event> = 'state -> 'event -> 'state
type Decide<'state, 'command, 'event, 'error> = 'command -> 'state -> Validation<'event list, 'error>

type EventStream<'state, 'command, 'event, 'error> =
    { Init: 'state
      Evolve: Evolve<'state, 'event>
      Decide: Decide<'state, 'command, 'event, 'error> }

module EventStream =
    let create initialState evolver decider =
        { Init = initialState
          Evolve = evolver
          Decide = decider }

    let build aggregate = List.fold aggregate.Evolve
    let rebuild aggregate = build aggregate aggregate.Init