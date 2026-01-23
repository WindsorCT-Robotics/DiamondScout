module ParagonRobotics.DiamondScout.Common.Phase

type SubPhase =
    { Name: string
       }

type Phase =
    | Teleop of SubPhase list
    | Autonomous of SubPhase list
    | Endgame of SubPhase list

type SubPhaseMapping<'T> = Map<SubPhase, 'T>

type PhaseMap<'a> =
    { Teleop: SubPhaseMapping<'a> option
      Autonomous: SubPhaseMapping<'a> option
      Endgame: SubPhaseMapping<'a> option }

    member this.Item
        with get (phase: Phase) =
            match phase with
            | Teleop _ -> this.Teleop
            | Autonomous _ -> this.Autonomous
            | Endgame _ -> this.Endgame
    static member ofMap teleop autonomous endgame = { Teleop = teleop; Autonomous = autonomous; Endgame = endgame }
    static member ofList (teleop: (SubPhase * 'a) seq option) (autonomous: (SubPhase * 'a) seq option) (endgame: (SubPhase * 'a) seq option) = { Teleop = Option.map Map.ofSeq teleop; Autonomous = Option.map Map.ofSeq autonomous; Endgame = Option.map Map.ofSeq endgame }
    
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PhaseMapModule =
    let map f m =
        { Teleop = f m.Teleop
          Autonomous = f m.Autonomous
          Endgame = f m.Endgame }
