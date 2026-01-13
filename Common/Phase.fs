module ParagonRobotics.DiamondScout.Common.Phase

type Phase =
    | Teleop
    | Autonomous
    | Endgame

type PhaseMap<'a> =
    { Teleop: 'a
      Autonomous: 'a
      Endgame: 'a }

    member this.Item
        with get (phase: Phase) =
            match phase with
            | Teleop -> this.Teleop
            | Autonomous -> this.Autonomous
            | Endgame -> this.Endgame

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PhaseMapModule =
    let map f m =
        { Teleop = f m.Teleop
          Autonomous = f m.Autonomous
          Endgame = f m.Endgame }
