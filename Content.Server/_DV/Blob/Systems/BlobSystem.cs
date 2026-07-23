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
    private EntityQuery<BlobPulseReceiverComponent> _pulseReceiverQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobComponent, ComponentInit>(OnBlobStart);

        SubscribeLocalEvent<BlobResourceProducerComponent, BlobNetworkPulseEvent>(OnResourcePulse);

        _pulseReceiverQuery = GetEntityQuery<BlobPulseReceiverComponent>();
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

        // We only care about nodes that are on the CORE's node group so don't need to venture
        // outside that and find all receivers in the game.
        foreach (var node in coreNodeGroup.Nodes)
        {
            if (!_pulseReceiverQuery.HasComp(node.Owner))
                continue;

            var pulseEvent = new BlobNetworkPulseEvent(blob);
            RaiseLocalEvent(node.Owner, ref pulseEvent);
        }

        /*
            Resources or other attributes may have changed due to the network pulse
            so it's good to make sure the client is aware.
        */
        Dirty(blob);
    }
}
