module ParagonRobotics.DiamondScout.Common.Remoting

open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common.Functional

type IUser =
    { register: UserId -> UserName -> Role -> AsyncValidation<UserId, UserData.Error>
      changeName: UserId -> UserName -> AsyncValidation<Unit, UserData.Error>
      changeRole: UserId -> Role -> AsyncValidation<Unit, UserData.Error>
      deactivate: UserId -> AsyncValidation<Unit, UserData.Error>
      allUsers: Async<List<UserId>>
      userById: UserId -> AsyncValidation<User, UserData.Error> }

type ITeams =
    { register: TeamId -> TeamNumber -> TeamName -> AsyncValidation<TeamId, Team.Error>
      changeName: TeamId -> TeamName -> AsyncValidation<Unit, Team.Error>
      addNote: TeamId -> NoteId -> UserId -> NoteContent -> AsyncValidation<Unit, Note.Error>
      removeNote: TeamId -> NoteId -> AsyncValidation<Unit, Note.Error>
      allTeams: Async<List<TeamId>>
      teamById: TeamId -> AsyncValidation<Team, Team.Error> }
