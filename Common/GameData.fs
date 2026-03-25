namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic

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
                          MatchNumber: MatchNumber }
                    type Scouted =
                        { DistrictCode: FrcDistrictCode
                          EventId: FrcEventId
                          MatchNumber: MatchNumber
                          Team: AllianceTeam
                          Result: ScoutingResults }

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

                    type Won =
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
                  RankingPointGrants: RankingPointGrant IReadOnlyCollection
                  SubPhaseScoreValues: SubPhaseMap<ScoreValue> }

            type NameChanged =
                { GamePieceId: GamePieceId
                  GamePieceName: GamePieceName }

            type RankingPointGrantsAdded =
                { GamePieceId: GamePieceId
                  RankingPointGrants: RankingPointGrant IReadOnlyCollection }

            type RankingPointGrantsRemoved =
                { GamePieceId: GamePieceId
                  RankingPointGrants: RankingPointGrant IReadOnlyCollection }

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
                  Notes: Notes }

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

    type Event =
        private
        | GameStarted of name: GameName * year: DateOnly
        | ChangeGameName of name: GameName
        | ChangeYear of year: DateOnly
        | RegisterDistrict of districtCode: FrcDistrictCode * districtName: FrcDistrictName
        | ChangeDistrictName of districtCode: FrcDistrictCode * districtName: FrcDistrictName
        | RegisterEvent of districtCode: FrcDistrictCode * eventId: FrcEventId * eventName: FrcEventName
        | ChangeEventName of districtCode: FrcDistrictCode * eventId: FrcEventId * eventName: FrcEventName
        | ScoutMatch of
            districtCode: FrcDistrictCode *
            eventId: FrcEventId *
            matchNumber: MatchNumber *
            team: AllianceTeam *
            result: ScoutingResults
        | DefineSubPhase of subPhaseId: SubPhaseId * subPhase: SubPhase
        | DefineGamePiece of gamePieceId: GamePieceId * gamePiece: GamePiece
        | DefineInfraction of infractionId: InfractionId * infraction: Infraction
