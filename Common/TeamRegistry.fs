namespace ParagonRobotics.DiamondScout.Common.TeamRegistries

open System.Collections.Generic
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Teams

type TeamRegistry = private { Teams: Set<TeamNumber> }

[<RequireQualifiedAccess>]
module Events =
    type Command =
        | RegisterTeam of TeamNumber
        | UnregisterTeam of TeamNumber

    type Error =
        | TeamAlreadyRegistered of TeamNumber
        | TeamNotRegistered of TeamNumber

    type Event =
        | TeamRegistered of TeamNumber
        | TeamUnregistered of TeamNumber

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module TeamRegistry =
        module private Event =
            let evolve registry event =
                match event with
                | Events.Event.TeamRegistered teamNumber ->
                    { registry with
                        Teams = registry.Teams.Add teamNumber }
                | Events.Event.TeamUnregistered teamNumber ->
                    { registry with
                        Teams = registry.Teams.Remove teamNumber }

        [<RequireQualifiedAccess>]
        module private OnlyIf =
            let teamIsRegistered registry teamNumber =
                match registry.Teams.Contains teamNumber with
                | true -> Validation.ok teamNumber
                | false -> teamNumber |> Events.Error.TeamNotRegistered |> Validation.error

            let teamIsNotRegistered registry teamNumber =
                match registry.Teams.Contains teamNumber with
                | true -> teamNumber |> Events.Error.TeamAlreadyRegistered |> Validation.error
                | false -> Validation.ok teamNumber

        module private Command =
            let decide command registry =
                match command with
                | Events.Command.RegisterTeam teamNumber ->
                    validation {
                        let! teamNumber = OnlyIf.teamIsNotRegistered registry teamNumber

                        return Events.Event.TeamRegistered teamNumber |> List.singleton
                    }
                | Events.Command.UnregisterTeam teamNumber ->
                    validation {
                        let! teamNumber = OnlyIf.teamIsRegistered registry teamNumber

                        return Events.Event.TeamUnregistered teamNumber |> List.singleton
                    }

        let initialState = { Teams = Set.empty }

        let state = Aggregate.create initialState Event.evolve Command.decide

type TeamRegistry with
    static member RegisterTeam teamNumber =
        (Events.Command.RegisterTeam teamNumber, TeamRegistry.state.Init)
        ||> TeamRegistry.state.Decide
        |> Validation.map _.ToReadOnlyList()

    static member UnregisterTeam teamNumber =
        (Events.Command.UnregisterTeam teamNumber, TeamRegistry.state.Init)
        ||> TeamRegistry.state.Decide
        |> Validation.map _.ToReadOnlyList()

    static member Evolve(registry: TeamRegistry, events: Events.Event IReadOnlyList) =
        events |> _.FromReadOnlyList() |> Aggregate.fold TeamRegistry.state registry
