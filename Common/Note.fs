namespace ParagonRobotics.DiamondScout.Common

type Note = { UserId: UserId; Text: string }

module Note =
    let create userId text = { UserId = userId; Text = text }
    let edit note text = { note with Text = text }
