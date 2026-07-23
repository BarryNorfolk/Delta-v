using Content.Shared._DV.Blob.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

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
}
