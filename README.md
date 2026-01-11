# DiamondScout by Paragon Robotics

This is the repository for Paragon Robotics' Scouting App, DiamondScount.

## Goals

One of the goals of the app is to teach the programming students Blazor PWA as a way to introduce and familiarize them
with .NET, a widely-used enterprise language. With Java and .NET under their belts, the students will have more
opportunities open to them come graduation.

The main goal of the app is to be a reusable. extensible, flexible app for scouting. The app should record both commmon
metrics between games, and game-specific metrics that we wish to rank teams on.

The app should be usable by other teams for the same purpose if desired; metrics should be customizable per team.

## Target Platforms

The front-end target platforms are Web, Desktop (Windows and Linux), and Android, supported by Blazor PWA.

Mac and iOS would be nice, but would require volunteers with dev machines running XCode.

The back-end target will be .NET 10.

## The Stack

The languages in the app will be F# and C#, with F# handling most of the domain modeling work and C# handling
everything else (unless a problem is more suited to the strengths of F#).

The front-end will be [reactive](https://reactivex.io/) and [event-driven](https://en.wikipedia.org/wiki/Event-driven_architecture), and the back-end will be [event-sourced](https://martinfowler.com/eaaDev/EventSourcing.html). A UI
being event-driven is pretty much the norm for modern UIs. Reactive UIs are an evolution of the standard event-driven UI
model. Event-sourced backends instead of storing data directly store the events and their associated metadata, which
allows for the data to be reconstructed and analyzed in more ways, but does add complexity in terms of versioning events.

The events will be stored in Kurrent, and the events will populate a SQL database (TBD). The front end will be
Blazor PWA.. Middlewares TBD.

[Saturn](https://saturnframework.org/) for back-end services, [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/#/)
for type-safe API calls, and a custom library for re-hydrating a Google Sheets spreadsheet based on the event store.
