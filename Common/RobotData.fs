namespace ParagonRobotics.DiamondScout.Common

open System

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
    { Name: string
      Team: TeamId
      EndgameCapable: EndgameCapable
      Drivetrain: Drivetrain
      Notes: Notes }

[<RequireQualifiedAccess>]
module Robot =
    let create name team scoringTier drivetrain =
        { Name = name
          Team = team
          EndgameCapable = TierCapability scoringTier
          Drivetrain = drivetrain
          Notes = Notes.Empty }

    let withEndgameCapabilities endgameCapability robot =
        { robot with
            EndgameCapable = endgameCapability }

    let withDrivetrain drivetrain robot = { robot with Drivetrain = drivetrain }

    let addNote noteId userId noteContents robot =
        { robot with
            Robot.Notes = robot.Notes.Add(noteId, Note.Create userId noteContents) }

    let removeNote noteId robot =
        { robot with
            Robot.Notes = robot.Notes.Remove(noteId) }

type Robot with
    static member Create name team scoringTier drivetrain =
        Robot.create name team scoringTier drivetrain

    member this.ChangeEndgameCapabilities endgameCapability =
        Robot.withEndgameCapabilities endgameCapability this

    member this.ChangeDrivetrain drivetrain = Robot.withDrivetrain drivetrain this

    member this.AddNote noteId userId noteContents =
        Robot.addNote noteId userId noteContents this
