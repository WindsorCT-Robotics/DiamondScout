using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ParagonRobotics.DiamondScout.Common;

namespace DiamondScout.WebApp.Shared.Pages;

public partial class UserSetup
{
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Inject] NavigationManager Nav { get; set; } = default!;

    protected string Name = "";
    protected string SelectedRole = "Scouter";

    protected async Task InitializeTablet()
    {
        var userId = UserId.NewUserId(Guid.NewGuid());

        var role = SelectedRole switch
        {
            "Admin" => Role.Admin,
            "Viewer" => Role.Viewer,
            _ => Role.Scouter
        };

        var user = new User(Name, role);

        var payload = new
        {
            UserId = userId,
            Name = user.Name,
            Role = user.Role.ToString()
        };

        await JS.InvokeVoidAsync(
            "localStorage.setItem",
            "diamondscout_user",
            JsonSerializer.Serialize(payload));

        Nav.NavigateTo("/");
    }
}