namespace ParagonRobotics.DiamondScout.Common

[<Struct>]
type TeamNumber = TeamNumber of int

[<Struct>]
type TeamName = TeamName of string

type Team = {
    TeamNumber: TeamNumber
    TeamName: TeamName
}