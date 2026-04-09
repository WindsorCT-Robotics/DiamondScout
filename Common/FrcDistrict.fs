namespace ParagonRobotics.DiamondScout.Common.Functional

open ParagonRobotics.DiamondScout.Common

[<Struct>]
type FrcDistrictCode =
    private
    | FrcDistrictCode of string

    member this.Value = let (FrcDistrictCode code) = this in code

    static member Create(code: string) =
        match code.Length with
        | 3 -> FrcDistrictCode code
        | _ -> invalidOp $"Invalid FRC district code: %s{code}"

[<Struct>]
type FrcDistrictName =
    | FrcDistrictName of string

    member this.Value = let (FrcDistrictName name) = this in name

type FrcDistrict =
    { Code: FrcDistrictCode
      Name: FrcDistrictName
      Events: Map<FrcEventId, FrcEvent> }
