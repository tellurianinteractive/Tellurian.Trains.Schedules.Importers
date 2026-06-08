// Minimal static host for a *published* standalone Blazor WebAssembly app.
// Serves the framework files (correct .wasm content type + compressed assets) and
// falls back to index.html so client-side routes (e.g. /workspace) resolve.
//
// Usage:
//   dotnet run --project tools/AppServer -c Release -- --path <publish>/wwwroot --urls http://localhost:5098

static string? GetArg(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

var path = GetArg(args, "--path") ?? Directory.GetCurrentDirectory();
var urls = GetArg(args, "--urls") ?? "http://localhost:5098";
var fullPath = Path.GetFullPath(path);

if (!File.Exists(Path.Combine(fullPath, "index.html")))
{
    Console.Error.WriteLine($"No index.html found in '{fullPath}'. Did you publish the app first?");
    return 1;
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Path.GetDirectoryName(fullPath),
    WebRootPath = Path.GetFileName(fullPath)
});

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

Console.WriteLine($"Serving {fullPath} at {urls}");
app.Run(urls);
return 0;
