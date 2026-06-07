# Genova.Crawler

Provides a .NET library for crawling a website and returning structured details about discovered resources.

> [!WARNING]
> This codebase is part of the Genova platform and should not be considered production-ready. It is published as source for review, experimentation, and reuse within Genova-related projects.

> [!IMPORTANT]
> A fresh public clone of this repository should not be expected to restore or build without additional Genova infrastructure. Many Genova dependencies are distributed through a private authenticated NuGet feed, and the public source does not include feed credentials or a complete public package graph.

## Installation

Add a reference to `Genova.Crawler` from your project, then restore and build:

```bash
dotnet restore
dotnet build
```

## Usage

```csharp
var crawler = new Crawler(followingClient, nonFollowingClient, new CrawlOptions
{
    BaseUrl = "https://example.com",
});

await foreach (var resource in crawler.DiscoverResourcesAsync())
{
    Console.WriteLine(resource.Url);
}
```

## Features

* Crawls from a base URL and optional starting paths
* Fetches individual resources with custom HTTP methods, headers, body, or form data
* Returns structured resource details such as status code, headers, cookies, content type, and response time
* Extracts linked resources from HTML and XML sitemap content
* Supports simple filename extension filtering and request throttling

## Notes

This library is part of the Genova platform and is intended to be consumed by other components rather than used as a standalone application.

## Thanks

* AngleSharp

## Third-Party Notices

This project has direct runtime dependencies on third-party NuGet packages, including `AngleSharp` (MIT). See each package's NuGet license metadata for full license and notice terms.

## License

GNU General Public License v3.0. See the `LICENSE` file for details.
