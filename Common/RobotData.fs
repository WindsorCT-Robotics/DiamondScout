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
      Notes: Map<NoteId, Note> }

[<RequireQualifiedAccess>]
module Robot =
    type Error =
        | ParameterExists of ParameterDefinitionId
        | ParameterDoesNotExist of ParameterDefinitionId
        | RobotNameEmpty
        | NoteError of Note.Error
        
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
            
        let robotNameNotEmpty (RobotName name as robotName)=
            match System.String.IsNullOrWhiteSpace name with
            | true -> RobotNameEmpty |> Validation.error
            | false -> robotName |> Validation.ok
            
    let createWithParameters name team endgameCapability pitScoutingParams drivetrain = validation {
        let! robotName = OnlyIf.robotNameNotEmpty name
        
        return 
            { Name = name
              Team = team
              EndgameCapable = endgameCapability
              Drivetrain = drivetrain
              PitScoutingParameters = pitScoutingParams
              Notes = Map.empty }
    }
    
    let create name team endgameCapability drivetrain = createWithParameters name team endgameCapability Map.empty drivetrain

    let withEndgameCapabilities endgameCapability robot =
        { robot with
            EndgameCapable = endgameCapability }

    let withDrivetrain drivetrain robot = { robot with Drivetrain = drivetrain }

    let addNote noteId userId noteContents robot = validation {
        let! note = Note.create userId noteContents
        
        return
            { robot with
                Robot.Notes = robot.Notes.Add(noteId, note) }
    }

    let removeNote noteId robot =
        { robot with
            Robot.Notes = robot.Notes.Remove(noteId) }

    let addPitScoutingParameter parameterId value robot =
