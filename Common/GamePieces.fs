namespace ParagonRobotics.DiamondScout.Common

type GamePiece =
    { Name: string
      Value: SubPhaseMap<ScoreValue option>
      RankPoints: RankingPointGrant list }

module GamePiece =
    let create name values rankPoints =
        { Name = name
          Value = values
          RankPoints = rankPoints }

    let changeName piece name = { piece with GamePiece.Name = name }
    let changeValue piece value = { piece with GamePiece.Value = value }
    let changeRankPoints piece rp = { piece with RankPoints = rp }