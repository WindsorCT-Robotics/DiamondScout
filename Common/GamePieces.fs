namespace ParagonRobotics.DiamondScout.Common

open System
open FsToolkit.ErrorHandling

[<Struct>]
type GamePieceId =
    private
    | GamePieceId of Guid

    static member Zero = GamePieceId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> GamePieceId
    member this.Value = let (GamePieceId guid) = this in guid

[<Struct>]
type GamePieceName = GamePieceName of string

type GamePiece = private { Name: GamePieceName }


[<RequireQualifiedAccess>]
module GamePiece =
    type Error = | GamePieceNameEmpty

    module private OnlyIf =
        let nameNotEmpty (GamePieceName name as gamePieceName) =
            match String.IsNullOrWhiteSpace name with
            | true -> GamePieceNameEmpty |> Validation.error
            | false -> gamePieceName |> Validation.ok

    let create name =
        validation {
            let! name = OnlyIf.nameNotEmpty name

            return { Name = name }
        }

    let withName name piece =
        validation {
            let! name = OnlyIf.nameNotEmpty name
            return { piece with GamePiece.Name = name }
        }
