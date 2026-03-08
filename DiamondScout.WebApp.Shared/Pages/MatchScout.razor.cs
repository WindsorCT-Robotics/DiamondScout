using Microsoft.AspNetCore.Components;

namespace DiamondScout.WebApp.Shared.Pages;

public partial class MatchScout : ComponentBase
{
    protected int? TeamNumber { get; set; }

    protected string SelectedAlliance { get; set; } = "Red";
    
    protected string? StartingPosition { get; set; }

    protected bool? PreloadedFuel { get; set; } = true;

    protected IReadOnlyList<string> AllianceOptions { get; } =
        new[] { "Red", "Blue" };
    
    protected IReadOnlyList<string> StartingOptions { get; } =
        new[] { "Hub", "Bump", "Trench" };
    
    protected IReadOnlyList<string> PreloadFuelBool { get; } =
        new[] { "Hub", "Bump", "Trench" };
}