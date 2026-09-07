using CUE4Parse.UE4.Assets.Exports.Animation;
using CUE4Parse.UE4.Objects.UObject;
using ImGuiNET;
using Snooper.Extensions;
using Snooper.UI;

namespace Snooper.Rendering.Components.Descriptors.Animations;

public abstract class AnimationDescriptor : IControllable
{
    public string Name { get; }
    public string Path { get; }

    public readonly SkeletonDescriptor Skeleton;

    protected AnimationDescriptor(UAnimationAsset owner, AnimationDescriptor? outer = null, FReferenceSkeleton? fallbackReference = null)
    {
        Name = owner.Name;
        Path = owner.GetCleanPath() ?? "N/A";
        Skeleton = outer?.Skeleton ?? Create();

        SkeletonDescriptor Create()
        {
            var skeleton = owner.Skeleton?.Load<USkeleton>();
            if (skeleton is null && fallbackReference is { } reference)
                skeleton = CreateTempSkeletonFromModel(reference);

            if (skeleton is null)
                throw new InvalidOperationException($"Failed to load skeleton for animation asset {owner.Name}");

            var descriptor = new SkeletonDescriptor(skeleton.ReferenceSkeleton);
            descriptor.SetOwner(skeleton);
            return descriptor;
        }
    }

    internal static USkeleton CreateTempSkeletonFromModel(FReferenceSkeleton reference)
    {
        return new USkeleton
        {
            ReferenceSkeleton = reference,
            BoneTree = new EBoneTranslationRetargetingMode[reference.FinalRefBoneInfo.Length],
            AnimRetargetSources = new Dictionary<FName, FReferencePose>()
        };
    }

    protected virtual string Subtitle => string.Empty;

    public virtual void DrawControls()
    {
        DrawHeader();

        ImGui.Spacing();
        ImGui.SeparatorText($"Bones  ({Skeleton.BoneCount})");
        Skeleton.DrawControls();
    }

    private void DrawHeader()
    {
        ImGui.TextUnformatted(Name);
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled));
        ImGui.TextUnformatted(Subtitle);

        ImGui.SetWindowFontScale(0.85f);
        ImGui.TextUnformatted($"Animation: {Path}");
        ImGui.TextUnformatted($"Skeleton: {Skeleton.Path}");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.PopStyleColor();
    }
}
