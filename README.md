# Multiplayer Vanilla Factions Expanded - Empire Patch

A RimWorld Multiplayer compatibility patch for [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380).

This mod is designed to improve multiplayer synchronization when playing with the [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380) mod and RimWorld Multiplayer.

## Features

- Adds multiplayer compatibility for [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380), including:
  - Art exhibit, grand ball and parade rituals (starting, leaving and cancelling).
  - Royalty tab: hierarchy view, noble invites, honors drag & drop, vassals and tithe settings.
  - Permits tab: accepting permits and returning all permits.
  - Royal permit workers: slicing beam, calls (shuttle, regiment, absolver, techfriar and others).
  - Deterministic tithe generation for settlements.
- Runs in complementary mode alongside upstream Multiplayer Compatibility: shared behavior patches are applied exactly once, while zones upstream does not cover are still patched.

## Requirements

- RimWorld (plus the Royalty DLC, required by Empire itself)
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- RimWorld Multiplayer
  - [GitHub version](https://github.com/rwmt/Multiplayer) or [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2606448745) version
- [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380)
- All Vanilla Factions Expanded - Empire dependencies (such as Vanilla Expanded Framework)

The host and every connected player must use compatible versions of all required mods.

## Installation

### Steam Workshop

Subscribe to the required mods and add them to your RimWorld mod list in the following order:

1. Harmony
2. Core
3. Royalty (required), Ideology, Biotech, and Anomaly, if applicable
4. RimWorld Multiplayer
5. [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380)
6. [Multiplayer Vanilla Factions Expanded - Empire Patch](https://github.com/Keullaeseu/Multiplayer-Vanilla-Factions-Expanded-Empire-Patch/releases/latest)

The patch should load after both RimWorld Multiplayer and [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380).

### Manual Installation

1. Download the latest release from the [**Releases**](https://github.com/Keullaeseu/Multiplayer-Vanilla-Factions-Expanded-Empire-Patch/releases/latest) section.
2. Extract the mod folder into your RimWorld `Mods` directory.
3. Enable the required mods in RimWorld.
4. Use the recommended load order listed above.
5. Make sure every multiplayer player has the same mod list, configuration, and load order.

## Multiplayer Usage

All players should have the following mods installed and enabled:

- RimWorld Multiplayer
- [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380)
- [Multiplayer Vanilla Factions Expanded - Empire Patch](https://github.com/Keullaeseu/Multiplayer-Vanilla-Factions-Expanded-Empire-Patch/releases/latest)
- All required Vanilla Factions Expanded - Empire dependencies

The host and all connected clients should use the same:

- RimWorld version
- RimWorld Multiplayer version
- Vanilla Factions Expanded - Empire version
- Multiplayer Vanilla Factions Expanded - Empire Patch version
- Mod configuration
- Mod load order

Do not add, remove, update, or reorder mods while players are connected to the same multiplayer session.

## Compatibility

This patch is intended to provide multiplayer compatibility for [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380).

It does not replace:

- [RimWorld Multiplayer](https://steamcommunity.com/sharedfiles/filedetails/?id=2606448745)
- [Vanilla Factions Expanded - Empire](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380)

It can be used together with upstream Multiplayer Compatibility: when its built-in Empire patch is detected, this patch only fills the gaps instead of duplicating behavior patches.

## Known Limitations

- Compatibility may be affected by future RimWorld updates.
- Compatibility may be affected by future updates to RimWorld Multiplayer or Vanilla Factions Expanded - Empire.
- If a future Empire update changes compiler-generated UI internals, individual zones log an error and are skipped while the rest of the patch keeps working.

## Credits

- [RimWorld Multiplayer on GitHub](https://github.com/rwmt/Multiplayer)
- [RimWorld Multiplayer on Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2606448745)
- [Vanilla Factions Expanded - Empire on Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2938820380)
- [VanillaFactionsExpanded-Empire on GitHub](https://github.com/Vanilla-Expanded/VanillaFactionsExpanded-Empire)
- [Multiplayer Vanilla Factions Expanded - Empire Patch](https://github.com/Keullaeseu)
