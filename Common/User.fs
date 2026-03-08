namespace ParagonRobotics.DiamondScout.Common

open System
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open ParagonRobotics.DiamondScout.Common.DomainEvents

type Role =
    | Admin
    | Viewer
    | Scouter

    member this.Match(isAdmin: Action, isViewer: Action, isScouter: Action) =
        match this with
        | Admin -> isAdmin.Invoke()
        | Viewer -> isViewer.Invoke()
        | Scouter -> isScouter.Invoke()

type User =
    { Name: string
      Role: Role
      IsActive: bool }

    member this.IsAdmin = this.Role = Admin
    member this.IsScouter = this.Role = Scouter
    member this.IsViewer = this.Role = Viewer

[<RequireQualifiedAccess>]
module User =
    let isAdmin user = user.Role = Admin
    let isScouter user = user.Role = Scouter
    let isViewer user = user.Role = Viewer
    let isAuthenticated (user: User option) = user.IsSome
    let isAnonymous (user: User option) = not (isAuthenticated user)
    let setRole role user = { user with Role = role }
    let setName name user = { user with Name = name }
    let deactivate user = { user with IsActive = false }
    let activate user = { user with IsActive = true }

    let create name role =
        { Name = name
          Role = role
          IsActive = true }

    type Event =
        | UserCreated of userId: UserId * user: User
        | NameChanged of userId: UserId * newName: string
        | RoleChanged of userId: UserId * newRole: Role
        | Deactivated of userId: UserId
        | Activated of userId: UserId
        | Deleted of userId: UserId

    module Event =
        let evolve users event =
            let change userId f =
                users |> Map.change userId (Option.map f)

            match event with
            | UserCreated(userId, user) -> users |> Map.add userId user
            | NameChanged(userId, name) -> change userId (setName name)
            | RoleChanged(userId, role) -> change userId (setRole role)
            | Deactivated userId -> change userId deactivate
            | Activated userId -> change userId activate
            | Deleted userId -> users |> Map.remove userId

    type Command =
        | Create of name: string * role: Role
        | ChangeName of userId: UserId * newName: string
        | ChangeRole of userId: UserId * newRole: Role
        | Deactivate of userId: UserId
        | Activate of userId: UserId
        | Delete of userId: UserId

    type Error =
        | NameNotProvided
        | RoleNotProvided
        | UserAlreadyDeactivated
        | UserAlreadyActivated
        | DuplicateName of userId: UserId
        | UserNotFound of userId: UserId

    module Validation =
        let nameNotTaken (users: Map<UserId, User>) user =
            match users |> Map.tryFindKey (fun _ u -> u.Name = user.Name) with
            | Some id -> id |> DuplicateName |> Validation.error
            | None -> Validation.ok user

        let userExists (users: Map<UserId, User>) userId =
            match users |> Map.tryFind userId with
            | Some user -> Validation.ok user
            | None -> userId |> UserNotFound |> Validation.error

        let nameNotEmpty user =
            match String.IsNullOrWhiteSpace user.Name with
            | true -> Validation.error NameNotProvided
            | false -> Validation.ok user

        let roleNotNull user =
            match Object.ReferenceEquals(user.Role, null) with
            | true -> Validation.error RoleNotProvided
            | false -> Validation.ok user

        let userNotDeactivated (user: User) =
            match user.IsActive with
            | true -> Validation.ok user
            | false -> Validation.error UserAlreadyDeactivated

        let userNotActivated (user: User) =
            match user.IsActive with
            | true -> Validation.error UserAlreadyActivated
            | false -> Validation.ok user

    [<RequireQualifiedAccess>]
    module Command =
        let decide command users =
            match command with
            | Create(name, role) ->
                let userId = Guid.NewGuid() |> UserId

                create name role
                |> Validation.nameNotEmpty
                >>= Validation.nameNotTaken users
                >>= Validation.roleNotNull
                >>= Validation.userNotDeactivated
                |> Validation.map (fun user -> [ UserCreated(userId, user) ])
            | ChangeRole(id, newRole) ->
                Validation.userExists users id
                |> Validation.map (setRole newRole)
                >>= Validation.roleNotNull
                |> Validation.map (fun user -> [ RoleChanged(id, user.Role) ])
            | Deactivate id ->
                Validation.userExists users id
                >>= Validation.userNotDeactivated
                |> Validation.map (fun _ -> [ Deactivated id ])
            | Activate id ->
                Validation.userExists users id
                >>= Validation.userNotActivated
                |> Validation.map (fun _ -> [ Activated id ])
            | ChangeName(id, newName) ->
                Validation.userExists users id
                |> Validation.map (setName newName)
                >>= Validation.nameNotEmpty
                |> Validation.map (fun user -> [ NameChanged(id, user.Name) ])
            | Delete id -> Validation.userExists users id |> Validation.map (fun _ -> [ Deleted id ])

    let userEventStream =
        EventStream.create Map.empty<UserId, User> Event.evolve Command.decide
