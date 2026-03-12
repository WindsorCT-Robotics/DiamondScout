namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type TeamId =
    private
    | TeamId of Guid

    static member Zero = TeamId Guid.Empty
    static member Create () = Guid.CreateVersion7() |> TeamId
    member this.Value = let (TeamId guid) = this in guid

[<Struct>]
type GameId =
    private
    | GameId of Guid

    static member Zero = GameId Guid.Empty
    static member Create () = Guid.CreateVersion7() |> GameId
    member this.Value = let (GameId guid) = this in guid

[<Struct>]
type UserId =
    private
    | UserId of Guid

    static member Zero = UserId Guid.Empty
    static member Create () = Guid.CreateVersion7() |> UserId
    member this.Value = let (UserId guid) = this in guid
    
[<Struct>]
type NoteId =
    private
    | NoteId of Guid
    static member Zero = NoteId Guid.Empty
    static member Create () = Guid.CreateVersion7() |> NoteId
    member this.Value = let (NoteId guid) = this in guid