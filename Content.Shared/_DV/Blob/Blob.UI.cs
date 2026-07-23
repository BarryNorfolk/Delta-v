namespace Content.Shared._DV.Blob;

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

[Serializable, NetSerializable]
public sealed record BlobUpgradeRadial(
    string Category, string Tooltip, SpriteSpecifier? Sprite, EntProtoId Prototype);

[Serializable, NetSerializable]
public sealed class BlobUpgradeOptionsState(List<BlobUpgradeRadial> upgrades) : BoundUserInterfaceState
{
    public List<BlobUpgradeRadial> Upgrades = upgrades;
}

[Serializable, NetSerializable]
public sealed class BlobUpgradeMessage(EntProtoId protoId) : BoundUserInterfaceMessage
{
    public EntProtoId ProtoId = protoId;
}

[ByRefEvent]
public record struct GetBlobUpgradesEvent(EntityUid Target)
{
    public List<BlobUpgradeRadial> Upgrades = new();
}
