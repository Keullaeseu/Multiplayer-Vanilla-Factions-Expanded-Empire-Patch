using HarmonyLib;
using Multiplayer.API;
using Multiplayer.Compat;
using RimWorld.Planet;
using Verse;
using VFEEmpire;

namespace MultiplayerVanillaFactionsExpandedEmpirePatch.Source.Mods;

/// <summary>Multiplayer sync for vassals: vassalizing settlements and managing tithe settings.</summary>
internal static class EmpireVassals
{
    internal static void Register()
    {
        MP.RegisterSyncMethod(typeof(EmpireVassals), nameof(SyncedVassalizeSettlement));
        // DoVassal in current assembly:
        //   lambda 0 = set Special (deliver specific), 1 = set SpecialNever (deliver never),
        //   lambdas 2/3 = Where/Select LINQ (do NOT sync),
        //   lambda 4 = set normal tithe setting.
        // (Old compat used 2,3,4 - ordinals shifted after mod update.)
        // DoVassal/DoPotentialVassal are private in current assembly, so use string literals (nameof doesn't work for private members).
        MpCompat.RegisterLambdaDelegate(typeof(RoyaltyTabWorker_Vassals), "DoVassal",
            0, 1, 4);
        EmpirePatchGuards.PatchBehavior(
            AccessTools.DeclaredMethod(typeof(RoyaltyTabWorker_Vassals),
                "DoPotentialVassal"),
            postfix: new HarmonyMethod(typeof(EmpireVassals), nameof(PostDoPotentialVassal)));

        // Needed for above
        MP.RegisterSyncWorker<TitheInfo>(SyncTitheInfo);

        // This method can be called from UI, and it uses RNG to create TitheInfo for a specific settlement.
        // Push/Pop state (with a seed using settlement ID and tile) to ensure the same tithe will be created for a specific settlement.
        EmpirePatchGuards.PatchBehavior(
            AccessTools.DeclaredMethod(typeof(WorldComponent_Vassals), nameof(WorldComponent_Vassals.GetTitheInfo)),
            new HarmonyMethod(typeof(EmpireVassals), nameof(PreGetTitheInfo)),
            new HarmonyMethod(typeof(EmpireVassals), nameof(PostGetTitheInfo)));
        // Called from delegate inside of RoyaltyTabWorker_Vassals.DoLeftBottom (confirmation dialog)
        MP.RegisterSyncMethod(typeof(WorldComponent_Vassals), nameof(WorldComponent_Vassals.ReleaseAllVassalsOf));
    }

    private static void SyncTitheInfo(SyncWorker sync, ref TitheInfo titheInfo)
    {
        if (sync.isWriting)
            sync.Write(titheInfo.Settlement);
        else
            titheInfo = WorldComponent_Vassals.Instance.GetTitheInfo(sync.Read<Settlement>());
    }

    private static void PreGetTitheInfo(Settlement settlement)
    {
        if (MP.IsInMultiplayer)
            // Ensure that no matter when the settlement tithe info is generated, it'll have identical contents for all players.
            // Using combination of settlement ID and tile to get a somewhat unique seed.
            // We could potentially add more to it, like something based on biome and/or faction. However, those could possibly
            // change due to mods changing biomes/making factions attack each other.
            Rand.PushState(Gen.HashCombineInt(settlement.ID, settlement.Tile));
    }

    private static void PostGetTitheInfo()
    {
        if (MP.IsInMultiplayer)
            Rand.PopState();
    }

    private static void PostDoPotentialVassal(TitheInfo vassal, bool canVassalize)
    {
        if (!MP.IsInMultiplayer || !canVassalize)
            return;

        if (vassal.Lord == null)
            return;

        var lord = vassal.Lord;
        // Cleanup data before syncing
        vassal.Lord = null;
        if (vassal.Setting != TitheSetting.Special)
            vassal.Setting = TitheSetting.Never;

        SyncedVassalizeSettlement(lord, vassal.Settlement);
    }

    private static void SyncedVassalizeSettlement(Pawn pawn, Settlement settlement)
    {
        var tithe = WorldComponent_Vassals.Instance.GetTitheInfo(settlement);

        tithe.Lord = pawn;
        tithe.DaysSinceDelivery = 0;
        if (tithe.Setting != TitheSetting.Special)
            tithe.Setting = TitheSetting.EveryWeek;
    }
}