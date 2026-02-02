module ParagonRobotics.DiamondScout.Common.GameData

open System
open ParagonRobotics.DiamondScout.Common.Identifiers

type NumericSpinnerType =
    | IntegralSpinner of int
    | DecimalSpinner of double
type ParameterType =
    // change to generic type for dropdown later
    | Dropdown of string list
    | TextBox of string
    | NumericSpinner of NumericSpinnerType
    | RadialSelection of string list
    | MultiSelect of string list

type Parameter = { Name: string; Value: ParameterType }

type Game =
    { Year: DateOnly
      Name: string
      Phases: SubPhaseId list
      GamePieces: GamePieceId list
      Infractions: InfractionId list
      PitResults: PitResultId list
      Events: EventId list
      Parameters: Parameter list}