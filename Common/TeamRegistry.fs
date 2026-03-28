namespace ParagonRobotics.DiamondScout.Common.Functional

open FsToolkit.ErrorHandling

type TeamRegistry = private { Teams: Set<TeamNumber> }

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

    let definition = AggregateDefinition.create initialState Event.evolve Command.decide
