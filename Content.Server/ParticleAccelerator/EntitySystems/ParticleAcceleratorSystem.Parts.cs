using Content.Server._DV.Machines.Components;
using Content.Server.ParticleAccelerator.Components;
using JetBrains.Annotations;
using Robust.Shared.Physics.Events;

namespace Content.Server.ParticleAccelerator.EntitySystems;

[UsedImplicitly]
public sealed partial class ParticleAcceleratorSystem
{
    private void InitializePartSystem()
    {
        SubscribeLocalEvent<ParticleAcceleratorPartComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<ParticleAcceleratorPartComponent, MoveEvent>(OnMoveEvent);
        SubscribeLocalEvent<ParticleAcceleratorPartComponent, PhysicsBodyTypeChangedEvent>(BodyTypeChanged);
    }

    public bool ValidateEmitter(string name,
        ParticleAcceleratorEmitterType type,
        MultipartMachineComponent multipartMachine)
    {
        var emitterEnt = multipartMachine.GetEnt(name);
        if (!TryComp<ParticleAcceleratorEmitterComponent>(emitterEnt, out var partState))
        {
            return false;
        }

        return partState.Type == type;
    }

    public void RescanParts(EntityUid uid, EntityUid? user = null, ParticleAcceleratorControlBoxComponent? controller = null)
    {
        if (!Resolve(uid, ref controller))
            return;

        if (controller!.CurrentlyRescanning)
            return;

        if (!TryComp<MultipartMachineComponent>(uid, out var multipartMachine))
            return;

        controller.Assembled = false;

        if (!_multipartMachine.Rescan((uid, multipartMachine)))
        {
            // All entities are not in the right place
            SwitchOff(uid, user, controller);
            return;
        }

        // Determine if the proper emitters are in the proper spots
        if (!ValidateEmitter("PortEmitter", ParticleAcceleratorEmitterType.Port, multipartMachine) ||
            !ValidateEmitter("ForeEmitter", ParticleAcceleratorEmitterType.Fore, multipartMachine) ||
            !ValidateEmitter("StarboardEmitter", ParticleAcceleratorEmitterType.Starboard, multipartMachine))
        {
            // One of the emitters is incrrect
            SwitchOff(uid, user, controller);
            return;
        }

        controller.Assembled = true;
        controller.CurrentlyRescanning = false;

        UpdatePowerDraw(uid, controller);
        UpdateUI(uid, controller);
    }

    private void OnComponentShutdown(EntityUid uid, ParticleAcceleratorPartComponent comp, ComponentShutdown args)
    {
        if (Exists(comp.Master))
            RescanParts(comp.Master!.Value);
    }

    private void BodyTypeChanged(EntityUid uid, ParticleAcceleratorPartComponent comp, ref PhysicsBodyTypeChangedEvent args)
    {
        if (Exists(comp.Master))
            RescanParts(comp.Master!.Value);
    }

    private void OnMoveEvent(EntityUid uid, ParticleAcceleratorPartComponent comp, ref MoveEvent args)
    {
        if (Exists(comp.Master))
            RescanParts(comp.Master!.Value);
    }
}
