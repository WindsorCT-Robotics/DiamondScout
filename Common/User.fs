namespace ParagonRobotics.DiamondScout.Common

type Role =
    | Admin
    | Viewer
    | Scouter

type User = {
    Name: string
    Role: Role
}

module User =
    let isAdmin user = user.Role = Admin
    let isScouter user = user.Role = Scouter
    let isViewer user = user.Role = Viewer
    let isAuthenticated (user: User option) = user.IsSome
    let isAnonymous (user: User option) = not (isAuthenticated user)
    let setRole role user = { user with Role = role }
    let create name role = { Name = name; Role = role }