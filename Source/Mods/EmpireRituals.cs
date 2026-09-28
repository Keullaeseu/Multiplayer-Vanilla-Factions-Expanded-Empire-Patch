using Multiplayer.API;
using Multiplayer.Compat;
using Verse.AI.Group;
using VFEEmpire;

namespace MultiplayerVanillaFactionsExpandedEmpirePatch.Source.Mods;

/// <summary>Multiplayer sync for the art exhibit, grand ball and parade rituals.</summary>
internal static class EmpireRituals
{
    internal static void Register()
    {
        // Art exhibit
        MP.RegisterSyncWorker<LordToil_ArtExhibit_Wait>(SyncArtExhibitWait);
        MP.RegisterSyncWorker<Command_ArtExhibit>(SyncArtExhibitCommand);
        // Open ritual menu
        MpCompat.RegisterLambdaMethod(typeof(LordToil_ArtExhibit_Wait),
            nameof(LordToil_ArtExhibit_Wait.ExtraFloatMenuOptions), 0);
        MP.RegisterSyncMethod(typeof(Command_ArtExhibit), nameof(Command_ArtExhibit.ProcessInput));
        // Leave/cancel art exhibit
        MpCompat.RegisterLambdaDelegate(typeof(LordJob_ArtExhibit), nameof(LordJob_ArtExhibit.GetPawnGizmos), 0, 2);

        // Grand ball
        MP.RegisterSyncWorker<LordToil_GrandBall_Wait>(SyncGrandBallWait);
        MP.RegisterSyncWorker<Command_GrandBall>(SyncGrandBallCommand);
        // Open ritual menu
        MpCompat.RegisterLambdaMethod(typeof(LordToil_GrandBall_Wait),
            nameof(LordToil_GrandBall_Wait.ExtraFloatMenuOptions), 0);
        MP.RegisterSyncMethod(typeof(Command_GrandBall), nameof(Command_GrandBall.ProcessInput));
        // Leave/cancel grand ball
        MpCompat.RegisterLambdaDelegate(typeof(LordJob_GrandBall), nameof(LordJob_GrandBall.GetPawnGizmos), 0, 2);

        // Parade
        MP.RegisterSyncWorker<LordToil_Parade_Wait>(SyncParadeWait);
        MP.RegisterSyncWorker<Command_Parade>(SyncParadeCommand);
        // Open ritual menu
        MpCompat.RegisterLambdaMethod(typeof(LordToil_Parade_Wait),
            nameof(LordToil_Parade_Wait.ExtraFloatMenuOptions), 0);
        MP.RegisterSyncMethod(typeof(Command_Parade), nameof(Command_Parade.ProcessInput));
        // Leave/cancel parade
        MpCompat.RegisterLambdaDelegate(typeof(LordJob_Parade), nameof(LordJob_Parade.GetPawnGizmos), 0, 2);
    }

    private static void SyncArtExhibitWait(SyncWorker sync, ref LordToil_ArtExhibit_Wait toil)
    {
        if (sync.isWriting)
            sync.Write(toil.lord);
        else
            toil = sync.Read<Lord>()?.curLordToil as LordToil_ArtExhibit_Wait;
    }

    private static void SyncArtExhibitCommand(SyncWorker sync, ref Command_ArtExhibit command)
    {
        if (sync.isWriting)
        {
            sync.Write(EmpireFields.ArtExhibitJobRef(command).lord);
        }
        else
        {
            var lord = sync.Read<Lord>();
            if (lord == null) return;

            if (lord.CurLordToil is LordToil_ArtExhibit_Wait toil)
                command = (Command_ArtExhibit)toil.GetPawnGizmos(toil.bestNoble).FirstOrDefault();
        }
    }

    private static void SyncGrandBallWait(SyncWorker sync, ref LordToil_GrandBall_Wait toil)
    {
        if (sync.isWriting)
            sync.Write(toil.lord);
        else
            toil = sync.Read<Lord>()?.curLordToil as LordToil_GrandBall_Wait;
    }

    private static void SyncGrandBallCommand(SyncWorker sync, ref Command_GrandBall command)
    {
        if (sync.isWriting)
        {
            sync.Write(EmpireFields.GrandBallJobRef(command).lord);
        }
        else
        {
            var lord = sync.Read<Lord>();
            if (lord == null) return;

            if (lord.CurLordToil is LordToil_GrandBall_Wait toil)
                command = (Command_GrandBall)toil.GetPawnGizmos(toil.bestNoble).FirstOrDefault();
        }
    }

    private static void SyncParadeWait(SyncWorker sync, ref LordToil_Parade_Wait toil)
    {
        if (sync.isWriting)
            sync.Write(toil.lord);
        else
            toil = sync.Read<Lord>()?.curLordToil as LordToil_Parade_Wait;
    }

    private static void SyncParadeCommand(SyncWorker sync, ref Command_Parade command)
    {
        if (sync.isWriting)
        {
            sync.Write(EmpireFields.ParadeJobRef(command).lord);
        }
        else
        {
            var lord = sync.Read<Lord>();
            if (lord == null) return;

            if (lord.CurLordToil is LordToil_Parade_Wait toil)
                command = (Command_Parade)toil.GetPawnGizmos(toil.bestNoble).FirstOrDefault();
        }
    }
}