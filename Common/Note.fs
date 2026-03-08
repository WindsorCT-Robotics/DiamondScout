namespace ParagonRobotics.DiamondScout.Common

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open ParagonRobotics.DiamondScout.Common.DomainEvents

type Note = { UserId: UserId; Text: string }

[<RequireQualifiedAccess>]
module Note =
    let create userId text = { UserId = userId; Text = text }
    let setUserId userId note = { note with UserId = userId }
    let edit text note = { note with Text = text }

    type Event =
        | Created of noteId: NoteId * note: Note
        | TextChanged of noteId: NoteId * text: string
        | Deleted of noteId: NoteId

    module Event =
        let evolve notes event =
            let change id f = notes |> Map.change id f

            match event with
            | Created(id, note) -> notes |> Map.add id note
            | TextChanged(id, text) -> edit text |> Option.map |> change id
            | Deleted id -> Map.remove id notes

    type Command =
        | Create of user: UserId * text: string
        | Edit of noteId: NoteId * text: string
        | Delete of noteId: NoteId

    type Error =
        | InvalidUserId of user: UserId
        | EmptyText
        | NoteDoesNotExist of noteId: NoteId

    module Validation =
        let noteExists noteId notes =
            match Map.tryFind noteId notes with
            | Some note -> Validation.ok note
            | None -> noteId |> NoteDoesNotExist |> Validation.error

        let userIdValid note =
            match note.UserId = UserId.Zero with
            | true -> Validation.error (InvalidUserId note.UserId)
            | false -> Validation.ok note

        let textNotEmpty note =
            match System.String.IsNullOrWhiteSpace note.Text with
            | true -> Validation.error EmptyText
            | false -> Validation.ok note

    module Command =
        let decide command notes =
            match command with
            | Create(user, text) ->
                let noteId = System.Guid.NewGuid() |> NoteId

                create user text
                |> Validation.userIdValid
                >>= Validation.textNotEmpty
                |> Validation.map (fun note -> [ Created(noteId, note) ])
            | Edit(id, text) ->
                Validation.noteExists id notes
                |> Validation.map (edit text)
                >>= Validation.textNotEmpty
                |> Validation.map (fun note -> [ TextChanged(id, note.Text) ])
            | Delete id -> Validation.noteExists id notes |> Validation.map (fun _ -> [ Deleted id ])

    let eventStream =
        EventStream.create Map.empty<NoteId, Note> Event.evolve Command.decide
