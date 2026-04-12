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
          FrcDistricts: Map<FrcDistrictCode, FrcDistrict>
          Periods: Periods
          GamePieces: Map<GamePieceId, GamePiece>
          Infractions: Map<InfractionId, Infraction>
          PitResults: Map<RobotId, Robot>
          ParameterDefinitions: Map<ParameterDefinitionId, ParameterDefinition>
          Notes: Map<NoteId, Note> }

[<RequireQualifiedAccess>]
type private GameState =
    | GameNotStarted
    | GameStarted of Game

[<RequireQualifiedAccess>]
module Game =
    [<RequireQualifiedAccess>]
    type GameError =
        | ParameterDoesNotExist of ParameterDefinitionId
        | NotAPitParameter of ParameterDefinitionId
        | NotAMatchParameter of ParameterDefinitionId
        | InvalidParameterValue of ParameterDefinitionId * ParameterValue
        | MissingParameterDefinitions of ParameterDefinitionId list
        | ExtraParameterDefinitions of ParameterDefinitionId list
        | RobotError of Robot.Error
        | ScoutingResultError of ScoutingResult.Error

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
    module EventArgs =
        type Started = { Name: GameName; Year: DateOnly }
        type NameChanged = { Name: GameName }
        type YearChanged = { Year: DateOnly }

        [<RequireQualifiedAccess>]
        module District =
            type Registered =
                { DistrictCode: FrcDistrictCode
                  DistrictName: FrcDistrictName }

            type NameChanged =
                { DistrictCode: FrcDistrictCode
                  DistrictName: FrcDistrictName }

            [<RequireQualifiedAccess>]
            module FrcEvent =
                type Registered =
                    { DistrictCode: FrcDistrictCode
                      EventId: FrcEventId
                      EventName: FrcEventName }

                type NameChanged =
                    { DistrictCode: FrcDistrictCode
                      EventId: FrcEventId
                      EventName: FrcEventName }

                [<RequireQualifiedAccess>]
                module Match =
                    type Registered =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchId: MatchId
                          TournamentLevel: TournamentLevel
                          MatchNumber: MatchNumber }

                    type Scouted =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchId: MatchId
                          Team: AllianceTeam
                          Result: ScoutingData }

                    type NoteAdded =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchId: MatchId
                          Team: AllianceTeam
                          NoteId: NoteId
                          UserId: UserId
                          Text: NoteContent }

                    type NoteTextChanged =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchId: MatchId
                          Team: AllianceTeam
                          NoteId: NoteId
                          Text: NoteContent }

                    type NoteRemoved =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchId: MatchId
                          Team: AllianceTeam
                          NoteId: NoteId }

                    type Concluded =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchId: MatchId
                          Winner: Alliance }

        [<RequireQualifiedAccess>]
        module Timeframe =
            type Defined =
                { Period: Period
                  SubPhaseName: TimeframeName
                  SubPhaseDescription: TimeframeDescription
                  ParentPhase: PeriodData }

            type NameChanged =
                { SubPhaseId: SubPhaseId
                  SubPhaseName: TimeframeName }

            type DescriptionChanged =
                { SubPhaseId: SubPhaseId
                  SubPhaseDescription: TimeframeDescription }

            type Removed = { SubPhaseId: SubPhaseId }

        [<RequireQualifiedAccess>]
        module GamePiece =
            type Defined =
                { GamePieceId: GamePieceId
                  GamePieceName: GamePieceName
                  RankingPointGrants: RankingPointGrant list
                  SubPhaseScoreValues: SubPhaseMap<ScoreValue> }

            type NameChanged =
                { GamePieceId: GamePieceId
                  GamePieceName: GamePieceName }

            type RankingPointGrantsAdded =
                { GamePieceId: GamePieceId
                  RankingPointGrants: RankingPointGrant list }

            type RankingPointGrantsRemoved =
                { GamePieceId: GamePieceId
                  RankingPointGrants: RankingPointGrant list }

            type ScoreAddedForSubPhase =
                { GamePieceId: GamePieceId
                  SubPhaseId: SubPhaseId
                  Score: ScoreValue }

            type ScoreRemovedForSubPhase =
                { GamePieceId: GamePieceId
                  SubPhaseId: SubPhaseId
                  Score: ScoreValue }

            type ScoreChangedForSubPhase =
                { GamePieceId: GamePieceId
                  SubPhaseId: SubPhaseId
                  OldScore: ScoreValue }

        [<RequireQualifiedAccess>]
        module Infraction =
            type Defined =
                { Id: InfractionId
                  Name: InfractionName
                  Severity: Foul option
                  Card: Card option }

            type NameChanged =
                { Id: InfractionId
                  Name: InfractionName }

            type SeverityChanged =
                { Id: InfractionId
                  Severity: Foul option }

            type CardChanged = { Id: InfractionId; Card: Card option }

        [<RequireQualifiedAccess>]
        module PitResult =
            type Scouted =
                { Id: RobotId
                  TeamId: TeamId
                  Name: RobotName
                  EndgameCapable: EndgameCapable
                  Drivetrain: Drivetrain
                  PitScoutingParameters: Map<ParameterDefinitionId, ParameterValue>
                  Notes: NoteId list }

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

        [<RequireQualifiedAccess>]
        module Parameter =
            type ValueSet =
                { RobotId: RobotId
                  DefinitionId: ParameterDefinitionId
                  Value: ParameterValue }

    [<RequireQualifiedAccess>]
    module Event =
        [<RequireQualifiedAccess>]
        type Match =
            | Registered of EventArgs.District.FrcEvent.Match.Registered
            | Scouted of EventArgs.District.FrcEvent.Match.Scouted
            | NoteAdded of EventArgs.District.FrcEvent.Match.NoteAdded
            | NoteTextChanged of EventArgs.District.FrcEvent.Match.NoteTextChanged
            | NoteRemoved of EventArgs.District.FrcEvent.Match.NoteRemoved
            | Concluded of EventArgs.District.FrcEvent.Match.Concluded

        [<RequireQualifiedAccess>]
        type FrcEvent =
            | Registered of EventArgs.District.FrcEvent.Registered
            | NameChanged of EventArgs.District.FrcEvent.NameChanged
            | MatchEvent of Match

        [<RequireQualifiedAccess>]
        type District =
            | Registered of EventArgs.District.Registered
            | NameChanged of EventArgs.District.NameChanged
            | FrcEventEvent of FrcEvent

        [<RequireQualifiedAccess>]
        type SubPhase =
            | Defined of EventArgs.Timeframe.Defined
            | NameChanged of EventArgs.Timeframe.NameChanged
            | DescriptionChanged of EventArgs.Timeframe.DescriptionChanged
            | Removed of EventArgs.Timeframe.Removed

        [<RequireQualifiedAccess>]
        type GamePiece =
            | Defined of EventArgs.GamePiece.Defined
            | NameChanged of EventArgs.GamePiece.NameChanged
            | RankingPointGrantsAdded of EventArgs.GamePiece.RankingPointGrantsAdded
            | RankingPointGrantsRemoved of EventArgs.GamePiece.RankingPointGrantsRemoved
            | ScoreAddedForSubPhase of EventArgs.GamePiece.ScoreAddedForSubPhase
            | ScoreRemovedForSubPhase of EventArgs.GamePiece.ScoreRemovedForSubPhase
            | ScoreChangedForSubPhase of EventArgs.GamePiece.ScoreChangedForSubPhase

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
        type ParameterDefinition =
            | Defined of EventArgs.ParameterDefinition.Defined
            | NameChanged of EventArgs.ParameterDefinition.NameChanged
            | SpecChanged of EventArgs.ParameterDefinition.SpecChanged

        [<RequireQualifiedAccess>]
        type Parameter = ValueSet of EventArgs.Parameter.ValueSet

        [<RequireQualifiedAccess>]
        type Game =
            | Started of EventArgs.Started
            | NameChanged of EventArgs.NameChanged
            | YearChanged of EventArgs.YearChanged
            | DistrictEvent of District
            | SubPhaseEvent of SubPhase
            | GamePieceEvent of GamePiece
            | InfractionEvent of Infraction
            | PitResultEvent of PitResult
            | ParameterDefinitionEvent of ParameterDefinition
            | ParameterEvent of Parameter

        let private evolve state event =
            match (state, event) with
            | GameState.GameNotStarted, Game.Started args ->
                { Year = args.Year
                  Name = args.Name
                  FrcDistricts = Map.empty
                  SubPhases = Map.empty
                  GamePieces = Map.empty
                  Infractions = Map.empty
                  PitResults = Map.empty
                  ParameterDefinitions = Map.empty
                  Notes = Map.empty }
                |> GameState.GameStarted
            | GameState.GameNotStarted, _ -> state
            | GameState.GameStarted game, Game.NameChanged args ->
                { game with Name = args.Name } |> GameState.GameStarted
            | GameState.GameStarted game, Game.YearChanged args ->
                { game with Year = args.Year } |> GameState.GameStarted
            | GameState.GameStarted game, Game.DistrictEvent districtEvent ->
                match districtEvent with
                | District.Registered args ->
                    let newDistrict =
                        { Code = args.DistrictCode
                          Name = args.DistrictName
                          Events = Map.empty }

                    { game with
                        FrcDistricts = Map.add args.DistrictCode newDistrict game.FrcDistricts }
                    |> GameState.GameStarted
                | District.NameChanged args ->
                    { game with
                        FrcDistricts =
                            game.FrcDistricts
                            |> Map.change args.DistrictCode (Option.map (fun d -> { d with Name = args.DistrictName })) }
                    |> GameState.GameStarted
                | District.FrcEventEvent frcEventEvent ->
                    let changeFrcEvent eventId transform maybeDistrict =
                        maybeDistrict
                        |> Option.map (fun district ->
                            { district with
                                FrcDistrict.Events = district.Events |> Map.change eventId (Option.map transform) })

                    let addFrcEvent eventId event maybeDistrict =
                        maybeDistrict
                        |> Option.map (fun district ->
                            { district with
                                FrcDistrict.Events = district.Events |> Map.add eventId event })

                    match frcEventEvent with
                    | FrcEvent.Registered args ->
                        { game with
                            FrcDistricts =
                                game.FrcDistricts
                                |> Map.change
                                    args.DistrictCode
                                    (addFrcEvent args.EventId (FrcEvent.create args.EventName)) }
                    | FrcEvent.NameChanged args ->
                        { game with
                            FrcDistricts =
                                game.FrcDistricts
                                |> Map.change
                                    args.DistrictCode
                                    (changeFrcEvent args.EventId (FrcEvent.changeName args.EventName)) }
                    | FrcEvent.MatchEvent matchEvent ->
                        match matchEvent with
                        | Match.Registered args ->
                            let _addMatch event =
                                Match.createMatch
                                    TournamentLevel.Qualification
                                    args.MatchNumber
                                    MatchScoutingResults.NotStarted
                                |> fun m -> FrcEvent.addOrReplaceMatch args.MatchId m event

                            { game with
                                FrcDistricts =
                                    game.FrcDistricts
                                    |> Map.change args.DistrictCode (changeFrcEvent args.EventId _addMatch) }
                        | Match.NoteAdded args ->
                            let (AllianceTeam (alliance, teamNumber)) = args.Team

                            let updateMatch m =
                                let result = Match.getScoutingResult alliance teamNumber m
                                let newResult = ScoutingResult.addOrReplaceNote args.NoteId result
                                match newResult with
                                | Ok nr -> Match.withScoutingResult alliance teamNumber m nr
                                | Error _ -> m

                            { game with
                                Notes =
                                    game.Notes
                                    |> Map.add args.NoteId { UserId = args.UserId; Text = args.Text }
                                FrcDistricts =
                                    game.FrcDistricts
                                    |> Map.change
                                        args.DistrictCode
                                        (changeFrcEvent args.EventId (FrcEvent.changeMatch args.MatchId updateMatch)) }

                        | Match.NoteTextChanged args ->
                            { game with
                                Notes =
                                    game.Notes
                                    |> Map.change args.NoteId (Option.map (fun n -> { n with Text = args.Text })) }

                        | Match.NoteRemoved args ->
                            let (AllianceTeam (alliance, teamNumber)) = args.Team

                            let updateMatch m =
                                let result = Match.getScoutingResult alliance teamNumber m
                                let newResult = ScoutingResult.removeNote args.NoteId result
                                match newResult with
                                | Ok nr -> Match.withScoutingResult alliance teamNumber m nr
                                | Error _ -> m

                            { game with
                                Notes = game.Notes |> Map.remove args.NoteId
                                FrcDistricts =
                                    game.FrcDistricts
                                    |> Map.change
                                        args.DistrictCode
                                        (changeFrcEvent args.EventId (FrcEvent.changeMatch args.MatchId updateMatch)) }
                        | _ -> { game with Name = game.Name } // TODO: handle other match events

                    |> GameState.GameStarted

            | GameState.GameStarted game, Game.PitResultEvent pitResultEvent ->
                match pitResultEvent with
                | PitResult.Scouted args ->
                    let robot =
                        { Name = args.Name
                          Team = args.TeamId
                          EndgameCapable = args.EndgameCapable
                          Drivetrain = args.Drivetrain
                          PitScoutingParameters = args.PitScoutingParameters
                          Notes = args.Notes }

                    { game with
                        PitResults = game.PitResults |> Map.add args.Id robot }
                | PitResult.NoteAdded args ->
                    let note = { UserId = args.UserId; Text = args.Text }
                    let robot = game.PitResults |> Map.tryFind args.Id
                    let updatedRobot =
                        robot |> Option.map (fun r ->
                            match Robot.addNote args.NoteId r with
                            | Ok ur -> ur
                            | Error _ -> r)

                    { game with
                        Notes = game.Notes |> Map.add args.NoteId note
                        PitResults =
                            match updatedRobot with
                            | Microsoft.FSharp.Core.Option.Some ur -> game.PitResults |> Map.add args.Id ur
                            | Microsoft.FSharp.Core.Option.None -> game.PitResults }
                | PitResult.NoteTextChanged args ->
                    { game with
                        Notes = game.Notes |> Map.change args.NoteId (Option.map (fun n -> { n with Text = args.Text })) }
                | PitResult.NoteRemoved args ->
                    let robot = game.PitResults |> Map.tryFind args.Id
                    let updatedRobot =
                        robot |> Option.map (fun r ->
                            match Robot.removeNote args.NoteId r with
                            | Ok ur -> ur
                            | Error _ -> r)

                    { game with
                        Notes = game.Notes |> Map.remove args.NoteId
                        PitResults =
                            match updatedRobot with
                            | Microsoft.FSharp.Core.Option.Some ur -> game.PitResults |> Map.add args.Id ur
                            | Microsoft.FSharp.Core.Option.None -> game.PitResults }
                | _ -> game
                |> GameState.GameStarted
            | GameState.GameStarted _ as gameState, _ -> gameState
