module ParagonRobotics.DiamondScout.Common.Ranking

open ParagonRobotics.DiamondScout.Common.Scoring

[<Struct>]
type RankingPoints = RankingPoints of int

type RankingPointsThreshold =
    | PointsThreshold of Points
    | ScoreThreshold of int

type RankingPointGrant =
    { Value: RankingPoints
      Threshold: RankingPointsThreshold }
