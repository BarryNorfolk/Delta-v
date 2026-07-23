using Content.Server.NodeContainer.Nodes;
using Content.Shared.NodeContainer;
using Robust.Shared.Map.Components;

namespace Content.Server._DV.Blob;

[DataDefinition]
public sealed partial class BlobNode : Node
{
    public override IEnumerable<Node> GetReachableNodes(TransformComponent xform,
        EntityQuery<NodeContainerComponent> nodeQuery,
        EntityQuery<TransformComponent> xformQuery,
        MapGridComponent? grid,
        IEntityManager entMan)
    {
        if (grid == null)
            yield break;

        var gridIndex = grid.TileIndicesFor(xform.Coordinates);

        foreach (var (_, node) in NodeHelpers.GetCardinalNeighborNodes(nodeQuery, grid, gridIndex))
        {
            if (node != this)
                yield return node;
        }
    }
}
