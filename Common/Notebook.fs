namespace ParagonRobotics.DiamondScout.Common.Notebooks

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling
open FSharp.Core
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Aggregates
open ParagonRobotics.DiamondScout.Common.Notes

[<Struct>]
type NotebookName = NotebookName of string

type Notebook =
    private
    | Closed
    | Open of
        {| Name: NotebookName
           Notes: NoteId list |}

[<Struct>]
type NotebookId =
    private
    | NotebookId of Guid

    static member Zero = Guid.Empty |> NotebookId
    static member Create = Guid.CreateVersion7() |> NotebookId
    member this.Value = let (NotebookId guid) = this in guid

[<RequireQualifiedAccess>]
module Events =
    type Error =
        | NoteExists of NoteId
        | NoteDoesNotExist of NoteId
        | NotebookClosed
        | NotebookAlreadyOpened
        | NotebookNameEmpty

    type Command =
        | Create of name: NotebookName
        | AddNote of NoteId
        | RemoveNote of NoteId

    type Event =
        | Created of name: NotebookName
        | NoteAdded of NoteId
        | NoteRemoved of NoteId

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Notebook =
        let private initialState = Closed

        let private evolve state event =
            match (state, event) with
            | Closed, Events.Event.Created name -> Open {| Name = name; Notes = [] |}
            | Open data, Events.Event.NoteAdded note ->
                Open
                    {| data with
                        Notes = note :: data.Notes |}
            | Open data, Events.Event.NoteRemoved note ->
                Open
                    {| data with
                        Notes = data.Notes |> List.filter (fun item -> item <> note) |}
            | _ -> state

        module private OnlyIf =
            let noteExists noteId notebook =
                match notebook with
                | Open data ->
                    match List.contains noteId data.Notes with
                    | true -> Validation.ok noteId
                    | false -> Events.Error.NoteDoesNotExist noteId |> Validation.error
                | Closed -> Validation.error Events.Error.NotebookClosed

            let noteDoesNotExist noteId notebook =
                match notebook with
                | Open data ->
                    match List.contains noteId data.Notes with
                    | true -> Events.Error.NoteExists noteId |> Validation.error
                    | false -> Validation.ok noteId
                | Closed -> Validation.error Events.Error.NotebookClosed

            let notebookOpen notebook =
                match notebook with
                | Open _ -> Validation.ok notebook
                | Closed -> Validation.error Events.Error.NotebookClosed

            let notebookClosed notebook =
                match notebook with
                | Closed -> Validation.ok notebook
                | Open _ -> Validation.error Events.Error.NotebookAlreadyOpened

            let notebookNameNotEmpty (NotebookName notebookName as nb) =
                match String.IsNullOrWhiteSpace notebookName with
                | true -> Events.Error.NotebookNameEmpty |> Validation.error
                | false -> nb |> Validation.ok

        let private decide command notebook =
            match (notebook, command) with
            | Closed, Events.Command.Create notebookName ->
                validation {
                    let! notebookName = OnlyIf.notebookNameNotEmpty notebookName

                    return [ Events.Event.Created notebookName ]
                }
            | _, Events.Command.Create _ -> Events.Error.NotebookAlreadyOpened |> Validation.error
            | Open _, Events.Command.AddNote noteId ->
                validation {
                    let! noteId = OnlyIf.noteDoesNotExist noteId notebook

                    return [ Events.Event.NoteAdded noteId ]
                }
            | Open _, Events.Command.RemoveNote noteId ->
                validation {
                    let! noteId = OnlyIf.noteExists noteId notebook

                    return [ Events.Event.NoteRemoved noteId ]
                }
            | Closed, _ -> Events.Error.NotebookClosed |> Validation.error

        let state = Aggregate.create initialState evolve decide

type Notebook with
    static member Create name =
        (Events.Command.Create name, Aggregate.init Notebook.state)
        ||> Aggregate.decide Notebook.state
        |> Validation.map _.ToReadOnlyList()

    static member AddNote noteId notebook =
        (Events.Command.AddNote noteId, notebook)
        ||> Aggregate.decide Notebook.state
        |> Validation.map _.ToReadOnlyList()

    static member RemoveNote noteId notebook =
        (Events.Command.RemoveNote noteId, notebook)
        ||> Aggregate.decide Notebook.state
        |> Validation.map _.ToReadOnlyList()

    static member Evolve notebook (events: IReadOnlyList<Events.Event>) =
        events |> _.FromReadOnlyList() |> Aggregate.fold Notebook.state notebook
