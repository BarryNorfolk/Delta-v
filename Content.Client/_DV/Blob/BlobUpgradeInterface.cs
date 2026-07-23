using Content.Client.UserInterface.Controls;
using Content.Shared._DV.Blob;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Client._DV.Blob;

[UsedImplicitly]
public sealed class BlobUpgradeInterface : BoundUserInterface
{
    private SimpleRadialMenu? _menu = null;

    public BlobUpgradeInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this); // TODO(Barry): If we end up with no deps, we can remove this
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.OpenOverMouseScreenPosition();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu == null)
            return;

        if (state is not BlobUpgradeOptionsState cast)
            return;

        var models = ConvertToButtons(cast.Upgrades);
        _menu!.SetButtons(models);
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(IReadOnlyList<BlobUpgradeRadial> allowedUpgrades)
    {
        var groupedUpgrades = new Dictionary<string, List<RadialMenuOptionBase>>();

        foreach (var upgrade in allowedUpgrades)
        {
            var groupModels = groupedUpgrades.GetOrNew(upgrade.Category);
            groupModels.Add(new RadialMenuActionOption<BlobUpgradeRadial>(HandleRadialMenuClick, upgrade)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(upgrade.Sprite),
                ToolTip = upgrade.Tooltip
            });
        }

        var models = new List<RadialMenuOptionBase>();
        foreach (var (category, upgrades) in groupedUpgrades)
        {
            models.Add(new RadialMenuNestedLayerOption(upgrades)
            {
                ToolTip = category
            });
        }

        return models;
    }

    private void HandleRadialMenuClick(BlobUpgradeRadial p)
    {
        SendMessage(new BlobUpgradeMessage(p.Prototype));
    }
}
