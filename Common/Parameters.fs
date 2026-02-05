module ParagonRobotics.DiamondScout.Common.Parameters

open ParagonRobotics.DiamondScout.Common.Identifiers

type NumericSpinnerType =
    | IntegralSpinner of int
    | DecimalSpinner of double

/// The *definition* of a parameter (its single, shared type + constraints + default).
type ParameterSpec =
    | Dropdown of options: string list * defaultChoice: string
    | TextBox of defaultText: string
    | NumericSpinner of NumericSpinnerType
    | RadialSelection of options: string list * defaultChoice: string
    | MultiSelect of options: string list * defaultChoices: string list

/// A single parameter definition that can be shared by multiple robots.
type ParameterDefinition =
    { Id: ParameterId
      Name: string
      Spec: ParameterSpec }

/// The *per-robot* value for a parameter definition.
type ParameterValue =
    | DropdownChoice of string
    | Text of string
    | Integral of int
    | Decimal of double
    | RadialChoice of string
    | MultiSelectChoices of string list

/// All parameter values for a given robot (keyed by the shared ParameterId).
type RobotParameters = Map<ParameterId, ParameterValue>

module Parameter =
    let defaultValue (def: ParameterDefinition) : ParameterValue =
        match def.Spec with
        | Dropdown (_, defaultChoice) -> DropdownChoice defaultChoice
        | TextBox defaultText -> Text defaultText
        | NumericSpinner (IntegralSpinner defaultInt) -> Integral defaultInt
        | NumericSpinner (DecimalSpinner defaultDouble) -> Decimal defaultDouble
        | RadialSelection (_, defaultChoice) -> RadialChoice defaultChoice
        | MultiSelect (_, defaultChoices) -> MultiSelectChoices defaultChoices

    /// Build a complete set of robot parameter values (guaranteed to contain every defined ParameterId).
    let createForRobot (defs: ParameterDefinition list) : RobotParameters =
        defs
        |> List.map (fun d -> d.Id, defaultValue d)
        |> Map.ofList

    let private validateOne (def: ParameterDefinition) (value: ParameterValue) : string list =
        let bad msg = [ $"Parameter '{def.Name}' ({def.Id}) invalid: {msg}" ]

        match def.Spec, value with
        | Dropdown (options, _), DropdownChoice choice when List.contains choice options -> []
        | Dropdown (options, _), DropdownChoice choice ->
            bad $"'{choice}' is not one of [{String.concat ", " options}]"
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
            bad $"'{choice}' is not one of [{String.concat ", " options}]"
        | RadialSelection _, _ ->
            bad "value type mismatch (expected RadialChoice)"

        | MultiSelect (options, _), MultiSelectChoices choices ->
            let invalid = choices |> List.filter (fun c -> not (List.contains c options))
            if List.isEmpty invalid then []
            else bad $"contains invalid selections [{String.concat ", " invalid}]"
        | MultiSelect _, _ ->
            bad "value type mismatch (expected MultiSelectChoices)"

    /// Ensures:
    /// 1) every robot has a value for every defined ParameterId
    /// 2) no values violate the definition constraints
    let validateComplete
        (defs: ParameterDefinition list)
        (values: RobotParameters)
        : Result<RobotParameters, string list> =

        let defIds = defs |> List.map (fun d -> d.Id) |> Set.ofList
        let valueIds = values |> Map.keys |> Set.ofSeq

        let missing =
            Set.difference defIds valueIds
            |> Set.toList
            |> List.map (fun pid -> $"Missing value for ParameterId {pid}")

        let invalid =
            defs
            |> List.collect (fun def ->
                match Map.tryFind def.Id values with
                | None -> []
                | Some v -> validateOne def v)

        match missing @ invalid with
        | [] -> Ok values
        | errs -> Error errs

    /// Convenience: fill in any missing values from defaults, leaving existing values as-is.
    /// Useful when new parameters are added after robots already have data.
    let withDefaultsFilled (defs: ParameterDefinition list) (values: RobotParameters) : RobotParameters =
        defs
        |> List.fold
            (fun acc def ->
                match Map.containsKey def.Id acc with
                | true -> acc
                | false -> Map.add def.Id (defaultValue def) acc)
            values