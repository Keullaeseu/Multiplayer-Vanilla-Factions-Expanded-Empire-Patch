using System.Reflection;
using HarmonyLib;
using Multiplayer.Compat;
using Verse;

namespace MultiplayerVanillaFactionsExpandedEmpirePatch.Source.Mods;

/// <summary>
///     Coexistence guards for running alongside upstream Multiplayer Compatibility:
///     detection of its Empire patch plus exactly-once application of Harmony behavior patches.
// </summary>
internal static class EmpirePatchGuards
{
    // Returns true if another loaded assembly (i.e. upstream Multiplayer Compatibility)
    // provides its own patch for VFE Empire. Used only for logging - MP registrations are
    // always (re-)applied to fill zones upstream doesn't cover, while Harmony behavior
    // patches go through PatchBehavior so nothing ever executes twice.
    internal static bool IsEmpirePatchProvidedUpstream()
    {
        var ownAssembly = typeof(VanillaFactionsExpandedEmpire).Assembly;
        var ownFullName = typeof(VanillaFactionsExpandedEmpire).FullName;

        foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (loadedAssembly == ownAssembly || loadedAssembly.IsDynamic)
                continue;

            Type[] types;
            try
            {
                types = loadedAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException reflectionTypeLoadException)
            {
                types = reflectionTypeLoadException.Types.Where(loadedType => loadedType != null).ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                if (type.FullName == ownFullName)
                    continue;

                foreach (var attribute in type.GetCustomAttributes(false))
                {
                    var attributeType = attribute.GetType();
                    if (attributeType.Name != "MpCompatForAttribute")
                        continue;

                    var packageId = attributeType.GetProperty("PackageId")?.GetValue(attribute, null) as string;
                    if ("OskarPotocki.VFE.Empire".Equals(packageId, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
        }

        return false;
    }

    // Runs one domain registration in isolation so a failure in one zone (e.g. a renamed
    // method or lambda in a mod update) is logged but no longer aborts the remaining zones.
    internal static void SafeRegister(string zoneName, Action register)
    {
        try
        {
            register();
        }
        catch (Exception exception)
        {
            Log.Error(
                $"{VanillaFactionsExpandedEmpire.LogPrefix} Failed to register {zoneName}, skipping that zone: {exception}");
        }
    }

    // Applies Harmony behavior patches only for slots not already covered by a same-ID patch
    // (i.e. upstream's). Guarantees exactly-once behavior when both mods are active, while still
    // filling the slot if upstream is absent or failed to cover it.
    internal static void PatchBehavior(MethodBase target, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
    {
        if (target == null)
        {
            Log.Warning($"{VanillaFactionsExpandedEmpire.LogPrefix} Skipping behavior patch for missing method.");
            return;
        }

        if (prefix != null && AlreadyCovered(target, HarmonyPatchType.Prefix))
            prefix = null;
        if (postfix != null && AlreadyCovered(target, HarmonyPatchType.Postfix))
            postfix = null;

        if (prefix == null && postfix == null)
            return;

        MpCompat.harmony.Patch(target, prefix, postfix);
    }

    internal static bool AlreadyCovered(MethodBase target, HarmonyPatchType type)
    {
        Patch[] patches = null;
        var patchInfo = Harmony.GetPatchInfo(target);
        if (patchInfo != null)
            patches = type switch
            {
                HarmonyPatchType.Prefix => patchInfo.Prefixes.ToArray(),
                HarmonyPatchType.Postfix => patchInfo.Postfixes.ToArray(),
                HarmonyPatchType.Transpiler => patchInfo.Transpilers.ToArray(),
                HarmonyPatchType.Finalizer => patchInfo.Finalizers.ToArray(),
                _ => null
            };

        return patches != null && patches.Any(patch => patch.owner == MpCompat.harmony.Id);
    }
}