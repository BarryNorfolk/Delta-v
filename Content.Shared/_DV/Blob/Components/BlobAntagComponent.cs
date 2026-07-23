using Content.Shared._DV.Blob.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Blob.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedBlobAntagSystem))]
public sealed partial class BlobAntagComponent : Component
{
    [DataField]
    public HashSet<EntProtoId> InnateActions = [
        "ActionBlobCreateNode",
    ];

    [DataField]
    public HashSet<EntityUid?> ActionEntities = [];

    [DataField]
    public float Energy = 60;

    [DataField]
    public float MaxEnergy = 480;

    [DataField]
    public float EnergyPerSecond = 1;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
