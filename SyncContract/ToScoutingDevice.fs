namespace ParagonRobotics.DiamondScout.SyncContract.ToScoutingDevice

// TODO: Consider loading all day's match data into scouting tablet in a single scan if data will fit
// TODO: Consider separating loading of match data from game data to minimize QR code size

open System.Collections.Generic
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.DomainEvents

type ScoutedTeam =
    { Team: Team
      Robot: struct (RobotId * Robot)
      ScoutingParameters: IReadOnlyDictionary<ParameterDefinitionId, struct (ParameterDefinition * ParameterValue)>
      ScoutResults: ScoutingResults }
      
type ScoutedMatch =
    {
        MatchNumber: MatchNumber
        Teams: IReadOnlyDictionary<TeamId, ScoutedTeam>
    }

type ScoutingDataModel =
    { GameName: string
      Users: IReadOnlyDictionary<UserId, User>
      Parameters: RobotParameters IReadOnlyCollection
      Phases: IReadOnlyDictionary<SubPhaseId, SubPhase>
      GamePieces: IReadOnlyDictionary<GamePieceId, GamePiece>
      Infractions: IReadOnlyDictionary<InfractionId, Infraction>
      Event: struct (FrcEventId * string) }
      
type CompleteDataModel =
    {
        ScoutingData: ScoutingDataModel
        Match: ScoutedMatch
    }
      
type EventStores = {
    UserStore: IEventStore<User.Event>
    NoteStore: IEventStore<Note.Event>
    TeamStore: IEventStore<Team.Event>
    PhaseStore: IEventStore<SubPhase.Event>
    GamePieceStore: IEventStore<GamePiece.Event>
    InfractionStore: IEventStore<Infraction.Event>
    RobotParameterDefinitionStore: IEventStore<ParameterDefinition.Event>
    RobotParameterStore: IEventStore<ParameterValue.Event>
    RobotStore: IEventStore<Robot.Event>
    MatchStore: IEventStore<Match.Event>
    EventsStore: IEventStore<FrcEvent.Event>
}
