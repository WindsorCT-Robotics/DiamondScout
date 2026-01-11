namespace ParagonRobotics.DiamondScout.Common.PitData

open ParagonRobotics.DiamondScout.Common.Teams

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

type PitScoutResult =
    { Team: Team
      EndgameCapable: EndgameCapable
      Drivetrain: Drivetrain
 }

type PitData =
    { TeamNumber: TeamNumber
      Results: PitScoutResult list }
