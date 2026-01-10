namespace ParagonRobotics.DiamondScout.Common.MatchData

open System.Collections.Generic
open ParagonRobotics.DiamondScout.Common

[<Struct>]
type Alliance =
    | Red
    | Blue

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
type ScoringTier = ScoringTier of int

[<Struct>]
type EndgameCapable =
    | TierCapability of ScoringTier
    | NotCapable

[<Struct>]
[<RequireQualifiedAccess>]
type EndgameResult =
    | Success of ScoringTier
    | Failure
    | NotAttempted

[<Struct>]
type RankingPoints = RankingPoints of int

[<Struct>]
type Points = Points of int

type Score =
    | FlatScore of Points
    | TieredScore of Dictionary<ScoringTier, Points>

type PhaseScores =
    { Teleop: Score voption
      Autonomous: Score voption
      Endgame: Score voption }

type RankPointThreshold =
    |PointsThreshold of Points
    | ScoreThreshold of int

[<Struct>]
type ScoringElement =
    { Name: string
      ScoreDefinition: PhaseScores
      RankPointThreshold: RankingPoints }

[<Struct>]
type BotStrategy =
    | Offense
    | Defense
    | Both

type Endgame =
    { Capable: EndgameCapable
      Result: EndgameResult }

    static member NotCapable =
        { Capable = NotCapable
          Result = EndgameResult.NotAttempted }

[<Struct>]
type EmergencyStop =
    | AutoEmergency
    | Emergency

[<Struct>]
type Foul =
    | Minor
    | Major

type Card =
    | Yellow
    | Red

type ScoutResult =
    { Team: Team
      Scores: Score list
      RankingPoints: RankingPoints
      Endgame: Endgame
      EmergencyStop: EmergencyStop voption
      Fouls: Foul list
      Cards: Card list }
