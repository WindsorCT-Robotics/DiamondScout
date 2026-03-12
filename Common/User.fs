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

type User = private {
    Name: UserName
    Role: Role
    IsActive: bool }
    with
    static member Zero = { Name = UserName "" ; Role = Viewer ; IsActive = false }
    member this.IsAdmin = this.Role = Admin
    member this.IsScouter = this.Role = Scouter
    member this.IsViewer = this.Role = Viewer

[<RequireQualifiedAccess>]
module User =
    let isAdmin user = user.Role = Admin
    let isScouter user = user.Role = Scouter
    let isViewer user = user.Role = Viewer
    let private setRole role user = { user with Role = role }
    let private setName name user = { user with Name = name }
    let private deactivate user = { user with IsActive = false }
    let private activate user = { user with IsActive = true }

    let create name role =
        { Name = name
          Role = role
          IsActive = true }

    type Event =
        | Registered of name: UserName * role: Role
        | NameChanged of  newName: UserName
        | RoleChanged of  newRole: Role
        | Deactivated

    module Event =
        let evolve user event =
            match event with
            | Registered (userName, role) ->  setName userName >> setRole role >> activate <| user
            | NameChanged name -> setName name user
            | RoleChanged role -> setRole role user
            | Deactivated -> deactivate user

    type Command =
        | Register of name: UserName * role: Role
        | ChangeName of newName: UserName
        | ChangeRole of newRole: Role
        | Deactivate

    type Error =
        | NameNotProvided
        | RoleNotProvided
        | UserAlreadyDeactivated

    module Validation =
        let nameNotEmpty (UserName name) =
            match String.IsNullOrWhiteSpace name with
            | true -> Validation.error NameNotProvided
            | false -> name |> UserName |> Validation.ok

        let userNotDeactivated user =
            match user.IsActive with
            | true -> Validation.ok user
            | false -> Validation.error UserAlreadyDeactivated

    [<RequireQualifiedAccess>]
    module Command =
        let decide command user =
            match command with
            | Register(name, role) ->
                validation {
                    let! name = name |> Validation.nameNotEmpty
                    
                    return (name, role) |> Registered |> List.singleton
                }
            | ChangeRole role ->
                role |> RoleChanged |> List.singleton |> Validation.ok
            | Deactivate ->
                validation {
                    let! _ = Validation.userNotDeactivated user
                    
                    return Deactivated |> List.singleton
                }
            | ChangeName newName ->
                validation {
                    let! name = newName |> Validation.nameNotEmpty
                    
                    return name |> NameChanged |> List.singleton
                }
                
    let definition =
        AggregateDefinition.create User.Zero Event.evolve Command.decide
