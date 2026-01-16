module ParagonRobotics.DiamondScout.Common.Phase

type SubPhase =
    { Name: string
       }

type Phase =
    | Teleop of SubPhase list
    | Autonomous of SubPhase list
    | Endgame of SubPhase list

type PhaseMap<'a> =
    { Teleop: 'a list
      Autonomous: 'a list
      Endgame: 'a list }

    member this.Item
        with get (phase: Phase) =
            match phase with
            | Teleop _ -> this.Teleop
            | Autonomous _ -> this.Autonomous
            | Endgame _ -> this.Endgame
    member this.ZipPhases teleop autonomous endgame =
        match teleop, autonomous, endgame with
        | Teleop teleopSubPhases, _, _ when teleopSubPhases.Length <> this.Teleop.Length -> invalidOp "Failed to map defined items to Teleop phase: length mismatch"
        | _, Autonomous autonomousSubPhases, _ when autonomousSubPhases.Length <> this.Autonomous.Length -> failwith "Failed to map defined items to Autonomous phase: length mismatch"
        | _, _, Endgame endgameSubPhases when endgameSubPhases.Length <> this.Endgame.Length -> failwith "Failed to map defined items to Endgame phase: length mismatch"
        | Teleop teleopSubPhases, Autonomous autonomousSubPhases, Endgame endgameSubPhases ->
            (List.zip this.Teleop teleopSubPhases, List.zip this.Autonomous autonomousSubPhases, List.zip this.Endgame endgameSubPhases)
        | _ -> invalidOp "Failed to map defined items to game phases: Phase definitions must be provided for all phases in order: teleop, autonomous, endgame"

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PhaseMapModule =
    let map f m =
        { Teleop = f m.Teleop
          Autonomous = f m.Autonomous
          Endgame = f m.Endgame }
