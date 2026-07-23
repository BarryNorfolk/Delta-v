using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Actions;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared._DV.Blob;
using Content.Shared._DV.Blob.Components;
using Content.Shared._DV.Blob.Systems;
using Content.Shared.NodeContainer;
using Content.Shared.NodeContainer.NodeGroups;

namespace Content.Server._DV.Blob.Systems;

public sealed class BlobSystem : SharedBlobSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

    private readonly string _blobNodeID = "blob";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobComponent, ComponentInit>(OnBlobStart);

        SubscribeLocalEvent<BlobResourceProducerComponent, BlobNetworkPulseEvent>(OnResourcePulse);
    }

    private void OnBlobStart(Entity<BlobComponent> blob, ref ComponentInit args)
    {
        foreach (var actionId in blob.Comp.InnateActions)
        {
            var actionEnt = _actions.AddAction(blob, actionId);
            blob.Comp.ActionEntities.Add(actionEnt);
        }
    }

    private void OnResourcePulse(Entity<BlobResourceProducerComponent> producer, ref BlobNetworkPulseEvent args)
    {
        if (!TryComp<BlobComponent>(args.Core, out var comp))
            return;

        AddEnergy((args.Core, comp), producer.Comp.EnergyPerPulse);
    }

    private bool GetCoreNodes(Entity<BlobComponent> blob, [NotNullWhen(true)] out INodeGroup? nodeGroup)
    {
        nodeGroup = null;

        if (!TryComp<NodeContainerComponent>(blob, out var nodeComp))
            return false;

        if (!_nodeContainer.TryGetNode<BlobNode>(nodeComp, _blobNodeID, out var coreNode))
            return false;

        if (coreNode.NodeGroup == null)
            return false; // Core is alone and cannot do anything

        nodeGroup = coreNode.NodeGroup;
        return true;
    }

    protected override void PulseNetwork(Entity<BlobComponent> blob)
    {
        base.PulseNetwork(blob);

        if (!GetCoreNodes(blob, out var coreNodeGroup))
            return;

        // TODO(Barry): Store this query so it's re-used
        var query = EntityQueryEnumerator<BlobPulseReceiverComponent>();
        while (query.MoveNext(out var receiver, out var comp))
        {
            if (!_nodeContainer.TryGetNode<BlobNode>(
                EntityManager.GetComponent<NodeContainerComponent>(receiver), _blobNodeID, out var node))
                continue; // Not a part of ANY node network?

            // TODO (Barry): Make sure that this node is actually connected and reachable
            if (!coreNodeGroup.Nodes.Any(x => x.Owner == node.Owner))
                continue; // Not on the same group as the core, or not reachable FROM the core

            var pulseEvent = new BlobNetworkPulseEvent(blob);
            RaiseLocalEvent(receiver, ref pulseEvent);
        }

        /*
            Resources or other attributes may have changed due to the network pulse
            so it's good to make sure the client is aware.
        */
        Dirty(blob);
    }
}
