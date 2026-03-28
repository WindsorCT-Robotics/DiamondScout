namespace ParagonRobotics.DiamondScout.Common.Functional

open System
open ParagonRobotics.DiamondScout.Common

[<Struct>]
type Alliance =
    | Red
    | Blue

    member this.Match(redAction: Action, blueAction: Action) =
        match this with
        | Red -> redAction
        | Blue -> blueAction

[<Struct>]
[<RequireQualifiedAccess>]
type EndgameResult =
    | Success of ScoringTier
    | Failure
    | NotAttempted

    member this.Match(successAction: Action<ScoringTier>, failureAction: Action, notAttemptedAction: Action) =
        match this with
        | Success tier -> successAction.Invoke tier
        | Failure -> failureAction.Invoke()
        | NotAttempted -> notAttemptedAction.Invoke()

[<Struct>]
type BotStrategy =
    | Offense
    | Defense
    | Both

    member this.Match(offenseAction: Action, defenseAction: Action, bothAction: Action) =
        match this with
        | Offense -> offenseAction.Invoke()
        | Defense -> defenseAction.Invoke()
        | Both -> bothAction.Invoke()

type Endgame =
    { Capable: EndgameCapable
      Result: EndgameResult }

[<Struct>]
type Breakdown =
    | AutoEmergency
    | Emergency
    | Malfunction

    member this.Match(autoEmergencyAction: Action, emergencyAction: Action, malfunctionAction: Action) =
        match this with
        | AutoEmergency -> autoEmergencyAction.Invoke()
        | Emergency -> emergencyAction.Invoke()
        | Malfunction -> malfunctionAction.Invoke()

type ScoreRecord =
    { GamePiece: GamePiece
      Tier: ScoringTier
      Phase: SubPhaseId }

    static member create gamePiece tier phase =
        { GamePiece = gamePiece
          Tier = tier
          Phase = phase }

type ScoutingData =
    { Team: TeamId
      Alliance: Alliance
      Scores: ScoreRecord list
      Endgame: Endgame
      Breakdowns: Breakdown list
      Infractions: Infractions
      Notes: Map<NoteId, Note> }

[<RequireQualifiedAccess>]
type ScoutingResult =
    | Scouted of ScoutingData
    | NotScouted

[<RequireQualifiedAccess>]
module ScoutingResult =
    let create team alliance endgameCapability =
        { Team = team
          Alliance = alliance
          Scores = []
          Endgame =
            { Capable = endgameCapability
              Result = EndgameResult.NotAttempted }
          Breakdowns = []
          Infractions = Infractions.Empty
          Notes = Map.empty }
        |> ScoutingResult.Scouted

    let recordScore phase gamePiece tier matchData =
        { matchData with
            Scores =
                let newScore = ScoreRecord.create gamePiece tier phase
                newScore :: matchData.Scores }

    let recordBreakdown breakdownData matchData =
        { matchData with
            Breakdowns =
                breakdownData :: matchData.Breakdowns }

    let recordInfraction infractionId infractionData matchData =
        { matchData with
            Infractions = matchData.Infractions.Add(infractionId, infractionData) }

    let recordEndgame endgameData matchData =
        { matchData with
            ScoutingData.Endgame.Result = endgameData }

    let addNote noteId note matchData =
        { matchData with
            ScoutingData.Notes = matchData.Notes.Add(noteId, note) }

    let removeNote noteId matchData =
        { matchData with
            ScoutingData.Notes = matchData.Notes.Remove noteId }
