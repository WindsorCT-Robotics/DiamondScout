namespace ParagonRobotics.DiamondScout.Common.Users

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Aggregates

[<Struct>]
type UserId =
    private
    | UserId of Guid

    static member Zero = UserId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> UserId
    member this.Value = let (UserId guid) = this in guid

[<Struct>]
type UserName = UserName of string

[<Struct>]
type Role =
    | Admin
    | Viewer
    | Scouter

    member this.Evaluate(isAdmin: Action, isViewer: Action, isScouter: Action) =
        match this with
        | Admin -> isAdmin.Invoke()
        | Viewer -> isViewer.Invoke()
        | Scouter -> isScouter.Invoke()

type UserData = private { Name: UserName; Role: Role }

[<RequireQualifiedAccess>]
type User =
    private
    | NotRegistered
    | Inactive of UserData
    | Registered of UserData

    member this.Evaluate(isEmpty: Action, isInactive: Action<UserData>, isActive: Action<UserData>) =
        match this with
        | NotRegistered -> isEmpty.Invoke()
        | Inactive data -> isInactive.Invoke(data)
        | Registered data -> isActive.Invoke(data)

[<RequireQualifiedAccess>]
module Events =
    type Event =
        | Registered of name: UserName * role: Role
        | NameChanged of newName: UserName
        | RoleChanged of newRole: Role
        | Deactivated

    type Command =
        | Register of name: UserName * role: Role
        | ChangeName of newName: UserName
        | ChangeRole of newRole: Role
        | Deactivate

    type Error =
        | UserNotCreated
        | NameNotProvided
        | RoleNotProvided
        | UserAlreadyDeactivated

        member this.Handle
            (notCreatedHandler: Action, noNameHandler: Action, noRoleHandler: Action, userDeactivatedHandler: Action)
            =
            match this with
            | UserNotCreated -> notCreatedHandler.Invoke()
            | NameNotProvided -> noNameHandler.Invoke()
            | RoleNotProvided -> noRoleHandler.Invoke()
            | UserAlreadyDeactivated -> userDeactivatedHandler.Invoke()

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module UserData =
        let isAdmin user = user.Role = Admin
        let isScouter user = user.Role = Scouter
        let isViewer user = user.Role = Viewer
        let private withRole role user = { user with Role = role }
        let private withName name user = { user with Name = name }

        let private create name role =
            { Name = name; Role = role } |> User.Registered

        module private Event =
            let evolve user event =
                match event with
                | Events.Event.Registered(userName, role) -> create userName role
                | Events.Event.NameChanged name ->
                    match user with
                    | User.NotRegistered -> User.NotRegistered
                    | User.Inactive data -> data |> withName name |> User.Inactive
                    | User.Registered data -> data |> withName name |> User.Registered
                | Events.Event.RoleChanged role ->
                    match user with
                    | User.NotRegistered -> User.NotRegistered
                    | User.Inactive data -> data |> withRole role |> User.Inactive
                    | User.Registered data -> data |> withRole role |> User.Registered
                | Events.Event.Deactivated ->
                    match user with
                    | User.NotRegistered -> User.NotRegistered
                    | User.Inactive _ as user -> user
                    | User.Registered user -> user |> User.Inactive

        [<RequireQualifiedAccess>]
        module private OnlyIf =
            let nameNotEmpty (UserName name) =
                match String.IsNullOrWhiteSpace name with
                | true -> Validation.error Events.Error.NameNotProvided
                | false -> name |> UserName |> Validation.ok

            let userNotDeactivated user =
                match user with
                | User.NotRegistered -> Validation.error Events.Error.UserNotCreated
                | User.Inactive _ -> Validation.error Events.Error.UserAlreadyDeactivated
                | User.Registered _ -> Validation.ok user

            let userNotActivated user =
                match user with
                | User.NotRegistered -> Validation.error Events.Error.UserNotCreated
                | User.Inactive _ -> Validation.ok user
                | User.Registered _ -> Validation.error Events.Error.UserAlreadyDeactivated

        [<RequireQualifiedAccess>]
        module private Command =
            let decide command user =
                match command with
                | Events.Command.Register(name, role) ->
                    validation {
                        let! name = name |> OnlyIf.nameNotEmpty

                        return (name, role) |> Events.Event.Registered |> List.singleton
                    }
                | Events.Command.ChangeRole role -> role |> Events.Event.RoleChanged |> List.singleton |> Validation.ok
                | Events.Command.Deactivate ->
                    validation {
                        let! _ = OnlyIf.userNotDeactivated user

                        return Events.Event.Deactivated |> List.singleton
                    }
                | Events.Command.ChangeName newName ->
                    validation {
                        let! name = newName |> OnlyIf.nameNotEmpty

                        return name |> Events.Event.NameChanged |> List.singleton
                    }

        let state = Aggregate.create User.NotRegistered Event.evolve Command.decide

type UserData with
    member this.IsAdmin = UserData.isAdmin this
    member this.isScouter = UserData.isScouter this
    member this.IsViewer = UserData.isViewer this

type User with
    static member Register(name: UserName, role: Role) =
        (Events.Command.Register(name, role), Aggregate.init UserData.state)
        ||> Aggregate.decide UserData.state
        |> Validation.map _.ToReadOnlyList()

    static member ChangeName(user: User, newName: UserName) =
        (Events.Command.ChangeName(newName), user)
        ||> Aggregate.decide UserData.state
        |> Validation.map _.ToReadOnlyList()

    static member ChangeRole(user: User, newRole: Role) =
        (Events.Command.ChangeRole(newRole), user)
        ||> Aggregate.decide UserData.state
        |> Validation.map _.ToReadOnlyList()

    static member Deactivate(user: User) =
        (Events.Command.Deactivate, user)
        ||> Aggregate.decide UserData.state
        |> Validation.map _.ToReadOnlyList()

    static member Evolve(user: User, events: Events.Event IReadOnlyList) =
        events |> _.FromReadOnlyList() |> Aggregate.fold UserData.state user
