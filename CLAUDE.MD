# TornBounty Project

## Overview
Windows Forms application for viewing bounties from the TORN online game API (v2 only).

## Tech Stack
- .NET 9.0 (net9.0-windows)
- Windows Forms (WinForms)
- C# with nullable enabled, implicit usings enabled
- System.Text.Json for JSON deserialization (no third-party packages)

## Project Structure
```
TornBounty/
  TornBounty/
    Models/
      BountyModels.cs       # API response DTOs (BountyResponse, Bounty, Metadata, PaginationLinks)
    Services/
      TornApiService.cs     # HttpClient-based service for Torn API v2 calls
    Form1.cs                # Main form logic (fetch, sort, pagination)
    Form1.Designer.cs       # UI layout (DataGridView, pagination buttons, API key input)
    Program.cs              # Entry point
    TornBounty.csproj       # Project file
  bounties.json             # Sample API response for reference
```

## API Details
- Base URL: `https://api.torn.com/v2/torn/bounties`
- Authentication: API key passed as `?key=YOUR_API_KEY` query parameter
- Only use Torn API **v2** - never v1
- Pagination: `_metadata.links.prev` and `_metadata.links.next` URLs from response

## Build & Run
```bash
cd TornBounty
dotnet build
dotnet run
```

## Conventions
- Use `System.Text.Json` with `[JsonPropertyName]` attributes for JSON mapping
- Use `sealed` on classes not intended for inheritance
- Use async/await with CancellationToken for API calls
- Handle nullable references properly (nullable enabled)
- Keep UI responsive: disable controls during loading, show status messages
- Sort by clicking column headers (target_level and reward columns supported)
