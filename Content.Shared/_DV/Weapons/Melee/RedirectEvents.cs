using Robust.Shared.Serialization;

namespace Content.Shared._DV.Weapons.Melee;

[Serializable, NetSerializable, ByRefEvent]
public sealed class OnRedirectingAttack : EntityEventArgs
{
    /// <summary>
    /// Which entity should assume the attack.
    /// Leave null to let the original entity pass through the attack.
    /// </summary>
    public NetEntity? NewAttackingEntity = null;
}
