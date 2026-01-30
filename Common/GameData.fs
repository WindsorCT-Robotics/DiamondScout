module ParagonRobotics.DiamondScout.Common.GameData

open System
open ParagonRobotics.DiamondScout.Common.Identifiers

type Game =
    { Year: DateOnly
      Name: string
      Phases: SubPhaseId list
      GamePieces: GamePieceId list
      Infractions: InfractionId list
      PitResults: PitResultId list
      Events: EventId list }