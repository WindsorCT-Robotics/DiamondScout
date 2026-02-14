using Microsoft.AspNetCore.Components;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using ParagonRobotics.DiamondScout.Common;

using Events = ParagonRobotics.DiamondScout.Common.Events;
using Identifiers = ParagonRobotics.DiamondScout.Common.Identifiers;
using MatchData = ParagonRobotics.DiamondScout.Common.MatchData;
using Scoring = ParagonRobotics.DiamondScout.Common.Scoring;
using Teams = ParagonRobotics.DiamondScout.Common.Teams;

namespace DiamondScout.Frontend.Pages;

public partial class EventConfig : ComponentBase
{
    private Events.Event? currentEvent;
    private string? currentEventKey;
    private List<MatchData.Match> matches = new();
    private int nextMatchId = 1;
    private bool isLoadingMatches = false;
    private string? apiStatusMessage;
    
    private NewEventModel newEventModel = new();
    private NewMatchModel newMatchModel = new();

    private void CreateEvent()
    {
        currentEvent = Events.create(newEventModel.Name ?? "", ListModule.Empty<Identifiers.MatchId>());
        currentEventKey = newEventModel.EventKey;
        apiStatusMessage = null;
        newEventModel = new NewEventModel();
        StateHasChanged();
    }

    private async Task LoadMatchesFromAPI()
    {
        if (string.IsNullOrEmpty(currentEventKey))
        {
            apiStatusMessage = "Error: No event key provided. Please create a new event with a valid FIRST event key.";
            return;
        }

        isLoadingMatches = true;
        apiStatusMessage = "Loading matches from FIRST API...";
        StateHasChanged();

        try
        {
            // TODO: Implement actual FIRST API call
            // For now, simulate loading with some example matches
            await SimulateAPICall();
            
            apiStatusMessage = $"Successfully loaded {matches.Count} matches from FIRST API.";
        }
        catch (Exception ex)
        {
            apiStatusMessage = $"Error loading matches from API: {ex.Message}";
        }
        finally
        {
            isLoadingMatches = false;
            StateHasChanged();
        }
    }

    private async Task SimulateAPICall()
    {
        // Simulate API delay
        await Task.Delay(2000);

        // Simulate loading qualification matches (typically 1-100+ for a competition)
        matches.Clear();
        for (int i = 1; i <= 12; i++) // Example: 12 qualification matches
        {
            var matchId = Identifiers.MatchId.NewMatchId(nextMatchId++);
            if (currentEvent != null)
            {
                currentEvent = Events.add(matchId, currentEvent);
            }

            var matchNumber = MatchData.MatchNumber.NewMatchNumber((uint)i);
            var match = MatchData.createMatch(matchNumber, ListModule.Empty<MatchData.MatchScoutResult>());
            matches.Add(match);
        }

        // TODO: Replace with actual FIRST API integration
        // Example API endpoint: https://www.thebluealliance.com/api/v3/event/{event_key}/matches
        // You would parse the JSON response and create Match objects with the actual teams
    }

    private void AddMatchManually()
    {
        if (currentEvent != null)
        {
            var matchId = Identifiers.MatchId.NewMatchId(nextMatchId++);
            currentEvent = Events.add(matchId, currentEvent);

            var matchNumber = MatchData.MatchNumber.NewMatchNumber((uint)newMatchModel.MatchNumber);
            var match = MatchData.createMatch(matchNumber, ListModule.Empty<MatchData.MatchScoutResult>());
            matches.Add(match);

            newMatchModel = new NewMatchModel { MatchNumber = newMatchModel.MatchNumber + 1 };
            apiStatusMessage = $"Manually added Match {matchNumber.Item}.";
            StateHasChanged();
        }
    }

    private void ResetEvent()
    {
        currentEvent = null;
        currentEventKey = null;
        matches.Clear();
        nextMatchId = 1;
        apiStatusMessage = null;
        newEventModel = new NewEventModel();
        newMatchModel = new NewMatchModel();
        StateHasChanged();
    }

    public class NewEventModel
    {
        public string? Name { get; set; } = "";
        public string? EventKey { get; set; } = "";
    }

    public class NewMatchModel
    {
        public int MatchNumber { get; set; } = 1;
    }
}