namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling

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

        type Event =
            private
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
            member this.Handle(notCreatedHandler: Action, noNameHandler: Action, noRoleHandler: Action, userDeactivatedHandler: Action) =
                match this with
                | UserNotCreated -> notCreatedHandler.Invoke()
                | NameNotProvided -> noNameHandler.Invoke()
                | RoleNotProvided -> noRoleHandler.Invoke()
                | UserAlreadyDeactivated -> userDeactivatedHandler.Invoke()

        module private Event =
            let evolve user event =
                match event with
                | Registered(userName, role) -> create userName role
                | NameChanged name ->
                    match user with
                    | User.NotRegistered -> User.NotRegistered
                    | User.Inactive data -> data |> withName name |> User.Inactive
                    | User.Registered data -> data |> withName name |> User.Registered
                | RoleChanged role ->
                    match user with
                    | User.NotRegistered -> User.NotRegistered
                    | User.Inactive data -> data |> withRole role |> User.Inactive
                    | User.Registered data -> data |> withRole role |> User.Registered
                | Deactivated ->
                    match user with
                    | User.NotRegistered -> User.NotRegistered
                    | User.Inactive _ as user -> user
                    | User.Registered user -> user |> User.Inactive

        [<RequireQualifiedAccess>]
        module private OnlyIf =
            let nameNotEmpty (UserName name) =
                match String.IsNullOrWhiteSpace name with
                | true -> Validation.error NameNotProvided
                | false -> name |> UserName |> Validation.ok

            let userNotDeactivated user =
                match user with
                | User.NotRegistered -> Validation.error UserNotCreated
                | User.Inactive _ -> Validation.error UserAlreadyDeactivated
                | User.Registered _ -> Validation.ok user

            let userNotActivated user =
                match user with
                | User.NotRegistered -> Validation.error UserNotCreated
                | User.Inactive _ -> Validation.ok user
                | User.Registered _ -> Validation.error UserAlreadyDeactivated

        [<RequireQualifiedAccess>]
        module private Command =
            let decide command user =
                match command with
                | Register(name, role) ->
                    validation {
                        let! name = name |> OnlyIf.nameNotEmpty

                        return (name, role) |> Registered |> List.singleton
                    }
                | ChangeRole role -> role |> RoleChanged |> List.singleton |> Validation.ok
                | Deactivate ->
                    validation {
                        let! _ = OnlyIf.userNotDeactivated user

                        return Deactivated |> List.singleton
                    }
                | ChangeName newName ->
                    validation {
                        let! name = newName |> OnlyIf.nameNotEmpty

                        return name |> NameChanged |> List.singleton
                    }

        let definition = AggregateDefinition.create User.NotRegistered Event.evolve Command.decide

type UserData with
    member this.IsAdmin = UserData.isAdmin this
    member this.isScouter = UserData.isScouter this
    member this.IsViewer = UserData.isViewer this
        
type User with
    static member Register (name: UserName, role: Role) =
        (UserData.Command.Register(name, role), NotRegistered) ||> UserData.definition.Decide |> Validation.map _.ToReadOnlyList()
    static member ChangeName (user: User, newName: UserName) =
        (UserData.Command.ChangeName(newName), user) ||> UserData.definition.Decide |> Validation.map _.ToReadOnlyList()
    static member ChangeRole (user: User, newRole: Role) =
        (UserData.Command.ChangeRole(newRole), user) ||> UserData.definition.Decide |> Validation.map _.ToReadOnlyList()
    static member Deactivate (user: User) =
        (UserData.Command.Deactivate, user) ||> UserData.definition.Decide |> Validation.map _.ToReadOnlyList()
    static member Evolve (user: User, events: UserData.Event IReadOnlyList) =
        events
        |> _.FromReadOnlyList()
        |> foldEvents UserData.definition user