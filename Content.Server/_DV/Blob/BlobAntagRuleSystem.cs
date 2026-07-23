using Content.Server._DV.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Blob;

public sealed class BlobAntagRuleSystem : GameRuleSystem<BlobAntagRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;

    public static readonly EntProtoId MindRole = "MindRoleBlobAntag";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobAntagRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelect);
    }

    private void OnAntagSelect(Entity<BlobAntagRuleComponent> uid, ref AfterAntagEntitySelectedEvent args)
    {
        var overmind = args.EntityUid;
        if (!_mind.TryGetMind(overmind, out var mindId, out var mind))
            return;

        _role.MindAddRole(mindId, MindRole, mind, true);

        _antag.SendBriefing(overmind, Loc.GetString("blob-role-roundstart-fluff"), Color.FromHex("#8ab34c"), null);// TODO(Barry): Briefing sound
        _antag.SendBriefing(overmind, Loc.GetString("blob-role-short-briefing"), Color.FromHex("#8ab34c"), null);
    }
}

