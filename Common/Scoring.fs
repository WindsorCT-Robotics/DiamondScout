module ParagonRobotics.DiamondScout.Common.Scoring

/// Defines a scoring tier.
[<Struct>]
type ScoringTier = {
    /// The name of the scoring tier.
    Name: string
    /// The level of the scoring tier.
    Level: int
}

/// <summary>The number of points a <see cref="T:ParagonRobotics.DiamondScout.Common.Scoring.Score"/> is worth.</summary>
[<Struct>]
type Points =
    /// <summary>
    /// Creates a new <see cref="T:ParagonRobotics.DiamondScout.Common.Scoring.Points"/> instance.
    /// </summary>
    /// <param name="points">The number of points.</param>
    | Points of points: int
    /// Represents zero points.
    static member Zero = Points 0

///<summary>
/// Represents completing a game's objective and the value of the objective in <see cref="T:ParagonRobotics.DiamondScout.Common.Scoring.Points"/>.
/// </summary>
type Score =
    /// A score that is not tiered.
    | FlatScore of points: Points
    /// <summary>A score that rewards points relative to a <see cref="T:ParagonRobotics.DiamondScout.Common.Scoring.ScoringTier"/>.</summary>
    | TieredScore of Map<ScoringTier, Points>
