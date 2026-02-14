module ParagonRobotics.DiamondScout.Common.Parameters

open ParagonRobotics.DiamondScout.Common.Identifiers

type NumericSpinnerType =
    | IntegralSpinner of int
    | DecimalSpinner of double

type IsActive = 
    | Active of bool
    | Inactive of bool
    
type ParameterSpec =
    | Dropdown of options: string list * defaultChoice: string
    | TextBox of defaultText: string
    | NumericSpinner of NumericSpinnerType
    | RadialSelection of options: string list * defaultChoice: string
    | MultiSelect of options: string list * defaultChoices: string list

type ParameterDefinition =
    { Name: string
      Spec: ParameterSpec
      Active: IsActive }

type ParameterValue =
    | DropdownChoice of string
    | Text of string
    | Integral of int
    | Decimal of double
    | RadialChoice of string
    | MultiSelectChoices of string list

type RobotParameters =
    { Robot: RobotId
      Parameters: Map<ParameterDefinitionId, ParameterValue> }

module Parameter =
    let defaultValue (def: ParameterDefinition) : ParameterValue =
        match def.Spec with
        | Dropdown (_, defaultChoice) -> DropdownChoice defaultChoice
        | TextBox defaultText -> Text defaultText
        | NumericSpinner (IntegralSpinner defaultInt) -> Integral defaultInt
        | NumericSpinner (DecimalSpinner defaultDouble) -> Decimal defaultDouble
        | RadialSelection (_, defaultChoice) -> RadialChoice defaultChoice
        | MultiSelect (_, defaultChoices) -> MultiSelectChoices defaultChoices

    let createForRobot (robotId: RobotId) (defs: (ParameterDefinitionId * ParameterDefinition) list) : RobotParameters =
        let parameters =
            defs
            |> List.map (fun (id, d) -> id, defaultValue d)
            |> Map.ofList
    
        { Robot = robotId
          Parameters = parameters }

    let private validateOne (def: ParameterDefinition) (value: ParameterValue) : string list =
        let bad msg = [ $"Parameter '{def.Name}' invalid: {msg}" ]
    
        match def.Spec, value with
        | Dropdown (options, _), DropdownChoice choice when List.contains choice options -> []
        | Dropdown (options, _), DropdownChoice choice ->
            let joinWithCommas (xs: string list) = String.concat ", " xs
            bad $"'{choice}' is not one of [{joinWithCommas options}]"
        | Dropdown _, _ ->
            bad "value type mismatch (expected DropdownChoice)"

        | TextBox _, Text _ -> []

        | TextBox _, _ ->
            bad "value type mismatch (expected Text)"

        | NumericSpinner (IntegralSpinner _), Integral _ -> []
        | NumericSpinner (DecimalSpinner _), Decimal _ -> []
        | NumericSpinner _, _ ->
            bad "value type mismatch (expected Integral or Decimal to match spinner type)"

        | RadialSelection (options, _), RadialChoice choice when List.contains choice options -> []
        | RadialSelection (options, _), RadialChoice choice ->
            let joinWithCommas (xs: string list) = String.concat ", " xs
            bad $"'{choice}' is not one of [{joinWithCommas options}]"
        | RadialSelection _, _ ->
            bad "value type mismatch (expected RadialChoice)"

        | MultiSelect (options, _), MultiSelectChoices choices ->
            let invalid = choices |> List.filter (fun c -> not (List.contains c options))
            let joinWithCommas (xs: string list) = String.concat ", " xs
            if List.isEmpty invalid then []
            else bad $"contains invalid selections [{joinWithCommas invalid}]"
        | MultiSelect _, _ ->
            bad "value type mismatch (expected MultiSelectChoices)"

    let validate
        (defs: (ParameterDefinitionId * ParameterDefinition) list)
        (values: RobotParameters)
        : Result<RobotParameters, string list> =

        let defIds = defs |> List.map fst |> Set.ofList
        let valueIds = values.Parameters |> Map.keys |> Set.ofSeq

        let missing =
            Set.difference defIds valueIds
            |> Set.toList
            |> List.map (fun pid -> $"Missing value for ParameterId {pid}")

        let invalid =
            defs
            |> List.collect (fun (id, def) ->
                match Map.tryFind id values.Parameters with
                | None -> []
                | Some v -> validateOne def v)

        match missing @ invalid with
        | [] -> Ok values
        | errs -> Error errs

    let withDefaultsFilled (defs: (ParameterDefinitionId * ParameterDefinition) list) (values: RobotParameters) : RobotParameters =
        let updatedParameters =
            defs
            |> List.fold
                (fun acc (id, def) ->
                    match Map.containsKey id acc with
                    | true -> acc
                    | false -> Map.add id (defaultValue def) acc)
                values.Parameters
    
        { values with Parameters = updatedParameters }