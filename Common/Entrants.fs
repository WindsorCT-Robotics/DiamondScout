namespace ParagonRobotics.DiamondScout.Common.Entrants

open System
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common.Matches
open ParagonRobotics.DiamondScout.Common.Results
open ParagonRobotics.DiamondScout.Common.Robots
open ParagonRobotics.DiamondScout.Common.Teams

[<Struct>]
type EntrantId =
    private | EntrantId of Guid
    static member Zero = EntrantId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> EntrantId
    member this.Value = let (EntrantId guid) = this in guid

type Entrant =
    private
        { Team: TeamId
          Robot: Robot
          ScoutingResults: Map<MatchId, ScoutingResult> }

type Error =
    | InvalidTeamId of TeamId
    | InvalidMatchId of MatchId
    | ScoutingResultAlreadyExists of MatchId
    | ScoutingResultNotFound of MatchId
    | RobotError of Robot.Error

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module Entrant =
        module private OnlyIf =
            let teamIdValid teamId =
                if teamId = TeamId.Zero then Error.InvalidTeamId teamId |> Validation.error
                else Validation.ok teamId

            let scoutingResultDoesNotExist matchId entrant =
                if Map.containsKey matchId entrant.ScoutingResults then 
                    Error.ScoutingResultAlreadyExists matchId |> Validation.error
                else Validation.ok matchId

            let scoutingResultExists matchId entrant =
                match Map.tryFind matchId entrant.ScoutingResults with
                | Some result -> Validation.ok result
                | None -> Error.ScoutingResultNotFound matchId |> Validation.error

        let create team robot =
            validation {
                let! team = OnlyIf.teamIdValid team
                return
                    { Team = team
                      Robot = robot
                      ScoutingResults = Map.empty }
            }

        let updateRobot robot entrant =
            { entrant with Robot = robot }

        let addScoutingResult matchId scoutingResult entrant =
            validation {
                let! _ = OnlyIf.scoutingResultDoesNotExist matchId entrant
                return { entrant with ScoutingResults = Map.add matchId scoutingResult entrant.ScoutingResults }
            }

        let updateScoutingResult matchId updater entrant =
            validation {
                let! result = OnlyIf.scoutingResultExists matchId entrant
                let! updatedResult = updater result
                return { entrant with ScoutingResults = Map.add matchId updatedResult entrant.ScoutingResults }
            }

type Entrant with
    static member Create(teamId, robot) =
        Entrant.create teamId robot

    static member AddScoutingResult(matchId, result, entrant) =
        Entrant.addScoutingResult matchId result entrant
