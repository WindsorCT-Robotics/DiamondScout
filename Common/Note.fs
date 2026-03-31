namespace ParagonRobotics.DiamondScout.Common.Functional

open ParagonRobotics.DiamondScout.Common
open FsToolkit.ErrorHandling

[<Struct>]
type NoteContent = NoteContent of string

[<RequireQualifiedAccess>]
module Note =
    type Error =
        | InvalidUserId of user: UserId
        | EmptyText

    [<RequireQualifiedAccess>]
    module internal OnlyIf =
        let userIdValid userId =
            match userId = UserId.Zero with
            | true -> userId |> InvalidUserId |> Validation.error
            | false -> Validation.ok userId

        let textNotEmpty (NoteContent text) =
            match System.String.IsNullOrWhiteSpace text with
            | true -> Validation.error EmptyText
            | false -> text |> NoteContent |> Validation.ok

type Note =
    private
        { UserId: UserId
          Text: NoteContent }

    static member TryCreate userId text =
        validation {
            let! userId = Note.OnlyIf.userIdValid userId
            and! text = Note.OnlyIf.textNotEmpty text

            return { UserId = userId; Text = text }
        }

    static member Create userId text =
        Note.TryCreate userId text
        |> Result.defaultWith (fun e -> $" Unable to create note: {e}" |> invalidOp)

    member this.Edit text =
        validation {
            let! text = Note.OnlyIf.textNotEmpty text
            return { this with Text = text }
        }
