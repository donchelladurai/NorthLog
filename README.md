# NorthLog

Instead of pulling together bits of code that I could talk about, I decided to build a little WPF app, using the tech stack listed in the job spec. 

The app contains both good and bad code, which I have narrated in the files themselves. I'm happy to talk about them in more detail during the call. 

## What it does

NorthLog is meant for geologists that I have assumed are working on a rig in the North Sea. A geologist picks a wellbore, sees its current details and report timeline, and submits a daily drilling report. 

## Tech

- .NET 10, WPF (`net10.0-windows`)
- Clean architecture with vertical feature slices
- Wolverine for CQRS dispatch, with its FluentValidation middleware for input shape
- A hand-rolled `Result<T>` for application-layer return values (deliberately not ErrorOr/OneOf — wanted the result pattern visible in the codebase, since we talked briefly about design patterns in the initial call)
- EF Core with the in-memory provider (chosen so the project runs anywhere with no setup; the abstractions are unchanged for SQL Server or another DB)
- Hand-rolled MVVM (`ViewModelBase`, `RelayCommand`, `AsyncRelayCommand`) so the plumbing is visible rather than hidden behind a source generator

## How to run it

Open `NorthLog.sln` in Visual Studio 2026 (or any recent VS that supports .NET 10 + WPF), set `NorthLog.Wpf` as the startup project, press F5. No external dependencies — it uses EF Core's in-memory provider, so the seed data populates on first launch and lives for the duration of the session.

## Project layout & Layout

```
NorthLog.Domain          — entities, aggregate root, invariants. No external deps.
NorthLog.Application     — Wolverine handlers, validators, DTOs. Knows Domain.
NorthLog.Infrastructure  — EF Core context, configurations, seed. Knows Application.
NorthLog.Wpf             — composition root + UI. Knows the rest, only to wire DI.
NorthLog.Domain.Tests    — invariant tests on the Wellbore aggregate.
```

The dependency arrows all point inward toward `Domain`. I wanted to hnighlight here the single most important property of clean architecture — if the arrows ever point the wrong way, the architecture is broken regardless of how nicely-named the folders are.

## How to read this codebase

There are two halves on purpose.

**The good half** — `Domain`, `Application`, `Infrastructure`, and `MainWindow` / `MainViewModel` in `Wpf`. This is how I'd structure a production feature: rich aggregate enforcing its own invariants, vertical slices in the application layer, EF configuration extracted per-entity, MVVM with proper async commands and per-operation DI scoping.

**The bad foil** — `LegacyReportsWindow`, `AppContext`, `FormationHelper`, and a deliberately-bad `GetWellboreDetailsHandler` that throws an exception where it should return a Result. Realistic anti-patterns I've planted on purpose — the kind that I have seem accumulate in real codebases. Each bad spot has a `// To the interviewers:` comment explaining what's wrong and what I'd do instead.

The two implementations of "submit a daily report" are the contrast worth looking at:

---

If you spot something that looks deliberately wrong — it probably is, and there's a comment nearby explaining why. If something looks subtly wrong without a comment — that's a real one and I'd genuinely like to know about it.

Thanks for taking the time to read it.
