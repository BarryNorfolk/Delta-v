namespace Content.Shared._DV.Blob.Components;

[RegisterComponent]
public sealed partial class BlobNodeComponent : Component;

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
