namespace ParagonRobotics.DiamondScout.Common.Functional

open System
open FsToolkit.ErrorHandling

[<Struct>]
type TimeframeName = TimeframeName of string

[<Struct>]
type TimeframeDuration = TimeframeDuration of TimeSpan

[<Struct>]
type TimeframeId =
    | TimeframeId of Guid
    static member Create() = Guid.CreateVersion7() |> TimeframeId
    member this.Value = let (TimeframeId guid) = this in guid
    static member Zero = TimeframeId Guid.Empty

type Timeframe = {
    Name: TimeframeName
    Duration: TimeframeDuration
}

type PeriodData =
    | PeriodData of Map<TimeframeId, Timeframe>

type Period =
    | Autonomous
    | Teleop
    
type Periods =
    { Autonomous: PeriodData
      Teleop: PeriodData }
    
[<RequireQualifiedAccess>]
module Timeframe =
    type Error =
        | TimeframeNameEmpty
        | TimeframeDurationTooShort
        | TimeframeIdAlreadyExists of TimeframeId
        | TimeframeIdDoesNotExist of TimeframeId
        
    module private OnlyIf =
        let nameNotEmpty (TimeframeName name) =
            match String.IsNullOrWhiteSpace name with
            | true -> TimeframeNameEmpty |> Validation.error
            | false -> name |> TimeframeName |> Validation.ok
            
        let durationNotTooShort (TimeframeDuration duration) =
            match duration < TimeSpan.FromSeconds 1.0 with
            | true -> TimeframeDurationTooShort |> Validation.error
            | false -> duration |> TimeframeDuration |> Validation.ok
            
        let idExists (PeriodData period) id =
            match period.ContainsKey id with
            | true -> Validation.ok id
            | false -> TimeframeIdDoesNotExist id |> Validation.error
            
        let idDoesNotExist (PeriodData period) id =
            match period.ContainsKey id with
            | true -> TimeframeIdAlreadyExists id |> Validation.error
            | false -> Validation.ok id
            
    let getPeriodData period periods =
        match period with
        | Autonomous -> periods.Autonomous
        | Teleop -> periods.Teleop

    let withPeriodData period periodData periods =
        match period with
        | Autonomous -> { periods with Autonomous = periodData }
        | Teleop -> { periods with Teleop = periodData }

    let addOrUpdateTimeframe period timeframeId name duration periods =
        let periodData = getPeriodData period periods
        let (PeriodData timeframeMap) = periodData

        validation {
            let! name = OnlyIf.nameNotEmpty name
            let! duration = OnlyIf.durationNotTooShort duration

            let updatedTimeframe =
                { Name = name
                  Duration = duration }

            let updatedMap =
                timeframeMap
                |> Map.add timeframeId updatedTimeframe

            return
                periods
                |> withPeriodData period (PeriodData updatedMap)
        }

    let removeTimeframe period timeframeId periods =
        let periodData = getPeriodData period periods
        let (PeriodData timeframeMap) = periodData
        
        validation {
            let! timeframeId = OnlyIf.idExists periodData timeframeId
            
            let updatedMap = timeframeMap |> Map.remove timeframeId
            
            return
                periods
                |> withPeriodData period (PeriodData updatedMap)
        }
