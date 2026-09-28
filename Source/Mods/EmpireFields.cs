using HarmonyLib;
using RimWorld;
using Verse;
using VFEEmpire;

namespace MultiplayerVanillaFactionsExpandedEmpirePatch.Source.Mods;

/// <summary>
///     Shared field references for private/protected VFEEmpire members accessed by the patch.
///     Centralized here so the display names of mod internals live in a single place.
/// </summary>
internal static class EmpireFields
{
    internal static readonly AccessTools.FieldRef<HonorsTracker, Pawn> HonorsTrackerPawnRef =
        AccessTools.FieldRefAccess<HonorsTracker, Pawn>("pawn");

    internal static readonly AccessTools.FieldRef<Honor, int> HonorIdRef =
        AccessTools.FieldRefAccess<Honor, int>("idNumber");

    internal static readonly AccessTools.FieldRef<GameComponent_Honors, List<Honor>> GameComponentHonorsListRef =
        AccessTools.FieldRefAccess<GameComponent_Honors, List<Honor>>("honors");

    internal static readonly AccessTools.FieldRef<Command_ArtExhibit, LordJob_ArtExhibit> ArtExhibitJobRef =
        AccessTools.FieldRefAccess<Command_ArtExhibit, LordJob_ArtExhibit>("job");

    internal static readonly AccessTools.FieldRef<Command_GrandBall, LordJob_GrandBall> GrandBallJobRef =
        AccessTools.FieldRefAccess<Command_GrandBall, LordJob_GrandBall>("job");

    internal static readonly AccessTools.FieldRef<Command_Parade, LordJob_Parade> ParadeJobRef =
        AccessTools.FieldRefAccess<Command_Parade, LordJob_Parade>("job");

    internal static readonly AccessTools.FieldRef<RoyalTitlePermitWorker_Slicing, Faction> SlicingFactionRef =
        AccessTools.FieldRefAccess<RoyalTitlePermitWorker_Slicing, Faction>("faction");

    internal static readonly AccessTools.FieldRef<RoyalTitlePermitWorker_Slicing, LocalTargetInfo> SlicingOriginRef =
        AccessTools.FieldRefAccess<RoyalTitlePermitWorker_Slicing, LocalTargetInfo>("origin");

    internal static readonly AccessTools.FieldRef<RoyalTitlePermitWorker_Call, Faction> CallFactionRef =
        AccessTools.FieldRefAccess<RoyalTitlePermitWorker_Call, Faction>("faction");
}