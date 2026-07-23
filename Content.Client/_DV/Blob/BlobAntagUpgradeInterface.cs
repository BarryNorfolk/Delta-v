using Content.Client.UserInterface.Controls;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._DV.Blob;

[UsedImplicitly]
public sealed class BlobAntagUpgradeInterface : BoundUserInterface
{
    // TODO(Barry): Figure this data part out
    private sealed class BaseBlobUpgrade;

    private SimpleRadialMenu? _menu = null;

    public BlobAntagUpgradeInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this); // TODO(Barry): If we end up with no deps, we can remove this
    }

    protected override void Open()
    {
        var ev = new GetBlobUpgradesEvent();
        EntMan.EventBus.RaiseLocalEvent(Owner, ref ev);

        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        var models = ConvertToButtons(ev.Upgrades);
        _menu.SetButtons(models);

        _menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(Dictionary<string, IReadOnlyList<BlobUpgradeRadial>> allUpgrades)
    {
        var models = new RadialMenuOptionBase[allUpgrades.Keys.Count];
        foreach (var (type, upgrades) in allUpgrades)
        {
            var upgradesInType = new RadialMenuActionOptionBase[upgrades.Count];
            for (var i = 0; i < upgrades.Count; i++)
            {
                var upgrade = upgrades[i];
                upgradesInType[i] = new RadialMenuActionOption<BaseBlobUpgrade>(HandleRadialMenuClick, new BaseBlobUpgrade())
                {
                    IconSpecifier = RadialMenuIconSpecifier.With(upgrade.Sprite),
                    ToolTip = upgrade.Tooltip
                };
            }

            for (var i = 0; i < allUpgrades.Keys.Count; i++)
            {
                models[i] = new RadialMenuNestedLayerOption(upgradesInType)
                {
                    ToolTip = "What?"
                };
            }

        }

        return models;
    }


    private void HandleRadialMenuClick(BaseBlobUpgrade p)
    {

    }
}
