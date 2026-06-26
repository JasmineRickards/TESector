using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Content.Shared.Preferences;
using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

/// <summary>
/// Requires a character to have, or not have, certain traits
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class TraitsRequirement : JobRequirement
{
    [DataField]
    public HashSet<ProtoId<TraitPrototype>> RequiredTraits = new();

    [DataField]
    public HashSet<ProtoId<TraitPrototype>> ExcludedTraits = new();

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        if (profile is null) //the profile could be null if the player is a ghost. In this case we don't need to block the role selection for ghostrole
            return true;

        var requiredSb = new StringBuilder();
        requiredSb.Append("[color=yellow]");
        foreach (var t in RequiredTraits)
        {
            var separator = RequiredTraits.Last() == t ? " " : ", ";
            requiredSb.Append(Loc.GetString(protoManager.Index(t).Name) + separator);
        }
        requiredSb.Append("[/color]");

        var excludedSb = new StringBuilder();
        excludedSb.Append("[color=yellow]");
        foreach (var t in ExcludedTraits)
        {
            var separator = ExcludedTraits.Last() == t ? " " : ", ";
            excludedSb.Append(Loc.GetString(protoManager.Index(t).Name) + separator);
        }
        excludedSb.Append("[/color]");

        if (RequiredTraits.Count > 0 && ExcludedTraits.Count > 0)
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-whitelisted-traits")}\n{requiredSb}\n{Loc.GetString("role-timer-blacklisted-traits")}\n{excludedSb}");
        }
        else if (RequiredTraits.Count > 0)
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-whitelisted-traits")}\n{requiredSb}");
        }
        else if (ExcludedTraits.Count > 0)
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-blacklisted-traits")}\n{excludedSb}");
        }
        else
        {
            return true;
        }

        var requirementsMet = false;

        //at least one of
        foreach (var trait in RequiredTraits)
        {
            if (profile.TraitPreferences.Contains(trait))
                requirementsMet = true;
        }

        if(RequiredTraits.Count == 0)
            requirementsMet = true;

        foreach (var trait in ExcludedTraits)
        {
            if (profile.TraitPreferences.Contains(trait))
                requirementsMet = false;
        }

        return requirementsMet;
    }
}
