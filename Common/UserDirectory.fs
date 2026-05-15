namespace ParagonRobotics.DiamondScout.Common.UserDirectories

open System.Collections.Generic
open FsToolkit.ErrorHandling
open FSharp.Collections
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Users

/// Username normalized for purposes of comparison.
[<Struct>]
type NormalizedUserName = NormalizedUserName of string

type UserDirectory =
    private
        { RegisteredUsers: Set<UserId>
          UserIdsByName: Map<NormalizedUserName, UserId> }

[<RequireQualifiedAccess>]
module Events =
    type Error =
        | UserAlreadyRegistered of UserId
        | UserNotFound of UserId
        | UserNameTaken of UserId
        | UserIdIsZero

    type Command =
        | Register of userId: UserId * name: UserName
        | ChangeName of userId: UserId * name: UserName
        | Deactivate of userId: UserId

    type Event =
        | Registered of userId: UserId * name: NormalizedUserName
        | NameChanged of userId: UserId * name: NormalizedUserName
        | Deactivated of userId: UserId

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module UserDirectory =
        module private Event =
            let evolve registry event =
                match event with
                | Events.Event.Registered(id, name) ->
                    { registry with
                        RegisteredUsers = registry.RegisteredUsers |> Set.add id
                        UserIdsByName = registry.UserIdsByName |> Map.add name id }
                | Events.Event.NameChanged(id, name) ->
                    { registry with
                        UserIdsByName = registry.UserIdsByName |> Map.add name id }
                | Events.Event.Deactivated id ->
                    { registry with
                        RegisteredUsers = registry.RegisteredUsers |> Set.remove id
                        UserIdsByName = registry.UserIdsByName |> Map.filter (fun _ userId -> userId <> id) }

        [<RequireQualifiedAccess>]
        module private OnlyIf =
            let userIdNotZero id =
                match id = UserId.Zero with
                | true -> Events.Error.UserIdIsZero |> Validation.error
                | false -> Validation.ok id

            let userIdIsUnique registry id =
                match Set.contains id registry.RegisteredUsers with
                | true -> id |> Events.Error.UserAlreadyRegistered |> Validation.error
                | false -> Validation.ok id

            let userNameIsUnique registry name =
                match Map.tryFind name registry.UserIdsByName with
                | Some takenId -> takenId |> Events.Error.UserNameTaken |> Validation.error
                | None -> Validation.ok name

            let userIsRegistered registry id =
                match Set.contains id registry.RegisteredUsers with
                | true -> Validation.ok id
                | false -> id |> Events.Error.UserNotFound |> Validation.error

        module private Command =
            let normalize (UserName name) =
                name.Trim().ToUpperInvariant() |> NormalizedUserName

            let decide command registry =
                match command with
                | Events.Command.Register(id, name) ->
                    validation {
                        let! _ = OnlyIf.userIdIsUnique registry id
                        and! id = OnlyIf.userIdNotZero id
                        and! name = normalize >> OnlyIf.userNameIsUnique registry <| name

                        return Events.Event.Registered(id, name) |> List.singleton
                    }
                | Events.Command.ChangeName(id, name) ->
                    validation {
                        let! id = OnlyIf.userIsRegistered registry id
                        and! name = normalize >> OnlyIf.userNameIsUnique registry <| name

                        return Events.Event.NameChanged(id, name) |> List.singleton
                    }
                | Events.Command.Deactivate id ->
                    validation {
                        let! id = OnlyIf.userIsRegistered registry id

                        return Events.Event.Deactivated id |> List.singleton
                    }

        let private initialState =
            { RegisteredUsers = Set.empty
              UserIdsByName = Map.empty }

        let state = Aggregate.create initialState Event.evolve Command.decide

type UserDirectory with
    static member Register(userId: UserId, name: UserName) =
        (Events.Command.Register(userId, name), UserDirectory.state.Init)
        ||> UserDirectory.state.Decide
        |> Validation.map _.ToReadOnlyList()

    static member ChangeName(users: UserDirectory, userId: UserId, name: UserName) =
        (Events.Command.ChangeName(userId, name), users)
        ||> UserDirectory.state.Decide
        |> Validation.map _.ToReadOnlyList()

    static member Deactivate(users: UserDirectory, userId: UserId) =
        (Events.Command.Deactivate(userId), users)
        ||> UserDirectory.state.Decide
        |> Validation.map _.ToReadOnlyList()

    static member Evolve(users: UserDirectory, events: Events.Event IReadOnlyList) =
        events |> _.FromReadOnlyList() |> Aggregate.fold UserDirectory.state users
