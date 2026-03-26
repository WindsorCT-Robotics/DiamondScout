namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic

type ParameterValue =
    | DropdownChoice of index: int
    | Text of content: string
    | Integral of value: int
    | Decimal of value: double
    | RadialChoice of index: int
    | MultiSelectChoices of indexes: int list
    | Checkbox of value: bool

[<RequireQualifiedAccess>]
module ParameterValue =
    let defaultValue (def: ParameterDefinition) : ParameterValue =
        match def.Spec with
        | Dropdown(_, defaultChoice) -> DropdownChoice defaultChoice
        | TextBox defaultText -> Text defaultText
        | NumericSpinner(IntegralSpinner defaultInt) -> Integral defaultInt
        | NumericSpinner(DecimalSpinner defaultDouble) -> Decimal defaultDouble
        | RadialSelection(_, defaultChoice) -> RadialChoice defaultChoice
        | MultiSelect(_, defaultChoices) -> MultiSelectChoices defaultChoices
        | ParameterSpec.Checkbox defaultValue -> Checkbox defaultValue

type ParameterValue with
    member this.Match
        (
            dropdownAction: Action<int>,
            textBoxAction: Action<string>,
            integralSpinnerAction: Action<int>,
            decimalSpinnerAction: Action<double>,
            radialSelectionAction: Action<int>,
            multiSelectAction: Action<int IReadOnlyList>,
            checkboxAction: Action<bool>
        ) =
        match this with
        | DropdownChoice choice -> dropdownAction.Invoke(choice)
        | Text text -> textBoxAction.Invoke(text)
        | Integral i -> integralSpinnerAction.Invoke(i)
        | Decimal d -> decimalSpinnerAction.Invoke(d)
        | RadialChoice choice -> radialSelectionAction.Invoke(choice)
        | MultiSelectChoices choices -> multiSelectAction.Invoke(choices)
        | Checkbox b -> checkboxAction.Invoke(b)

    static member GetDefaultValue(def: ParameterDefinition) = ParameterValue.defaultValue def
