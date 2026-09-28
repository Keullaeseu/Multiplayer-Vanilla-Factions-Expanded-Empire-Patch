using Multiplayer.API;
using Multiplayer.Compat;
using Verse;
using VFEEmpire;

namespace MultiplayerVanillaFactionsExpandedEmpirePatch.Source.Mods;

/// <summary>Multiplayer sync for honors: granting, moving and removing them plus their trackers.</summary>
internal static class EmpireHonors
{
    internal static void Register()
    {
        // Syncing adding/removing honors (used by drag & drop in Honors tab, as well as debug actions)
        MP.RegisterSyncMethod(typeof(HonorUtility), nameof(HonorUtility.AddHonor));
        MP.RegisterSyncMethod(typeof(HonorUtility), nameof(HonorUtility.RemoveHonor));
        MP.RegisterSyncMethod(typeof(HonorUtility), nameof(HonorUtility.RemoveAllHonors));

        // Needed for above
        MP.RegisterSyncWorker<Honor>(SyncHonor);

        // Grant all honors (dev)
        MpCompat.RegisterLambdaMethod(typeof(HonorsTracker), nameof(HonorsTracker.GetGizmos), 4).SetDebugOnly();
        MP.RegisterSyncWorker<HonorsTracker>(SyncHonorsTracker);
    }

    private static void SyncHonor(SyncWorker sync, ref Honor honor)
    {
        if (sync.isWriting)
        {
            // Use the raw pawn field (not the Pawn property, which falls back to ExamplePawn)
            sync.Write(honor.pawn);
            sync.Write(EmpireFields.HonorIdRef(honor));
        }
        else
        {
            var pawn = sync.Read<Pawn>();
            var honorId = sync.Read<int>();

            IEnumerable<Honor> honors = EmpireFields.GameComponentHonorsListRef(GameComponent_Honors.Instance);
            if (pawn != null)
                honors = honors.Concat(pawn.Honors().AllHonors);

            honor = honors.FirstOrDefault(candidateHonor => EmpireFields.HonorIdRef(candidateHonor) == honorId);
        }
    }

    private static void SyncHonorsTracker(SyncWorker sync, ref HonorsTracker tracker)
    {
        if (sync.isWriting)
            sync.Write(EmpireFields.HonorsTrackerPawnRef(tracker));
        else
            tracker = sync.Read<Pawn>().Honors();
    }
}