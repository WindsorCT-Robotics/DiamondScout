module ParagonRobotics.DiamondScout.Common.Remoting

open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common.Users
open ParagonRobotics.DiamondScout.Common.Teams
open ParagonRobotics.DiamondScout.Common.Notes

type IUser =
    { register: UserId -> UserName -> Role -> AsyncValidation<UserId, Users.Events.Error>
      changeName: UserId -> UserName -> AsyncValidation<Unit, Users.Events.Error>
      changeRole: UserId -> Role -> AsyncValidation<Unit, Users.Events.Error>
      deactivate: UserId -> AsyncValidation<Unit, Users.Events.Error>
      allUsers: Async<List<UserId>>
      userById: UserId -> AsyncValidation<User, Users.Events.Error> }

type ITeams =
    { register: TeamId -> TeamNumber -> TeamName -> AsyncValidation<TeamId, Teams.Events.Error>
      changeName: TeamId -> TeamName -> AsyncValidation<Unit, Teams.Events.Error>
      addNote: TeamId -> NoteId -> UserId -> NoteContent -> AsyncValidation<Unit, Notes.Events.Error>
      removeNote: TeamId -> NoteId -> AsyncValidation<Unit, Notes.Events.Error>
      allTeams: Async<List<TeamId>>
      teamById: TeamId -> AsyncValidation<Team, Teams.Events.Error> }
