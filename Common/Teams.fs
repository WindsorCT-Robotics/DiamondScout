namespace ParagonRobotics.DiamondScout.Common

open System.Collections.Generic
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Functional.AggregateDefinition

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
        Notes: NotebookId
    }

[<RequireQualifiedAccess>]
type Team =
    private
    | Unregistered
    | Registered of TeamData

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Team =
        type Command =
            | Register of teamNumber: TeamNumber * teamName: TeamName * notebook: NotebookId
            | ChangeName of teamName: TeamName

        type Error =
            | TeamAlreadyRegistered
            | TeamNotRegistered
            | TeamNameEmpty

        type Event =
            | Registered of teamNumber: TeamNumber * teamName: TeamName * notebook: NotebookId
            | TeamNameChanged of teamName: TeamName

        module private Event =
            let evolve team event =
                match event with
                | Registered(teamNumber, teamName, notebook) ->
                    match team with
                    | Team.Unregistered ->
                        { TeamNumber = teamNumber
                          TeamName = teamName
                          Notes = notebook }
                        |> Team.Registered
                    | Team.Registered _ as team -> team
                | TeamNameChanged teamName ->
                    match team with
                    | Team.Registered data -> { data with TeamName = teamName } |> Team.Registered
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

        module private Command =
            let decide command team =
                match command with
                | Register(teamNumber, teamName, notebook) ->
                    validation {
                        let! _ = OnlyIf.teamIsNotRegistered team
                        let! teamName = OnlyIf.teamNameNotEmpty teamName

                        return Registered(teamNumber, teamName, notebook) |> List.singleton
                    }
                | ChangeName teamName ->
                    validation {
                        let! _ = OnlyIf.teamIsRegistered team
                        let! teamName = OnlyIf.teamNameNotEmpty teamName

                        return TeamNameChanged teamName |> List.singleton
                    }

        let definition =
            create Team.Unregistered Event.evolve Command.decide

type Team with
    static member Register teamNumber teamName notebook =
        (Team.Register (teamNumber, teamName, notebook), Team.definition.Init) ||> Team.definition.Decide |> Validation.map _.ToReadOnlyList()
        
    static member Rename team newName =
        (Team.ChangeName newName, team) ||> Team.definition.Decide |> Validation.map _.ToReadOnlyList()
        
    static member Evolve team (events: IReadOnlyList<Team.Event>) =
        events
        |> _.FromReadOnlyList()
        |> foldEvents Team.definition team