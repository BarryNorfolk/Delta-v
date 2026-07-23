namespace Content.Shared._DV.Actions;

/// <summary>
/// Component for marking an action costing some amount of resources to perform.
/// Useful for roles which have some amount of resources, where you don't want to clutter
/// the main role component with each abilities' cost.
/// </summary>
[RegisterComponent]
public sealed partial class ActionCostComponent : Component
{
    [DataField(required: true)]
    public int Cost;
}
