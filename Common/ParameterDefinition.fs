namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic

type NumericSpinnerType =
    | IntegralSpinner of int
    | DecimalSpinner of double

    member this.Match(integralSpinnerAction: Action<int>, decimalSpinnerAction: Action<double>) =
        match this with
        | IntegralSpinner i -> integralSpinnerAction.Invoke i
        | DecimalSpinner d -> decimalSpinnerAction.Invoke d

type ParameterSpec =
    | Dropdown of options: string list * defaultChoice: string
    | TextBox of defaultText: string
    | NumericSpinner of NumericSpinnerType
    | RadialSelection of options: string list * defaultChoice: string
    | MultiSelect of options: string list * defaultChoices: string list

    member this.Match
        (
            dropdownAction: Action<string IReadOnlyList, string>,
            textBoxAction: Action<string>,
            numericSpinnerAction: Action<NumericSpinnerType>,
            radialSelectionAction: Action<string IReadOnlyList, string>,
            multiSelectAction: Action<string IReadOnlyList, string IReadOnlyList>
        ) =
        match this with
        | Dropdown(options, defaultChoice) -> dropdownAction.Invoke(options, defaultChoice)
        | TextBox defaultText -> textBoxAction.Invoke(defaultText)
        | NumericSpinner spinnerType -> numericSpinnerAction.Invoke(spinnerType)
        | RadialSelection(options, defaultChoice) -> radialSelectionAction.Invoke(options, defaultChoice)
        | MultiSelect(options, defaultChoices) -> multiSelectAction.Invoke(options, defaultChoices)

type ParameterDefinition = { Name: string; Spec: ParameterSpec }

[<RequireQualifiedAccess>]
module ParameterDefinition =
    let create name spec = { Name = name; Spec = spec }
    let changeName name (param: ParameterDefinition) = { param with Name = name }
    let changeSpec spec (param: ParameterDefinition) = { param with Spec = spec }

    type Event =
        | ParameterCreated of ParameterDefinition