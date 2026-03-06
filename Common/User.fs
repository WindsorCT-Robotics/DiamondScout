namespace ParagonRobotics.DiamondScout.Common

open System

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
    /// Clones the user and gives it the new role.
    member this.MakeAdmin() = { this with Role = Admin }
    member this.MakeScouter() = { this with Role = Scouter }
    member this.MakeViewer() = { this with Role = Viewer }
    static member Create name role = { Name = name; Role = role; IsActive = true }

[<RequireQualifiedAccess>]
module User =
    let isAdmin user = user.Role = Admin
    let isScouter user = user.Role = Scouter
    let isViewer user = user.Role = Viewer
    let isAuthenticated (user: User option) = user.IsSome
    let isAnonymous (user: User option) = not (isAuthenticated user)
    let setRole role user = { user with Role = role }
    let create name role = { Name = name; Role = role; IsActive = true }
    let deactivate user = { user with IsActive = false }

    type Events =
        | Created of name: string * role: Role
        | RoleChanged of newRole: Role
        | Deactivated

    module Events =
        let evolve user event =
            match event with
                | Created (name, role) -> create name role
                | RoleChanged role -> setRole role user
                | Deactivated -> deactivate user

    type Command =
        | Create of name: string * role: Role
        | ChangeRole of newRole: Role
        | Deactivate

    module Commands =
