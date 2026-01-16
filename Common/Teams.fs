module ParagonRobotics.DiamondScout.Common.Teams

/// A FIRST Robotics Competition team number.
[<Struct>]
type TeamNumber =
    /// <summary>Creates a new <see cref="T:ParagonRobotics.DiamondScout.Common.Teams.TeamNumber"/> instance.</summary>
    /// <param name="teamNumber">The team number.</param>
    | TeamNumber of teamNumber: int

/// A FIRST Robotics Competition team name.
[<Struct>]
type TeamName =
    /// <summary>Creates a new <see cref="T:ParagonRobotics.DiamondScout.Common.Teams.TeamName"/> instance.</summary>
    /// <param name="teamName">The team name.</param>
    | TeamName of teamName: string

/// A FIRST Robotics Competition team.
type Team =
    { /// The team's number.
      TeamNumber: TeamNumber
      /// The team's name.
      TeamName: TeamName }
