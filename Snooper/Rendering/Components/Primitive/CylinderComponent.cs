using System.Numerics;
using CUE4Parse.UE4.Assets.Exports.Component;
using Snooper.Rendering.Components.Descriptors;

namespace Snooper.Rendering.Components.Primitive;

public class CylinderComponent : ShapeComponent
{
    public CylinderComponent(UCylinderComponent component) : base(component)
    {
        Color ??= new Vector3(0.15f, 0.15f, 0.45f);

        const float defaultRadius = 0.5f;

        var radius = defaultRadius;
        if (component.TryGetValue(out float collisionRadius, "CollisionRadius"))
        {
            radius = collisionRadius * Settings.GlobalScale;
        }

        const float defaultHeight = 1.0f;

        var height = defaultHeight;
        if (component.TryGetValue(out float collisionHeight, "CollisionHeight"))
        {
            height = collisionHeight * Settings.GlobalScale;
        }

        var bounds = new CullingBounds(Vector3.Zero, new Vector3(radius, height / 2.0f, radius));
        Descriptor = new PrimitiveDescriptor<Vector3>(bounds, () => new Geometry(radius, height / 2.0f));

        Materials[0].InlineContainer = new MaterialDataContainer(Color.Value, LineThickness);
    }

    private class Geometry : DebugGeometry
    {
        public Geometry(float radius, float halfHeight) : this(Vector3.Zero, radius, halfHeight)
        {

        }

        public Geometry(Vector3 center, float radius, float halfHeight)
        {
            const int segments = 32;

            var vertices = new List<Vector3>();

            var topCenter = center with { Y = center.Y + halfHeight };
            var bottomCenter = center with { Y = center.Y - halfHeight };

            // Bottom circle
            for (var i = 0; i < segments; i++)
            {
                var angle1 = 2.0f * MathF.PI * i / segments;
                var angle2 = 2.0f * MathF.PI * (i + 1) / segments;

                var p1 = new Vector3(
                    bottomCenter.X + radius * MathF.Cos(angle1),
                    bottomCenter.Y,
                    bottomCenter.Z + radius * MathF.Sin(angle1)
                );

                var p2 = new Vector3(
                    bottomCenter.X + radius * MathF.Cos(angle2),
                    bottomCenter.Y,
                    bottomCenter.Z + radius * MathF.Sin(angle2)
                );

                vertices.Add(p1);
                vertices.Add(p2);
            }

            // Top circle
            for (var i = 0; i < segments; i++)
            {
                var angle1 = 2.0f * MathF.PI * i / segments;
                var angle2 = 2.0f * MathF.PI * (i + 1) / segments;

                var p1 = new Vector3(
                    topCenter.X + radius * MathF.Cos(angle1),
                    topCenter.Y,
                    topCenter.Z + radius * MathF.Sin(angle1)
                );

                var p2 = new Vector3(
                    topCenter.X + radius * MathF.Cos(angle2),
                    topCenter.Y,
                    topCenter.Z + radius * MathF.Sin(angle2)
                );

                vertices.Add(p1);
                vertices.Add(p2);
            }

            // Vertical lines
            for (var i = 0; i < 4; i++)
            {
                var angle = MathF.PI / 2.0f * i;

                var bottomPoint = new Vector3(
                    bottomCenter.X + radius * MathF.Cos(angle),
                    bottomCenter.Y,
                    bottomCenter.Z + radius * MathF.Sin(angle)
                );

                var topPoint = new Vector3(
                    topCenter.X + radius * MathF.Cos(angle),
                    topCenter.Y,
                    topCenter.Z + radius * MathF.Sin(angle)
                );

                vertices.Add(bottomPoint);
                vertices.Add(topPoint);
            }

            Vertices = vertices.ToArray();

            Indices = new uint[Vertices.Length];
            for (uint i = 0; i < Indices.Length; i++)
            {
                Indices[i] = i;
            }
        }
    }
}
