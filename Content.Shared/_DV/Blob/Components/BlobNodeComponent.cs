using Robust.Shared.GameStates;

namespace Content.Shared._DV.Blob.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class BlobNodeComponent : Component
{
    /// <summary>
    /// The core of the blob this node was spawned from.
    /// May be null in the case where this IS the core.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? BlobCore = default!;
}

[RegisterComponent]
public sealed partial class BlobPulseReceiverComponent : Component;

[RegisterComponent]
public sealed partial class BlobSimpleNodeComponent : Component;

[RegisterComponent]
public sealed partial class BlobResourceProducerComponent : Component
{
    [DataField]
    public int EnergyPerPulse = 10;
}
