namespace ParagonRobotics.DiamondScout.Common.Functional

open System
open ParagonRobotics.DiamondScout.Common

[<Struct>]
type GameName = GameName of string

type Game =
    private
        { Year: DateOnly
          Name: GameName
          FrcDistricts: Map<FrcDistrictCode, FrcDistrict>
          SubPhases: Map<SubPhaseId, SubPhase>
          GamePieces: Map<GamePieceId, GamePiece>
          Infractions: Map<InfractionId, Infraction>
          PitResults: Map<RobotId, Robot>
          ParameterDefinitions: Map<ParameterDefinitionId, ParameterDefinition>
          Parameters: Map<RobotId, Map<ParameterDefinitionId, ParameterValue>> }

[<RequireQualifiedAccess>]
type private GameState =
    | GameNotStarted
    | GameStarted of Game

[<RequireQualifiedAccess>]
module Game =
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
                          MatchNumber: MatchNumber }
                        
                    type Scouted =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchNumber: MatchNumber
                          Team: AllianceTeam
                          Result: ScoutingData }

                    type NoteAdded =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchNumber: MatchNumber
                          Team: AllianceTeam
                          NoteId: NoteId
                          UserId: UserId
                          Text: NoteContent }

                    type NoteTextChanged =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchNumber: MatchNumber
                          Team: AllianceTeam
                          NoteId: NoteId
                          Text: NoteContent }

                    type NoteRemoved =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchNumber: MatchNumber
                          Team: AllianceTeam
                          NoteId: NoteId }

                    type Concluded =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          Winner: Alliance }

        [<RequireQualifiedAccess>]
        module SubPhase =
            type Defined =
                { SubPhaseId: SubPhaseId
                  SubPhaseName: SubPhaseName
                  SubPhaseDescription: SubPhaseDescription
                  ParentPhase: Phase }

            type NameChanged =
                { SubPhaseId: SubPhaseId
                  SubPhaseName: SubPhaseName }

            type DescriptionChanged =
                { SubPhaseId: SubPhaseId
                  SubPhaseDescription: SubPhaseDescription }

            type Removed =
                { SubPhaseId: SubPhaseId }

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

            type CardChanged =
                { Id: InfractionId
                  Card: Card option }

        [<RequireQualifiedAccess>]
        module PitResult =
            type Scouted =
                { Id: RobotId
                  TeamId: TeamId
                  Name: RobotName
                  EndgameCapable: EndgameCapable
                  Drivetrain: Drivetrain
                  Notes: Map<NoteId, Note> }

            type RobotNameChanged =
                { Id: RobotId
                  Name: RobotName }

            type TeamChanged =
                { Id: RobotId
                  TeamId: TeamId }

            type EndgameCapabilitiesChanged =
                { Id: RobotId
                  EndgameCapable: EndgameCapable }

            type DrivetrainChanged =
                { Id: RobotId
                  Drivetrain: Drivetrain }

            type NoteAdded =
                { Id: RobotId
                  NoteId: NoteId
                  UserId: UserId
                  Text: NoteContent }

            type NoteTextChanged =
                { Id: RobotId
                  NoteId: NoteId
                  Text: NoteContent }

            type NoteRemoved =
                { Id: RobotId
                  NoteId: NoteId }

        [<RequireQualifiedAccess>]
        module ParameterDefinition =
            type Defined =
                { Id: ParameterDefinitionId
                  Name: ParameterDefinitionName
                  Spec: ParameterSpec }

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
            | Defined of EventArgs.SubPhase.Defined
            | NameChanged of EventArgs.SubPhase.NameChanged
            | DescriptionChanged of EventArgs.SubPhase.DescriptionChanged
            | Removed of EventArgs.SubPhase.Removed

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
        type Parameter =
            | ValueSet of EventArgs.Parameter.ValueSet

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
                  Parameters = Map.empty }
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

                    { game with FrcDistricts = Map.add args.DistrictCode newDistrict game.FrcDistricts }
                    |> GameState.GameStarted
                | District.NameChanged args ->
                    { game with FrcDistricts = game.FrcDistricts |> Map.change args.DistrictCode  (Option.map (fun d -> {d with Name = args.DistrictName })) }
                    |> GameState.GameStarted
                | District.FrcEventEvent frcEventEvent ->
                    let changeFrcEvent eventId transform maybeDistrict =
                        maybeDistrict
                        |> Option.map (fun district -> 
                            { district with
                                  FrcDistrict.Events =
                                      district.Events
                                      |> Map.change eventId (Option.map transform) })
                    
                    let addFrcEvent eventId event maybeDistrict =
                        maybeDistrict
                        |> Option.map (fun district ->
                            { district with
                                  FrcDistrict.Events =
                                      district.Events
                                      |> Map.add eventId event })
                        
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
                                      (changeFrcEvent args.EventId (FrcEvent.changeName args.EventName))}
                    | FrcEvent.MatchEvent matchEvent ->
                        let addMatch matchId  maybeEvent =
                            maybeEvent
                            |> Option.map (fun event ->
                                { event with
                                      Matches = event.Matches |> FrcEvent.addMatch (Match.createMatch matchNumber (MatchScoutingResults []) })
                        match matchEvent with
                        | Match.Registered args ->
                            { game with
                                  FrcDistricts =
                                      game.FrcDistricts
                                      |> Map.change
                                          args.DistrictCode
                                          ((changeFrcEvent args.EventId  }

                    |> GameState.GameStarted
                    