namespace ParagonRobotics.DiamondScout.Common

type Role =
    | Admin
    | Scouter

type User = {
    Name: string
    Role: Role
}

module User =
    let isAdmin (user: User) = user.Role = Admin
    let isScouter (user: User) = user.Role = Scouter
    let isAuthenticated (user: User option) = user.IsSome
    let isAnonymous (user: User option) = not (isAuthenticated user)
    let setRole role user = { user with Role = role }
    let create name role = { Name = name; Role = role }