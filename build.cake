var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var artifactsDir = Directory("./artifacts");
var solution = "./Stuntman.sln";

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(solution);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreBuild(solution, new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = new DotNetCoreMSBuildSettings
        {
            TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
            Verbosity = DotNetCoreVerbosity.Minimal
        }

        // msbuild.log specified explicitly, see https://github.com/cake-build/cake/issues/1764
        .AddFileLogger(new MSBuildFileLoggerSettings { LogFile = "msbuild.log" })
    });
});

Task("Run-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projectFiles = GetFiles("./tests/**/*.csproj");
    foreach(var file in projectFiles)
    {
        DotNetCoreTool(file, "xunit", "-configuration " + configuration);
    }
});

Task("Package")
    .IsDependentOn("Run-Tests")
    .Does(() =>
{
    DotNetCorePack("./src/Core/Core.csproj", new DotNetCorePackSettings
    {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = artifactsDir
    });
});

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);
