namespace ParagonRobotics.DiamondScout.Common

[<Struct>]
type Drivetrain =
    | Swerve
    | Mech
    | Tank
    | Other of string

    override this.ToString() =
        match this with
        | Swerve -> "Swerve"
        | Mech -> "Mecanum"
        | Tank -> "Tank"
        | Other s -> s

[<Struct>]
type EndgameCapable =
    | TierCapability of ScoringTier
    | NotCapable

type Robot =
    { Name: string
      Team: TeamId
      Game: GameId
      EndgameCapable: EndgameCapable
      Drivetrain: Drivetrain
      Notes: Note list }

module Robot =
    let create name team game scoringTier drivetrain = { Name = name; Team = team; Game = game; EndgameCapable = TierCapability scoringTier; Drivetrain = drivetrain; Notes = [] }
    let associateTeam robot team = { robot with Team = team }
    let associateGame robot game = { robot with Game = game }
    let setEndgameCapabilities robot endgameCapability = { robot with EndgameCapable = endgameCapability }
    let setDrivetrain robot drivetrain = { robot with Drivetrain = drivetrain }
    let addNote robot note = { robot with Robot.Notes = note :: robot.Notes }