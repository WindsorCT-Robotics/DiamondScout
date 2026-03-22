namespace ParagonRobotics.DiamondScout.Common

open System
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common.DomainEvents

[<Struct>]
type UserName = UserName of string

[<Struct>]
type Role =
    | Admin
    | Viewer
    | Scouter

    member this.Match(isAdmin: Action, isViewer: Action, isScouter: Action) =
        match this with
        | Admin -> isAdmin.Invoke()
        | Viewer -> isViewer.Invoke()
        | Scouter -> isScouter.Invoke()

type UserData =
    { Name: UserName
      Role: Role }

    static member Zero = { Name = UserName ""; Role = Viewer }

    member this.IsAdmin = this.Role = Admin
    member this.IsScouter = this.Role = Scouter
    member this.IsViewer = this.Role = Viewer

[<RequireQualifiedAccess>]
type User =
    private
    | Empty
    | Inactive of UserData
    | Active of UserData

    member this.Match(isEmpty: Action, isInactive: Action<UserData>, isActive: Action<UserData>) =
        match this with
        | Empty -> isEmpty.Invoke()
        | Inactive data -> isInactive.Invoke(data)
        | Active data -> isActive.Invoke(data)

[<RequireQualifiedAccess>]
module UserData =
    let isAdmin user = user.Role = Admin
    let isScouter user = user.Role = Scouter
    let isViewer user = user.Role = Viewer
    let private setRole role user = { user with Role = role }
    let private setName name user = { user with Name = name }

    let private create name role =
        { Name = name; Role = role } |> User.Active

    type Event =
        private
        | Registered of name: UserName * role: Role
        | NameChanged of newName: UserName
        | RoleChanged of newRole: Role
        | Deactivated

    module private Event =
        let evolve user event =
            match event with
            | Registered(userName, role) -> create userName role
            | NameChanged name ->
                match user with
                | User.Empty -> User.Empty
                | User.Inactive data -> data |> setName name |> User.Inactive
                | User.Active data -> data |> setName name |> User.Active
            | RoleChanged role ->
                match user with
                | User.Empty -> User.Empty
                | User.Inactive data -> data |> setRole role |> User.Inactive
                | User.Active data -> data |> setRole role |> User.Active
            | Deactivated ->
                match user with
                | User.Empty -> User.Empty
                | User.Inactive _ as user -> user
                | User.Active user -> user |> User.Inactive

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

    [<RequireQualifiedAccess>]
    module private OnlyIf =
        let nameNotEmpty (UserName name) =
            match String.IsNullOrWhiteSpace name with
            | true -> Validation.error NameNotProvided
            | false -> name |> UserName |> Validation.ok

        let userNotDeactivated user =
            match user with
            | User.Empty -> Validation.error UserNotCreated
            | User.Inactive _ -> Validation.error UserAlreadyDeactivated
            | User.Active _ -> Validation.ok user

        let userNotActivated user =
            match user with
            | User.Empty -> Validation.error UserNotCreated
            | User.Inactive _ -> Validation.ok user
            | User.Active _ -> Validation.error UserAlreadyDeactivated

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

    let definition = AggregateDefinition.create User.Empty Event.evolve Command.decide
