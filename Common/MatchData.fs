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
type EmergencyStop =
    | AutoEmergency
    | Emergency

type MatchScoutResult =
    { Team: Team
      Alliance: Alliance
      RankingPoints: RankingPoints
      Scoring: PhaseMap<Score>
      Endgame: Endgame
      EmergencyStop: EmergencyStop option
      Infractions: InfractionId list }
