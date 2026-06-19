# PsDotNet

A strongly typed, intuitive C# library for automating Adobe Photoshop via COM.

[![NuGet](https://img.shields.io/nuget/v/PsDotNet.svg)](https://www.nuget.org/packages/PsDotNet)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Install

```powershell
dotnet add package PsDotNet
```

```csharp
using PsDotNet;

IPsApplication app = PsConnection.StartAndConnect();
IPsDocument doc = app.Documents.Add(512, 512, 72, "MyDoc", EPsDocumentMode.psRGB);
```

## Quick start

A complete working sample is in [`samples/PsDotNet.QuickStart`](samples/PsDotNet.QuickStart).

```powershell
git clone https://github.com/nodewerks/psdotnet.git
cd psdotnet/samples/PsDotNet.QuickStart
dotnet run
```

## Listener (download)

The PsDotNet Listener is a WPF UI for inspecting Photoshop's COM type system, ID containers, and JS sections. Useful when developing custom Photoshop tooling on top of PsDotNet.

Download the latest from the [**Releases**](https://github.com/nodewerks/psdotnet/releases) page.

## Supported Photoshop versions

CS3 through CC2025.

## Requirements

- Windows
- .NET 8 or newer
- Adobe Photoshop installed

## License

MIT — see [LICENSE](LICENSE).

## Contributing

Source code is maintained privately. Please [open an issue](https://github.com/nodewerks/psdotnet/issues) for bug reports and feature requests — pull requests on this repo are not accepted (it's a one-way mirror). See [CONTRIBUTING.md](CONTRIBUTING.md) for details.
