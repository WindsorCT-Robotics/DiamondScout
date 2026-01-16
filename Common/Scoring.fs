module ParagonRobotics.DiamondScout.Common.Scoring

[<Struct>]
type ScoringTier = { Name: string; Level: int }

[<Struct>]
type Points =
    | Points of int
    static member Zero = Points 0

type Score =
    | FlatScore of Points
    | TieredScore of Map<ScoringTier, Points>
