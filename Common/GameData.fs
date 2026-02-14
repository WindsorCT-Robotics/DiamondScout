module ParagonRobotics.DiamondScout.Common.GameData

open System
open ParagonRobotics.DiamondScout.Common.Identifiers
open ParagonRobotics.DiamondScout.Common.Parameters

type Game =
    { Year: DateOnly
      Name: string
      Phases: SubPhaseId list
      GamePieces: GamePieceId list
      Infractions: InfractionId list
      PitResults: RobotId list
      Events: EventId list
      Parameters: ParameterDefinitionId list }