namespace ParagonRobotics.DiamondScout.Common.Functional

open ParagonRobotics.DiamondScout.Common.Functional

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
