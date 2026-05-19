namespace ParagonRobotics.DiamondScout.Common.Teams

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.Aggregates
open ParagonRobotics.DiamondScout.Common.Notebooks

[<Struct>]
type TeamId =
    private
    | TeamId of Guid

    static member Zero = TeamId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> TeamId
    member this.Value = let (TeamId guid) = this in guid

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

[<RequireQualifiedAccess>]
type Team =
    private
    | Unregistered
    | Registered of
        {| TeamNumber: TeamNumber
           TeamName: TeamName
           Notes: NotebookId |}

[<RequireQualifiedAccess>]
module Events =
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

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Team =
        module private Event =
            let evolve team event =
                match event with
                | Events.Event.Registered(teamNumber, teamName, notebook) ->
                    match team with
                    | Team.Unregistered ->
                        {| TeamNumber = teamNumber
                           TeamName = teamName
                           Notes = notebook |}
                        |> Team.Registered
                    | Team.Registered _ as team -> team
                | Events.Event.TeamNameChanged teamName ->
                    match team with
                    | Team.Registered data -> {| data with TeamName = teamName |} |> Team.Registered
                    | Team.Unregistered as team -> team

        [<RequireQualifiedAccess>]
        module private OnlyIf =
            let teamIsRegistered team =
                match team with
                | Team.Registered team -> team |> Validation.ok
                | Team.Unregistered -> Events.Error.TeamNotRegistered |> Validation.error

            let teamIsNotRegistered team =
                match team with
                | Team.Registered _ -> Events.Error.TeamAlreadyRegistered |> Validation.error
                | Team.Unregistered -> team |> Validation.ok

            let teamNameNotEmpty (TeamName name) =
                match String.IsNullOrWhiteSpace name with
                | true -> Events.Error.TeamNameEmpty |> Validation.error
                | false -> name |> TeamName |> Validation.ok

        module private Command =
            let decide command team =
                match command with
                | Events.Command.Register(teamNumber, teamName, notebook) ->
                    validation {
                        let! _ = OnlyIf.teamIsNotRegistered team
                        let! teamName = OnlyIf.teamNameNotEmpty teamName

                        return Events.Event.Registered(teamNumber, teamName, notebook) |> List.singleton
                    }
                | Events.Command.ChangeName teamName ->
                    validation {
                        let! _ = OnlyIf.teamIsRegistered team
                        let! teamName = OnlyIf.teamNameNotEmpty teamName

                        return Events.Event.TeamNameChanged teamName |> List.singleton
                    }

        let state = Aggregate.create Team.Unregistered Event.evolve Command.decide

type Team with
    static member Register teamNumber teamName notebook =
        (Events.Command.Register(teamNumber, teamName, notebook), Aggregate.init Team.state)
        ||> Aggregate.decide Team.state
        |> Validation.map _.ToReadOnlyList()

    static member Rename team newName =
        (Events.Command.ChangeName newName, team)
        ||> Aggregate.decide Team.state
        |> Validation.map _.ToReadOnlyList()

    static member Evolve team (events: IReadOnlyList<Events.Event>) =
        events |> _.FromReadOnlyList() |> Aggregate.fold Team.state team
