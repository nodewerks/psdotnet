# PsDotNet.QuickStart

A minimal sample showing how to use the [PsDotNet](https://www.nuget.org/packages/PsDotNet) library to drive Adobe Photoshop from C#.

## Prerequisites

- Windows
- .NET 8 SDK
- Adobe Photoshop installed (CS6 or newer)

## Run

```powershell
dotnet run --project PsDotNet.QuickStart.csproj
```

The sample creates a 512×512 RGB document, applies several filter effects to a layer, adds styled text with an outer-glow + stroke layer style, flattens, and saves a PNG to your Desktop as `sampleDocument.png`.

## What it shows

- Connecting to a specific Photoshop version (`PsConnection.StartAndConnect("2024")`)
- Creating documents and layers
- Applying filters (Clouds, Twirl, Chalk and Charcoal, Accented Edges)
- Working with text layers, fonts, and color
- Building and applying layer styles
- Saving in different file formats

See [`Program.cs`](Program.cs) for the full code (~115 lines, well-commented).

## License

MIT.
