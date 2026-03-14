namespace ParagonRobotics.DiamondScout.Common

open System

/// Represents FIRST Robotics Competition ranking points.
[<Struct>]
type RankingPoints =
    /// <summary>Creates a <see cref="T:ParagonRobotics.DiamondScout.Common.Ranking.RankingPoints"/> value.</summary>
    /// <param name="rankingPoints">The number of ranking points.</param>
    | RankingPoints of rankingPoints: uint

    /// Represents zero ranking points.
    static member Zero = RankingPoints 0u
    static member (+)(RankingPoints left, RankingPoints right) = left + right |> RankingPoints
    member this.Value = let (RankingPoints rankingPoints) = this in rankingPoints

/// The requirements for a team to receive ranking points.
type RankingPointsThreshold =
    /// Creates a <see cref="T:ParagonRobotics.DiamondScout.Common.Ranking.RankingPointsThreshold"/> based on scoring the specified number of <see cref="T:ParagonRobotics.DiamondScout.Common.Scoring.Points"/>.
    | PointsThreshold of threshold: Points
    /// Creates a <see cref="T:ParagonRobotics.DiamondScout.Common.Ranking.RankingPointsThreshold"/> based on scoring the specified number of times, regardless of point value.
    | ScoreThreshold of threshold: uint

    member this.Match(pointsThresholdAction: Action<Points>, scoreThresholdAction: Action<uint>) =
        match this with
        | PointsThreshold points -> pointsThresholdAction.Invoke points
        | ScoreThreshold score -> scoreThresholdAction.Invoke score

/// The number of <see cref="T:ParagonRobotics.DiamondScout.Common.Ranking.RankingPoints"/> a team receives for meeting the specified <see cref="T:ParagonRobotics.DiamondScout.Common.Ranking.RankingPointsThreshold"/>.
type RankingPointGrant =
    {
        /// <summary>The number of <see cref="T:ParagonRobotics.DiamondScout.Common.Ranking.RankingPoints"/> granted when meeting the <see cref="T:ParagonRobotics.DiamondScout.Common.Ranking.RankingPointsThreshold"/> criteria.</summary>
        Value: RankingPoints
        /// <summary>The <see cref="T:ParagonRobotics.DiamondScout.Common.Ranking.RankingPointsThreshold"/> criteria.</summary>
        Threshold: RankingPointsThreshold
    }

[<RequireQualifiedAccess>]
module Ranking =
    let create value threshold =
        { Value = value; Threshold = threshold }

    let changeValue value rankingPointGrant =
        { rankingPointGrant with Value = value }

    let changeThreshold threshold rankingPointGrant =
        { rankingPointGrant with
            Threshold = threshold }

type RankingPointGrant with
    member this.ChangeValue value = Ranking.changeValue value this
    member this.ChangeThreshold threshold = Ranking.changeThreshold threshold this