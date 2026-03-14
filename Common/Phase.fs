namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type PhaseName = PhaseName of string

[<Struct>]
type PhaseDescription = PhaseDescription of string

type Phase =
    | Autonomous
    | Teleop
    | Endgame

    member this.Match(autonomousAction: Action, teleopAction: Action, endgameAction: Action) =
        match this with
        | Autonomous -> autonomousAction.Invoke()
        | Teleop -> teleopAction.Invoke()
        | Endgame -> endgameAction.Invoke()

type SubPhase =
    { Name: PhaseName
      Description: PhaseDescription
      ParentPhase: Phase }

type SubPhaseMap<'T> = Map<SubPhase, 'T>

[<RequireQualifiedAccess>]
module SubPhase =
    let create phase desc name =
        { Name = name
          Description = desc
          ParentPhase = phase }

    let changeName name subPhase = { subPhase with SubPhase.Name = name }

    let changeDescription desc subPhase =
        { subPhase with
            SubPhase.Description = desc }

    let changePhase subPhase phase = { subPhase with ParentPhase = phase }
