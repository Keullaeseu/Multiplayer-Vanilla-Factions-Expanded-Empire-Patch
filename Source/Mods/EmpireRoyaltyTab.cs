using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Multiplayer.Compat;
using Verse;
using VFEEmpire;
using VFEEmpire.HarmonyPatches;

namespace MultiplayerVanillaFactionsExpandedEmpirePatch.Source.Mods;

/// <summary>Multiplayer sync for the royalty tab window, its workers, hierarchy and title cache.</summary>
internal static class EmpireRoyaltyTab
{
    internal static void Register()
    {
        MP.RegisterSyncWorker<MainTabWindow_Royalty>(SyncMainTabWindowRoyalty, shouldConstruct: true);
        MP.RegisterSyncWorker<RoyaltyTabWorker>(SyncRoyaltyTabWorker, isImplicit: true, shouldConstruct: true);

        // Hierarchy, not much to patch here
        // (Re)generates data
        MP.RegisterSyncMethod(typeof(RoyaltyTabWorker_Hierarchy),
            nameof(RoyaltyTabWorker_Hierarchy.Notify_Open));
        // Invite pawn
        var inviteDelegates = MpCompat.RegisterLambdaDelegate(typeof(RoyaltyTabWorker_Hierarchy),
            nameof(RoyaltyTabWorker_Hierarchy.DoMainSection), 1);
        if (inviteDelegates != null && inviteDelegates.Length > 0)
            ApplyRoyalPawnTransform(inviteDelegates[0]);

        // Basically when called, it removes the pawns from list with pawns with titles, and if they still have them - they get re-added.
        // Causes the order to change, which could cause issues before the method is synced.
        var royaltyTrackerPostfix = AccessTools.DeclaredMethod(
            typeof(ColonistTitleCache.RoyaltyTracker), nameof(ColonistTitleCache.RoyaltyTracker.Postfix));
        if (!EmpirePatchGuards.AlreadyCovered(royaltyTrackerPostfix, HarmonyPatchType.Prefix))
            PatchingUtilities.PatchCancelInInterface(royaltyTrackerPostfix);
    }

    private static void SyncMainTabWindowRoyalty(SyncWorker sync, ref MainTabWindow_Royalty tab)
    {
        sync.Bind(ref tab.CurCharacter);

        if (sync.isWriting)
            sync.Write(MP.CanUseDevMode && tab.DevMode);
        else
            tab.DevMode = sync.Read<bool>();
    }

    private static void SyncRoyaltyTabWorker(SyncWorker sync, ref RoyaltyTabWorker worker)
    {
        worker.parent ??= new MainTabWindow_Royalty();
        sync.Bind(ref worker.parent);
    }

    private static int WriteRoyalPawn(Pawn pawn)
    {
        return pawn.thingIDNumber;
    }

    private static Pawn ReadRoyalPawn(int pawnId)
    {
        return WorldComponent_Hierarchy.Instance.TitleHolders?.Find(pawn => pawn.thingIDNumber == pawnId);
    }

    // The invited noble is captured in a compiler-generated display class whose layout differs
    // between mod builds (old one: "CS$<>8__locals1/pawn", current Roslyn: "pawn").
    // Resolve the actual path at runtime instead of hardcoding it; if resolution fails the
    // delegate stays registered without the custom serializer rather than aborting startup.
    private static void ApplyRoyalPawnTransform(ISyncDelegate inviteDelegate)
    {
        try
        {
            var fieldPath = FindCapturedNobleFieldPath();
            if (fieldPath == null)
            {
                Log.Warning(
                    $"{VanillaFactionsExpandedEmpire.LogPrefix} Could not locate captured noble field for Hierarchy invite, syncing without pawn transform.");
                return;
            }

            inviteDelegate.TransformField(fieldPath, Serializer.New<Pawn, int>(WriteRoyalPawn, ReadRoyalPawn));
        }
        catch (Exception exception)
        {
            Log.Warning(
                $"{VanillaFactionsExpandedEmpire.LogPrefix} Failed to apply Hierarchy invite pawn transform, continuing without it: {exception.Message}");
        }
    }

    private static string FindCapturedNobleFieldPath()
    {
        MethodInfo inviteLambda;
        try
        {
            inviteLambda = MpMethodUtil.GetLambda(typeof(RoyaltyTabWorker_Hierarchy), "DoMainSection",
                MethodType.Normal, null, 1);
        }
        catch
        {
            return null;
        }

        var displayClass = inviteLambda.DeclaringType;
        if (displayClass == null)
            return null;

        // Modern Roslyn layout: the noble is a direct field of the display class.
        var directField = AccessTools.DeclaredField(displayClass, "pawn");
        if (directField != null && typeof(Pawn).IsAssignableFrom(directField.FieldType))
            return "pawn";

        // Legacy layout: the noble lives one level deeper inside a locals holder.
        foreach (var holderField in AccessTools.GetDeclaredFields(displayClass))
        {
            var holderType = holderField.FieldType;
            if (holderType == null || !holderType.IsNested || holderType.IsValueType)
                continue;

            var nestedField = AccessTools.DeclaredField(holderType, "pawn");
            if (nestedField != null && typeof(Pawn).IsAssignableFrom(nestedField.FieldType))
                return $"{holderField.Name}/pawn";
        }

        return null;
    }
}