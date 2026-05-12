namespace ParagonRobotics.DiamondScout.Common

open System.Collections.Generic
open FsToolkit.ErrorHandling

type TeamRegistry = private { Teams: Set<TeamNumber> }


[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module TeamRegistry =
        type Command =
            | RegisterTeam of TeamNumber
            | UnregisterTeam of TeamNumber

        type Error =
            | TeamAlreadyRegistered of TeamNumber
            | TeamNotRegistered of TeamNumber

        type Event =
            | TeamRegistered of TeamNumber
            | TeamUnregistered of TeamNumber

        module private Event =
            let evolve registry event =
                match event with
                | TeamRegistered teamNumber ->
                    { registry with
                        Teams = registry.Teams.Add teamNumber }
                | TeamUnregistered teamNumber ->
                    { registry with
                        Teams = registry.Teams.Remove teamNumber }

        [<RequireQualifiedAccess>]
        module private OnlyIf =
            let teamIsRegistered registry teamNumber =
                match registry.Teams.Contains teamNumber with
                | true -> Validation.ok teamNumber
                | false -> teamNumber |> TeamNotRegistered |> Validation.error

            let teamIsNotRegistered registry teamNumber =
                match registry.Teams.Contains teamNumber with
                | true -> teamNumber |> TeamAlreadyRegistered |> Validation.error
                | false -> Validation.ok teamNumber

        module private Command =
            let decide command registry =
                match command with
                | RegisterTeam teamNumber ->
                    validation {
                        let! teamNumber = OnlyIf.teamIsNotRegistered registry teamNumber

                        return TeamRegistered teamNumber |> List.singleton
                    }
                | UnregisterTeam teamNumber ->
                    validation {
                        let! teamNumber = OnlyIf.teamIsRegistered registry teamNumber

                        return TeamUnregistered teamNumber |> List.singleton
                    }

        let initialState = { Teams = Set.empty }

        let definition = create initialState Event.evolve Command.decide

type TeamRegistry with
    static member RegisterTeam teamNumber =
        (TeamRegistry.RegisterTeam teamNumber, TeamRegistry.definition.Init)
        ||> TeamRegistry.definition.Decide
        |> Validation.map _.ToReadOnlyList()

    static member UnregisterTeam teamNumber =
        (TeamRegistry.UnregisterTeam teamNumber, TeamRegistry.definition.Init)
        ||> TeamRegistry.definition.Decide
        |> Validation.map _.ToReadOnlyList()

    static member Evolve(registry: TeamRegistry, events: TeamRegistry.Event IReadOnlyList) =
        events |> _.FromReadOnlyList() |> foldEvents TeamRegistry.definition registry
