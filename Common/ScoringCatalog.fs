namespace ParagonRobotics.DiamondScout.Common.ScoringCatalogs

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.GamePieces
open ParagonRobotics.DiamondScout.Common.Periods
open ParagonRobotics.DiamondScout.Common.Scoring

[<Struct>]
type ScoringCatalogId =
    private
    | ScoringCatalogId of Guid

    static member Zero = ScoringCatalogId Guid.Empty

    static member Create() =
        Guid.CreateVersion7() |> ScoringCatalogId

    member this.Value = let (ScoringCatalogId guid) = this in guid

[<RequireQualifiedAccess>]
type ScoringCatalog =
    private
    | Closed
    | Open of Map<GamePieceId, Map<TimeframeId, ScoreValue>>

type Error =
    | ScoringCatalogAlreadyOpen
    | ScoringCatalogNotOpen
    | GamePieceNotFound of gamePiece: GamePieceId
    | TimeframeNotFound of struct (GamePieceId * TimeframeId)
    | ValueExistsForTimeframe of struct (GamePieceId * TimeframeId * ScoreValue)
    | ValueDoesNotExistForTimeframe of struct (GamePieceId * TimeframeId)

    member this.Handle
        (
            handleAlreadyOpenCatalog: Action,
            handleUnopenedCatalog: Action,
            handleMissingGamePiece: Action<GamePieceId>,
            handleMissingTimeframe: Action<GamePieceId, TimeframeId>,
            handleNonExistentValue: Action<GamePieceId, TimeframeId>,
            handleAlreadyExistingValue: Action<GamePieceId, TimeframeId, ScoreValue>
        ) =
        match this with
        | ScoringCatalogAlreadyOpen -> handleAlreadyOpenCatalog.Invoke()
        | ScoringCatalogNotOpen -> handleUnopenedCatalog.Invoke()
        | GamePieceNotFound gamePiece -> handleMissingGamePiece.Invoke(gamePiece)
        | TimeframeNotFound(gamePiece, timeframe) -> handleMissingTimeframe.Invoke(gamePiece, timeframe)
        | ValueExistsForTimeframe(gamePiece, timeframe, value) ->
            handleAlreadyExistingValue.Invoke(gamePiece, timeframe, value)
        | ValueDoesNotExistForTimeframe(gamePiece, timeframe) -> handleNonExistentValue.Invoke(gamePiece, timeframe)

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module ScoringCatalog =
        [<RequireQualifiedAccess>]
        module OnlyIf =
            let catalogClosed catalog =
                match catalog with
                | ScoringCatalog.Closed -> Validation.ok catalog
                | ScoringCatalog.Open _ -> ScoringCatalogAlreadyOpen |> Validation.error

            let catalogOpen catalog =
                match catalog with
                | ScoringCatalog.Closed -> ScoringCatalogNotOpen |> Validation.error
                | ScoringCatalog.Open data -> Validation.ok data

            let gamePieceExists data (gamePiece: GamePieceId) =
                match Map.containsKey gamePiece data with
                | true -> Validation.ok data
                | false -> GamePieceNotFound gamePiece |> Validation.error

            let timeframeExists data gamePiece (timeframe: TimeframeId) =
                match data |> Map.find gamePiece |> Map.containsKey timeframe with
                | true -> Validation.ok data
                | false -> TimeframeNotFound(gamePiece, timeframe) |> Validation.error

            let valueDoesNotExist data gamePiece timeframe =
                match data |> Map.find gamePiece |> Map.containsKey timeframe with
                | true ->
                    ValueExistsForTimeframe(gamePiece, timeframe, data |> Map.find gamePiece |> Map.find timeframe)
                    |> Validation.error
                | false -> Validation.ok data

            let valueExists data gamePiece timeframe =
                match data |> Map.find gamePiece |> Map.containsKey timeframe with
                | true -> Validation.ok data
                | false -> ValueDoesNotExistForTimeframe(gamePiece, timeframe) |> Validation.error

        [<RequireQualifiedAccess>]
        module ScoringCatalog =
            let ``open`` catalog =
                validation {
                    let! _ = OnlyIf.catalogClosed catalog

                    return ScoringCatalog.Open Map.empty
                }

            let addValue gamePiece timeFrame scoringValue catalog =
                validation {
                    let! data = OnlyIf.catalogOpen catalog
                    let! data = OnlyIf.valueDoesNotExist data gamePiece timeFrame

                    return
                        data
                        |> Map.add gamePiece (data |> Map.find gamePiece |> Map.add timeFrame scoringValue)
                        |> ScoringCatalog.Open
                }

            let removeValue gamePiece timeFrame catalog =
                validation {
                    let! data = OnlyIf.catalogOpen catalog
                    let! data = OnlyIf.gamePieceExists data gamePiece
                    let! data = OnlyIf.timeframeExists data gamePiece timeFrame

                    return
                        data
                        |> Map.change gamePiece (Option.map (Map.remove timeFrame))
                        |> ScoringCatalog.Open
                }

            let changeValue gamePiece timeFrame scoringValue catalog =
                validation {
                    let! data = OnlyIf.catalogOpen catalog
                    let! data = OnlyIf.gamePieceExists data gamePiece
                    let! data = OnlyIf.timeframeExists data gamePiece timeFrame
                    let! data = OnlyIf.valueExists data gamePiece timeFrame

                    return
                        data
                        |> Map.change gamePiece (Option.map (Map.add timeFrame scoringValue))
                        |> ScoringCatalog.Open
                }

type ScoringCatalog with
    member this.Values: IReadOnlyDictionary<GamePieceId, IReadOnlyDictionary<TimeframeId, ScoreValue>> | null =
        match this with
        | ScoringCatalog.Open data ->
            data
            |> Map.map (fun _ tf -> tf.ToReadOnlyDictionary())
            |> _.ToReadOnlyDictionary()
        | ScoringCatalog.Closed -> null
