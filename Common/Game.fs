namespace ParagonRobotics.DiamondScout.Common.Functional

open System
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common


[<Struct>]
type GameName = GameName of string

type Game =
    private
        { Year: DateOnly
          Name: GameName
          Periods: Periods
          GamePieces: Map<GamePieceId, GamePiece>
          Infractions: Map<InfractionId, Infraction>
          PitResults: Map<RobotId, Robot>
          ScoutingResults: Map<ScoutingResultId, ScoutingResult>
          ParameterDefinitions: Map<ParameterDefinitionId, ParameterDefinition>
          Notes: Map<NoteId, Note> }

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
            | NotAPitScoutingParameter of ParameterDefinitionId
            | NotAMatchScoutingParameter of ParameterDefinitionId
            | InvalidParameterValue of ParameterDefinitionId * ParameterValue
            | MissingParameterDefinitions of ParameterDefinitionId list
            | ExtraParameterDefinitions of ParameterDefinitionId list

        type RobotError =
            | RobotAlreadyExists of RobotId
            | RobotDoesNotExist of RobotId

        [<RequireQualifiedAccess>]
        type GameError =
            | GameAlreadyStarted
            | GameNotStarted
            | RobotError of Robot.Error
            | ScoutingResultError of ScoutingResult.Error
            | NoteError of Note.Error
            | TimeframeError of Timeframe.Error
            | GamePieceError of GamePiece.Error
            | InfractionError of Infraction.Error
            | TimeframeNotFound of TimeframeId
            | GamePieceNotFound of GamePieceId
            | InfractionNotFound of InfractionId
            | RobotNotFound of RobotId
            | ScoutingResultNotFound of ScoutingResultId
            | ParameterDefinitionNotFound of ParameterDefinitionId
            | NoteNotFound of NoteId

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
                    GameError.InvalidParameterValue(id, value) |> Validation.error
            | Microsoft.FSharp.Core.Option.None -> GameError.ParameterDoesNotExist id |> Validation.error

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
                GameError.MissingParameterDefinitions missing |> Validation.error
            elif not (List.isEmpty extra) then
                GameError.ExtraParameterDefinitions extra |> Validation.error
            else
                let results =
                    actualParams
                    |> Map.toSeq
                    |> Seq.map (fun (id, value) -> parameterValueMatches defs id value)
                    |> Seq.toList

                let errors =
                    results
                    |> List.choose (function
                        | Error e -> Microsoft.FSharp.Core.Option.Some e
                        | Ok _ -> Microsoft.FSharp.Core.Option.None)
                    |> List.concat

                if List.isEmpty errors then
                    Validation.ok actualParams
                else
                    Result.Error errors

    [<RequireQualifiedAccess>]
    type Command =
        | Start of name: GameName * year: DateOnly
        | ChangeName of name: GameName
        | ChangeYear of year: DateOnly
        // Timeframe
        | DefineTimeframe of period: Period * id: TimeframeId * name: TimeframeName * duration: TimeframeDuration
        | RemoveTimeframe of period: Period * id: TimeframeId
        // GamePiece
        | DefineGamePiece of id: GamePieceId * name: GamePieceName * values: Map<TimeframeId, ScoreValue>
        | ChangeGamePieceName of id: GamePieceId * name: GamePieceName
        | ChangeGamePieceScoreValues of id: GamePieceId * values: Map<TimeframeId, ScoreValue>
        // Infraction
        | DefineInfraction of id: InfractionId * name: InfractionName * severity: Foul voption * card: Card voption
        | ChangeInfractionName of id: InfractionId * name: InfractionName
        | ChangeInfractionSeverity of id: InfractionId * severity: Foul voption
        | ChangeInfractionCard of id: InfractionId * card: Card voption
        // PitResult
        | ScoutPit of
            id: RobotId *
            teamId: TeamId *
            name: RobotName *
            endgame: EndgameCapable *
            drivetrain: Drivetrain *
            parameters: Map<ParameterDefinitionId, ParameterValue>
        | ChangeRobotName of id: RobotId * name: RobotName
        | ChangeTeamId of id: RobotId * teamId: TeamId
        | ChangeEndgameCapabilities of id: RobotId * endgame: EndgameCapable
        | ChangeDrivetrain of id: RobotId * drivetrain: Drivetrain
        | AddPitNote of robotId: RobotId * noteId: NoteId * userId: UserId * text: NoteContent
        | ChangePitNoteText of robotId: RobotId * noteId: NoteId * text: NoteContent
        | RemovePitNote of robotId: RobotId * noteId: NoteId
        // ScoutingResult
        | RecordScoutingResult of id: ScoutingResultId * result: ScoutingResult
        | AddScoutingNote of scoutingResultId: ScoutingResultId * noteId: NoteId * userId: UserId * text: NoteContent
        | ChangeScoutingNoteText of scoutingResultId: ScoutingResultId * noteId: NoteId * text: NoteContent
        | RemoveScoutingNote of scoutingResultId: ScoutingResultId * noteId: NoteId
        // ParameterDefinition
        | DefineParameter of
            id: ParameterDefinitionId *
            name: ParameterDefinitionName *
            spec: ParameterSpec *
            category: ParameterCategory
        | ChangeParameterName of id: ParameterDefinitionId * name: ParameterDefinitionName
        | ChangeParameterSpec of id: ParameterDefinitionId * spec: ParameterSpec
        | ChangeParameterCategory of id: ParameterDefinitionId * category: ParameterCategory
        // Parameter values
        | SetPitParameterValue of robotId: RobotId * definitionId: ParameterDefinitionId * value: ParameterValue
        | UnsetPitParameterValue of robotId: RobotId * definitionId: ParameterDefinitionId
        | SetMatchParameterValue of
            scoutingResultId: ScoutingResultId *
            definitionId: ParameterDefinitionId *
            value: ParameterValue
        | UnsetMatchParameterValue of scoutingResultId: ScoutingResultId * definitionId: ParameterDefinitionId

    [<RequireQualifiedAccess>]
    module EventArgs =
        type Started = { Name: GameName; Year: DateOnly }
        type NameChanged = { Name: GameName }
        type YearChanged = { Year: DateOnly }

        [<RequireQualifiedAccess>]
        module Timeframe =
            type Defined =
                { Period: Period
                  TimeframeId: TimeframeId
                  Name: TimeframeName
                  Duration: TimeframeDuration }

            type Removed =
                { Period: Period
                  TimeframeId: TimeframeId }

        [<RequireQualifiedAccess>]
        module GamePiece =
            type Defined =
                { GamePieceId: GamePieceId
                  GamePieceName: GamePieceName
                  ScoreValues: Map<TimeframeId, ScoreValue> }

            type NameChanged =
                { GamePieceId: GamePieceId
                  GamePieceName: GamePieceName }

            type ScoreValuesChanged =
                { GamePieceId: GamePieceId
                  ScoreValues: Map<TimeframeId, ScoreValue> }

        [<RequireQualifiedAccess>]
        module Infraction =
            type Defined =
                { Id: InfractionId
                  Name: InfractionName
                  Severity: Foul voption
                  Card: Card voption }

            type NameChanged =
                { Id: InfractionId
                  Name: InfractionName }

            type SeverityChanged =
                { Id: InfractionId
                  Severity: Foul voption }

            type CardChanged =
                { Id: InfractionId; Card: Card voption }

        [<RequireQualifiedAccess>]
        module PitResult =
            type Scouted =
                { Id: RobotId
                  TeamId: TeamId
                  Name: RobotName
                  EndgameCapable: EndgameCapable
                  Drivetrain: Drivetrain
                  PitScoutingParameters: Map<ParameterDefinitionId, ParameterValue> }

            type RobotNameChanged = { Id: RobotId; Name: RobotName }
            type TeamChanged = { Id: RobotId; TeamId: TeamId }

            type EndgameCapabilitiesChanged =
                { Id: RobotId
                  EndgameCapable: EndgameCapable }

            type DrivetrainChanged = { Id: RobotId; Drivetrain: Drivetrain }

            type NoteAdded =
                { Id: RobotId
                  NoteId: NoteId
                  UserId: UserId
                  Text: NoteContent }

            type NoteTextChanged =
                { Id: RobotId
                  NoteId: NoteId
                  Text: NoteContent }

            type NoteRemoved = { Id: RobotId; NoteId: NoteId }

        [<RequireQualifiedAccess>]
        module ScoutingResult =
            type Recorded =
                { Id: ScoutingResultId
                  Result: ScoutingResult }

            type NoteAdded =
                { Id: ScoutingResultId
                  NoteId: NoteId
                  UserId: UserId
                  Text: NoteContent }

            type NoteTextChanged =
                { Id: ScoutingResultId
                  NoteId: NoteId
                  Text: NoteContent }

            type NoteRemoved =
                { Id: ScoutingResultId; NoteId: NoteId }

        [<RequireQualifiedAccess>]
        module ParameterDefinition =
            type Defined =
                { Id: ParameterDefinitionId
                  Name: ParameterDefinitionName
                  Spec: ParameterSpec
                  Category: ParameterCategory }

            type NameChanged =
                { Id: ParameterDefinitionId
                  Name: ParameterDefinitionName }

            type SpecChanged =
                { Id: ParameterDefinitionId
                  Spec: ParameterSpec }

            type CategoryChanged =
                { Id: ParameterDefinitionId
                  Category: ParameterCategory }

        [<RequireQualifiedAccess>]
        module Parameter =
            type PitValueSet =
                { RobotId: RobotId
                  DefinitionId: ParameterDefinitionId
                  Value: ParameterValue }

            type PitValueUnset =
                { RobotId: RobotId
                  DefinitionId: ParameterDefinitionId }

            type MatchValueSet =
                { ScoutingResultId: ScoutingResultId
                  DefinitionId: ParameterDefinitionId
                  Value: ParameterValue }

            type MatchValueUnset =
                { ScoutingResultId: ScoutingResultId
                  DefinitionId: ParameterDefinitionId }

    [<RequireQualifiedAccess>]
    module Event =
        [<RequireQualifiedAccess>]
        type Timeframe =
            | Defined of EventArgs.Timeframe.Defined
            | Removed of EventArgs.Timeframe.Removed

        [<RequireQualifiedAccess>]
        type GamePiece =
            | Defined of EventArgs.GamePiece.Defined
            | NameChanged of EventArgs.GamePiece.NameChanged
            | ScoreValuesChanged of EventArgs.GamePiece.ScoreValuesChanged

        [<RequireQualifiedAccess>]
        type Infraction =
            | Defined of EventArgs.Infraction.Defined
            | NameChanged of EventArgs.Infraction.NameChanged
            | SeverityChanged of EventArgs.Infraction.SeverityChanged
            | CardChanged of EventArgs.Infraction.CardChanged

        [<RequireQualifiedAccess>]
        type PitResult =
            | Scouted of EventArgs.PitResult.Scouted
            | RobotNameChanged of EventArgs.PitResult.RobotNameChanged
            | TeamChanged of EventArgs.PitResult.TeamChanged
            | EndgameCapabilitiesChanged of EventArgs.PitResult.EndgameCapabilitiesChanged
            | DrivetrainChanged of EventArgs.PitResult.DrivetrainChanged
            | NoteAdded of EventArgs.PitResult.NoteAdded
            | NoteTextChanged of EventArgs.PitResult.NoteTextChanged
            | NoteRemoved of EventArgs.PitResult.NoteRemoved

        [<RequireQualifiedAccess>]
        type ScoutingResult =
            | Recorded of EventArgs.ScoutingResult.Recorded
            | NoteAdded of EventArgs.ScoutingResult.NoteAdded
            | NoteTextChanged of EventArgs.ScoutingResult.NoteTextChanged
            | NoteRemoved of EventArgs.ScoutingResult.NoteRemoved

        [<RequireQualifiedAccess>]
        type ParameterDefinition =
            | Defined of EventArgs.ParameterDefinition.Defined
            | NameChanged of EventArgs.ParameterDefinition.NameChanged
            | SpecChanged of EventArgs.ParameterDefinition.SpecChanged
            | CategoryChanged of EventArgs.ParameterDefinition.CategoryChanged

        [<RequireQualifiedAccess>]
        type Parameter =
            | PitValueSet of EventArgs.Parameter.PitValueSet
            | PitValueUnset of EventArgs.Parameter.PitValueUnset
            | MatchValueSet of EventArgs.Parameter.MatchValueSet
            | MatchValueUnset of EventArgs.Parameter.MatchValueUnset

        [<RequireQualifiedAccess>]
        type GameEvent =
            | Started of EventArgs.Started
            | NameChanged of EventArgs.NameChanged
            | YearChanged of EventArgs.YearChanged
            | TimeframeEvent of Timeframe
            | GamePieceEvent of GamePiece
            | InfractionEvent of Infraction
            | PitResultEvent of PitResult
            | ScoutingResultEvent of ScoutingResult
            | ParameterDefinitionEvent of ParameterDefinition
            | ParameterEvent of Parameter

        let private initialPeriods =
            { Autonomous = PeriodData Map.empty
              Teleop = PeriodData Map.empty }

        let private setPitParameter
            (robotId: RobotId)
            (definitionId: ParameterDefinitionId)
            (value: ParameterValue)
            game
            =
            match game.ParameterDefinitions.TryFind definitionId, game.PitResults.TryFind robotId with
            | Some definition, Some robot when definition.Category = ParameterCategory.Pit ->
                if ParameterValue.isValid definition.Spec value then
                    match Robot.setPitScoutingParameter definitionId value robot with
                    | Ok updatedRobot ->
                        { game with
                            PitResults = game.PitResults |> Map.add robotId updatedRobot }
                    | Error _ -> game
                else
                    game
            | _ -> game

        let private unsetPitParameter (robotId: RobotId) (definitionId: ParameterDefinitionId) game =
            match game.PitResults.TryFind robotId with
            | Some robot ->
                match Robot.removePitScoutingParameter definitionId robot with
                | Ok updatedRobot ->
                    { game with
                        PitResults = game.PitResults |> Map.add robotId updatedRobot }
                | Error _ -> game
            | Microsoft.FSharp.Core.Option.None -> game

        let private setMatchParameter
            (scoutingResultId: ScoutingResultId)
            (definitionId: ParameterDefinitionId)
            (value: ParameterValue)
            game
            =
            match game.ParameterDefinitions.TryFind definitionId, game.ScoutingResults.TryFind scoutingResultId with
            | Some definition, Some scoutingResult when definition.Category = ParameterCategory.Match ->
                if ParameterValue.isValid definition.Spec value then
                    match ScoutingResult.setScoutingParameterValue definitionId value scoutingResult with
                    | Ok updatedResult ->
                        { game with
                            ScoutingResults = game.ScoutingResults |> Map.add scoutingResultId updatedResult }
                    | Error _ -> game
                else
                    game
            | _ -> game

        let private unsetMatchParameter
            (scoutingResultId: ScoutingResultId)
            (definitionId: ParameterDefinitionId)
            game
            =
            match game.ScoutingResults.TryFind scoutingResultId with
            | Some scoutingResult ->
                match ScoutingResult.unsetScoutingParameterValue definitionId scoutingResult with
                | Ok updatedResult ->
                    { game with
                        ScoutingResults = game.ScoutingResults |> Map.add scoutingResultId updatedResult }
                | Error _ -> game
            | Microsoft.FSharp.Core.Option.None -> game

        let evolve state event =
            match state, event with
            | GameState.GameNotStarted, GameEvent.Started args ->
                { Year = args.Year
                  Name = args.Name
                  Periods = initialPeriods
                  GamePieces = Map.empty
                  Infractions = Map.empty
                  PitResults = Map.empty
                  ScoutingResults = Map.empty
                  ParameterDefinitions = Map.empty
                  Notes = Map.empty }
                |> GameState.GameStarted
            | GameState.GameNotStarted, _ -> state
            | GameState.GameStarted game, GameEvent.Started _ -> GameState.GameStarted game
            | GameState.GameStarted game, GameEvent.NameChanged args ->
                { game with Name = args.Name } |> GameState.GameStarted
            | GameState.GameStarted game, GameEvent.YearChanged args ->
                { game with Year = args.Year } |> GameState.GameStarted
            | GameState.GameStarted game, GameEvent.TimeframeEvent timeframeEvent ->
                let updatedGame =
                    match timeframeEvent with
                    | Timeframe.Defined args ->
                        match
                            Timeframe.addOrUpdateTimeframe
                                args.Period
                                args.TimeframeId
                                args.Name
                                args.Duration
                                game.Periods
                        with
                        | Ok periods -> { game with Periods = periods }
                        | Error _ -> game
                    | Timeframe.Removed args ->
                        match Timeframe.removeTimeframe args.Period args.TimeframeId game.Periods with
                        | Ok periods -> { game with Periods = periods }
                        | Error _ -> game

                updatedGame |> GameState.GameStarted
            | GameState.GameStarted game, GameEvent.GamePieceEvent gamePieceEvent ->
                let updatedGame =
                    match gamePieceEvent with
                    | GamePiece.Defined args ->
                        match GamePiece.create args.GamePieceName args.ScoreValues with
                        | Ok gamePiece ->
                            { game with
                                GamePieces = game.GamePieces |> Map.add args.GamePieceId gamePiece }
                        | Error _ -> game
                    | GamePiece.NameChanged args ->
                        { game with
                            GamePieces =
                                game.GamePieces
                                |> Map.change
                                    args.GamePieceId
                                    (Option.map (fun piece ->
                                        match GamePiece.withName args.GamePieceName piece with
                                        | Ok updatedPiece -> updatedPiece
                                        | Error _ -> piece)) }
                    | GamePiece.ScoreValuesChanged args ->
                        { game with
                            GamePieces =
                                game.GamePieces
                                |> Map.change args.GamePieceId (Option.map (GamePiece.changeValue args.ScoreValues)) }

                updatedGame |> GameState.GameStarted
            | GameState.GameStarted game, GameEvent.InfractionEvent infractionEvent ->
                let updatedGame =
                    match infractionEvent with
                    | Infraction.Defined args ->
                        match Infraction.create args.Card args.Severity args.Name with
                        | Ok infraction ->
                            { game with
                                Infractions = game.Infractions |> Map.add args.Id infraction }
                        | Error _ -> game
                    | Infraction.NameChanged args ->
                        { game with
                            Infractions =
                                game.Infractions
                                |> Map.change
                                    args.Id
                                    (Option.map (fun infraction ->
                                        match Infraction.changeName infraction args.Name with
                                        | Ok updatedInfraction -> updatedInfraction
                                        | Error _ -> infraction)) }
                    | Infraction.SeverityChanged args ->
                        { game with
                            Infractions =
                                game.Infractions
                                |> Map.change
                                    args.Id
                                    (Option.map (fun infraction -> Infraction.changeSeverity infraction args.Severity)) }
                    | Infraction.CardChanged args ->
                        { game with
                            Infractions =
                                game.Infractions
                                |> Map.change
                                    args.Id
                                    (Option.map (fun infraction -> Infraction.changeCard infraction args.Card)) }

                updatedGame |> GameState.GameStarted
            | GameState.GameStarted game, GameEvent.PitResultEvent pitResultEvent ->
                let updatedGame =
                    match pitResultEvent with
                    | PitResult.Scouted args ->
                        match
                            Robot.createWithParameters
                                args.Name
                                args.TeamId
                                args.EndgameCapable
                                args.PitScoutingParameters
                                args.Drivetrain
                        with
                        | Ok robot ->
                            { game with
                                PitResults = game.PitResults |> Map.add args.Id robot }
                        | Error _ -> game
                    | PitResult.RobotNameChanged args ->
                        { game with
                            PitResults =
                                game.PitResults
                                |> Map.change args.Id (Option.map (fun robot -> { robot with Name = args.Name })) }
                    | PitResult.TeamChanged args ->
                        { game with
                            PitResults =
                                game.PitResults
                                |> Map.change args.Id (Option.map (fun robot -> { robot with Team = args.TeamId })) }
                    | PitResult.EndgameCapabilitiesChanged args ->
                        { game with
                            PitResults =
                                game.PitResults
                                |> Map.change
                                    args.Id
                                    (Option.map (fun robot -> Robot.withEndgameCapabilities args.EndgameCapable robot)) }
                    | PitResult.DrivetrainChanged args ->
                        { game with
                            PitResults =
                                game.PitResults
                                |> Map.change
                                    args.Id
                                    (Option.map (fun robot -> Robot.withDrivetrain args.Drivetrain robot)) }
                    | PitResult.NoteAdded args ->
                        match game.PitResults.TryFind args.Id, Note.create args.UserId args.Text with
                        | Some robot, Ok note ->
                            match Robot.addNote args.NoteId robot with
                            | Ok updatedRobot ->
                                { game with
                                    Notes = game.Notes |> Map.add args.NoteId note
                                    PitResults = game.PitResults |> Map.add args.Id updatedRobot }
                            | Error _ -> game
                        | _ -> game
                    | PitResult.NoteTextChanged args ->
                        { game with
                            Notes =
                                game.Notes
                                |> Map.change
                                    args.NoteId
                                    (Option.map (fun note ->
                                        match Note.edit args.Text note with
                                        | Ok updatedNote -> updatedNote
                                        | Error _ -> note)) }
                    | PitResult.NoteRemoved args ->
                        match game.PitResults.TryFind args.Id with
                        | Some robot ->
                            match Robot.removeNote args.NoteId robot with
                            | Ok updatedRobot ->
                                { game with
                                    Notes = game.Notes |> Map.remove args.NoteId
                                    PitResults = game.PitResults |> Map.add args.Id updatedRobot }
                            | Error _ -> game
                        | Microsoft.FSharp.Core.Option.None -> game

                updatedGame |> GameState.GameStarted
            | GameState.GameStarted game, GameEvent.ScoutingResultEvent scoutingResultEvent ->
                let updatedGame =
                    match scoutingResultEvent with
                    | ScoutingResult.Recorded args ->
                        { game with
                            ScoutingResults = game.ScoutingResults |> Map.add args.Id args.Result }
                    | ScoutingResult.NoteAdded args ->
                        match game.ScoutingResults.TryFind args.Id, Note.create args.UserId args.Text with
                        | Some scoutingResult, Ok note ->
                            match ScoutingResult.addOrReplaceNote args.NoteId scoutingResult with
                            | Ok updatedScoutingResult ->
                                { game with
                                    Notes = game.Notes |> Map.add args.NoteId note
                                    ScoutingResults = game.ScoutingResults |> Map.add args.Id updatedScoutingResult }
                            | Error _ -> game
                        | _ -> game
                    | ScoutingResult.NoteTextChanged args ->
                        { game with
                            Notes =
                                game.Notes
                                |> Map.change
                                    args.NoteId
                                    (Option.map (fun note ->
                                        match Note.edit args.Text note with
                                        | Ok updatedNote -> updatedNote
                                        | Error _ -> note)) }
                    | ScoutingResult.NoteRemoved args ->
                        match game.ScoutingResults.TryFind args.Id with
                        | Some scoutingResult ->
                            match ScoutingResult.removeNote args.NoteId scoutingResult with
                            | Ok updatedScoutingResult ->
                                { game with
                                    Notes = game.Notes |> Map.remove args.NoteId
                                    ScoutingResults = game.ScoutingResults |> Map.add args.Id updatedScoutingResult }
                            | Error _ -> game
                        | Microsoft.FSharp.Core.Option.None -> game

                updatedGame |> GameState.GameStarted
            | GameState.GameStarted game, GameEvent.ParameterDefinitionEvent parameterDefinitionEvent ->
                let updatedGame =
                    match parameterDefinitionEvent with
                    | ParameterDefinition.Defined args ->
                        { game with
                            ParameterDefinitions =
                                game.ParameterDefinitions
                                |> Map.add args.Id (ParameterDefinition.create args.Name args.Spec args.Category) }
                    | ParameterDefinition.NameChanged args ->
                        { game with
                            ParameterDefinitions =
                                game.ParameterDefinitions
                                |> Map.change args.Id (Option.map (ParameterDefinition.withName args.Name)) }
                    | ParameterDefinition.SpecChanged args ->
                        { game with
                            ParameterDefinitions =
                                game.ParameterDefinitions
                                |> Map.change args.Id (Option.map (ParameterDefinition.withSpec args.Spec)) }
                    | ParameterDefinition.CategoryChanged args ->
                        { game with
                            ParameterDefinitions =
                                game.ParameterDefinitions
                                |> Map.change args.Id (Option.map (ParameterDefinition.withCategory args.Category)) }

                updatedGame |> GameState.GameStarted
            | GameState.GameStarted game, GameEvent.ParameterEvent parameterEvent ->
                let updatedGame =
                    match parameterEvent with
                    | Parameter.PitValueSet args -> setPitParameter args.RobotId args.DefinitionId args.Value game
                    | Parameter.PitValueUnset args -> unsetPitParameter args.RobotId args.DefinitionId game
                    | Parameter.MatchValueSet args ->
                        setMatchParameter args.ScoutingResultId args.DefinitionId args.Value game
                    | Parameter.MatchValueUnset args -> unsetMatchParameter args.ScoutingResultId args.DefinitionId game

                updatedGame |> GameState.GameStarted

    [<RequireQualifiedAccess>]
    module private OnlyIf =
        let gameStarted state =
            match state with
            | GameState.GameStarted game -> game |> Validation.ok
            | GameState.GameNotStarted -> GameError.GameNotStarted |> Validation.error

        let gameNotStarted state =
            match state with
            | GameState.GameNotStarted -> state |> Validation.ok
            | GameState.GameStarted _ -> GameError.GameAlreadyStarted |> Validation.error

        let timeframeExists (game: Game) (period: Period) (id: TimeframeId) =
            let periodData = Timeframe.getPeriodData period game.Periods
            let (PeriodData timeframeMap) = periodData

            match timeframeMap.ContainsKey id with
            | true -> id |> Validation.ok
            | false -> GameError.TimeframeNotFound id |> Validation.error

        let gamePieceExists (game: Game) (id: GamePieceId) =
            match game.GamePieces.TryFind id with
            | Some gp -> gp |> Validation.ok
            | None -> GameError.GamePieceNotFound id |> Validation.error

        let infractionExists (game: Game) (id: InfractionId) =
            match game.Infractions.TryFind id with
            | Some infraction -> infraction |> Validation.ok
            | None -> GameError.InfractionNotFound id |> Validation.error

        let robotExists (game: Game) (id: RobotId) =
            match game.PitResults.TryFind id with
            | Some robot -> robot |> Validation.ok
            | None -> GameError.RobotNotFound id |> Validation.error

        let scoutingResultExists (game: Game) (id: ScoutingResultId) =
            match game.ScoutingResults.TryFind id with
            | Some result -> result |> Validation.ok
            | None -> GameError.ScoutingResultNotFound id |> Validation.error

        let parameterDefinitionExists (game: Game) (id: ParameterDefinitionId) =
            match game.ParameterDefinitions.TryFind id with
            | Some definition -> definition |> Validation.ok
            | None -> GameError.ParameterDefinitionNotFound id |> Validation.error

        let noteExists (game: Game) (id: NoteId) =
            match game.Notes.TryFind id with
            | Some note -> note |> Validation.ok
            | None -> GameError.NoteNotFound id |> Validation.error

        let mapRobotError result =
            result |> Result.mapError (List.map GameError.RobotError)

        let mapScoutingResultError result =
            result |> Result.mapError (List.map GameError.ScoutingResultError)

        let mapNoteError result =
            result |> Result.mapError (List.map GameError.NoteError)

        let mapTimeframeError result =
            result |> Result.mapError (List.map GameError.TimeframeError)

        let mapGamePieceError result =
            result |> Result.mapError (List.map GameError.GamePieceError)

        let mapInfractionError result =
            result |> Result.mapError (List.map GameError.InfractionError)

    [<RequireQualifiedAccess>]
    module Command =
        let decide command state =
            match command with
            | Command.Start(name, year) ->
                validation {
                    let! _ = OnlyIf.gameNotStarted state
                    return [ Event.GameEvent.Started { Name = name; Year = year } ]
                }
            | Command.ChangeName name ->
                validation {
                    let! _ = OnlyIf.gameStarted state
                    return [ Event.GameEvent.NameChanged { Name = name } ]
                }
            | Command.ChangeYear year ->
                validation {
                    let! _ = OnlyIf.gameStarted state
                    return [ Event.GameEvent.YearChanged { Year = year } ]
                }
            | Command.DefineTimeframe(period, id, name, duration) ->
                validation {
                    let! game = OnlyIf.gameStarted state

                    let! _ =
                        Timeframe.addOrUpdateTimeframe period id name duration game.Periods
                        |> OnlyIf.mapTimeframeError

                    return
                        [ Event.GameEvent.TimeframeEvent(
                              Event.Timeframe.Defined
                                  { Period = period
                                    TimeframeId = id
                                    Name = name
                                    Duration = duration }
                          ) ]
                }
            | Command.RemoveTimeframe(period, id) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = Timeframe.removeTimeframe period id game.Periods |> OnlyIf.mapTimeframeError

                    return
                        [ Event.GameEvent.TimeframeEvent(Event.Timeframe.Removed { Period = period; TimeframeId = id }) ]
                }
            | Command.DefineGamePiece(id, name, values) ->
                validation {
                    let! _ = OnlyIf.gameStarted state
                    and! _ = GamePiece.create name values |> OnlyIf.mapGamePieceError

                    return
                        [ Event.GameEvent.GamePieceEvent(
                              Event.GamePiece.Defined
                                  { GamePieceId = id
                                    GamePieceName = name
                                    ScoreValues = values }
                          ) ]
                }
            | Command.ChangeGamePieceName(id, name) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! piece = OnlyIf.gamePieceExists game id
                    let! _ = GamePiece.withName name piece |> OnlyIf.mapGamePieceError

                    return
                        [ Event.GameEvent.GamePieceEvent(
                              Event.GamePiece.NameChanged
                                  { GamePieceId = id
                                    GamePieceName = name }
                          ) ]
                }
            | Command.ChangeGamePieceScoreValues(id, values) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.gamePieceExists game id

                    return
                        [ Event.GameEvent.GamePieceEvent(
                              Event.GamePiece.ScoreValuesChanged
                                  { GamePieceId = id
                                    ScoreValues = values }
                          ) ]
                }
            | Command.DefineInfraction(id, name, severity, card) ->
                validation {
                    let! _ = OnlyIf.gameStarted state
                    and! _ = Infraction.create card severity name |> OnlyIf.mapInfractionError

                    return
                        [ Event.GameEvent.InfractionEvent(
                              Event.Infraction.Defined
                                  { Id = id
                                    Name = name
                                    Severity = severity
                                    Card = card }
                          ) ]
                }
            | Command.ChangeInfractionName(id, name) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! infraction = OnlyIf.infractionExists game id
                    let! _ = Infraction.changeName infraction name |> OnlyIf.mapInfractionError
                    return [ Event.GameEvent.InfractionEvent(Event.Infraction.NameChanged { Id = id; Name = name }) ]
                }
            | Command.ChangeInfractionSeverity(id, severity) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.infractionExists game id

                    return
                        [ Event.GameEvent.InfractionEvent(
                              Event.Infraction.SeverityChanged { Id = id; Severity = severity }
                          ) ]
                }
            | Command.ChangeInfractionCard(id, card) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.infractionExists game id
                    return [ Event.GameEvent.InfractionEvent(Event.Infraction.CardChanged { Id = id; Card = card }) ]
                }
            | Command.ScoutPit(id, teamId, name, endgame, drivetrain, parameters) ->
                validation {
                    let! game = OnlyIf.gameStarted state

                    let! _ =
                        Robot.createWithParameters name teamId endgame parameters drivetrain
                        |> OnlyIf.mapRobotError

                    and! _ = GameValidation.parametersMatch game.ParameterDefinitions ParameterCategory.Pit parameters

                    return
                        [ Event.GameEvent.PitResultEvent(
                              Event.PitResult.Scouted
                                  { Id = id
                                    TeamId = teamId
                                    Name = name
                                    EndgameCapable = endgame
                                    Drivetrain = drivetrain
                                    PitScoutingParameters = parameters }
                          ) ]
                }
            | Command.ChangeRobotName(id, name) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.robotExists game id
                    return [ Event.GameEvent.PitResultEvent(Event.PitResult.RobotNameChanged { Id = id; Name = name }) ]
                }
            | Command.ChangeTeamId(id, teamId) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.robotExists game id
                    return [ Event.GameEvent.PitResultEvent(Event.PitResult.TeamChanged { Id = id; TeamId = teamId }) ]
                }
            | Command.ChangeEndgameCapabilities(id, endgame) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.robotExists game id

                    return
                        [ Event.GameEvent.PitResultEvent(
                              Event.PitResult.EndgameCapabilitiesChanged { Id = id; EndgameCapable = endgame }
                          ) ]
                }
            | Command.ChangeDrivetrain(id, drivetrain) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.robotExists game id

                    return
                        [ Event.GameEvent.PitResultEvent(
                              Event.PitResult.DrivetrainChanged { Id = id; Drivetrain = drivetrain }
                          ) ]
                }
            | Command.AddPitNote(robotId, noteId, userId, text) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! robot = OnlyIf.robotExists game robotId
                    and! _ = Note.create userId text |> OnlyIf.mapNoteError
                    let! _ = Robot.addNote noteId robot |> OnlyIf.mapRobotError

                    return
                        [ Event.GameEvent.PitResultEvent(
                              Event.PitResult.NoteAdded
                                  { Id = robotId
                                    NoteId = noteId
                                    UserId = userId
                                    Text = text }
                          ) ]
                }
            | Command.ChangePitNoteText(robotId, noteId, text) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.robotExists game robotId
                    and! note = OnlyIf.noteExists game noteId
                    let! _ = Note.edit text note |> OnlyIf.mapNoteError

                    return
                        [ Event.GameEvent.PitResultEvent(
                              Event.PitResult.NoteTextChanged
                                  { Id = robotId
                                    NoteId = noteId
                                    Text = text }
                          ) ]
                }
            | Command.RemovePitNote(robotId, noteId) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! robot = OnlyIf.robotExists game robotId
                    let! _ = Robot.removeNote noteId robot |> OnlyIf.mapRobotError

                    return
                        [ Event.GameEvent.PitResultEvent(Event.PitResult.NoteRemoved { Id = robotId; NoteId = noteId }) ]
                }
            | Command.RecordScoutingResult(id, result) ->
                validation {
                    let! _ = OnlyIf.gameStarted state

                    return
                        [ Event.GameEvent.ScoutingResultEvent(
                              Event.ScoutingResult.Recorded { Id = id; Result = result }
                          ) ]
                }
            | Command.AddScoutingNote(scoutingResultId, noteId, userId, text) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! result = OnlyIf.scoutingResultExists game scoutingResultId
                    and! _ = Note.create userId text |> OnlyIf.mapNoteError
                    let! _ = ScoutingResult.addOrReplaceNote noteId result |> OnlyIf.mapScoutingResultError

                    return
                        [ Event.GameEvent.ScoutingResultEvent(
                              Event.ScoutingResult.NoteAdded
                                  { Id = scoutingResultId
                                    NoteId = noteId
                                    UserId = userId
                                    Text = text }
                          ) ]
                }
            | Command.ChangeScoutingNoteText(scoutingResultId, noteId, text) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.scoutingResultExists game scoutingResultId
                    and! note = OnlyIf.noteExists game noteId
                    let! _ = Note.edit text note |> OnlyIf.mapNoteError

                    return
                        [ Event.GameEvent.ScoutingResultEvent(
                              Event.ScoutingResult.NoteTextChanged
                                  { Id = scoutingResultId
                                    NoteId = noteId
                                    Text = text }
                          ) ]
                }
            | Command.RemoveScoutingNote(scoutingResultId, noteId) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! result = OnlyIf.scoutingResultExists game scoutingResultId
                    let! _ = ScoutingResult.removeNote noteId result |> OnlyIf.mapScoutingResultError

                    return
                        [ Event.GameEvent.ScoutingResultEvent(
                              Event.ScoutingResult.NoteRemoved
                                  { Id = scoutingResultId
                                    NoteId = noteId }
                          ) ]
                }
            | Command.DefineParameter(id, name, spec, category) ->
                validation {
                    let! _ = OnlyIf.gameStarted state

                    return
                        [ Event.GameEvent.ParameterDefinitionEvent(
                              Event.ParameterDefinition.Defined
                                  { Id = id
                                    Name = name
                                    Spec = spec
                                    Category = category }
                          ) ]
                }
            | Command.ChangeParameterName(id, name) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.parameterDefinitionExists game id

                    return
                        [ Event.GameEvent.ParameterDefinitionEvent(
                              Event.ParameterDefinition.NameChanged { Id = id; Name = name }
                          ) ]
                }
            | Command.ChangeParameterSpec(id, spec) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.parameterDefinitionExists game id

                    return
                        [ Event.GameEvent.ParameterDefinitionEvent(
                              Event.ParameterDefinition.SpecChanged { Id = id; Spec = spec }
                          ) ]
                }
            | Command.ChangeParameterCategory(id, category) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! _ = OnlyIf.parameterDefinitionExists game id

                    return
                        [ Event.GameEvent.ParameterDefinitionEvent(
                              Event.ParameterDefinition.CategoryChanged { Id = id; Category = category }
                          ) ]
                }
            | Command.SetPitParameterValue(robotId, definitionId, value) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! robot = OnlyIf.robotExists game robotId
                    and! definition = OnlyIf.parameterDefinitionExists game definitionId
                    and! _ = GameValidation.parameterValueMatches game.ParameterDefinitions definitionId value

                    let! _ =
                        if definition.Category <> ParameterCategory.Pit then
                            GameError.NotAPitParameter definitionId |> Validation.error
                        else
                            Validation.ok ()

                    let! _ = Robot.setPitScoutingParameter definitionId value robot |> OnlyIf.mapRobotError

                    return
                        [ Event.GameEvent.ParameterEvent(
                              Event.Parameter.PitValueSet
                                  { RobotId = robotId
                                    DefinitionId = definitionId
                                    Value = value }
                          ) ]
                }
            | Command.UnsetPitParameterValue(robotId, definitionId) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! robot = OnlyIf.robotExists game robotId
                    let! _ = Robot.removePitScoutingParameter definitionId robot |> OnlyIf.mapRobotError

                    return
                        [ Event.GameEvent.ParameterEvent(
                              Event.Parameter.PitValueUnset
                                  { RobotId = robotId
                                    DefinitionId = definitionId }
                          ) ]
                }
            | Command.SetMatchParameterValue(scoutingResultId, definitionId, value) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! result = OnlyIf.scoutingResultExists game scoutingResultId
                    and! definition = OnlyIf.parameterDefinitionExists game definitionId
                    and! _ = GameValidation.parameterValueMatches game.ParameterDefinitions definitionId value

                    let! _ =
                        if definition.Category <> ParameterCategory.Match then
                            GameError.NotAMatchParameter definitionId |> Validation.error
                        else
                            Validation.ok ()

                    let! _ =
                        ScoutingResult.setScoutingParameterValue definitionId value result
                        |> OnlyIf.mapScoutingResultError

                    return
                        [ Event.GameEvent.ParameterEvent(
                              Event.Parameter.MatchValueSet
                                  { ScoutingResultId = scoutingResultId
                                    DefinitionId = definitionId
                                    Value = value }
                          ) ]
                }
            | Command.UnsetMatchParameterValue(scoutingResultId, definitionId) ->
                validation {
                    let! game = OnlyIf.gameStarted state
                    let! result = OnlyIf.scoutingResultExists game scoutingResultId

                    let! _ =
                        ScoutingResult.unsetScoutingParameterValue definitionId result
                        |> OnlyIf.mapScoutingResultError

                    return
                        [ Event.GameEvent.ParameterEvent(
                              Event.Parameter.MatchValueUnset
                                  { ScoutingResultId = scoutingResultId
                                    DefinitionId = definitionId }
                          ) ]
                }

    let state =
        Aggregate.create GameState.GameNotStarted Event.evolve Command.decide

    let start name year : Event.GameEvent =
        Event.GameEvent.Started { Name = name; Year = year }
