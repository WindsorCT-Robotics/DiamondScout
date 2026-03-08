using Microsoft.AspNetCore.Components;
using ParagonRobotics.DiamondScout.Common;

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

    protected string AutoShootingAccuracyDisplay =>
        AutoAccuracyOptions
            .FirstOrDefault(option => option.Value.Equals(AutoShootingAccuracy))
            ?.Label
        ?? "Average";

    protected void OnAutoAccuracyChanged(ChangeEventArgs e)
    {
        var selectedLabel = e.Value?.ToString() ?? "Average";

        AutoShootingAccuracy = AutoAccuracyOptions
            .FirstOrDefault(option => option.Label == selectedLabel)
            ?.Value
            ?? QualitativeScoring.Average;
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
}