namespace ParagonRobotics.DiamondScout.Common.Functional

open ParagonRobotics.DiamondScout.Common

[<Struct>]
type FrcDistrictCode =
    | FrcDistrictCode of string

[<Struct>]
type FrcDistrictName =
    | FrcDistrictName of string

type FrcDistrict =
    { Code: FrcDistrictCode
      Name: FrcDistrictName
      Events: Map<FrcEventId, FrcEvent> }
    
module FrcDistrict =
    let create code name = { Code = code; Name = name; Events = Map.empty }
    let changeName name district = { district with FrcDistrict.Name = name }
    let addOrReplaceEvent eventId event district = { district with Events = district.Events |> Map.add eventId event }
    let removeEvent eventId district = { district with Events = district.Events |> Map.remove eventId }
    let changeEvent eventId transform district = { district with Events = district.Events |> Map.change eventId (Option.map transform) }
