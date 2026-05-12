namespace ParagonRobotics.DiamondScout.Common

open System

[<Struct>]
type ParameterDefinitionId =
    private
    | ParameterDefinitionId of Guid

    static member Zero = ParameterDefinitionId Guid.Empty

    static member Create() =
        Guid.CreateVersion7() |> ParameterDefinitionId

    member this.Value = let (ParameterDefinitionId guid) = this in guid

[<Struct>]
type ParameterDefinitionName = ParameterDefinitionName of string

type NumericSpinnerType =
    | IntegralSpinner of int
    | DecimalSpinner of double

    member this.Match(integralSpinnerAction: Action<int>, decimalSpinnerAction: Action<double>) =
        match this with
        | IntegralSpinner i -> integralSpinnerAction.Invoke i
        | DecimalSpinner d -> decimalSpinnerAction.Invoke d

type ParameterSpec =
    | Dropdown of options: string list * defaultChoice: int
    | TextBox of defaultText: string
    | NumericSpinner of NumericSpinnerType
    | RadialSelection of options: string list * defaultChoice: int
    | MultiSelect of options: string list * defaultChoices: int list
    | Checkbox of defaultState: bool

[<RequireQualifiedAccess>]
type ParameterCategory =
    | Pit
    | Match

type ParameterDefinition =
    { Name: ParameterDefinitionName
      Spec: ParameterSpec
      Category: ParameterCategory }

[<RequireQualifiedAccess>]
module ParameterDefinition =
    let create name spec category =
        { Name = name
          Spec = spec
          Category = category }

    let withName name (param: ParameterDefinition) = { param with Name = name }
    let withSpec spec (param: ParameterDefinition) = { param with Spec = spec }
    let withCategory category (param: ParameterDefinition) = { param with Category = category }
