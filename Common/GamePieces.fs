namespace ParagonRobotics.DiamondScout.Common.Functional

[<Struct>]
type GamePieceName = GamePieceName of string

type GamePiece =
    { Name: GamePieceName
      SubPhaseScoreValues: SubPhaseMap<ScoreValue>
      RankingPointGrants: RankingPointGrant list }

    static member Create name values rankPoints =
        { Name = name
          SubPhaseScoreValues = values
          RankingPointGrants = rankPoints }

[<RequireQualifiedAccess>]
module GamePiece =
    let create name values rankPoints =
        { Name = name
          SubPhaseScoreValues = values
          RankingPointGrants = rankPoints }

    let changeName name piece = { piece with GamePiece.Name = name }

    let changeValue value piece =
        { piece with
            GamePiece.SubPhaseScoreValues = value }

    let changeRankPoints rp piece = { piece with RankingPointGrants = rp }

type GamePiece with
    member this.ChangeName name = GamePiece.changeName name this
    member this.ChangeValue value = GamePiece.changeValue value this
    member this.ChangeRankPoints rp = GamePiece.changeRankPoints rp this
