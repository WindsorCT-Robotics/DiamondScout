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
      ScoutingParameters: Map<ParameterDefinitionId, ParameterValue>
      Endgame: Endgame
      Breakdowns: Breakdown list
      Infractions: Map<InfractionId, Infraction>
      Notes: NoteId list }

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
        | ParameterExists of ParameterDefinitionId
        | ParameterDoesNotExist of ParameterDefinitionId
        | DuplicateNote of noteId: NoteId
        | NoteNotFound of noteId: NoteId
        | InvalidTeamId of teamId: TeamId
        | InvalidInfractionId of infractionId: InfractionId
        | InvalidParameterId of parameterId: ParameterDefinitionId
        | InvalidNoteId of noteId: NoteId
        | InvalidSubPhaseId of subPhaseId: SubPhaseId

    [<RequireQualifiedAccess>]
    module private OnlyIf =
        let scoutingIsNotStarted scoutingResult =
            match scoutingResult with
            | ScoutingResult.NotStarted -> Validation.ok ()
            | ScoutingResult.Scouting _ -> Error.ScoutingInProgress |> Validation.error
            | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error

        let scoutingIsStarted scoutingResult =
            match scoutingResult with
            | ScoutingResult.Scouting data -> data |> Validation.ok
            | ScoutingResult.Finalized data -> data |> Validation.ok
            | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error

        let scoutingIsInProgress scoutingResult =
            match scoutingResult with
            | ScoutingResult.Scouting data -> data |> Validation.ok
            | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error
            | ScoutingResult.Finalized _ -> Error.ScoutingResultFinalized |> Validation.error

        let parameterDoesNotExist scoutingData parameterId =
            match scoutingData.ScoutingParameters.ContainsKey parameterId with
            | true -> ParameterExists parameterId |> Validation.error
            | false -> parameterId |> Validation.ok

        let parameterExists scoutingData parameterId =
            match scoutingData.ScoutingParameters.ContainsKey parameterId with
            | true -> parameterId |> Validation.ok
            | false -> ParameterDoesNotExist parameterId |> Validation.error

        let noteIdUnique scoutingData noteId =
            match List.contains noteId scoutingData.Notes with
            | true -> noteId |> DuplicateNote |> Validation.error
            | false -> noteId |> Validation.ok

        let noteExists scoutingData noteId =
            match List.contains noteId scoutingData.Notes with
            | true -> noteId |> Validation.ok
            | false -> noteId |> NoteNotFound |> Validation.error

        let teamIdValid teamId =
            match teamId = TeamId.Zero with
            | true -> teamId |> InvalidTeamId |> Validation.error
            | false -> teamId |> Validation.ok

        let infractionIdValid infractionId =
            match infractionId = InfractionId.Zero with
            | true -> infractionId |> InvalidInfractionId |> Validation.error
            | false -> infractionId |> Validation.ok

        let parameterIdValid parameterId =
            match parameterId = ParameterDefinitionId.Zero with
            | true -> parameterId |> InvalidParameterId |> Validation.error
            | false -> parameterId |> Validation.ok

        let noteIdValid noteId =
            match noteId = NoteId.Zero with
            | true -> noteId |> InvalidNoteId |> Validation.error
            | false -> noteId |> Validation.ok

        let subPhaseIdValid subPhaseId =
            match subPhaseId = SubPhaseId.Zero with
            | true -> subPhaseId |> InvalidSubPhaseId |> Validation.error
            | false -> subPhaseId |> Validation.ok

        let scoutingIsFinalized scoutingResult =
            match scoutingResult with
            | ScoutingResult.Finalized data -> data |> Validation.ok
            | ScoutingResult.Scouting _ -> Error.ScoutingInProgress |> Validation.error
            | ScoutingResult.NotStarted -> Error.ScoutingNotStarted |> Validation.error

    let startScouting team alliance endgameCapability scoutingResult =
        validation {
            do! OnlyIf.scoutingIsNotStarted scoutingResult
            let! team = OnlyIf.teamIdValid team

            return
                { Team = team
                  Alliance = alliance
                  Scores = []
                  Endgame = { Capable = endgameCapability; Result = EndgameResult.NotAttempted }
                  Breakdowns = []
                  ScoutingParameters = Map.empty
                  Infractions = Map.empty
                  Notes = [] }
                |> ScoutingResult.Scouting
        }

    let recordScore phase gamePiece tier scoutingResult =
        validation {
            let! scoutingData = OnlyIf.scoutingIsInProgress scoutingResult
            let! phase = OnlyIf.subPhaseIdValid phase

            return
                { scoutingData with
                    Scores =
                        let newScore = ScoreRecord.create gamePiece tier phase
                        newScore :: scoutingData.Scores }
                |> ScoutingResult.Scouting
        }

    let recordBreakdown breakdownData scoutingResult =
        validation {
            let! scoutingData = OnlyIf.scoutingIsInProgress scoutingResult

            return
                { scoutingData with
                    Breakdowns = breakdownData :: scoutingData.Breakdowns }
                |> ScoutingResult.Scouting
        }

    let recordInfraction infractionId infractionData scoutingResult =
        validation {
            let! scoutingData = OnlyIf.scoutingIsInProgress scoutingResult
            let! infractionId = OnlyIf.infractionIdValid infractionId

            return
                { scoutingData with
                    Infractions = scoutingData.Infractions.Add(infractionId, infractionData) }
                |> ScoutingResult.Scouting
        }

    let recordEndgame endgameData scoutingResult =
        validation {
            let! scoutingData = OnlyIf.scoutingIsInProgress scoutingResult

            return
                { scoutingData with
                    ScoutingData.Endgame.Result = endgameData }
                |> ScoutingResult.Scouting
        }

    let addNote noteId scoutingResult =
        validation {
            let! scoutingData = OnlyIf.scoutingIsStarted scoutingResult

            let updatedData =
                { scoutingData with
                    Notes = noteId :: scoutingData.Notes }

            return
                match scoutingResult with
                | ScoutingResult.Scouting _ -> ScoutingResult.Scouting updatedData
                | ScoutingResult.Finalized _ -> ScoutingResult.Finalized updatedData
                | _ -> scoutingResult
        }

    let removeNote noteId scoutingResult =
        validation {
            let! scoutingData = OnlyIf.scoutingIsStarted scoutingResult

            let updatedData =
                { scoutingData with
                    Notes = scoutingData.Notes |> List.filter (fun id -> id <> noteId) }

            return
                match scoutingResult with
                | ScoutingResult.Scouting _ -> ScoutingResult.Scouting updatedData
                | ScoutingResult.Finalized _ -> ScoutingResult.Finalized updatedData
                | _ -> scoutingResult
        }

    let setScoutingParameterValue parameterId value scoutingResult =
        validation {
            let! scoutingData = OnlyIf.scoutingIsInProgress scoutingResult
            let! parameterId = OnlyIf.parameterIdValid parameterId
            let! parameterId = OnlyIf.parameterDoesNotExist scoutingData parameterId

            return
                { scoutingData with
                    ScoutingParameters = scoutingData.ScoutingParameters.Add(parameterId, value) }
                |> ScoutingResult.Scouting
        }

    let removeScoutingParameterValue parameterId scoutingResult =
        validation {
            let! scoutingData = OnlyIf.scoutingIsInProgress scoutingResult
            let! parameterId = OnlyIf.parameterIdValid parameterId
            let! parameterId = OnlyIf.parameterExists scoutingData parameterId

            return
                { scoutingData with
                    ScoutingParameters = scoutingData.ScoutingParameters.Remove(parameterId) }
                |> ScoutingResult.Scouting
        }

    let mustBeFinalized scoutingResult =
        OnlyIf.scoutingIsFinalized scoutingResult
