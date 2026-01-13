module ParagonRobotics.DiamondScout.Common.PitData

open ParagonRobotics.DiamondScout.Common.Identifiers
open ParagonRobotics.DiamondScout.Common.Scoring
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
      Game: GameId
      EndgameCapable: EndgameCapable
      Drivetrain: Drivetrain }
