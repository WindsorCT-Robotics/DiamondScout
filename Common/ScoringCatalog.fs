namespace ParagonRobotics.DiamondScout.Common.ScoringCatalogs

open System
open FsToolkit.ErrorHandling
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
    member this.Handle(
        handleAlreadyOpenCatalog: Action,
        handleUnopenedCatalog: Action,
        handleMissingGamePiece: Action<GamePieceId>,
        handleMissingTimeframe: Action<GamePieceId, TimeframeId>) =
            match this with
            | ScoringCatalogAlreadyOpen -> handleAlreadyOpenCatalog.Invoke()
            | ScoringCatalogNotOpen -> handleUnopenedCatalog.Invoke()
            | GamePieceNotFound gamePiece -> handleMissingGamePiece.Invoke(gamePiece)
            | TimeframeNotFound (gamePiece, timeframe) -> handleMissingTimeframe.Invoke(gamePiece, timeframe)

[<AutoOpen>]
module Functional =
    [<RequireQualifiedAccess>]
    module ScoringCatalog =
        [<RequireQualifiedAccess>]
        module OnlyIf =
            let catalogOpen catalog =
                match catalog with
                | ScoringCatalog.Closed -> Validation.ok catalog
                | ScoringCatalog.Open _ -> ScoringCatalogAlreadyOpen |> Validation.error
                
            let catalogClosed catalog =
                match catalog with
                | ScoringCatalog.Closed -> ScoringCatalogAlreadyOpen |> Validation.error
                | ScoringCatalog.Open data -> Validation.ok data
                
            let gamePieceExists data (gamePiece: GamePieceId) =
                match Map.containsKey gamePiece data with
                | true -> Validation.ok data
                | false -> GamePieceNotFound gamePiece |> Validation.error
                
            let timeframeExists data gamePiece (timeframe: TimeframeId) =
                match data |> Map.find gamePiece |> Map.containsKey timeframe with
                | true -> Validation.ok data
                | false -> TimeframeNotFound (gamePiece, timeframe) |> Validation.error
                
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
                    let! data 
                }