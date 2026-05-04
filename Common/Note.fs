namespace ParagonRobotics.DiamondScout.Common

open System.Collections.Generic
open ParagonRobotics.DiamondScout.Common
open FsToolkit.ErrorHandling

[<Struct>]
type NoteContent = NoteContent of string

type Note =
    private
        { UserId: UserId
          DateAdded: System.DateTime
          Content: NoteContent }

[<RequireQualifiedAccess>]
type NoteState =
    | NotCreated
    | Created of Note
    
[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Note =
        type Error =
            | InvalidUserId of user: UserId
            | EmptyText
            | NoteNotCreated
            | NoteAlreadyCreated

        type Command =
            | Create of user: UserId * dateAdded: System.DateTime * content: NoteContent
            | Edit of content: NoteContent
            
        type Event =
            | Created of user: UserId * dateAdded: System.DateTime * content: NoteContent
            | Edited of content: NoteContent

        [<RequireQualifiedAccess>]
        module private OnlyIf =
            let noteCreated noteState  =
                match noteState with
                | NoteState.NotCreated -> Validation.error Error.NoteNotCreated
                | NoteState.Created _ -> Validation.ok ()

            let noteNotCreated noteState =
                match noteState with
                | NoteState.NotCreated -> Validation.ok ()
                | NoteState.Created _ -> Validation.error Error.NoteAlreadyCreated

            let userIdValid userId =
                match userId = UserId.Zero with
                | true -> userId |> InvalidUserId |> Validation.error
                | false -> Validation.ok userId

            let textNotEmpty (NoteContent text) =
                match System.String.IsNullOrWhiteSpace text with
                | true -> Validation.error EmptyText
                | false -> text |> NoteContent |> Validation.ok
                
        let private evolve note event =
            match (note, event) with
            | NoteState.NotCreated, Created(userId, dateAdded, noteContent) ->
                { UserId = userId; DateAdded = dateAdded; Content = noteContent }
                |> NoteState.Created
            | NoteState.NotCreated as notCreated, _ -> notCreated
            | NoteState.Created _ as note, Created _ -> note
            | NoteState.Created note, Edited newContent ->
                { note with Content = newContent }
                |> NoteState.Created

        let private decide command state =
            match command with
                | Command.Create(userId, dateAdded, noteContent) ->
                    validation {
                        do! OnlyIf.noteNotCreated state |> Validation.map (fun _ -> ())
                        let! userId = OnlyIf.userIdValid userId
                        and! noteContent = OnlyIf.textNotEmpty noteContent

                        return [ Event.Created (userId, dateAdded, noteContent) ]
                    }
                | Command.Edit noteContent ->
                    validation {
                        do! OnlyIf.noteCreated state
                        let! noteContent = OnlyIf.textNotEmpty noteContent
                        return [ Event.Edited noteContent ]
                    }
        
        let definition =
            { Init = NoteState.NotCreated
              Decide = decide
              Evolve = evolve }
            
type Note with
    static member Create userId dateAdded content =
        (Note.Create (userId, dateAdded, content), NoteState.NotCreated) ||> Note.definition.Decide |> Validation.map _.ToReadOnlyList()
        
    static member Edit content note =
        (Note.Edit content, note) ||> Note.definition.Decide |> Validation.map _.ToReadOnlyList()
        
    static member Evolve note (events: IReadOnlyList<Note.Event>) =
        events
        |> _.FromReadOnlyList()
        |> foldEvents Note.definition note
    