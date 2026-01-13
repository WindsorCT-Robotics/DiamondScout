module ParagonRobotics.DiamondScout.Common.Teams

[<Struct>]
type TeamNumber = TeamNumber of int

[<Struct>]
type TeamName = TeamName of string

type Team =
    { TeamNumber: TeamNumber
      TeamName: TeamName }
