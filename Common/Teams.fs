namespace ParagonRobotics.DiamondScout.Common

/// A FIRST Robotics Competition team number.
[<Struct>]
type TeamNumber =
    /// <summary>Creates a new <see cref="T:ParagonRobotics.DiamondScout.Common.Teams.TeamNumber"/> instance.</summary>
    /// <param name="teamNumber">The team number.</param>
    | TeamNumber of teamNumber: uint

/// A FIRST Robotics Competition team name.
[<Struct>]
type TeamName =
    /// <summary>Creates a new <see cref="T:ParagonRobotics.DiamondScout.Common.Teams.TeamName"/> instance.</summary>
    /// <param name="teamName">The team name.</param>
    | TeamName of teamName: string

/// A FIRST Robotics Competition team.
type Team =
    {
        /// The team's number.
        TeamNumber: TeamNumber
        /// The team's name.
        TeamName: TeamName
        /// Notes about the team.
        Notes: Note list
    }

module Team =
    let create teamNumber teamName =
        { TeamNumber = teamNumber
          TeamName = teamName
          Notes = [] }

    let changeName team teamName = { team with TeamName = teamName }

    let addNote team note =
        { team with Notes = note :: team.Notes }
