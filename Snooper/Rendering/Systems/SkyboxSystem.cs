using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Snooper.Core.Containers.Buffers;
using Snooper.Core.Containers.Programs;
using Snooper.Rendering.Components;
using Snooper.Rendering.Components.Camera;
using Snooper.Rendering.Components.Skybox;

namespace Snooper.Rendering.Systems;

public class SkyboxSystem : PrimitiveSystem<CubeComponent>
{
    public override uint Order => 1;
    protected override bool AllowDerivation => true;
    protected override Dictionary<CommandBufferType, ShaderProgram> Shaders { get; } = new()
    {
        [CommandBufferType.Transparent] = new EmbeddedShader("Skybox/skybox")
    };

    protected override void PreRender(CameraComponent camera, ShaderProgram shader)
    {
        var view = camera.ViewMatrix;
        view.M41 = 0;
        view.M42 = 0;
        view.M43 = 0;

        shader.Use();
        shader.SetUniform("uViewMatrix", view);
        shader.SetUniform("uProjectionMatrix", camera.ProjectionMatrix);

        switch (_component)
        {
            case CubemapComponent cubemap:
            {
                if (!cubemap.IsLoaded)
                    LoadCubemap(cubemap);

                shader.SetUniform("uUseCubemap", true);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.TextureCubeMap, cubemap.TextureHandle);
                shader.SetUniform("uCubemap", 0);
                break;
            }
            case AtmosphericComponent atmospheric:
            {
                shader.SetUniform("uUseCubemap", false);
                shader.SetUniform("uSunPos", atmospheric.Sun.Position);
                shader.SetUniform("uSunIntensity", atmospheric.Sun.Intensity);
                shader.SetUniform("uSunRadius", atmospheric.Sun.Radius);
                shader.SetUniform("uSunAtmosphereRadius", atmospheric.Sun.AtmosphereRadius);
                break;
            }
        }

        GL.DepthFunc(DepthFunction.Lequal);
        GL.DepthMask(false);
    }

    protected override void PostRender(CameraComponent camera, ShaderProgram shader)
    {
        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Less);
        shader.Unuse();
    }

    private static void LoadCubemap(CubemapComponent cubemap)
    {
        if (cubemap.TextureHandle == -1)
            cubemap.TextureHandle = GL.GenTexture();

        GL.BindTexture(TextureTarget.TextureCubeMap, cubemap.TextureHandle);

        for (var i = 0; i < cubemap.Faces.Length; i++)
        {
            using var image = Image.Load<Rgba32>(cubemap.Faces[i]);
            var pixels = new byte[4 * image.Width * image.Height];
            image.CopyPixelDataTo(pixels);

            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        }

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

        cubemap.IsLoaded = true;
    }

    protected override void OnActorComponentEnqueued(CubeComponent component)
    {
        base.OnActorComponentEnqueued(component);

        if (_component is not null)
            throw new InvalidOperationException("Only one SkyboxComponent can be added to the system at a time.");

        _component = component;
    }

    private CubeComponent? _component;
}
