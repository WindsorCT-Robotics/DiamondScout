using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ParagonRobotics.DiamondScout.Common;

namespace DiamondScout.WebApp.Shared.Pages;

public partial class DeviceSetup
{
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Inject] NavigationManager Nav { get; set; } = default!;

    protected string Name = "";
    protected string SelectedRole = "Scouter";

    protected bool ShowResetPasswordPrompt = false;
    protected string ResetPassword = "";
    protected string? ResetError;
    
    
    private record User(string Name, string Role);
    
    protected async Task InitializeDevice()
    {
        var user = JsonSerializer.Deserialize<User>(await JS.InvokeAsync<string>("localStorage.getItem", "diamondscout_user"));
        if (user != null)
        {
            Name = user.Name;
            SelectedRole = user.Role;
        }
    }

    protected async Task ResetUserData()
    {
        ShowResetPasswordPrompt = true;
    }
    
    protected async Task ConfirmResetPassword()
    {
        const string CorrectPassword = "0571"; // move to config later

        if (ResetPassword != CorrectPassword)
        {
            ResetError = "Incorrect password.";
            return;
        }

        ResetError = null;
        ShowResetPasswordPrompt = false;

        await JS.InvokeVoidAsync("localStorage.removeItem", "diamondscout_user");

        Nav.NavigateTo("/usersetup", forceLoad: true);
    }
}