namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic

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

type SubPhaseMap<'T> = IReadOnlyDictionary<SubPhase, 'T>

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

    let changePhase phase subPhase = { subPhase with ParentPhase = phase }

type SubPhase with
    member this.ChangeName name = SubPhase.changeName name this
    member this.ChangeDescription desc = SubPhase.changeDescription desc this
    member this.ChangePhase phase = SubPhase.changePhase phase this
