namespace ParagonRobotics.DiamondScout.Common

open System

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
        type GameStarted = { Name: GameName; Year: DateOnly }
        type ChangeGameName = { Name: GameName }
        type ChangeYear = { Year: DateOnly }

        type RegisterDistrict =
            { DistrictCode: FrcDistrictCode
              DistrictName: FrcDistrictName }

        type ChangeDistrictName =
            { DistrictCode: FrcDistrictCode
              DistrictName: FrcDistrictName }

        type RegisterEvent =
            { DistrictCode: FrcDistrictCode
              EventId: FrcEventId
              EventName: FrcEventName }

        type ChangeEventName =
            { DistrictCode: FrcDistrictCode
              EventId: FrcEventId
              EventName: FrcEventName }

        type ScoutMatch =
            { DistrictCode: FrcDistrictCode
              EventId: FrcEventId
              MatchNumber: MatchNumber
              Team: AllianceTeam
              Result: ScoutingResults }

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
