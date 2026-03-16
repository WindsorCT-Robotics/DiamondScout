namespace ParagonRobotics.DiamondScout.Common

open System.Linq
open System.Collections.Generic
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

[<RequireQualifiedAccess>]
[<Struct>]
type Notes =
    | Notes of notes: IReadOnlyDictionary<NoteId, Note>

    static member Empty = Map.empty<NoteId, Note> :> IReadOnlyDictionary<NoteId, Note> |> Notes

    static member internal AsMap(notes: IReadOnlyDictionary<NoteId, Note>) =
        match notes with
        | :? Map<NoteId, Note> as map -> map
        | map -> map.Select (|KeyValue|) |> Map.ofSeq

    member internal this.AsMap() =
        let (Notes notes) = this
        Notes.AsMap notes

    // Store incoming type as a Map internally, regardless of the underlying input type
    static member Create(notes: IReadOnlyDictionary<NoteId, Note>) =
        Notes.AsMap notes :> IReadOnlyDictionary<NoteId, Note> |> Notes

    member this.Add(id, note) =
        this.AsMap() |> Map.add id note |> Notes.Create

    member this.Remove id =
        this.AsMap() |> Map.remove id |> Notes.Create

    member this.ContainsKey id =
        let (Notes notes) = this in notes.ContainsKey id
