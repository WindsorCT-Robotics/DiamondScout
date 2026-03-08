namespace ParagonRobotics.DiamondScout.Common.DomainEvents

open System.Collections.Generic
open ParagonRobotics.DiamondScout.Common.DomainEvents

type StreamVersion = StreamVersion of int64
type StreamId = StreamId of string

type LoadResult<'event> =
    { Events: 'event IReadOnlyCollection
      Version: StreamVersion }

type ExpectedVersion =
    | Any
    | Exact of StreamVersion

type EventStoreError =
    | StreamNotFound of streamId: StreamId
    | ConcurrencyConflict of streamId: StreamId * expectedVersion: ExpectedVersion * actualVersion: StreamVersion
    | InfrastructureError of err: exn

type IEventStore<'event> =
    abstract member Load: StreamId -> Async<Result<LoadResult<'event>, EventStoreError>>

    abstract member Append:
        StreamId * ExpectedVersion * 'event IReadOnlyCollection -> Async<Result<StreamVersion, EventStoreError>>

type EventStreamContext<'event> =
    { StreamId: StreamId
      EventStore: IEventStore<'event> }

type EventStreamState<'state, 'event> =
    { Events: 'event IReadOnlyCollection
      Version: StreamVersion
      AggregateState: 'state }

type WorkflowError<'domainError> =
    | DomainError of 'domainError
    | EventStoreError of EventStoreError

type EventSourcedWorkflow<'event, 'domainError, 'a> =
    EventStreamContext<'event> -> Async<Result<'a, WorkflowError<'domainError>>>

type EventSourcedWorkflowBuilder() =
    member _.Return(x: 'a) : EventSourcedWorkflow<'event, 'domainError, 'a> = fun _ -> async.Return(Ok x)

    member _.ReturnFrom(workflow: EventSourcedWorkflow<'event, 'domainError, 'a>) = workflow

    member _.Bind(workflow, binder) =
        fun ctx ->
            async {
                let! result = workflow ctx

                match result with
                | Ok value -> return! binder value ctx
                | Error err -> return Error err
            }

    member _.Zero() = fun _ -> async.Return(Ok())

    member _.Delay(f: unit -> EventSourcedWorkflow<'event, 'domainError, 'a>) = fun ctx -> f () ctx

type ExecutionResult<'state, 'event> =
    { PreviousState: 'state
      NewEvents: 'event IReadOnlyCollection
      NewState: 'state
      NewVersion: StreamVersion }

module EventSourcedWorkflow =
    let eventStream = EventSourcedWorkflowBuilder()

    let getContext: EventSourcedWorkflow<_, _, _> = fun ctx -> ctx |> Ok |> async.Return

    let load: EventSourcedWorkflow<_, _, _> =
        fun ctx ->
            async {
                let! result = ctx.EventStore.Load ctx.StreamId
                return result |> Result.mapError EventStoreError
            }

    let rehydrate aggregate =
        eventStream {
            let! loaded = load

            let state = loaded.Events |> List.ofSeq |> List.fold aggregate.Evolve aggregate.Init

            return
                { Events = loaded.Events
                  Version = loaded.Version
                  AggregateState = state }
        }

    let decide aggregate command state =
        fun _ -> aggregate.Decide command state |> Result.mapError DomainError |> async.Return

    let append expectedVersion events =
        fun ctx ->
            async {
                let! result = ctx.EventStore.Append(ctx.StreamId, expectedVersion, events)

                return result |> Result.mapError EventStoreError
            }

    let executeWith expectedVersionFor aggregate command =
        eventStream {
            let! stream = rehydrate aggregate
            let! newEvents = decide aggregate command stream.AggregateState
            let expectedVersion = expectedVersionFor stream
            let! newVersion = append expectedVersion newEvents

            let newState = newEvents |> Seq.fold aggregate.Evolve stream.AggregateState

            return
                { PreviousState = stream.AggregateState
                  NewEvents = newEvents
                  NewState = newState
                  NewVersion = newVersion }
        }

    let execute aggregate command =
        executeWith (fun stream -> Exact stream.Version) aggregate command

    let run streamId eventStore workflow =
        let ctx =
            { StreamId = streamId
              EventStore = eventStore }

        workflow ctx
