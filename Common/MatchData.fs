namespace ParagonRobotics.DiamondScout.Common.MatchData

open System.Collections.Generic
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Identifiers

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
    
type GamePhase =
    | Teleop
    | Autonomous
    | Endgame

[<Struct>]
type PhaseScores = PhaseScores of Dictionary<GamePhase, Score>

type RankPointThreshold =
    | PointsThreshold of Points * RankingPoints
    | ScoreThreshold of int * RankingPoints

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

[<Struct>]
type Card =
    | Yellow
    | Red

[<Struct>]
type Infraction =
    { Foul: Foul voption
      Card: Card voption }

type ScoutResult =
    { Team: Team
      Alliance: Alliance
      Scores: Score list
      RankingPoints: RankingPoints
      Endgame: Endgame
      EmergencyStop: EmergencyStop voption
      Infractions: Infraction list }

type MatchData =
    { Name: string
      Results: ScoutResult list }

type EventData = { Name: string; Matches: MatchId list }
