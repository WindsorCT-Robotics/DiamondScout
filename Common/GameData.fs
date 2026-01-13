module ParagonRobotics.DiamondScout.Common.GameData

open System
open ParagonRobotics.DiamondScout.Common.Identifiers

type Game =
    { Year: DateOnly
      Name: string
      GamePieces: GamePieceId list
      Infractions: InfractionId list
      PitResults: PitResultId list
      MatchResults: MatchResultId list }
