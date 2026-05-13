namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type GamePieceScoringRuleId =
    private
    | GamePieceScoringRuleId of Guid

    static member Zero = GamePieceScoringRuleId Guid.Empty

    static member Create() =
        Guid.CreateVersion7() |> GamePieceScoringRuleId

    member this.Value = let (GamePieceScoringRuleId guid) = this in guid

type GamePieceScoringRule =
    { GamePieceId: GamePieceId
      TimeframeId: TimeframeId
      ScoreValue: ScoreValue }
