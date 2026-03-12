namespace ParagonRobotics.DiamondScout.Common

open System.Collections.Generic

type GamePiece =
    { Name: string
      PhaseScore: SubPhaseMap<ScoreValue>
      RankPoints: RankingPointGrant list }

    static member Create name (values: IReadOnlyDictionary<SubPhase, ScoreValue>) rankPoints =
        { Name = name
          PhaseScore =
            match values with
            | :? Map<SubPhase, ScoreValue> as m -> m
            | d -> d |> Seq.map (|KeyValue|) |> Map.ofSeq
          RankPoints = rankPoints }

    member this.ChangeName name = { this with Name = name }
    member this.ChangeValue value = { this with PhaseScore = value }
    member this.ChangeRankPoints rp = { this with RankPoints = rp }

[<RequireQualifiedAccess>]
module GamePiece =
    let create name values rankPoints =
        { Name = name
          PhaseScore = values
          RankPoints = rankPoints }

    let changeName piece name = { piece with GamePiece.Name = name }

    let changeValue piece value =
        { piece with
            GamePiece.PhaseScore = value }

    let changeRankPoints piece rp = { piece with RankPoints = rp }
