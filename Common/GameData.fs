module ParagonRobotics.DiamondScout.Common.GameData

open System
open ParagonRobotics.DiamondScout.Common.MatchData

type Game =
    { Year: DateOnly
      Name: string
      AutonomousChallenges: ScoringElement list
      GamePieces: ScoringElement list
      EndgameChallenge: ScoringElement list
      Infractions: Infraction list }
