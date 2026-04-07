namespace ParagonRobotics.DiamondScout.Common.Functional

open System
open FsToolkit.ErrorHandling
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
    | Finalized of ScoutingData
    | Scouting of ScoutingData
    | NotStarted

[<RequireQualifiedAccess>]
module ScoutingResult =
    type Error =
        | ScoutingNotStarted
        | ScoutingResultFinalized
        | ScoutingInProgress

    let startScouting team alliance endgameCapability  scoutingResult= 
        match scoutingResult with
        | ScoutingResult.NotStarted ->
            { Team = team
              Alliance = alliance
              Scores = []
              Endgame =
                { Capable = endgameCapability
                  Result = EndgameResult.NotAttempted }
              Breakdowns = []
              Infractions = Infractions.Empty
              Notes = Map.empty }
            |> ScoutingResult.Scouting
            |> Validation.ok
        | ScoutingResult.Scouting _ -> Error.ScoutingInProgress |> Validation.error
        | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error

    let recordScore phase gamePiece tier scoutingResult =
        match scoutingResult with
        | ScoutingResult.Scouting scoutingData ->
            { scoutingData with
                Scores =
                    let newScore = ScoreRecord.create gamePiece tier phase
                    newScore :: scoutingData.Scores }
            |> ScoutingResult.Scouting
            |> Validation.ok
        | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error
        | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error

    let recordBreakdown breakdownData scoutingResult =
        match scoutingResult with
        | ScoutingResult.Scouting scoutingData ->
            { scoutingData with
                Breakdowns =
                    breakdownData :: scoutingData.Breakdowns }
            |> ScoutingResult.Scouting
            |> Validation.ok
        | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error
        | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error

    let recordInfraction infractionId infractionData scoutingResult =
        match scoutingResult with
        | ScoutingResult.Scouting matchData ->
            { matchData with Infractions = matchData.Infractions.Add(infractionId, infractionData) }
            |> ScoutingResult.Scouting
            |> Validation.ok
        | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error
        | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error

    let recordEndgame endgameData scoutingResult =
        match scoutingResult with
        | ScoutingResult.Scouting scoutingData ->
            { scoutingData with ScoutingData.Endgame.Result = endgameData }
            |> ScoutingResult.Scouting
            |> Validation.ok
        | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error
        | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error

    let addNote noteId note scoringResult =
        match scoringResult with
        | ScoutingResult.Scouting scoutingData ->
            { scoutingData with ScoutingData.Notes = scoutingData.Notes.Add(noteId, note) }
            |> ScoutingResult.Scouting
            |> Validation.ok
        | ScoutingResult.Finalized scoutingData -> 
            { scoutingData with ScoutingData.Notes = scoutingData.Notes.Add(noteId, note) }
            |> ScoutingResult.Finalized
            |> Validation.ok
        | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error

    let removeNote noteId scoringResult =
        match scoringResult with
        | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error
        | ScoutingResult.Scouting matchData ->
            { matchData with
                ScoutingData.Notes = matchData.Notes.Remove noteId }
            |> ScoutingResult.Scouting
            |> Validation.ok
        | ScoutingResult.Finalized matchData ->
            { matchData with
                ScoutingData.Notes = matchData.Notes.Remove noteId }
            |> ScoutingResult.Finalized
            |> Validation.ok

    let mustBeFinalized scoutingResult =
        match scoutingResult with
        | ScoutingResult.Finalized _ as scoutingResult -> scoutingResult |> Validation.ok
        | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error
        | ScoutingResult.Scouting _ -> Error.ScoutingInProgress |> Validation.error
        
    let mustBeInProgress scoutingResult =
        match scoutingResult with
        | ScoutingResult.Scouting _ as scoutingResult -> scoutingResult |> Validation.ok
        | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error
        | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error
        
    let mustBeNotStarted scoutingResult =
        match scoutingResult with
        | ScoutingResult.NotStarted as scoutingResult -> scoutingResult |> Validation.ok
        | ScoutingResult.Scouting _ -> Error.ScoutingInProgress |> Validation.error
        | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error