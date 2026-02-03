using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Collections;
using Game = ParagonRobotics.DiamondScout.Common.GameData.Game;

namespace DiamondScout.Web.Services;

public sealed class GameYearCatalog
{
    // What the UI needs (keeps Blazor pages simple)
    public sealed record SelectionResult(int Year, string Status, Game? Game);

    public IReadOnlyList<int> GetSelectableYears(DateOnly today)
        => Enumerable.Range(today.Year, 6).ToList(); // current year through +5

    public SelectionResult GetSelection(int year, DateOnly today)
    {
        var isReleased = today >= new DateOnly(year, 2, 1);

        if (!isReleased)
            return new SelectionResult(year, "To Be Determined", null);

        if (_knownGames.TryGetValue(year, out var game))
            return new SelectionResult(year, "Released", game);

        return new SelectionResult(year, "Missing configuration", null);
    }

    private static readonly Dictionary<int, Game> _knownGames = new()
    {
      
        [2026] = new Game(
             year: new DateOnly(2026, 1, 1),
             name: "Rebuilt",
             phases: ListModule.Empty<ParagonRobotics.DiamondScout.Common.Identifiers.SubPhaseId>(),
             gamePieces: ListModule.Empty<ParagonRobotics.DiamondScout.Common.Identifiers.GamePieceId>(),
             infractions: ListModule.Empty<ParagonRobotics.DiamondScout.Common.Identifiers.InfractionId>(),
             pitResults: ListModule.Empty<ParagonRobotics.DiamondScout.Common.Identifiers.RobotId>(),
             events: ListModule.Empty<ParagonRobotics.DiamondScout.Common.Identifiers.EventId>(),
             parameters: ListModule.Empty<ParagonRobotics.DiamondScout.Common.Parameters.ParameterDefinition>()
        )
    };
}