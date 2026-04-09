namespace ParagonRobotics.DiamondScout.Common.Functional

open FsToolkit.ErrorHandling

[<Struct>]
type GamePieceName = GamePieceName of string

type GamePiece =
    private
        { Name: GamePieceName
          SubPhaseScoreValues: SubPhaseMap<ScoreValue>
          RankingPointGrants: RankingPointGrant list }

[<RequireQualifiedAccess>]
module GamePiece =
    type Error = | GamePieceNameEmpty

    module private OnlyIf =
        let nameNotEmpty (GamePieceName name as gamePieceName) =
            match System.String.IsNullOrWhiteSpace name with
            | true -> GamePieceNameEmpty |> Validation.error
            | false -> gamePieceName |> Validation.ok

    let create name values rankPoints =
        validation {
            let! name = OnlyIf.nameNotEmpty name

            return
                { Name = name
                  SubPhaseScoreValues = values
                  RankingPointGrants = rankPoints }
        }

    let withName name piece =
        validation {
            let! name = OnlyIf.nameNotEmpty name
            return { piece with GamePiece.Name = name }
        }

    let changeValue value piece =
        { piece with
            GamePiece.SubPhaseScoreValues = value }

    let changeRankPoints rp piece = { piece with RankingPointGrants = rp }
