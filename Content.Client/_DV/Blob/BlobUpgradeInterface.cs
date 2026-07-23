using Content.Client.UserInterface.Controls;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Client._DV.Blob;

[UsedImplicitly]
public sealed class BlobUpgradeInterface : BoundUserInterface
{
    // TODO(Barry): Figure this data part out
    private sealed class BaseBlobUpgrade;

    private SimpleRadialMenu? _menu = null;

    public BlobUpgradeInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this); // TODO(Barry): If we end up with no deps, we can remove this
    }

    protected override void Open()
    {
        base.Open();

        var ev = new GetBlobUpgradesEvent(Owner);
        EntMan.EventBus.RaiseLocalEvent(Owner, ref ev);

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);

        var models = ConvertToButtons(ev.Upgrades);
        _menu!.SetButtons(models);

        _menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(IReadOnlyList<BlobUpgradeRadial> allowedUpgrades)
    {
        var groupedUpgrades = new Dictionary<string, List<RadialMenuOptionBase>>();

        foreach (var upgrade in allowedUpgrades)
        {
            var groupModels = groupedUpgrades.GetOrNew(upgrade.Category);
            groupModels!.Add(new RadialMenuActionOption<BaseBlobUpgrade>(HandleRadialMenuClick, new BaseBlobUpgrade())
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

    private void HandleRadialMenuClick(BaseBlobUpgrade p)
    {

    }
}
