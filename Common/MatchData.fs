module ParagonRobotics.DiamondScout.Common.MatchData

open ParagonRobotics.DiamondScout.Common.Phase
open ParagonRobotics.DiamondScout.Common.RobotData
open ParagonRobotics.DiamondScout.Common.Ranking
open ParagonRobotics.DiamondScout.Common.Scoring
open ParagonRobotics.DiamondScout.Common.Teams
open ParagonRobotics.DiamondScout.Common.Identifiers

[<Struct>]
type MatchNumber = MatchNumber of int

[<Struct>]
type Alliance =
    | Red
    | Blue

[<Struct>]
[<RequireQualifiedAccess>]
type EndgameResult =
    | Success of ScoringTier
    | Failure
    | NotAttempted

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
type Breakdown =
    | AutoEmergency
    | Emergency
    | Malfunction

type MatchScoutResult =
    { Team: Team
      Alliance: Alliance
      RankingPoints: RankingPoints
      Scores: SubPhaseMap<Score>
      Endgame: Endgame
      Breakdowns: Breakdown list option
      Infractions: InfractionId list }

type Match = { MatchNumber: MatchNumber; MatchScoutResults: MatchScoutResult list }