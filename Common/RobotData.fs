namespace ParagonRobotics.DiamondScout.Common.Functional

open System
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common

[<Struct>]
type RobotName =
    private
    | RobotName of string

    member this.Value = let (RobotName name) = this in name

[<Struct>]
type Drivetrain =
    | Swerve
    | Mech
    | Tank
    | Other of string

    override this.ToString() =
        match this with
        | Swerve -> "Swerve"
        | Mech -> "Mecanum"
        | Tank -> "Tank"
        | Other s -> s

    member this.Match(swerveAction: Action, mechAction: Action, tankAction: Action, otherAction: Action<string>) =
        match this with
        | Swerve -> swerveAction.Invoke()
        | Mech -> mechAction.Invoke()
        | Tank -> tankAction.Invoke()
        | Other name -> otherAction.Invoke(name)

[<Struct>]
type EndgameCapable =
    | TierCapability of ScoringTier
    | NotCapable

    member this.Match(tierCapable: Action<ScoringTier>, notCapable: Action) =
        match this with
        | TierCapability tier -> tierCapable.Invoke(tier)
        | NotCapable -> notCapable.Invoke()

type Robot =
    { Name: RobotName
      Team: TeamId
      EndgameCapable: EndgameCapable
      Drivetrain: Drivetrain
      PitScoutingParameters: Map<ParameterDefinitionId, ParameterValue>
      Notes: NoteId list }

[<RequireQualifiedAccess>]
module Robot =
    type Error =
        | ParameterExists of ParameterDefinitionId
        | ParameterDoesNotExist of ParameterDefinitionId
        | RobotNameEmpty
        | DuplicateNote of noteId: NoteId
        | NoteNotFound of noteId: NoteId

    [<RequireQualifiedAccess>]
    module private OnlyIf =
        let parameterDoesNotExist parameterId robot =
            match robot.PitScoutingParameters.ContainsKey parameterId with
            | true -> ParameterExists parameterId |> Validation.error
            | false -> parameterId |> Validation.ok

        let parameterExists parameterId robot =
            match robot.PitScoutingParameters.ContainsKey parameterId with
            | true -> parameterId |> Validation.ok
            | false -> ParameterDoesNotExist parameterId |> Validation.error

        let robotNameNotEmpty (RobotName name as robotName) =
            match String.IsNullOrWhiteSpace name with
            | true -> RobotNameEmpty |> Validation.error
            | false -> robotName |> Validation.ok
            
        let noteIdUnique robot noteId =
            match List.contains noteId robot.Notes with
            | true -> noteId |> DuplicateNote |> Validation.error
            | false -> noteId |> Validation.ok
            
        let noteExists robot noteId =
            match List.contains noteId robot.Notes with
            | true -> noteId |> Validation.ok
            | false -> noteId |> NoteNotFound |> Validation.error

    let createWithParameters name team endgameCapability pitScoutingParams drivetrain =
        validation {
            let! name = OnlyIf.robotNameNotEmpty name

            return
                { Name = name
                  Team = team
                  EndgameCapable = endgameCapability
                  Drivetrain = drivetrain
                  PitScoutingParameters = pitScoutingParams
                  Notes = [] }
        }

    let create name team endgameCapability drivetrain =
        createWithParameters name team endgameCapability Map.empty drivetrain

    let withEndgameCapabilities endgameCapability robot =
        { robot with
            EndgameCapable = endgameCapability }

    let withDrivetrain drivetrain robot = { robot with Drivetrain = drivetrain }

    let addNote noteId robot =
        validation {
            let! noteId = OnlyIf.noteIdUnique robot noteId
            
            return
                { robot with
                    Robot.Notes = noteId :: robot.Notes }
        }

    let removeNote noteId robot =
        validation {
            let! noteId = OnlyIf.noteExists robot noteId
            
            return
                { robot with
                    Robot.Notes = robot.Notes |> List.filter (fun id -> id <> noteId) }
        }

    let setPitScoutingParameter parameterId value robot =
        validation {
            let! parameterId = OnlyIf.parameterDoesNotExist parameterId robot

            return
                { robot with
                    PitScoutingParameters = robot.PitScoutingParameters.Add(parameterId, value) }
        }

    let removePitScoutingParameter parameterId robot =
        validation {
            let! parameterId = OnlyIf.parameterExists parameterId robot

            return
                { robot with
                    PitScoutingParameters = robot.PitScoutingParameters.Remove(parameterId) }
        }
