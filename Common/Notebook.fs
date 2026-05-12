namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling
open FSharp.Core

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

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Notebook =
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

        let private initialState = Closed

        let private evolve state event =
            match (state, event) with
            | Closed, Created name -> Open {| Name = name; Notes = [] |}
            | Open data, NoteAdded note ->
                Open
                    {| data with
                        Notes = note :: data.Notes |}
            | Open data, NoteRemoved note ->
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
                    | false -> NoteDoesNotExist noteId |> Validation.error
                | Closed -> Validation.error NotebookClosed

            let noteDoesNotExist noteId notebook =
                match notebook with
                | Open data ->
                    match List.contains noteId data.Notes with
                    | true -> NoteExists noteId |> Validation.error
                    | false -> Validation.ok noteId
                | Closed -> Validation.error NotebookClosed

            let notebookOpen notebook =
                match notebook with
                | Open _ -> Validation.ok notebook
                | Closed -> Validation.error NotebookClosed

            let notebookClosed notebook =
                match notebook with
                | Closed -> Validation.ok notebook
                | Open _ -> Validation.error NotebookAlreadyOpened

            let notebookNameNotEmpty (NotebookName notebookName as nb) =
                match String.IsNullOrWhiteSpace notebookName with
                | true -> NotebookNameEmpty |> Validation.error
                | false -> nb |> Validation.ok

        let private decide command notebook =
            match (notebook, command) with
            | Closed, Create notebookName ->
                validation {
                    let! notebookName = OnlyIf.notebookNameNotEmpty notebookName

                    return [ Created notebookName ]
                }
            | _, Create _ -> NotebookAlreadyOpened |> Validation.error
            | Open _, AddNote noteId ->
                validation {
                    let! noteId = OnlyIf.noteDoesNotExist noteId notebook

                    return [ NoteAdded noteId ]
                }
            | Open _, RemoveNote noteId ->
                validation {
                    let! noteId = OnlyIf.noteExists noteId notebook

                    return [ NoteRemoved noteId ]
                }
            | Closed, _ -> NotebookClosed |> Validation.error

        let definition = create initialState evolve decide

type Notebook with
    static member Create name =
        (Notebook.Create name, Notebook.definition.Init)
        ||> Notebook.definition.Decide
        |> Validation.map _.ToReadOnlyList()

    static member AddNote noteId notebook =
        (Notebook.AddNote noteId, notebook)
        ||> Notebook.definition.Decide
        |> Validation.map _.ToReadOnlyList()

    static member RemoveNote noteId notebook =
        (Notebook.RemoveNote noteId, notebook)
        ||> Notebook.definition.Decide
        |> Validation.map _.ToReadOnlyList()

    static member Evolve notebook (events: IReadOnlyList<Notebook.Event>) =
        events |> _.FromReadOnlyList() |> foldEvents Notebook.definition notebook
