using System.Diagnostics.CodeAnalysis;
using Content.Server._DV.Machines.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.Server._DV.Machines.EntitySystems;

public sealed class MultipartMachineSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultipartMachineComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<MultipartMachineComponent> ent, ref ComponentStartup args)
    {
        for (var i = 0; i < ent.Comp.Parts.Count; ++i)
        {
            var part = ent.Comp.Parts[i];
            if (!_factory.TryGetRegistration(part.Component, out var registration))
            {
                // DONOTMERGE: Giga failure case
                return;
            }

            part.ComponentType = registration.Type;
        }
    }

    public EntityUid? GetPartEntity(Entity<MultipartMachineComponent?> ent, string partName)
    {
        if (!Resolve(ent, ref ent.Comp))
            return null;

        return ent.Comp.GetEnt(partName);
    }

    private bool ScanPart(EntityUid gridUid,
        Vector2i coordinates,
        EntityQuery<IComponent> query,
        MapGridComponent grid,
        ref MachinePart part)
    {
        // Safety first, nuke any existing data
        part.Assembled = false;
        part.Entity = null;

        foreach (var entity in _mapSystem.GetAnchoredEntities(gridUid, grid, coordinates))
        {
            // DONOTMERGE - TODO: Expand check some more?
            if (query.TryGetComponent(entity, out var comp))
            {
                part.Entity = entity;
                return true;
            }
        }

        return false;
    }

    public bool Rescan(Entity<MultipartMachineComponent> ent)
    {
        // Get all required transform information to start looking for the other parts based on their offset
        var xformQuery = GetEntityQuery<TransformComponent>();
        if (!xformQuery.TryGetComponent(ent.Owner, out var xform) || !xform.Anchored)
        {
            return false;
        }

        var gridUid = xform.GridUid;
        if (gridUid == null || gridUid != xform.ParentUid || !TryComp<MapGridComponent>(gridUid, out var grid))
        {
            return false;
        }

        // Whichever component has the Multipart machine should be counted as the origin of the machine
        var machineOrigin = _mapSystem.TileIndicesFor(gridUid!.Value, grid, xform.Coordinates);
        Angle? direction = null;

        var missingParts = false;
        for (var i = 0; i < ent.Comp.Parts.Count; ++i)
        {
            var part = ent.Comp.Parts[i];
            var query = _entManager.GetEntityQuery(part.ComponentType!);

            if (direction.HasValue)
            {
                // We have already found some entity that roughly matchesz, so we can
                // use that direction for future lookups.
                // Not using this means the orientations of the parts could be wildly different and still
                // "Match" the expected offsets
                var expectedLocation = machineOrigin + part.Offset.Rotate(direction.Value);
                ScanPart(gridUid.Value, expectedLocation, query, grid, ref part);
            }
            else
            {
                // We have NO idea where our parts could be orientated so we'll have to iterate through 360 degrees
                // to try and find a match
                Angle curAngle = 0;
                for (var j = 0; j < 4; ++j)
                {
                    var guessedLocation = machineOrigin + part.Offset.Rotate(curAngle);
                    if (ScanPart(gridUid.Value, guessedLocation, query, grid, ref part))
                    {
                        // This entity succeeds, store the direction we used to get this one and expect all
                        // future machine parts to match this direction.
                        direction = curAngle;
                        break;
                    }

                    curAngle += Math.PI / 2;
                }
            }

            if (!part.Entity.HasValue)
            {
                missingParts = true;
            }
        }

        return !missingParts;
    }
}
