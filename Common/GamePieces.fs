module ParagonRobotics.DiamondScout.Common.GamePieces

open ParagonRobotics.DiamondScout.Common.Phase
open ParagonRobotics.DiamondScout.Common.Ranking
open ParagonRobotics.DiamondScout.Common.Scoring

type GamePiece =
    { Name: string
      Value: PhaseMap<Score option>
      RankPoints: RankingPointGrant list }
