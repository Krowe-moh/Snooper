using System.Numerics;
using CUE4Parse.Compression;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider.Usmap;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.Engine;
using CUE4Parse.UE4.Versions;
using Editor;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SharpLzo;
using Snooper.Rendering.Actors;
using Snooper.Rendering.Components;
using Snooper.Rendering.Components.Light;
using Snooper.Rendering.Components.Primitive;
using Snooper.Rendering.Components.Skybox;
using Snooper.Rendering.Components.Transforms;

Compression.UseLZO(static (source, destination, out written) =>
{
    var result = Lzo.TryDecompress(source, source.Length, destination, out written);
    return result == LzoResult.OK;
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .MinimumLevel.Information()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}]: {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Literate)
    .WriteTo.Sink(ImGuiSink.Instance)
    .CreateLogger();

OodleHelper.Initialize();
ZlibHelper.Initialize(ZlibHelper.DLL_NAME);

const string dir = @"C:\Program Files\Epic Games\rocketleague\TAGame\CookedPCConsole";
const string mapping = @"C:\Users\krowe\Downloads\RocketLeague.usmap";
var version = new VersionContainer(EGame.GAME_RocketLeague);

var provider = new DefaultFileProvider(dir, SearchOption.AllDirectories, version, StringComparer.OrdinalIgnoreCase);
if (!string.IsNullOrEmpty(mapping))
    provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mapping);
provider.Initialize();
provider.PostMount();
provider.LoadVirtualPaths();

var snooper = new EditorWindow(60, 1500, 900, provider, false);
var scene = new Actor("Example Scene");
scene.Components.Add(new BoxComponent(Vector3.Zero, Vector3.One));

var camera = new CameraActor("Camera");
camera.CameraComponent.LocalTransform.Position = new Vector3(1, 2, -0.5f);
camera.CameraComponent.LocalTransform.Rotation = new Quaternion(0, -1, 0, 1);
scene.Children.Add(camera);

var grid = new Actor("Grid");
grid.Components.Add(new GridComponent());
//scene.Children.Add(grid);

var cubemap = new Actor("Skybox");
cubemap.Components.Add(new CubemapComponent());
scene.Children.Add(cubemap);

var sun = new Actor("Sun Light");
sun.Components.Add(new DirectionalLightComponent(MathF.PI, new Vector3(1.0f, 0.87f, 0.72f), new Transform(new Quaternion(new Vector3(0.5f, -0.5f, 0.0f), 1.0f)), "Directional Light"));
scene.Children.Add(sun);

scene.Children.Add(new WorldActor(provider.LoadPackageObject<UWorld>("CookedPCConsole/Labs_Basin_P.TheWorld")));

snooper.Manager.LoadScene(scene);
snooper.Run();
