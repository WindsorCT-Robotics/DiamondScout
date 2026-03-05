namespace ParagonRobotics.DiamondScout.Common.DomainEvents

open FsToolkit.ErrorHandling

type Aggregate<'state, 'command, 'event, 'error> =
    { Init: 'state
      Evolve: 'state -> 'command -> 'state
      Decide: 'command -> 'state -> Validation<'event list, 'error> }

module Aggregate =
    let create initialState evolver decider =
        { Init = initialState
          Evolve = evolver
          Decide = decider }

    let build aggregate = List.fold aggregate.Evolve
    let rebuild aggregate = build aggregate aggregate.Init
