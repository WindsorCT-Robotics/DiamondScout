namespace ParagonRobotics.DiamondScout.Common

open FsToolkit.ErrorHandling

/// A FIRST Robotics Competition team number.
[<Struct>]
type TeamNumber =
    /// <summary>Creates a new TeamNumber instance.</summary>
    /// <param name="teamNumber">The team number.</param>
    | TeamNumber of teamNumber: uint

/// A FIRST Robotics Competition team name.
[<Struct>]
type TeamName =
    /// <summary>Creates a new TeamName instance.</summary>
    /// <param name="teamName">The team name.</param>
    | TeamName of teamName: string

/// A FIRST Robotics Competition team.
type Team = private {
    /// The team's number.
    TeamNumber: TeamNumber
    /// The team's name.
    TeamName: TeamName
    /// Notes about the team.
    Notes: Map<NoteId, Note>
}

[<RequireQualifiedAccess>]
module Team =
    type Command =
        | Register of teamNumber: TeamNumber * teamName: TeamName
        | ChangeName of teamName: TeamName
        | AddNote of noteId: NoteId * userId: UserId * noteContent: NoteContent
        | RemoveNote of noteId: NoteId
        
    type Error =
        | TeamNameEmpty
        | DuplicateNote of noteId: NoteId
        | NoteNotFound of noteId: NoteId
        | NoteError of Note.Error
    
    type Event =
        | TeamNameChanged of teamName: TeamName
        | NoteAdded of noteId: NoteId * userId: UserId * noteContent: NoteContent
        | NoteRemoved of noteId: NoteId
        
    module Event =
        
    module Validation =
        let teamNameNotEmpty (TeamName name) =
            match System.String.IsNullOrWhiteSpace name with
            | true -> TeamNameEmpty |> Validation.error
            | false -> name |> TeamName |> Validation.ok
            
        let noteIdUnique team id =
            match Map.containsKey id team.Notes with
            | true -> id |> DuplicateNote |> Validation.error
            | false -> id |> Validation.ok
        
        let noteExists team id =
            match Map.containsKey id team.Notes with
            | true -> id |> Validation.ok
            | false -> id |> NoteNotFound |> Validation.error
            
    module Command = 
    let create teamNumber teamName = validation {
        let! teamName = Validation.teamNameNotEmpty teamName
        return
            { TeamNumber = teamNumber
              TeamName = teamName
              Notes = Map.empty }
    }

    let changeName teamName team = validation {
        let! teamName = Validation.teamNameNotEmpty teamName
        
        return { team with TeamName = teamName }
    }

    let addNote noteId userId noteContent team =
        { team with Notes = Note.Create userId noteContent  }

