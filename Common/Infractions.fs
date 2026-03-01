namespace ParagonRobotics.DiamondScout.Common

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

module Infraction =
    let create card severity name =
        { Name = name
          Severity = severity
          Card = card }

    let changeName infraction name =
        { infraction with
            Infraction.Name = name }

    let changeSeverity infraction severity = { infraction with Severity = severity }
    let changeCard infraction card = { infraction with Card = card }
