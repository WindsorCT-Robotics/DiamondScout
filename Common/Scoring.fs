namespace ParagonRobotics.DiamondScout.Common

/// Defines a scoring tier.
[<Struct>]
type ScoringTier =
    {
        /// The name of the scoring tier.
        Name: string
        /// The level of the scoring tier.
        Level: uint
    }

type QualitativeScoring =
    | Poor
    | BelowAverage
    | Average
    | AboveAverage
    | Excellent
    
/// <summary>The number of points a <see cref="T:ParagonRobotics.DiamondScout.Common.Scoring.Score"/> is worth.</summary>
[<Struct>]
type Points =
    /// <summary>
    /// Creates a new <see cref="T:ParagonRobotics.DiamondScout.Common.Scoring.Points"/> instance.
    /// </summary>
    /// <param name="points">The number of points.</param>
    | Points of points: uint

    /// Represents zero points.
    static member Zero = Points 0u
    static member (+)(Points left, Points right) = left + right |> Points

/// A serializable/config-friendly representation of a score.
type ScoreValue =
    | Flat of Points
    | ByTier of Map<ScoringTier, Points>
    | Qualitative of QualitativeScoring

/// A scoring value that can be evaluated given a ScoringTier.
type Score =
    private
    | Score of (ScoringTier -> Points)

module Score =
    let getPoints tier (Score score) : Points =
        score tier

    let compile (rule: ScoreValue) : Score =
        match rule with
        | Flat points ->
            Score(fun _tier -> points)

        | ByTier tiers ->
            Score(fun tier ->
                match Map.tryFind tier tiers with
                | Some pts -> pts
                | None ->
                    invalidArg (nameof tier) $"Tier '{tier.Name}' (Level {tier.Level}) was not defined for this score.")
        
        | Qualitative qualitative ->
            Score(fun _tier ->
                match qualitative with
                | Poor -> Points 1u
                | BelowAverage -> Points 2u
                | Average -> Points 3u
                | AboveAverage -> Points 4u
                | Excellent -> Points 5u)