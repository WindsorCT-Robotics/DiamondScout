namespace ParagonRobotics.DiamondScout.Common

open System

type Game =
    { Year: DateOnly
      Name: string
      Phases: SubPhaseId list
      GamePieces: GamePieceId list
      Infractions: InfractionId list
      PitResults: RobotId list
      Events: EventId list
      Parameters: ParameterDefinitionId list }