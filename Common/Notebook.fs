namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type NotebookName = NotebookName of string

type Notebook =
    private
        { Name: NotebookName; Notes: NoteId list }
        
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
        
        type Command =
            | Create of name: NotebookName
            | AddNote of NoteId
            | RemoveNote of NoteId
            
        type Event =
            | Created of name: NotebookName
            | NoteAdded of NoteId
            | NoteRemoved of NoteId
            
        let initialState = { Name = String.Empty |> NotebookName; Notes = [] }
        
        let evolve state event =
            match event with
            | Created name -> { state with Name = name; Notes = [] }
            | NoteAdded note -> { state with Notes = note :: state.Notes }
            | NoteRemoved note -> { state with Notes = state.Notes |> List.filter (fun item -> item <> note) }