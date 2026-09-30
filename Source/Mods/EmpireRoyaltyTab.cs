using System.Reflection;
using System.Text;
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
    // between mod builds (observed: "CS$<>8__locals2/CS$<>8__locals1/pawn").
    // Resolve the actual path at runtime instead of hardcoding it; if resolution fails the
    // delegate stays registered without the custom serializer rather than aborting startup.
    // The warning below dumps the real layout - paste it when reporting so the matcher can be hardened.
    private static void ApplyRoyalPawnTransform(ISyncDelegate inviteDelegate)
    {
        try
        {
            var fieldPath = FindCapturedNobleFieldPath(out var layoutDump);
            if (fieldPath == null)
            {
                Log.Warning(
                    $"{VanillaFactionsExpandedEmpire.LogPrefix} Could not locate captured noble field for Hierarchy invite, syncing without pawn transform. Display class layout: {layoutDump}");
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

    private static string FindCapturedNobleFieldPath(out string layoutDump)
    {
        layoutDump = "(unresolved)";
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

        layoutDump = DumpDisplayClass(displayClass);

        // The noble's source variable is named "pawn" (the invited colonist parameter is "p"),
        // so match by name: exact first, then case-insensitive, then substring.
        // Captures can nest several holders deep (observed: CS$<>8__locals2/CS$<>8__locals1/pawn),
        // so walk holder references recursively with cycle protection.
        return FindNobleFieldPathRecursive(displayClass, new HashSet<Type>(), null);
    }

    private static string FindNobleFieldPathRecursive(Type displayClass, HashSet<Type> visitedTypes, string prefix)
    {
        if (displayClass == null || !visitedTypes.Add(displayClass))
            return null;

        var directField = FindNobleField(displayClass);
        if (directField != null)
            return prefix == null ? directField.Name : $"{prefix}/{directField.Name}";

        foreach (var holderField in AccessTools.GetDeclaredFields(displayClass))
        {
            var holderType = holderField.FieldType;
            if (holderType == null || !holderType.IsNested || holderType.IsValueType)
                continue;

            var holderPath = prefix == null ? holderField.Name : $"{prefix}/{holderField.Name}";
            var nestedPath = FindNobleFieldPathRecursive(holderType, visitedTypes, holderPath);
            if (nestedPath != null)
                return nestedPath;
        }

        return null;
    }

    private static FieldInfo FindNobleField(Type displayClass)
    {
        FieldInfo fallbackField = null;
        foreach (var candidateField in AccessTools.GetDeclaredFields(displayClass))
        {
            if (candidateField.FieldType == null || !typeof(Pawn).IsAssignableFrom(candidateField.FieldType))
                continue;

            if (candidateField.Name == "pawn")
                return candidateField;
            if (fallbackField == null && candidateField.Name.Equals("pawn", StringComparison.OrdinalIgnoreCase))
                fallbackField = candidateField;
            if (fallbackField == null && candidateField.Name.Contains("pawn"))
                fallbackField = candidateField;
        }

        return fallbackField;
    }

    private static string DumpDisplayClass(Type displayClass)
    {
        var dumpBuilder = new StringBuilder();
        dumpBuilder.Append(displayClass.FullName).Append('{');
        AppendFieldsDump(dumpBuilder, displayClass);
        dumpBuilder.Append('}');
        return dumpBuilder.ToString();
    }

    private static void AppendFieldsDump(StringBuilder dumpBuilder, Type displayClass)
    {
        var firstField = true;
        foreach (var field in AccessTools.GetDeclaredFields(displayClass))
        {
            if (!firstField)
                dumpBuilder.Append(',');
            firstField = false;

            var fieldType = field.FieldType;
            dumpBuilder.Append(field.Name).Append(':').Append(fieldType == null ? "?" : fieldType.FullName);

            if (fieldType != null && fieldType.IsNested && !fieldType.IsValueType && fieldType != typeof(Pawn))
            {
                dumpBuilder.Append('{');
                AppendFieldsDump(dumpBuilder, fieldType);
                dumpBuilder.Append('}');
            }
        }
    }
}