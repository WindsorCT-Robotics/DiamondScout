namespace ParagonRobotics.DiamondScout.Common

open System
open FsToolkit.ErrorHandling

[<Struct>]
type TimeframeId =
    private
    | TimeframeId of Guid

    static member Zero = TimeframeId Guid.Empty
    static member Create() = Guid.CreateVersion7() |> TimeframeId
    member this.Value = let (TimeframeId guid) = this in guid

[<Struct>]
type TimeframeName = TimeframeName of string

[<Struct>]
type TimeframeDuration = TimeframeDuration of TimeSpan

type Timeframe =
    private
        { Name: TimeframeName
          Duration: TimeframeDuration }

type Period =
    | Autonomous
    | Teleop

type Periods =
    private
        { Autonomous: TimeframeId list
          Teleop: TimeframeId list
          AllTimeframes: Map<TimeframeId, Timeframe> }

[<RequireQualifiedAccess>]
module Timeframe =
    type Error =
        | TimeframeNameEmpty
        | TimeframeDurationTooShort
        | TimeframeAlreadyExists of TimeframeName
        | TimeframeDoesNotExist of TimeframeName

    module private OnlyIf =
        let nameNotEmpty (TimeframeName name as timeframeName) =
            match String.IsNullOrWhiteSpace name with
            | true -> TimeframeNameEmpty |> Validation.error
            | false -> timeframeName |> Validation.ok

        let durationNotTooShort (TimeframeDuration duration as timeframeDuration) =
            match duration < TimeSpan.FromSeconds 1.0 with
            | true -> TimeframeDurationTooShort |> Validation.error
            | false -> timeframeDuration |> Validation.ok

        let nameExists (timeframes: Timeframe list) name =
            match timeframes |> List.exists (fun t -> t.Name = name) with
            | true -> Validation.ok name
            | false -> TimeframeDoesNotExist name |> Validation.error

        let nameDoesNotExist (timeframes: Timeframe list) name =
            match timeframes |> List.exists (fun t -> t.Name = name) with
            | true -> TimeframeAlreadyExists name |> Validation.error
            | false -> Validation.ok name

    let getTimeframes period periods =
        let ids =
            match period with
            | Autonomous -> periods.Autonomous
            | Teleop -> periods.Teleop

        ids |> List.choose periods.AllTimeframes.TryFind

    let withTimeframes period timeframeIdList periods =
        match period with
        | Autonomous ->
            { periods with
                Autonomous = timeframeIdList }
        | Teleop ->
            { periods with
                Teleop = timeframeIdList }

    let addTimeframe period id name duration periods =
        let timeframeList = getTimeframes period periods

        validation {
            let! name = OnlyIf.nameNotEmpty name
            let! _ = OnlyIf.nameDoesNotExist timeframeList name
            let! duration = OnlyIf.durationNotTooShort duration

            let newTimeframe = { Name = name; Duration = duration }

            let ids =
                match period with
                | Autonomous -> periods.Autonomous
                | Teleop -> periods.Teleop

            let updatedIds = ids @ [ id ]
            let updatedAll = periods.AllTimeframes |> Map.add id newTimeframe

            return
                { periods with
                    AllTimeframes = updatedAll }
                |> withTimeframes period updatedIds
        }

    let updateTimeframe period name newName duration periods =
        let timeframeList = getTimeframes period periods

        validation {
            let! name = OnlyIf.nameNotEmpty name
            let! _ = OnlyIf.nameExists timeframeList name
            let! newName = OnlyIf.nameNotEmpty newName
            let! duration = OnlyIf.durationNotTooShort duration

            let! _ =
                if name <> newName then
                    OnlyIf.nameDoesNotExist timeframeList newName
                else
                    Validation.ok newName

            let timeframeId =
                periods.AllTimeframes
                |> Map.tryPick (fun id t -> if t.Name = name then Some id else None)

            match timeframeId with
            | None -> return periods // Should not happen if nameExists passed
            | Some id ->
                let updatedTimeframe = { Name = newName; Duration = duration }
                let updatedAll = periods.AllTimeframes |> Map.add id updatedTimeframe

                return
                    { periods with
                        AllTimeframes = updatedAll }
        }

    let removeTimeframe period name periods =
        let timeframeList = getTimeframes period periods

        validation {
            let! name = OnlyIf.nameExists timeframeList name

            let timeframeId =
                periods.AllTimeframes
                |> Map.tryPick (fun id t -> if t.Name = name then Some id else None)

            match timeframeId with
            | None -> return periods
            | Some id ->
                let ids =
                    match period with
                    | Autonomous -> periods.Autonomous
                    | Teleop -> periods.Teleop

                let updatedIds = ids |> List.filter (fun tId -> tId <> id)
                let updatedAll = periods.AllTimeframes |> Map.remove id

                return
                    { periods with
                        AllTimeframes = updatedAll }
                    |> withTimeframes period updatedIds
        }

    type Direction =
        | Up
        | Down

    let moveTimeframe period name direction periods =
        let timeframeList = getTimeframes period periods

        let swap i j list =
            let arr = List.toArray list
            let temp = arr[i]
            arr[i] <- arr[j]
            arr[j] <- temp
            List.ofArray arr

        validation {
            let! name = OnlyIf.nameExists timeframeList name

            let timeframeId =
                periods.AllTimeframes
                |> Map.tryPick (fun id t -> if t.Name = name then Some id else None)

            match timeframeId with
            | None -> return periods
            | Some id ->
                let ids =
                    match period with
                    | Autonomous -> periods.Autonomous
                    | Teleop -> periods.Teleop

                let index = ids |> List.findIndex (fun tId -> tId = id)

                let updatedIds =
                    match direction with
                    | Up when index > 0 -> swap index (index - 1) ids
                    | Down when index < ids.Length - 1 -> swap index (index + 1) ids
                    | _ -> ids

                return periods |> withTimeframes period updatedIds
        }
