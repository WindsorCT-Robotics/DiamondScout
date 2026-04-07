namespace ParagonRobotics.DiamondScout.Common.Functional

open System
open ParagonRobotics.DiamondScout.Common

[<Struct>]
type SubPhaseName = SubPhaseName of string

[<Struct>]
type SubPhaseDescription = SubPhaseDescription of string

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
    { Name: SubPhaseName
      Description: SubPhaseDescription
      ParentPhase: Phase }

type SubPhaseMap<'T> = Map<SubPhaseId, 'T>

[<RequireQualifiedAccess>]
module SubPhase =
    let create phase desc name =
        { Name = name
          Description = desc
          ParentPhase = phase }

    let withName name subPhase = { subPhase with SubPhase.Name = name }

    let withDescription desc subPhase =
        { subPhase with
            SubPhase.Description = desc }

    let withPhase phase subPhase = { subPhase with ParentPhase = phase }
