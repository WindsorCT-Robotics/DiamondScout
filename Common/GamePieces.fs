namespace ParagonRobotics.DiamondScout.Common

open System.Collections.Generic

type GamePiece =
    { Name: string
      PhaseScore: SubPhaseMap<ScoreValue>
      RankPoints: RankingPointGrant list }

    static member Create name values rankPoints =
        { Name = name
          PhaseScore = values
          RankPoints = rankPoints }

[<RequireQualifiedAccess>]
module GamePiece =
    let create name values rankPoints =
        { Name = name
          PhaseScore = values
          RankPoints = rankPoints }

    let changeName name piece = { piece with GamePiece.Name = name }

    let changeValue value piece =
        { piece with
            GamePiece.PhaseScore = value }

    let changeRankPoints rp piece = { piece with RankPoints = rp }

type GamePiece with
    member this.ChangeName name = GamePiece.changeName name this
    member this.ChangeValue value = GamePiece.changeValue value this
    member this.ChangeRankPoints rp = GamePiece.changeRankPoints rp this
