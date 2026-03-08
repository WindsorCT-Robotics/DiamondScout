using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using ParagonRobotics.DiamondScout.Common;

namespace DiamondScout.WebApp.Shared.Pages;

public partial class DeviceSetup
{
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Inject] NavigationManager Nav { get; set; } = default!;

    protected string Name = "";
    protected string SelectedRole = "";

    protected List<string> AvailableUsers { get; set; } = [];
    protected List<string> AvailableRoles { get; set; } = [];

    protected bool ShowResetPasswordPrompt = false;
    protected string ResetPassword = "";
    protected string? ResetError;
    protected string? ScanError;

    private List<ScannedUser> _scannedUsers = [];

    private sealed class ScanPayload
    {
        public List<ScannedUser> Users { get; set; } = [];
    }

    private sealed class ScannedUser
    {
        public string Name { get; set; } = "";
        public List<string> Roles { get; set; } = [];
    }

    protected async Task HandleScan(string scannedValue)
    {
        try
        {
            ScanError = null;

            var payload = JsonSerializer.Deserialize<ScanPayload>(
                scannedValue,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (payload is null || payload.Users.Count == 0)
            {
                ScanError = "The scanned QR code did not contain any users.";
                return;
            }

            _scannedUsers = payload.Users
                .Where(u => !string.IsNullOrWhiteSpace(u.Name))
                .ToList();

            if (_scannedUsers.Count == 0)
            {
                ScanError = "The scanned QR code did not contain valid users.";
                return;
            }

            AvailableUsers = _scannedUsers
                .Select(u => u.Name)
                .Distinct()
                .ToList();

            Name = AvailableUsers.First();

            UpdateRolesForSelectedUser();

            await InvokeAsync(StateHasChanged);
        }
        catch (JsonException)
        {
            ScanError = "The scanned QR code was not valid JSON.";
        }
    }

    protected Task OnUserChangedInternal(ChangeEventArgs e)
    {
        Name = e.Value?.ToString() ?? "";
        UpdateRolesForSelectedUser();
        return Task.CompletedTask;
    }

    private void UpdateRolesForSelectedUser()
    {
        var selectedUser = _scannedUsers.FirstOrDefault(u => u.Name == Name);

        AvailableRoles = selectedUser?.Roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct()
            .ToList() ?? [];

        SelectedRole = AvailableRoles.FirstOrDefault() ?? "";
    }

    protected async Task InitializeDevice()
    {
        var userId = UserId.NewUserId(Guid.NewGuid());

        var role = SelectedRole switch
        {
            "Admin" => Role.Admin,
            "Viewer" => Role.Viewer,
            _ => Role.Scouter
        };

        var user = new User(Name, role, true);

        var payload = new
        {
            UserId = userId.Item.ToString(),
            Name = user.Name,
            Role = user.Role.ToString()
        };

        await JS.InvokeVoidAsync(
            "localStorage.setItem",
            "diamondscout_user",
            JsonSerializer.Serialize(payload));

        Nav.NavigateTo("/");
    }

    protected Task ResetUserData()
    {
        ShowResetPasswordPrompt = true;
        return Task.CompletedTask;
    }

    protected async Task ConfirmResetPassword()
    {
        const string correctPassword = "0571"; // move to config later

        if (ResetPassword != correctPassword)
        {
            ResetError = "Incorrect password.";
            return;
        }

        ResetError = null;
        ShowResetPasswordPrompt = false;

        await JS.InvokeVoidAsync("localStorage.removeItem", "diamondscout_user");

        Nav.NavigateTo("/devicesetup", forceLoad: true);
    }
}