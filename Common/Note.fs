namespace ParagonRobotics.DiamondScout.Common.Notes

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Aggregates
open ParagonRobotics.DiamondScout.Common.Users

[<Struct>]
type NoteId =
    private
    | NoteId of Guid

    static member Zero = NoteId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> NoteId
    member this.Value = let (NoteId guid) = this in guid

[<Struct>]
type NoteContent = NoteContent of string

type Note =
    private
        { UserId: UserId
          DateAdded: DateTime
          Content: NoteContent }

[<RequireQualifiedAccess>]
type NoteState =
    | NotCreated
    | Created of Note

[<RequireQualifiedAccess>]
module Events =
    type Error =
        | InvalidUserId of user: UserId
        | EmptyText
        | NoteNotCreated
        | NoteAlreadyCreated

    type Command =
        | Create of user: UserId * dateAdded: DateTime * content: NoteContent
        | Edit of content: NoteContent

    type Event =
        | Created of user: UserId * dateAdded: DateTime * content: NoteContent
        | Edited of content: NoteContent

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Note =
        [<RequireQualifiedAccess>]
        module private OnlyIf =
            let noteCreated noteState =
                match noteState with
                | NoteState.NotCreated -> Validation.error Events.Error.NoteNotCreated
                | NoteState.Created _ -> Validation.ok ()

            let noteNotCreated noteState =
                match noteState with
                | NoteState.NotCreated -> Validation.ok ()
                | NoteState.Created _ -> Validation.error Events.Error.NoteAlreadyCreated

            let userIdValid userId =
                match userId = UserId.Zero with
                | true -> userId |> Events.Error.InvalidUserId |> Validation.error
                | false -> Validation.ok userId

            let textNotEmpty (NoteContent text) =
                match String.IsNullOrWhiteSpace text with
                | true -> Validation.error Events.Error.EmptyText
                | false -> text |> NoteContent |> Validation.ok

        let private evolve note event =
            match (note, event) with
            | NoteState.NotCreated, Events.Event.Created(userId, dateAdded, noteContent) ->
                { UserId = userId
                  DateAdded = dateAdded
                  Content = noteContent }
                |> NoteState.Created
            | NoteState.NotCreated as notCreated, _ -> notCreated
            | NoteState.Created _ as note, Events.Event.Created _ -> note
            | NoteState.Created note, Events.Event.Edited newContent ->
                { note with Content = newContent } |> NoteState.Created

        let private decide command state =
            match command with
            | Events.Command.Create(userId, dateAdded, noteContent) ->
                validation {
                    do! OnlyIf.noteNotCreated state |> Validation.map (fun _ -> ())
                    let! userId = OnlyIf.userIdValid userId
                    and! noteContent = OnlyIf.textNotEmpty noteContent

                    return [ Events.Event.Created(userId, dateAdded, noteContent) ]
                }
            | Events.Command.Edit noteContent ->
                validation {
                    do! OnlyIf.noteCreated state
                    let! noteContent = OnlyIf.textNotEmpty noteContent
                    return [ Events.Event.Edited noteContent ]
                }

        let state =
            Aggregate.create NoteState.NotCreated evolve decide

type Note with
    static member Create userId dateAdded content =
        (Events.Command.Create(userId, dateAdded, content), Aggregate.init Note.state)
        ||> Aggregate.decide Note.state
        |> Validation.map _.ToReadOnlyList()

    static member Edit content note =
        (Events.Command.Edit content, note)
        ||> Aggregate.decide Note.state
        |> Validation.map _.ToReadOnlyList()

    static member Evolve note (events: IReadOnlyList<Events.Event>) =
        events |> _.FromReadOnlyList() |> Aggregate.fold Note.state note
