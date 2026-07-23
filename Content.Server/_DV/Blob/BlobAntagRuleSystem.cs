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

    private readonly EntProtoId _blobOvermind = "BlobAntagOvermind";

    public static readonly EntProtoId MindRole = "MindRoleBlobAntag";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobAntagRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelect);
    }

    private void OnAntagSelect(Entity<BlobAntagRuleComponent> uid, ref AfterAntagEntitySelectedEvent args)
    {
        if (!_mind.TryGetMind(args.EntityUid, out var mindId, out var mind))
            return;

        var transform = Transform(args.EntityUid);

        /*
            TODO(Barry): This feels very messy since it's creating a new body for the antag AFTER it's already been selected.
                         I don't know, however, if there are other examples of this from upstream to work with.
        */
        var overmind = Spawn(_blobOvermind, transform.Coordinates);
        EnsureComp<BlobAntagRuleComponent>(overmind);

        _mind.ControlMob(args.EntityUid, overmind);
        _role.MindAddRole(mindId, MindRole, mind, true);

        /*
            TODO(Barry): Delete the old body? How is this going to work with an actual mid-round antag spawning in?
                         Would they already BE in the right antag body?
        */
        QueueDel(args.EntityUid);

        _antag.SendBriefing(overmind, Loc.GetString("blob-role-roundstart-fluff"), Color.FromHex("#8ab34c"), null);// TODO(Barry): Briefing sound
        _antag.SendBriefing(overmind, Loc.GetString("blob-role-short-briefing"), Color.FromHex("#8ab34c"), null);
    }
}

