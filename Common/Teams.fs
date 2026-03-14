namespace ParagonRobotics.DiamondScout.Common

open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common.DomainEvents

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
type TeamData =
    {
        /// The team's number.
        TeamNumber: TeamNumber
        /// The team's name.
        TeamName: TeamName
        /// Notes about the team.
        Notes: Notes
    }

[<RequireQualifiedAccess>]
type Team =
    private
    | Unregistered
    | Registered of TeamData

[<RequireQualifiedAccess>]
module Team =
    type Command =
        | Register of teamNumber: TeamNumber * teamName: TeamName
        | ChangeName of teamName: TeamName
        | AddNote of noteId: NoteId * userId: UserId * noteContent: NoteContent
        | RemoveNote of noteId: NoteId

    type Error =
        | TeamAlreadyRegistered
        | TeamNotRegistered
        | TeamNameEmpty
        | DuplicateNote of noteId: NoteId
        | NoteNotFound of noteId: NoteId
        | NoteError of Note.Error

    type Event =
        | Registered of teamNumber: TeamNumber * teamName: TeamName
        | TeamNameChanged of teamName: TeamName
        | NoteAdded of noteId: NoteId * userId: UserId * noteContent: NoteContent
        | NoteRemoved of noteId: NoteId

    module private Event =
        let evolve team event =
            match event with
            | Registered(teamNumber, teamName) ->
                match team with
                | Team.Unregistered ->
                    { TeamNumber = teamNumber
                      TeamName = teamName
                      Notes = Notes.Empty }
                    |> Team.Registered
                | Team.Registered _ as team -> team
            | TeamNameChanged teamName ->
                match team with
                | Team.Registered data -> { data with TeamName = teamName } |> Team.Registered
                | Team.Unregistered as team -> team
            | NoteAdded(noteId, userId, noteContent) ->
                match team with
                | Team.Registered data -> { data with Notes = data.Notes.Add(noteId, Note.Create userId noteContent) } |> Team.Registered
                | Team.Unregistered as team -> team
            | NoteRemoved noteId ->
                match team with
                | Team.Registered data -> { data with Notes = data.Notes.Remove noteId } |> Team.Registered
                | Team.Unregistered as team -> team

    [<RequireQualifiedAccess>]
    module private OnlyIf =
        let teamIsRegistered team =
            match team with
            | Team.Registered team -> team |> Validation.ok
            | Team.Unregistered -> TeamNotRegistered |> Validation.error

        let teamIsNotRegistered team =
            match team with
            | Team.Registered _ -> TeamAlreadyRegistered |> Validation.error
            | Team.Unregistered -> team |> Validation.ok

        let teamNameNotEmpty (TeamName name) =
            match System.String.IsNullOrWhiteSpace name with
            | true -> TeamNameEmpty |> Validation.error
            | false -> name |> TeamName |> Validation.ok

        let noteIdUnique data id =
            match data.Notes.ContainsKey id with
            | true -> id |> DuplicateNote |> Validation.error
            | false -> id |> Validation.ok

        let noteExists data id =
            match data.Notes.ContainsKey id with
            | true -> id |> Validation.ok
            | false -> id |> NoteNotFound |> Validation.error

    module private Command =
        let decide command team =
            match command with
            | Register(teamNumber, teamName) ->
                validation {
                    let! _ = OnlyIf.teamIsNotRegistered team
                    let! teamName = OnlyIf.teamNameNotEmpty teamName

                    return Registered(teamNumber, teamName) |> List.singleton
                }
            | ChangeName teamName ->
                validation {
                    let! _ = OnlyIf.teamIsRegistered team
                    let! teamName = OnlyIf.teamNameNotEmpty teamName

                    return TeamNameChanged teamName |> List.singleton
                }
            | AddNote(noteId, userId, noteContent) ->
                validation {
                    let! data = OnlyIf.teamIsRegistered team
                    let! noteId = OnlyIf.noteIdUnique data noteId
                    let! _ = Note.TryCreate userId noteContent |> Validation.mapError NoteError

                    return NoteAdded(noteId, userId, noteContent) |> List.singleton
                }
            | RemoveNote noteId ->
                validation {
                    let! data = OnlyIf.teamIsRegistered team
                    let! noteId = OnlyIf.noteExists data noteId

                    return NoteRemoved noteId |> List.singleton
                }

    let definition =
        AggregateDefinition.create Team.Unregistered Event.evolve Command.decide
