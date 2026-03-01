namespace ParagonRobotics.DiamondScout.Common

type GamePiece =
    { Name: string
      PhaseScore: SubPhaseMap<ScoreValue>
      RankPoints: RankingPointGrant list }

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
