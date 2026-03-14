namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling

type ParameterValue =
    | DropdownChoice of index: int
    | Text of content: string
    | Integral of value: int
    | Decimal of value: double
    | RadialChoice of index: int
    | MultiSelectChoices of indexes: int list
    | Checkbox of value: bool

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

type RobotParameters =
    { ParameterValues: Map<RobotId, Map<ParameterDefinitionId, ParameterValue>> }

[<RequireQualifiedAccess>]
module ParameterValue =
    type Event =
        | ParameterValueAdded of
            robotId: RobotId *
            parameterDefinitionId: ParameterDefinitionId *
            parameterValue: ParameterValue
        | ParameterValueChanged of
            robotId: RobotId *
            parameterDefinitionId: ParameterDefinitionId *
            parameterValue: ParameterValue
        | ParameterValueRemoved of
            robotId: RobotId *
            parameterDefinitionId: ParameterDefinitionId *
            parameterValue: ParameterValue
        // External Events
        | ParameterDefinitionAdded of
            robotId: RobotId *
            parameterDefinitionId: ParameterDefinitionId *
            parameterDefinition: ParameterDefinition
        | ParameterDefinitionChanged of
            robotId: RobotId *
            parameterDefinitionId: ParameterDefinitionId *
            parameterDefinition: ParameterDefinition
        | ParameterDefinitionRemoved of robotId: RobotId * parameterDefinitionId: ParameterDefinitionId
        | RobotAdded of robotId: RobotId * parameters: Map<ParameterDefinitionId, ParameterValue>
        | RobotRemoved of robotId: RobotId

    type Error =
        | MissingValue of parameterId: ParameterDefinitionId
        | ValueTypeMismatch of parameterName: string * expected: string
        | InvalidChoiceIndex of parameterName: string * index: int
        | InvalidChoiceIndexes of parameterName: string * indexes: int list

    let defaultValue (def: ParameterDefinition) : ParameterValue =
        match def.Spec with
        | Dropdown(_, defaultChoice) -> DropdownChoice defaultChoice
        | TextBox defaultText -> Text defaultText
        | NumericSpinner(IntegralSpinner defaultInt) -> Integral defaultInt
        | NumericSpinner(DecimalSpinner defaultDouble) -> Decimal defaultDouble
        | RadialSelection(_, defaultChoice) -> RadialChoice defaultChoice
        | MultiSelect(_, defaultChoices) -> MultiSelectChoices defaultChoices
        | ParameterSpec.Checkbox defaultValue -> Checkbox defaultValue

    module Validation =
        let private ok value = Validation.ok value
        let private error err = Validation.error err

        let parameterExists id parameters =
            match Map.tryFind id parameters with
            | Some value -> ok value
            | None -> id |> MissingValue |> error

        let private indexInRange def options index =
            if index >= 0 && index < List.length options then
                ok index
            else
                error (InvalidChoiceIndex(def.Name, index))

        let private indexesInRange def options indexes =
            let invalidIndexes =
                indexes |> List.filter (fun index -> index < 0 || index >= List.length options)

            match invalidIndexes with
            | [] -> ok indexes
            | xs -> error (InvalidChoiceIndexes(def.Name, xs))

        let valueMatchesSpec (def: ParameterDefinition) (value: ParameterValue) =
            match def.Spec, value with
            | Dropdown(options, _), DropdownChoice choice ->
                indexInRange def options choice |> Validation.map (fun _ -> value)

            | Dropdown _, _ -> error (ValueTypeMismatch(def.Name, "DropdownChoice"))

            | TextBox _, Text _ -> ok value

            | TextBox _, _ -> error (ValueTypeMismatch(def.Name, "Text"))

            | NumericSpinner(IntegralSpinner _), Integral _ -> ok value

            | NumericSpinner(DecimalSpinner _), Decimal _ -> ok value

            | NumericSpinner(IntegralSpinner _), _ -> error (ValueTypeMismatch(def.Name, "Integral"))

            | NumericSpinner(DecimalSpinner _), _ -> error (ValueTypeMismatch(def.Name, "Decimal"))

            | RadialSelection(options, _), RadialChoice choice ->
                indexInRange def options choice |> Validation.map (fun _ -> value)

            | RadialSelection _, _ -> error (ValueTypeMismatch(def.Name, "RadialChoice"))

            | MultiSelect(options, _), MultiSelectChoices choices ->
                indexesInRange def options choices |> Validation.map (fun _ -> value)

            | MultiSelect _, _ -> error (ValueTypeMismatch(def.Name, "MultiSelectChoices"))
            | ParameterSpec.Checkbox _, Checkbox _ -> ok value
            | ParameterSpec.Checkbox _, _ -> error (ValueTypeMismatch(def.Name, "Checkbox"))
