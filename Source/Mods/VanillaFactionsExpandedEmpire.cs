using Multiplayer.Compat;
using Verse;

namespace MultiplayerVanillaFactionsExpandedEmpirePatch.Source.Mods;

/// <summary>
///     Multiplayer Patch for Vanilla Factions Expanded - Empire by Oskar Potocki, xrushha, legodude17, Allie, Last Update:
///     28 Sep @ 1:29pm 2026
///     <see href="https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380" />
///     <see href="https://github.com/Vanilla-Expanded/VanillaFactionsExpanded-Empire" />
///     Based on the original implementation from Multiplayer-Compatibility (Source_Referenced/VanillaFactionsEmpire.cs),
///     updated for the current VFEEmpire assembly.
///     Domain logic lives in the Empire*.cs files; this class only bootstraps registration.
/// </summary>
[MpCompatFor("OskarPotocki.VFE.Empire")]
public class VanillaFactionsExpandedEmpire
{
    public const string LogPrefix = "[Multiplayer Vanilla Factions Expanded - Empire Patch]";

    public VanillaFactionsExpandedEmpire(ModContentPack content)
    {
        // Defer registration until loading is finished: all Mod constructors (including
        // upstream Multiplayer Compatibility, if present) have run by then, so we can
        // reliably detect whether its Empire patch will handle syncing instead of us.
        LongEventHandler.ExecuteWhenFinished(Init);
    }

    private static void Init()
    {
        // Complementary mode: MP registrations are (re-)applied unconditionally - duplicates are
        // benign metadata, and this fills zones upstream doesn't cover (Permits tab, DoVassal 0/1).
        // Harmony behavior patches go through PatchBehavior (applied only if not already covered),
        // so nothing ever executes twice (e.g. the slicing beam firing twice).
        var upstreamPresent = EmpirePatchGuards.IsEmpirePatchProvidedUpstream();
        Log.Message(upstreamPresent
            ? $"{LogPrefix} Upstream Compatibility patch detected - running in complementary mode."
            : $"{LogPrefix} Initializing...");

        EmpirePatchGuards.SafeRegister(nameof(EmpireRituals), EmpireRituals.Register);
        EmpirePatchGuards.SafeRegister(nameof(EmpireRoyaltyTab), EmpireRoyaltyTab.Register);
        EmpirePatchGuards.SafeRegister(nameof(EmpireHonors), EmpireHonors.Register);
        EmpirePatchGuards.SafeRegister(nameof(EmpireVassals), EmpireVassals.Register);
        EmpirePatchGuards.SafeRegister(nameof(EmpirePermits), EmpirePermits.Register);

        Log.Message($"{LogPrefix} Initialized.");
    }
}