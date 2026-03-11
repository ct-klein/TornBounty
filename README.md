# TornBounty

A Windows desktop application for browsing active bounties in the online game [TORN](https://www.torn.com). Built with .NET 9 and Windows Forms.

## Features

- Fetches live bounty data from the **Torn API v2**
- Paginate through results with Previous / Next navigation
- Sort bounties by **target level** or **reward** by clicking column headers
- Displays record count and fetch timestamp
- No third-party dependencies -- uses only `System.Text.Json`

## Requirements

- Windows 10 or later
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A valid [Torn API key](https://www.torn.com/preferences.php#tab=api)

## Getting Started

```bash
git clone https://github.com/ct-klein/TornBounty.git
cd TornBounty/TornBounty
dotnet run
```

Enter your API key in the text field and click **Fetch Bounties** to load data.

## Screenshot

_Coming soon_

## License

This project is provided as-is for personal use.
