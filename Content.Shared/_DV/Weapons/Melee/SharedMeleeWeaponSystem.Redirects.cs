
using Content.Shared._DV.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;

namespace Content.Shared.Weapons.Melee;

public abstract partial class SharedMeleeWeaponSystem : EntitySystem
{
    private bool RedirectedAttack(EntityUid user, AttackEvent msg, EntitySessionEventArgs args)
    {
        if (!HasComp<RedirectingAttackerComponent>(user))
            return false; // Not redirectable

        // Need to know where/who we're attacking
        var targetPosition = GetTargetPosition(msg);

        // Run event to check which, if any, entity should take over this
        var ev = new OnRedirectingAttack();
        RaiseLocalEvent(user, ref ev);

        if (!ev.NewAttackingEntity.HasValue)
            return false; // Entity declined to redirect

        if (!TryGetWeapon(user, out var redirectedWeaponUid, out var redirectedWeapon) ||
            redirectedWeapon == null)
            return false; // Entity we redirected to ended up without a weapon

        // Attack is valid for redirection
        // TODO (Barry): Might be funky here with the message override, unsure
        AttemptAttack(
            GetEntity(ev.NewAttackingEntity.Value),
            redirectedWeaponUid,
            redirectedWeapon,
            msg,
            args.SenderSession);

        return true; // Attack was redirected
    }

    private EntityCoordinates? GetTargetPosition(AttackEvent msg)
    {
        switch (msg)
        {
            case LightAttackEvent light:
                return null;
            default:
                return null;
        }
    }
}
