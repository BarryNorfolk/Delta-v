using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DV.Blob;

[Prototype]
public sealed partial class BlobAntagUpgradePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Category { get; private set; } = "Undefined";

    [DataField]
    public string Tooltip { get; private set; } = "Unknown";

    [DataField(required: true)]
    public SpriteSpecifier? Sprite { get; private set; }

    [DataField(required: true)]
    public EntProtoId Creates { get; private set; }

    [DataField(required: true)]
    public EntityWhitelist? AllowedFrom;
}
