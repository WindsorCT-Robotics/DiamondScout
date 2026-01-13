module ParagonRobotics.DiamondScout.Common.Infractions

[<Struct>]
type Foul =
    | Minor
    | Major

[<Struct>]
type Card =
    | Yellow
    | Red

type Infraction =
    { Name: string
      Severity: Foul option
      Card: Card option }