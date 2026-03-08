namespace ParagonRobotics.DiamondScout.Common

open System

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
    { Name: string
      Description: string
      Phase: Phase }

    static member Create phase desc name =
        { Name = name
          Description = desc
          Phase = phase }

    member this.ChangeName name = { this with Name = name }
    member this.ChangeDescription desc = { this with Description = desc }
    member this.ChangePhase phase = { this with Phase = phase }

type SubPhaseMap<'T> = Map<SubPhaseId, 'T>

[<RequireQualifiedAccess>]
module SubPhase =
    let create phase desc name =
        { Name = name
          Description = desc
          Phase = phase }

    let changeName subPhase name = { subPhase with SubPhase.Name = name }

    let changeDescription subPhase desc =
        { subPhase with
            SubPhase.Description = desc }

    let changePhase subPhase phase = { subPhase with Phase = phase }

    type Event =
        | AddPhase of subPhaseId: SubPhaseId * phase: SubPhase
        | NameChanged of subPhaseId: SubPhaseId * newName: string
        | DescriptionChanged of subPhaseId: SubPhaseId * newDescription: string
        | DeletePhase of subPhaseId: SubPhaseId
