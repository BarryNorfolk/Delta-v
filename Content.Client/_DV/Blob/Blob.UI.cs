using Robust.Shared.Utility;

namespace Content.Client._DV.Blob;

public sealed class BlobUpgradeRadial
{
    public SpriteSpecifier? Sprite;

    public string? Tooltip;
}

[ByRefEvent]
public record struct GetBlobUpgradesEvent()
{
    public Dictionary<string, IReadOnlyList<BlobUpgradeRadial>> Upgrades = new();
}
