namespace ParagonRobotics.DiamondScout.Common.Functional

open ParagonRobotics.DiamondScout.Common

[<Struct>]
type FrcDistrictCode =
    | FrcDistrictCode of string

[<Struct>]
type FrcDistrictName =
    | FrcDistrictName of string

    member this.Value = let (FrcDistrictName name) = this in name

type FrcDistrict =
    { Code: FrcDistrictCode
      Name: FrcDistrictName
      Events: Map<FrcEventId, FrcEvent> }
