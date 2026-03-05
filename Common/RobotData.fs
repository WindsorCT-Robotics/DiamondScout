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

    member this.Match(swerveAction : Action, mechAction : Action, tankAction : Action, otherAction : Action<string>) =
        match this with
        | Swerve -> swerveAction.Invoke()
        | Mech -> mechAction.Invoke()
        | Tank -> tankAction.Invoke()
        | Other name -> otherAction.Invoke(name)

[<Struct>]
type EndgameCapable =
    | TierCapability of ScoringTier
    | NotCapable
    member this.Match(tierCapable : Action<ScoringTier>, notCapable : Action) =
        match this with
        | TierCapability tier -> tierCapable.Invoke(tier)
        | NotCapable -> notCapable.Invoke()

type Robot =
    { Name: string
      Team: TeamId
      Game: GameId
      EndgameCapable: EndgameCapable
      Drivetrain: Drivetrain
      Notes: Note list }
    static member Create name team game scoringTier drivetrain = { Name = name; Team = team; Game = game; EndgameCapable = TierCapability scoringTier; Drivetrain = drivetrain; Notes = [] }
    member this.AssociateTeam team = { this with Team = team }
    member this.AssociateGame game = { this with Game = game }
    member this.SetEndgameCapabilities endgameCapability = { this with EndgameCapable = endgameCapability }
    member this.SetDrivetrain drivetrain = { this with Drivetrain = drivetrain }
    member this.AddNote note = { this with Notes = note :: this.Notes }

module Robot =
    let create name team game scoringTier drivetrain =
        { Name = name
          Team = team
          Game = game
          EndgameCapable = TierCapability scoringTier
          Drivetrain = drivetrain
          Notes = [] }

    let associateTeam robot team = { robot with Team = team }
    let associateGame robot game = { robot with Game = game }

    let setEndgameCapabilities robot endgameCapability =
        { robot with
            EndgameCapable = endgameCapability }

    let setDrivetrain robot drivetrain = { robot with Drivetrain = drivetrain }

    let addNote robot note =
        { robot with
            Robot.Notes = note :: robot.Notes }
