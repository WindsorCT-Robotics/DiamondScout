namespace ParagonRobotics.DiamondScout.SyncContract.ToScoutingDevice

// TODO: Consider loading all day's match data into scouting tablet in a single scan if data will fit
// TODO: Consider separating loading of match data from game data to minimize QR code size

open System.Collections.Generic
open ParagonRobotics.DiamondScout.Common
open ParagonRobotics.DiamondScout.Common.DomainEvents
open ZstdNet
open System.Text.Json

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

type ScoutingDataError =
    | DataTooLarge of data: byte[]
    
type ScoutingDeviceDataGenerator(gameId: GameId, matchId: MatchId, eventStores: EventStores) =
    static member private Compress data =
        use compressorOptions = new CompressionOptions(CompressionOptions.MaxCompressionLevel)
        use compressor = new Compressor(compressorOptions)
        let jsonOptions = JsonSerializerOptions()
        jsonOptions.WriteIndented <- false
        
        JsonSerializer.SerializeToUtf8Bytes(data, jsonOptions)
        |> compressor.Wrap
        |> function
            | data when data.Length > 2953 -> data |> DataTooLarge |> Error
            | data -> data |> Ok
            
    member this.GenerateGameData() =
        
    member this.GenerateMatchData() =
        
    member this.GenerateCompleteData() =