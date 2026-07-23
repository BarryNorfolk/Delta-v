using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Blob.Components;

[RegisterComponent]
public sealed partial class BlobNodePlacementLimiterComponent : Component
{
    /// <summary>
    /// A component to check when finding matching entities
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(ComponentNameSerializer))]
    public string Component = "";

    /// <summary>
    /// The minimum distance a matching entity must be for this to be a valid placement
    /// </summary>
    [DataField(required: true)]
    public int MinTileDistance = 1;

    /// <summary>
    /// The maximum number of this 'Kind' of node that can be placed
    /// Set to '-1' in order to have as many as you'd like.
    /// </summary>
    [DataField]
    public int MaximumCount = -1;
}
