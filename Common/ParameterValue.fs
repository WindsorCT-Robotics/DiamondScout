namespace ParagonRobotics.DiamondScout.Common.Parameters

type ParameterValue =
    | DropdownChoice of index: int
    | Text of content: string
    | Integral of value: int
    | Decimal of value: double
    | RadialChoice of index: int
    | MultiSelectChoices of indexes: int list
    | Checkbox of value: bool

[<AutoOpen>]
module Functional =
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

        let isValid (spec: ParameterSpec) (value: ParameterValue) =
            match spec, value with
            | Dropdown(options, _), DropdownChoice index -> index >= 0 && index < options.Length
            | TextBox _, Text _ -> true
            | NumericSpinner(IntegralSpinner _), Integral _ -> true
            | NumericSpinner(DecimalSpinner _), Decimal _ -> true
            | RadialSelection(options, _), RadialChoice index -> index >= 0 && index < options.Length
            | MultiSelect(options, _), MultiSelectChoices indexes ->
                indexes |> List.forall (fun i -> i >= 0 && i < options.Length)
            | ParameterSpec.Checkbox _, Checkbox _ -> true
            | _ -> false

type ParameterValue with
    static member DefaultValue(def: ParameterDefinition) = ParameterValue.defaultValue def
    static member IsValid (spec: ParameterSpec) (value: ParameterValue) = ParameterValue.isValid spec value

type ScoutingParameters<'TKey when 'TKey: comparison> = Map<ParameterDefinition, Map<'TKey, ParameterValue>>
