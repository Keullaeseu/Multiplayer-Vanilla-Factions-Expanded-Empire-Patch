using HarmonyLib;
using Multiplayer.API;
using Multiplayer.Compat;
using RimWorld;
using Verse;
using VFEEmpire;

namespace MultiplayerVanillaFactionsExpandedEmpirePatch.Source.Mods;

/// <summary>Multiplayer sync for royal permits and the permits tab.</summary>
internal static class EmpirePermits
{
    internal static void Register()
    {
        MP.RegisterSyncMethod(typeof(EmpirePermits), nameof(SyncSlingBeam));
        EmpirePatchGuards.PatchBehavior(
            AccessTools.DeclaredMethod(typeof(RoyalTitlePermitWorker_Slicing),
                nameof(RoyalTitlePermitWorker_Slicing.OrderForceTarget)),
            new HarmonyMethod(typeof(EmpirePermits), nameof(PreSlicingBeamOrderForceTarget)));

        MP.RegisterSyncMethod(typeof(RoyalTitlePermitWorker_Call),
            nameof(RoyalTitlePermitWorker_Call.OrderForceTarget));
        MP.RegisterSyncWorker<RoyalTitlePermitWorker_Call>(SyncCallPermit, isImplicit: true);

        MpCompat.RegisterLambdaDelegate(typeof(RoyalTitlePermitWorker_CallAbsolver),
            nameof(RoyalTitlePermitWorker_CallAbsolver.GetRoyalAidOptions), 0);
        MpCompat.RegisterLambdaDelegate(typeof(RoyalTitlePermitWorker_CallTechfriar),
            nameof(RoyalTitlePermitWorker_CallTechfriar.GetRoyalAidOptions), 0);

        // Permits tab (not covered by the original compat):
        // Accept permit (DoLeftRect) calls Pawn_RoyaltyTracker.AddPermit,
        // Return all permits (DoLeftBottom) calls Pawn_RoyaltyTracker.RefundPermits (direct + via confirmation dialog).
        // Syncing the vanilla methods covers both UI paths.
        MP.RegisterSyncMethod(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.AddPermit));
        MP.RegisterSyncMethod(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.RefundPermits));
    }

    private static void SyncCallPermit(SyncWorker sync, ref RoyalTitlePermitWorker_Call permitWorker)
    {
        if (sync.isWriting)
        {
            sync.Write(EmpireFields.CallFactionRef(permitWorker));
        }
        else
        {
            // permitWorker instance is provided by MP for implicit workers;
            // if null (shouldn't happen for targeted permits), we can't reconstruct without def context.
            if (permitWorker != null)
                EmpireFields.CallFactionRef(permitWorker) = sync.Read<Faction>();
            else
                sync.Read<Faction>();
        }
    }

    private static bool PreSlicingBeamOrderForceTarget(RoyalTitlePermitWorker_Slicing __instance,
        LocalTargetInfo target)
    {
        // If the origin is not valid (not assigned yet) it means it's the first pass of the targeting, let the player pick second one before syncing
        if (!MP.IsInMultiplayer || !EmpireFields.SlicingOriginRef(__instance).IsValid || MP.IsExecutingSyncCommand)
            return true;

        // Both targets are selected, sync the permit usage
        SyncSlingBeam(__instance, target, EmpireFields.SlicingOriginRef(__instance),
            EmpireFields.SlicingFactionRef(__instance));
        return false;
    }

    private static void SyncSlingBeam(RoyalTitlePermitWorker_Slicing permitWorker, LocalTargetInfo target,
        LocalTargetInfo origin, Faction faction)
    {
        EmpireFields.SlicingFactionRef(permitWorker) = faction;
        EmpireFields.SlicingOriginRef(permitWorker) = origin;
        permitWorker.OrderForceTarget(target);
    }
}