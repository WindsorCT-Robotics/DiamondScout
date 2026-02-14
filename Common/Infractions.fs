module ParagonRobotics.DiamondScout.Common.Infractions

[<Struct>]
type Foul =
    | Minor of Penalty: uint
    | Major of Penalty: uint

[<Struct>]
type Card =
    | Yellow
    | Red

type Infraction =
    { Name: string
      Severity: Foul option
      Card: Card option }
