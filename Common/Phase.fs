module ParagonRobotics.DiamondScout.Common.Phase

open ParagonRobotics.DiamondScout.Common.Identifiers

type Phase =
    | Autonomous
    | Teleop
    | Endgame

type SubPhase =
    { Name: string
      Description: string
      Phase: Phase }

type SubPhaseMap<'T> = Map<SubPhaseId, 'T>
