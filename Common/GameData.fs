namespace ParagonRobotics.DiamondScout.Common

open System

type Game =
    { Year: DateOnly
      Name: string
      Phases: SubPhaseId list
      GamePieces: GamePieceId list
      Infractions: InfractionId list
      PitResults: RobotId list
      Events: FrcEventId list
      Parameters: ParameterDefinitionId list }

[<RequireQualifiedAccess>]
module Game =
    let setYear year game = { game with Year = year }
    let setName name game = { game with Game.Name = name }

    let addPhase phase game =
        { game with
            Phases = phase :: game.Phases }

    let removePhase phase game =
        { game with
            Phases = game.Phases |> List.filter (not << (=) phase) }

    let addGamePiece gamePiece game =
        { game with
            GamePieces = gamePiece :: game.GamePieces }

    let removeGamePiece gamePiece game =
        { game with
            GamePieces = game.GamePieces |> List.filter (not << (=) gamePiece) }

    let addInfraction infraction game =
        { game with
            Game.Infractions = infraction :: game.Infractions }

    let removeInfraction infraction game =
        { game with
            Game.Infractions = game.Infractions |> List.filter (not << (=) infraction) }

    let addPitResult pitResult game =
        { game with
            PitResults = pitResult :: game.PitResults }

    let removePitResult pitResult game =
        { game with
            PitResults = game.PitResults |> List.filter (not << (=) pitResult) }

    let addFrcEvent event game =
        { game with
            Events = event :: game.Events }

    let removeFrcEvent event game =
        { game with
            Events = game.Events |> List.filter (not << (=) event) }

    let addParameter parameter game =
        { game with
            Game.Parameters = parameter :: game.Parameters }

    let removeParameter parameter game =
        { game with
            Game.Parameters = game.Parameters |> List.filter (not << (=) parameter) }

    type Event =
        | GameCreated of gameId: GameId * game: Game
        | SubPhaseCreated of gameId: GameId * subPhaseId: SubPhaseId
        | SubPhaseDeleted of gameId: GameId * subPhaseId: SubPhaseId
        | GamePieceCreated of gameId: GameId * gamePieceId: GamePieceId
        | GamePieceDeleted of gameId: GameId * gamePieceId: GamePieceId
        | InfractionCreated of gameId: GameId * infractionId: InfractionId
        | InfractionDeleted of gameId: GameId * infractionId: InfractionId
        | PitResultCreated of gameId: GameId * pitResultId: RobotId
        | PitResultDeleted of gameId: GameId * pitResultId: RobotId
        | ParameterDefinitionCreated of gameId: GameId * parameterDefinitionId: ParameterDefinitionId
        | ParameterDefinitionDeleted of gameId: GameId * parameterDefinitionId: ParameterDefinitionId
        | FrcEventCreated of gameId: GameId * frcEventId: FrcEventId
        | FrcEventDeleted of gameId: GameId * frcEventId: FrcEventId
