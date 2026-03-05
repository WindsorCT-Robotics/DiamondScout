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
    static member Create(year, name) = {
        Year = year
        Name = name
        Phases = []
        GamePieces = []
        Infractions = []
        PitResults = []
        Events = []
        Parameters = []
    }
    member this.AddPhase(phase) = { this with Phases = phase :: this.Phases }
    member this.AddGamePiece(piece) = { this with GamePieces = piece :: this.GamePieces }
    member this.AddInfraction(infraction) = { this with Infractions = infraction :: this.Infractions }
    member this.AddPitResult(robot) = { this with PitResults = robot :: this.PitResults }
    member this.AddEvent(event) = { this with Events = event :: this.Events }
    member this.AddParameter(parameter) = { this with Parameters = parameter :: this.Parameters }
    member this.RemovePhase(phase) = { this with Phases = this.Phases |> List.filter (fun p -> p <> phase) }
    member this.RemoveGamePiece(piece) = { this with GamePieces = this.GamePieces |> List.filter (fun p -> p <> piece) }
    member this.RemoveInfraction(infraction) = { this with Infractions = this.Infractions |> List.filter (fun p -> p <> infraction) }
    member this.RemovePitResult(robot) = { this with PitResults = this.PitResults |> List.filter (fun p -> p <> robot) }
    member this.RemoveEvent(event) = { this with Events = this.Events |> List.filter (fun p -> p <> event) }
    member this.RemoveParameter(parameter) = { this with Parameters = this.Parameters |> List.filter (fun p -> p <> parameter) }
