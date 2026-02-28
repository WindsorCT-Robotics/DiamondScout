namespace ParagonRobotics.DiamondScout.Common

type Phase =
    | Autonomous
    | Teleop
    | Endgame

type SubPhase =
    { Name: string
      Description: string
      Phase: Phase }

type SubPhaseMap<'T> = Map<SubPhaseId, 'T>

module SubPhase =
    let create phase desc name = { Name = name; Description = desc; Phase = phase }
    let changeName subPhase name = { subPhase with SubPhase.Name = name }
    let changeDescription subPhase desc = { subPhase with SubPhase.Description = desc }
    let changePhase subPhase phase = { subPhase with Phase = phase }