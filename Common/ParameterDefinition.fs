namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic

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

    member this.Match
        (
            dropdownAction: Action<string IReadOnlyList, int>,
            textBoxAction: Action<string>,
            numericSpinnerAction: Action<NumericSpinnerType>,
            radialSelectionAction: Action<string IReadOnlyList, int>,
            multiSelectAction: Action<string IReadOnlyList, int IReadOnlyList>,
            checkboxAction: Action<bool>
        ) =
        match this with
        | Dropdown(options, defaultChoice) -> dropdownAction.Invoke(options, defaultChoice)
        | TextBox defaultText -> textBoxAction.Invoke(defaultText)
        | NumericSpinner spinnerType -> numericSpinnerAction.Invoke(spinnerType)
        | RadialSelection(options, defaultChoice) -> radialSelectionAction.Invoke(options, defaultChoice)
        | MultiSelect(options, defaultChoices) -> multiSelectAction.Invoke(options, defaultChoices)
        | Checkbox defaultOption -> checkboxAction.Invoke(defaultOption)

type ParameterDefinition =
    { Name: ParameterDefinitionName
      Spec: ParameterSpec }

[<RequireQualifiedAccess>]
module ParameterDefinition =
    let create name spec = { Name = name; Spec = spec }
    let changeName name (param: ParameterDefinition) = { param with Name = name }
    let changeSpec spec (param: ParameterDefinition) = { param with Spec = spec }

type ParameterDefinition with
    static member Create name spec = ParameterDefinition.create name spec

    member this.Rename name =
        ParameterDefinition.changeName name this

    member this.Modify spec =
        ParameterDefinition.changeSpec spec this
