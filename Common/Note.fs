namespace ParagonRobotics.DiamondScout.Common

type Note =
    { UserId: UserId
      Text: string }

    /// Creates a new note.
    static member Create(userId, text) = { UserId = userId; Text = text }
    /// Returns a new Note with the modified text.
    member this.Edit(text) = { this with Text = text }

module Note =
    let create userId text = Note.Create(userId, text)
    let edit note text = { note with Text = text }
