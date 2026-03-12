namespace ParagonRobotics.DiamondScout.Common

open FsToolkit.ErrorHandling

[<Struct>]
type NoteContent = NoteContent of string

[<RequireQualifiedAccess>]
module Note =
    type Error =
        | InvalidUserId of user: UserId
        | EmptyText

    module Validation =
        let userIdValid userId =
            match userId = UserId.Zero with
            | true -> userId |> InvalidUserId |> Validation.error 
            | false -> Validation.ok userId

        let textNotEmpty (NoteContent text) =
            match System.String.IsNullOrWhiteSpace text with
            | true -> Validation.error EmptyText
            | false -> text |> NoteContent|> Validation.ok 

type Note = private {
    UserId: UserId
    Text: NoteContent
} with
  static member Create userId text =
        validation {
            let! userId = Note.Validation.userIdValid userId
            and! text = Note.Validation.textNotEmpty text
            
            return { UserId = userId; Text = text }
        }
  member this.Edit text = validation {
        let! text = Note.Validation.textNotEmpty text
        
        return { this with Text = text }
    }

