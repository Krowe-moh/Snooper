using Snooper.Core;
using Snooper.Rendering.Systems;

namespace Snooper.Rendering.Components.Skybox;

[DefaultActorSystem(typeof(SkyboxSystem))]
public class CubemapComponent : CubeComponent
{
    public string[] Faces =
    {
        "px.png", "nx.png",
        "py.png", "ny.png",
        "pz.png", "nz.png"
    };

    internal int TextureHandle = -1;
    internal bool IsLoaded;

    public override string Icon => "\uf03e";
}
