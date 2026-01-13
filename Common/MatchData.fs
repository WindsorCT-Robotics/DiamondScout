module ParagonRobotics.DiamondScout.Common.MatchData

open ParagonRobotics.DiamondScout.Common.Phase
open ParagonRobotics.DiamondScout.Common.PitData
open ParagonRobotics.DiamondScout.Common.Ranking
open ParagonRobotics.DiamondScout.Common.Scoring
open ParagonRobotics.DiamondScout.Common.Teams
open ParagonRobotics.DiamondScout.Common.Identifiers

[<Struct>]
type Alliance =
    | Red
    | Blue

[<Struct>]
type AutoScore = 
    | AutoScored
    | AutoNotScored
    | NoAuto

[<Struct>]
type ScoringTier = ScoringTier of int

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

type MatchScoutResult =
    { Team: Team
      Alliance: Alliance
      Autos: AutoScore list
      Scores: Score list
      RankingPoints: RankingPoints
      Endgame: Endgame
      EmergencyStop: EmergencyStop voption
      Infractions: Infraction list }

type MatchData =
    { Name: string
      Results: ScoutResult list }

type EventData = { Name: string; Matches: MatchId list }
