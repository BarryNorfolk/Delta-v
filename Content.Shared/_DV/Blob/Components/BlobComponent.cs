using Content.Shared._DV.Blob.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Blob.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedBlobSystem))]
[AutoGenerateComponentState]
public sealed partial class BlobComponent : Component
{
    [DataField]
    public HashSet<EntProtoId> InnateActions = [
        "ActionBlobCreateNode",
        "ActionBlobUpgradeNode",
    ];

    [DataField]
    public HashSet<EntityUid?> ActionEntities = [];

    [DataField, AutoNetworkedField]
    public int Energy = 60;

    [DataField]
    public int MaxEnergy = 480;

    [DataField]
    public int EnergyPerSecond = 1;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextBlobPulse = TimeSpan.Zero;

    [DataField]
    public TimeSpan BlobPulseDelay = TimeSpan.FromSeconds(1);
}
