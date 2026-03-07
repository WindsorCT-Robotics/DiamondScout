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
    static member Zero = { Name = ""; Role = Viewer; IsActive = true }

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

    type Event =
        | NameChanged of newName: string
        | RoleChanged of newRole: Role
        | Deactivated
        | Activated

    module Event =
        let evolve user event =
            match event with
                | NameChanged name -> setName name user
                | RoleChanged role -> setRole role user
                | Deactivated -> deactivate user
                | Activated -> activate user

    type Command =
        | Create of name: string * role: Role
        | ChangeName of newName: string
        | ChangeRole of newRole: Role
        | Deactivate
        | Activate

    type Error =
        | NameNotProvided
        | RoleNotProvided
        | UserAlreadyDeactivated
        | UserAlreadyActivated

    module Validation =
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

        let userAlreadyActivated (user: User) =
            match user.IsActive with
            | true -> Validation.error UserAlreadyActivated
            | false -> Validation.ok user

    module Command =
        let decide command user =
            match command with
            | Create (name, role) ->
                setName name >> setRole role >> activate
                <| user
                |> Validation.nameNotEmpty
                >>= Validation.roleNotNull
                |> Validation.map (fun user -> [ NameChanged user.Name; RoleChanged user.Role; Activated])
            | ChangeRole newRole ->
                setRole newRole user
                |> Validation.roleNotNull
                |> Validation.map (fun user -> [ RoleChanged user.Role ])
            | Deactivate ->
                Validation.userNotDeactivated user
                |> Validation.map (fun _ -> [Deactivated])
            | Activate ->
                user
                |> Validation.userAlreadyActivated
                |> Validation.map (fun _ -> [Activated])
            | ChangeName newName ->
                setName newName user
                |> Validation.nameNotEmpty
                |> Validation.map (fun user -> [ NameChanged user.Name ])

    let userEventStream =
        { Init = User.Zero
          Evolve = Event.evolve
          Decide = Command.decide }