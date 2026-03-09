using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using ParagonRobotics.DiamondScout.Common;
using QRCoder;

namespace DiamondScout.WebApp.Shared.Pages;

public partial class MatchScout : ComponentBase
{
    protected int? TeamNumber { get; set; }

    protected string SelectedAlliance { get; set; } = "Red";
    
    protected string? StartingPosition { get; set; }

    protected string? PreloadedFuel { get; set; } = "Yes";

    protected string? CollectedFuel { get; set; } = "No";
    
    protected string? ShotFuel { get; set; } = "Yes";

    protected QualitativeScoring AutoShootingAccuracy { get; set; } = QualitativeScoring.Average;

    protected EndgameResult AutoL1ClimbResult { get; set; } = EndgameResult.NotAttempted;

    protected EndgameResult EndgameClimbResult { get; set; } = EndgameResult.NotAttempted;

    protected QualitativeScoring TeleopFuelPerCycle { get; set; } = QualitativeScoring.Average;

    protected QualitativeScoring TeleopShootingAccuracy { get; set; } = QualitativeScoring.Average;

    protected bool IsShootingWhileMoving { get; set; }

    protected bool IsShootingFromCertainPositionsOnly { get; set; }

    protected int TeleopCycleCount { get; set; }

    protected bool HadDefensePlayedAgainstThem { get; set; }

    protected QualitativeScoring DefenseHandling { get; set; } = QualitativeScoring.Average;

    protected string FuelCollectionSource { get; set; } = "No";

    protected string FuelShuttlingMethod { get; set; } = "No";

    protected QualitativeScoring FuelShuttlingEffectiveness { get; set; } = QualitativeScoring.Average;

    protected bool PlayedDefense { get; set; }

    protected QualitativeScoring DefenseEffectiveness { get; set; } = QualitativeScoring.Average;

    protected QualitativeScoring DriverSkill { get; set; } = QualitativeScoring.Average;

    protected QualitativeScoring RobotReliability { get; set; } = QualitativeScoring.Average;

    protected bool HadRobotIssues { get; set; }

    protected Note? RobotIssuesNote { get; set; }

    protected Note? AdditionalThoughtsNote { get; set; }

    protected UserId CurrentScoutUserId { get; set; } = UserId.Zero;

    protected string? GeneratedQrPayload { get; set; }

    protected string? GeneratedQrSvg { get; set; }

    protected bool IsQrModalOpen { get; set; }

    protected string? QrGenerationError { get; set; }

    protected IReadOnlyList<string> AllianceOptions { get; } =
        new[] { "Red", "Blue" };

    protected IReadOnlyList<string> StartingOptions { get; } =
        new[] { "Hub", "Bump", "Trench" };

    protected IReadOnlyList<string> IsFuelLoaded { get; } =
        new[] { "Yes", "No" };

    protected IReadOnlyList<string> IsMoreFuelCollected { get; } =
        new[] { "No", "Yes - Chute", "Yes - Neutral Zone", "Yes - Depot" };

    protected IReadOnlyList<string> IsFuelShot { get; } =
        new[] { "Yes", "No" };

    protected IReadOnlyList<AccuracyOption> AutoAccuracyOptions { get; } =
        new[]
        {
            new AccuracyOption("Poor", QualitativeScoring.Poor),
            new AccuracyOption("Below Average", QualitativeScoring.BelowAverage),
            new AccuracyOption("Average", QualitativeScoring.Average),
            new AccuracyOption("Above Average", QualitativeScoring.AboveAverage),
            new AccuracyOption("Excellent", QualitativeScoring.Excellent)
        };

    protected IReadOnlyList<string> AutoL1ClimbOptions { get; } =
        new[] { "Yes", "Attempted", "No" };

    protected IReadOnlyList<string> EndgameClimbOptions { get; } =
        new[] { "Yes - L1", "Yes - L2", "Yes - L3", "Attempted", "No" };

    protected IReadOnlyList<AccuracyOption> TeleopFuelPerCycleOptions { get; } =
        new[]
        {
            new AccuracyOption("None", QualitativeScoring.Poor),
            new AccuracyOption("A Few", QualitativeScoring.BelowAverage),
            new AccuracyOption("Average", QualitativeScoring.Average),
            new AccuracyOption("Many", QualitativeScoring.AboveAverage),
            new AccuracyOption("Most", QualitativeScoring.Excellent)
        };

    protected IReadOnlyList<AccuracyOption> TeleopAccuracyOptions { get; } =
        new[]
        {
            new AccuracyOption("Poor", QualitativeScoring.Poor),
            new AccuracyOption("Below Average", QualitativeScoring.BelowAverage),
            new AccuracyOption("Average", QualitativeScoring.Average),
            new AccuracyOption("Above Average", QualitativeScoring.AboveAverage),
            new AccuracyOption("Excellent", QualitativeScoring.Excellent)
        };

    protected IReadOnlyList<AccuracyOption> DefenseHandlingOptions { get; } =
        new[]
        {
            new AccuracyOption("Poor", QualitativeScoring.Poor),
            new AccuracyOption("Below Average", QualitativeScoring.BelowAverage),
            new AccuracyOption("Average", QualitativeScoring.Average),
            new AccuracyOption("Above Average", QualitativeScoring.AboveAverage),
            new AccuracyOption("Excellent", QualitativeScoring.Excellent)
        };

    protected IReadOnlyList<string> YesNoOptions { get; } =
        new[] { "Yes", "No" };

    protected IReadOnlyList<string> FuelCollectionSourceOptions { get; } =
        new[] { "No", "From Outpost Chute", "From Ground" };

    protected IReadOnlyList<string> FuelShuttlingMethodOptions { get; } =
        new[] { "No", "Shoot", "Push" };

    protected IReadOnlyList<AccuracyOption> FuelShuttlingEffectivenessOptions { get; } =
        new[]
        {
            new AccuracyOption("Poor", QualitativeScoring.Poor),
            new AccuracyOption("Below Average", QualitativeScoring.BelowAverage),
            new AccuracyOption("Average", QualitativeScoring.Average),
            new AccuracyOption("Above Average", QualitativeScoring.AboveAverage),
            new AccuracyOption("Excellent", QualitativeScoring.Excellent)
        };

    protected IReadOnlyList<AccuracyOption> DefenseEffectivenessOptions { get; } =
        new[]
        {
            new AccuracyOption("Poor", QualitativeScoring.Poor),
            new AccuracyOption("Below Average", QualitativeScoring.BelowAverage),
            new AccuracyOption("Average", QualitativeScoring.Average),
            new AccuracyOption("Above Average", QualitativeScoring.AboveAverage),
            new AccuracyOption("Excellent", QualitativeScoring.Excellent)
        };

    protected IReadOnlyList<AccuracyOption> DriverSkillOptions { get; } =
        new[]
        {
            new AccuracyOption("Poor", QualitativeScoring.Poor),
            new AccuracyOption("Below Average", QualitativeScoring.BelowAverage),
            new AccuracyOption("Average", QualitativeScoring.Average),
            new AccuracyOption("Above Average", QualitativeScoring.AboveAverage),
            new AccuracyOption("Excellent", QualitativeScoring.Excellent)
        };

    protected IReadOnlyList<AccuracyOption> RobotReliabilityOptions { get; } =
        new[]
        {
            new AccuracyOption("Poor", QualitativeScoring.Poor),
            new AccuracyOption("Below Average", QualitativeScoring.BelowAverage),
            new AccuracyOption("Average", QualitativeScoring.Average),
            new AccuracyOption("Above Average", QualitativeScoring.AboveAverage),
            new AccuracyOption("Excellent", QualitativeScoring.Excellent)
        };

    protected string AutoShootingAccuracyDisplay =>
        AutoAccuracyOptions
            .FirstOrDefault(option => option.Value.Equals(AutoShootingAccuracy))
            ?.Label
        ?? "Average";

    protected string AutoL1ClimbDisplay => GetAutoL1ClimbDisplay(AutoL1ClimbResult);

    protected string EndgameClimbDisplay => GetEndgameClimbDisplay(EndgameClimbResult);

    protected string TeleopFuelPerCycleDisplay =>
        TeleopFuelPerCycleOptions
            .FirstOrDefault(option => option.Value.Equals(TeleopFuelPerCycle))
            ?.Label
        ?? "Average";

    protected string TeleopShootingAccuracyDisplay =>
        TeleopAccuracyOptions
            .FirstOrDefault(option => option.Value.Equals(TeleopShootingAccuracy))
            ?.Label
        ?? "Average";

    protected string DefenseHandlingDisplay =>
        DefenseHandlingOptions
            .FirstOrDefault(option => option.Value.Equals(DefenseHandling))
            ?.Label
        ?? "Average";

    protected string FuelShuttlingEffectivenessDisplay =>
        FuelShuttlingEffectivenessOptions
            .FirstOrDefault(option => option.Value.Equals(FuelShuttlingEffectiveness))
            ?.Label
        ?? "Average";

    protected string DefenseEffectivenessDisplay =>
        DefenseEffectivenessOptions
            .FirstOrDefault(option => option.Value.Equals(DefenseEffectiveness))
            ?.Label
        ?? "Average";

    protected string DriverSkillDisplay =>
        DriverSkillOptions
            .FirstOrDefault(option => option.Value.Equals(DriverSkill))
            ?.Label
        ?? "Average";

    protected string RobotReliabilityDisplay =>
        RobotReliabilityOptions
            .FirstOrDefault(option => option.Value.Equals(RobotReliability))
            ?.Label
        ?? "Average";

    protected string ShootingWhileMovingDisplay
    {
        get => IsShootingWhileMoving ? "Yes" : "No";
        set => IsShootingWhileMoving = value == "Yes";
    }

    protected string ShootingFromCertainPositionsOnlyDisplay
    {
        get => IsShootingFromCertainPositionsOnly ? "Yes" : "No";
        set => IsShootingFromCertainPositionsOnly = value == "Yes";
    }

    protected string DefensePlayedAgainstThemDisplay
    {
        get => HadDefensePlayedAgainstThem ? "Yes" : "No";
        set => HadDefensePlayedAgainstThem = value == "Yes";
    }

    protected string PlayedDefenseDisplay
    {
        get => PlayedDefense ? "Yes" : "No";
        set => PlayedDefense = value == "Yes";
    }

    protected string HadRobotIssuesDisplay
    {
        get => HadRobotIssues ? "Yes" : "No";
        set => HadRobotIssues = value == "Yes";
    }

    protected string RobotIssuesText
    {
        get => RobotIssuesNote?.Text ?? string.Empty;
        set => RobotIssuesNote = CreateOrUpdateNote(RobotIssuesNote, value);
    }

    protected string AdditionalThoughtsText
    {
        get => AdditionalThoughtsNote?.Text ?? string.Empty;
        set => AdditionalThoughtsNote = CreateOrUpdateNote(AdditionalThoughtsNote, value);
    }

    protected bool ShowFuelShuttlingEffectiveness =>
        FuelShuttlingMethod != "No";

    protected bool ShowDefenseEffectiveness =>
        PlayedDefense;

    protected bool ShowRobotIssuesText =>
        HadRobotIssues;

    private static ScoringTier L1Tier => new("L1", 1u);

    private static ScoringTier L2Tier => new("L2", 2u);

    private static ScoringTier L3Tier => new("L3", 3u);

    private Note CreateOrUpdateNote(Note? existingNote, string text)
    {
        var userId = existingNote?.UserId ?? CurrentScoutUserId;
        return new Note(userId, text);
    }

    protected void GenerateQrPayload()
    {
        try
        {
            QrGenerationError = null;

            var payload = new CompactMatchScoutQrPayload
            {
                Tn = TeamNumber,
                Al = SelectedAlliance,
                Sp = StartingPosition,
                Pf = PreloadedFuel,
                Cf = CollectedFuel,
                Sf = ShotFuel,
                Asa = MapQualitative(AutoShootingAccuracy),
                Ac = MapEndgameResultCompact(AutoL1ClimbResult),

                Tfc = MapQualitative(TeleopFuelPerCycle),
                Tsa = MapQualitative(TeleopShootingAccuracy),
                Swm = IsShootingWhileMoving,
                Scp = IsShootingFromCertainPositionsOnly,
                Tcy = TeleopCycleCount,
                Dpa = HadDefensePlayedAgainstThem,
                Dhr = MapQualitative(DefenseHandling),

                Fcs = FuelCollectionSource,
                Fsm = FuelShuttlingMethod,
                Fse = ShowFuelShuttlingEffectiveness
                    ? MapQualitative(FuelShuttlingEffectiveness)
                    : null,
                Pld = PlayedDefense,
                Pde = ShowDefenseEffectiveness
                    ? MapQualitative(DefenseEffectiveness)
                    : null,

                Eg = MapEndgameResultCompact(EndgameClimbResult),

                Drv = MapQualitative(DriverSkill),
                Rel = MapQualitative(RobotReliability),
                Hri = HadRobotIssues,
                Rin = ShowRobotIssuesText
                    ? RobotIssuesNote?.Text
                    : null,
                Add = AdditionalThoughtsNote?.Text
            };

            var qrPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.M);

            var svgQrCode = new SvgQRCode(qrData);
            GeneratedQrSvg = svgQrCode.GetGraphic(10);

            IsQrModalOpen = true;
        }
        catch (Exception ex)
        {
            GeneratedQrSvg = null;
            IsQrModalOpen = false;
            QrGenerationError = ex.Message;
        }
    }

    protected void CloseQrModal()
    {
        IsQrModalOpen = false;
    }

    
    protected void OnAutoAccuracyChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        AutoShootingAccuracy = AutoAccuracyOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
    }

    protected void OnAutoL1ClimbChanged(ChangeEventArgs e)
    {
        var selectedValue = e.Value?.ToString() ?? "No";

        AutoL1ClimbResult = selectedValue switch
        {
            "Yes" => EndgameResult.NewSuccess(L1Tier),
            "Attempted" => EndgameResult.Failure,
            _ => EndgameResult.NotAttempted
        };
    }

    protected void OnEndgameClimbChanged(ChangeEventArgs e)
    {
        var selectedValue = e.Value?.ToString() ?? "No";

        EndgameClimbResult = selectedValue switch
        {
            "Yes - L1" => EndgameResult.NewSuccess(L1Tier),
            "Yes - L2" => EndgameResult.NewSuccess(L2Tier),
            "Yes - L3" => EndgameResult.NewSuccess(L3Tier),
            "Attempted" => EndgameResult.Failure,
            _ => EndgameResult.NotAttempted
        };
    }

    protected void OnTeleopFuelPerCycleChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        TeleopFuelPerCycle = TeleopFuelPerCycleOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
    }

    protected void OnTeleopShootingAccuracyChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        TeleopShootingAccuracy = TeleopAccuracyOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
    }

    protected void OnDefenseHandlingChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        DefenseHandling = DefenseHandlingOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
    }

    protected void OnFuelShuttlingEffectivenessChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        FuelShuttlingEffectiveness = FuelShuttlingEffectivenessOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
    }

    protected void OnDefenseEffectivenessChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        DefenseEffectiveness = DefenseEffectivenessOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
    }

    protected void OnDriverSkillChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        DriverSkill = DriverSkillOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
    }

    protected void OnRobotReliabilityChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        RobotReliability = RobotReliabilityOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
    }

    protected void IncrementTeleopCycleCount()
    {
        TeleopCycleCount++;
    }

    protected void DecrementTeleopCycleCount()
    {
        if (TeleopCycleCount > 0)
        {
            TeleopCycleCount--;
        }
    }

    private static string GetAutoL1ClimbDisplay(EndgameResult result)
    {
        var display = "No";

        result.Match(
            _ => display = "Yes",
            () => display = "Attempted",
            () => display = "No");

        return display;
    }

    private static string GetEndgameClimbDisplay(EndgameResult result)
    {
        var display = "No";

        result.Match(
            tier => display = $"Yes - {tier.Name}",
            () => display = "Failed",
            () => display = "No");

        return display;
    }

    protected sealed class AccuracyOption
    {
        public AccuracyOption(string label, QualitativeScoring value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }

        public QualitativeScoring Value { get; }
    }

    protected sealed class MatchScoutQrPayload
    {
        public int? TeamNumber { get; set; }
        public string? Alliance { get; set; }
        public string? StartingPosition { get; set; }
        public string? PreloadedFuel { get; set; }
        public string? CollectedFuel { get; set; }
        public string? ShotFuel { get; set; }
        public string? AutoShootingAccuracy { get; set; }
        public CompactEndgameResultPayload? AutoL1Climb { get; set; }
        public CompactEndgameResultPayload? EndgameClimb { get; set; }
        public string? TeleopFuelPerCycle { get; set; }
        public string? TeleopShootingAccuracy { get; set; }
        public bool IsShootingWhileMoving { get; set; }
        public bool IsShootingFromCertainPositionsOnly { get; set; }
        public int TeleopCycleCount { get; set; }
        public bool HadDefensePlayedAgainstThem { get; set; }
        public string? DefenseHandling { get; set; }
        public string? FuelCollectionSource { get; set; }
        public string? FuelShuttlingMethod { get; set; }
        public string? FuelShuttlingEffectiveness { get; set; }
        public bool PlayedDefense { get; set; }
        public string? DefenseEffectiveness { get; set; }
        public string? DriverSkill { get; set; }
        public string? RobotReliability { get; set; }
        public bool HadRobotIssues { get; set; }
        public string? RobotIssues { get; set; }
        public string? AdditionalThoughts { get; set; }
    }

    protected sealed class CompactMatchScoutQrPayload
    {
        public int? Tn { get; set; }
        public string? Al { get; set; }
        public string? Sp { get; set; }
        public string? Pf { get; set; }
        public string? Cf { get; set; }
        public string? Sf { get; set; }
        public string? Asa { get; set; }
        public CompactEndgameResultPayload? Ac { get; set; }
        public string? Tfc { get; set; }
        public string? Tsa { get; set; }
        public bool Swm { get; set; }
        public bool Scp { get; set; }
        public int Tcy { get; set; }
        public bool Dpa { get; set; }
        public string? Dhr { get; set; }

        public string? Fcs { get; set; }
        public string? Fsm { get; set; }
        public string? Fse { get; set; }
        public bool Pld { get; set; }
        public string? Pde { get; set; }
        public CompactEndgameResultPayload? Eg { get; set; }
        public string? Drv { get; set; }
        public string? Rel { get; set; }
        public bool Hri { get; set; }
        public string? Rin { get; set; }
        public string? Add { get; set; }
    }

    protected sealed class CompactEndgameResultPayload
    {
        public string T { get; set; } = "N";
        public int? L { get; set; }
    }
    
    private static string MapQualitative(QualitativeScoring scoring)
    {
        if (scoring.Equals(QualitativeScoring.Poor))
        {
            return "P";
        }

        if (scoring.Equals(QualitativeScoring.BelowAverage))
        {
            return "BA";
        }

        if (scoring.Equals(QualitativeScoring.Average))
        {
            return "A";
        }

        if (scoring.Equals(QualitativeScoring.AboveAverage))
        {
            return "AA";
        }

        if (scoring.Equals(QualitativeScoring.Excellent))
        {
            return "E";
        }

        return "A";
    }

    private static CompactEndgameResultPayload MapEndgameResultCompact(EndgameResult result)
    {
        var payload = new CompactEndgameResultPayload
        {
            T = "N"
        };

        result.Match(
            tier =>
            {
                payload.T = "S";
                payload.L = (int)tier.Level;
            },
            () => payload.T = "F",
            () => payload.T = "N");

        return payload;
    }
}