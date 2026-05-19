namespace ParagonRobotics.DiamondScout.Common.Games

open System
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Infractions
open ParagonRobotics.DiamondScout.Common.Notes
open ParagonRobotics.DiamondScout.Common.Periods
open ParagonRobotics.DiamondScout.Common.Scoring
open ParagonRobotics.DiamondScout.Common.Teams
open ParagonRobotics.DiamondScout.Common.GamePieces
open ParagonRobotics.DiamondScout.Common.Parameters
open ParagonRobotics.DiamondScout.Common.Robots
open ParagonRobotics.DiamondScout.Common.Results
open ParagonRobotics.DiamondScout.Common.Entrants
open ParagonRobotics.DiamondScout.Common.Matches
open ParagonRobotics.DiamondScout.Common.Users

[<Struct>]
type GameName = GameName of string

type Game =
    private
        { Year: DateOnly
          Name: GameName
          Periods: Periods
          GamePieces: Map<GamePieceId, GamePiece>
          Infractions: Map<InfractionId, Infraction>
          ParameterDefinitions: Map<ParameterDefinitionId, ParameterDefinition> }

[<RequireQualifiedAccess>]
type GameState =
    | GameNotStarted
    | GameStarted of Game

[<RequireQualifiedAccess>]
module Game =
    [<RequireQualifiedAccess>]
    module Error =
        [<RequireQualifiedAccess>]
        type ParameterError =
            | ParameterDefinitionExists of ParameterDefinitionId
            | ParameterDefinitionDoesNotExist of ParameterDefinitionId
            | InvalidParameterValue of ParameterDefinitionId * ParameterValue
            | MissingParameterDefinitions of ParameterDefinitionId list
            | ExtraParameterDefinitions of ParameterDefinitionId list

        [<RequireQualifiedAccess>]
        type GameError =
            | GameAlreadyStarted
            | GameNotStarted
            | TimeframeError of TimeframeError
            | GamePieceError of GamePieces.Error
            | InfractionError of Infraction.Error
            | TimeframeNotFound of TimeframeId
            | GamePieceNotFound of GamePieceId
            | InfractionNotFound of InfractionId
            | ParameterDefinitionNotFound of ParameterDefinitionId

    [<RequireQualifiedAccess>]
    module GameValidation =
        let parameterValueMatches
            (defs: Map<ParameterDefinitionId, ParameterDefinition>)
            (id: ParameterDefinitionId)
            (value: ParameterValue)
            =
            match defs.TryFind id with
            | Some def ->
                if ParameterValue.isValid def.Spec value then
                    Validation.ok value
                else
                    Error.ParameterError.InvalidParameterValue(id, value) |> Validation.error
            | Microsoft.FSharp.Core.Option.None -> Error.ParameterError.ParameterDefinitionDoesNotExist id |> Validation.error

        let parametersMatch
            (defs: Map<ParameterDefinitionId, ParameterDefinition>)
            (category: ParameterCategory)
            (actualParams: Map<ParameterDefinitionId, ParameterValue>)
            =
            let expectedDefs = defs |> Map.filter (fun _ def -> def.Category = category)
            let expectedIds = expectedDefs |> Map.keys |> Set.ofSeq
            let actualIds = actualParams |> Map.keys |> Set.ofSeq

            let missing = Set.difference expectedIds actualIds |> Set.toList
            let extra = Set.difference actualIds expectedIds |> Set.toList

            if not (List.isEmpty missing) then
                Error.ParameterError.MissingParameterDefinitions missing |> Validation.error
            elif not (List.isEmpty extra) then
                Error.ParameterError.ExtraParameterDefinitions extra |> Validation.error
            else
                let results =
                    actualParams
                    |> Map.toSeq
                    |> Seq.map (fun (id, value) -> parameterValueMatches defs id value)
                    |> Seq.toList
                
                results 
                |> List.fold (fun acc res -> 
                    match acc, res with
                    | Validation.Ok _, Validation.Ok _ -> acc
                    | Validation.Error e1, Validation.Error e2 -> Validation.Error (e1 @ e2)
                    | Validation.Error _, _ -> acc
                    | _, Validation.Error e2 -> Validation.Error e2
                ) (Validation.Ok actualParams)

    [<RequireQualifiedAccess>]
    module Events =
        type GameStartedArgs = { Year: DateOnly; Name: GameName }
        type GameNameChangedArgs = { Name: GameName }
        type GameYearChangedArgs = { Year: DateOnly }

        type Event =
            | GameStarted of GameStartedArgs
            | GameNameChanged of GameNameChangedArgs
            | GameYearChanged of GameYearChangedArgs
            | TimeframeDefined of Period * TimeframeId * TimeframeName * TimeframeDuration
            | TimeframeRemoved of Period * TimeframeName
            | GamePieceDefined of GamePieceId * GamePieceName
            | GamePieceChanged of GamePieceId * GamePieceName
            | InfractionDefined of InfractionId * Infraction
            | InfractionChanged of InfractionId * Infraction
            | ParameterDefined of ParameterDefinitionId * ParameterDefinition
            | ParameterChanged of ParameterDefinitionId * ParameterDefinition

        type Command =
            | StartGame of GameStartedArgs
            | ChangeGameName of GameNameChangedArgs
            | ChangeGameYear of GameYearChangedArgs
            | DefineTimeframe of Period * TimeframeId * TimeframeName * TimeframeDuration
            | RemoveTimeframe of Period * TimeframeName
            | DefineGamePiece of GamePieceId * GamePieceName
            | ChangeGamePiece of GamePieceId * GamePieceName
            | DefineInfraction of InfractionId * Infraction
            | ChangeInfraction of InfractionId * Infraction
            | DefineParameter of ParameterDefinitionId * ParameterDefinition
            | ChangeParameter of ParameterDefinitionId * ParameterDefinition

    let private evolve state event =
        match state, event with
        | GameState.GameNotStarted, Events.Event.GameStarted args ->
            GameState.GameStarted
                { Year = args.Year
                  Name = args.Name
                  Periods = Unchecked.defaultof<Periods>
                  GamePieces = Map.empty
                  Infractions = Map.empty
                  ParameterDefinitions = Map.empty }
        | GameState.GameStarted game, Events.Event.TimeframeDefined (period, id, name, duration) ->
            // In a real implementation, we would call Timeframe.addTimeframe
            state
        | GameState.GameStarted game, Events.Event.GamePieceDefined (id, name) ->
            // In a real implementation, we would call GamePiece.create and add to map
            state
        | GameState.GameStarted game, Events.Event.InfractionDefined (id, infraction) ->
            GameState.GameStarted { game with Infractions = Map.add id infraction game.Infractions }
        | GameState.GameStarted game, Events.Event.ParameterDefined (id, def) ->
            GameState.GameStarted { game with ParameterDefinitions = Map.add id def game.ParameterDefinitions }
        | _ -> state

    let decide command state =
        match command with
        | Events.Command.StartGame args ->
            validation {
                return [ Events.Event.GameStarted args ]
            }
        | _ -> Validation.ok []

    [<RequireQualifiedAccess>]
    module Internal =
        let dummy = ()

    let aggregate : Aggregate<GameState, Events.Command, Events.Event, Error.GameError list> = 
        Aggregate.create GameState.GameNotStarted evolve decide
