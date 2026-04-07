namespace ParagonRobotics.DiamondScout.Common.Functional

open ParagonRobotics.DiamondScout.Common
open FsToolkit.ErrorHandling

[<Struct>]
type NoteContent = NoteContent of string

type Note =
    private
        { UserId: UserId
          Text: NoteContent }

[<RequireQualifiedAccess>]
module Note =
    type Error =
        | InvalidUserId of user: UserId
        | EmptyText
        | NotFound of noteId: NoteId

    [<RequireQualifiedAccess>]
    module private OnlyIf =
        let userIdValid userId =
            match userId = UserId.Zero with
            | true -> userId |> InvalidUserId |> Validation.error
            | false -> Validation.ok userId

        let textNotEmpty (NoteContent text) =
            match System.String.IsNullOrWhiteSpace text with
            | true -> Validation.error EmptyText
            | false -> text |> NoteContent |> Validation.ok
            
    let create userId text =
        validation {
            let! userId = OnlyIf.userIdValid userId
            and! text = OnlyIf.textNotEmpty text

            return { UserId = userId; Text = text }
        }

    let edit text note =
        validation {
            let! text = OnlyIf.textNotEmpty text
            return { note with Text = text }
        }
