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
      Role: Role }

    member this.IsAdmin = this.Role = Admin
    member this.IsScouter = this.Role = Scouter
    member this.IsViewer = this.Role = Viewer
    /// Clones the user and gives it the new role.
    member this.MakeAdmin() = { this with Role = Admin }
    member this.MakeScouter() = { this with Role = Scouter }
    member this.MakeViewer() = { this with Role = Viewer }
    static member Create name role = { Name = name; Role = role }

module User =
    let isAdmin user = user.Role = Admin
    let isScouter user = user.Role = Scouter
    let isViewer user = user.Role = Viewer
    let isAuthenticated (user: User option) = user.IsSome
    let isAnonymous (user: User option) = not (isAuthenticated user)
    let setRole role user = { user with Role = role }
    let create name role = { Name = name; Role = role }
