namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type TeamId =
    private
    | TeamId of Guid

    static member Zero = TeamId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> TeamId
    member this.Value = let (TeamId guid) = this in guid

[<Struct>]
type GameId =
    private
    | GameId of Guid

    static member Zero = GameId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> GameId
    member this.Value = let (GameId guid) = this in guid

[<Struct>]
type UserId =
    private
    | UserId of Guid

    static member Zero = UserId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> UserId
    member this.Value = let (UserId guid) = this in guid

[<Struct>]
type NoteId =
    private
    | NoteId of Guid

    static member Zero = NoteId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> NoteId
    member this.Value = let (NoteId guid) = this in guid

[<Struct>]
type SubPhaseId =
    private
    | SubPhaseId of Guid

    static member Zero = SubPhaseId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> SubPhaseId
    member this.Value = let (SubPhaseId guid) = this in guid

[<Struct>]
type InfractionId =
    private
    | InfractionId of Guid

    static member Zero = InfractionId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> InfractionId
    member this.Value = let (InfractionId guid) = this in guid

[<Struct>]
type RobotId =
    private
    | RobotId of Guid

    static member Zero = RobotId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> RobotId
    member this.Value = let (RobotId guid) = this in guid

[<Struct>]
type MatchId =
    private
    | MatchId of Guid

    static member Zero = MatchId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> MatchId
    member this.Value = let (MatchId guid) = this in guid

[<Struct>]
type EventId =
    | EventId of Guid

    static member Zero = EventId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> EventId
    member this.Value = let (EventId guid) = this in guid
