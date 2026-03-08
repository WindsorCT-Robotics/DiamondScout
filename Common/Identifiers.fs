namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type TeamId =
    | TeamId of Guid

    static member Zero = TeamId Guid.Empty

[<Struct>]
type MatchId =
    | MatchId of Guid

    static member Zero = MatchId Guid.Empty

[<Struct>]
type FrcEventId =
    | FrcEventId of Guid

    static member Zero = FrcEventId Guid.Empty

[<Struct>]
type GameId =
    | GameId of Guid

    static member Zero = GameId Guid.Empty

[<Struct>]
type RobotId =
    | RobotId of Guid

    static member Zero = RobotId Guid.Empty

[<Struct>]
type MatchResultId =
    | MatchResultId of Guid

    static member Zero = MatchResultId Guid.Empty

[<Struct>]
type InfractionId =
    | InfractionId of Guid

    static member Zero = InfractionId Guid.Empty

[<Struct>]
type GamePieceId =
    | GamePieceId of Guid

    static member Zero = GamePieceId Guid.Empty

[<Struct>]
type SubPhaseId =
    | SubPhaseId of Guid

    static member Zero = SubPhaseId Guid.Empty

[<Struct>]
type ParameterDefinitionId =
    | ParameterDefinitionId of Guid

    static member Zero = ParameterDefinitionId Guid.Empty

[<Struct>]
type RobotParametersId =
    | RobotParametersId of Guid

    static member Zero = RobotParametersId Guid.Empty

[<Struct>]
type UserId =
    | UserId of Guid

    static member Zero = UserId Guid.Empty

[<Struct>]
type NoteId =
    | NoteId of Guid

    static member Zero = NoteId Guid.Empty

[<Struct>]
type ScoutingResultsId =
    | ScoutingResultsId of Guid

    static member Zero = ScoutingResultsId Guid.Empty
