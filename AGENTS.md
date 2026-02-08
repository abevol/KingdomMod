# AGENTS.md - KingdomMod Repository Guidelines

---

## ⚡ Current Branch Work Context

**Branch:** `dev`  
**Task:**

**Current Status:**

**Key Points:**

**完成条件:**

---

## Project Overview

A C# .NET modding project for the game **Kingdom Two Crowns** using BepInEx plugin framework.
Supports both IL2CPP and Mono game versions.

**Projects**: OverlayMap, StaminaBar, DevTools, BetterPayableUpgrade, SharedLib

**Game Reference Source Code**: `~\Documents\ILSpy\KingdomTwoCrowns\2.3.2\Assembly-CSharp`  
This directory contains decompiled game source code for reference when implementing mod features.

---

## Build Commands

```bash
# Build Debug version (uses IL2CPP libs)
dotnet build -c Debug

# Build all projects for IL2CPP version (release)
dotnet build -c BIE6_IL2CPP

# Build all projects for Mono version (release)
dotnet build -c BIE6_Mono

# Build specific project
dotnet build OverlayMap/OverlayMap.csproj -c BIE6_IL2CPP

# Restore packages
dotnet restore

# Clean
dotnet clean
```

**Build Configurations**:

- `Debug`: Development build with IL2CPP
- `BIE6_IL2CPP`: Release for IL2CPP game version
- `BIE6_Mono`: Release for Mono game version (netstandard2.1)

**Build Artifacts**: Automatically copied to game plugins folder via MSBuild target.

---

## Code Style Guidelines

### Formatting (from .editorconfig)

- **Indent**: 4 spaces (no tabs)
- **Line endings**: CRLF (Windows-style)
- **Encoding**: UTF-8 with BOM for .cs files
- **Trim trailing whitespace**: Yes
- **Braces**: Required (csharp_prefer_braces = true)
- **Namespace**: Block-scoped (not file-scoped)

### Naming Conventions

- **Types** (classes, structs, interfaces, enums): PascalCase
- **Interfaces**: Prefix with `I` (e.g., `IComponentMapper`)
- **Methods/Properties/Events**: PascalCase
- **Fields/Parameters/Variables**: camelCase
- **Private fields**: `_camelCase` (underscore prefix)

### Using Directives

```csharp
// Order: System -> Third-party -> Project
using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using KingdomMod.SharedLib;
using static KingdomMod.OverlayMap.OverlayMapHolder;

// Place outside namespace
```

### Project-Specific Patterns

**Conditional Compilation**:

```csharp
#if IL2CPP
using BepInEx.Unity.IL2CPP;
#endif

#if MONO
using BepInEx.Unity.Mono;
#endif
```

**Plugin Structure**:

```csharp
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("KingdomTwoCrowns.exe")]
public class MyPlugin : BasePlugin  // or BaseUnityPlugin for Mono
```

**Logging**:

```csharp
// Use BepInEx ManualLogSource
public ManualLogSource LogSource => Log;  // IL2CPP
public ManualLogSource LogSource => Logger;  // Mono

// Log levels
LogSource.LogInfo($"Message");
LogSource.LogWarning($"Warning");
LogSource.LogError($"Error");
```

**Harmony Patching**:

```csharp
[HarmonyPatch(typeof(TargetClass), nameof(TargetClass.Method))]
public class MyPatcher
{
    [HarmonyPostfix]
    static void Postfix(ref ReturnType __result, ParamType __0)
    {
        // Patch logic
    }
}
```

### Language Features

- Use latest C# version (`<LangVersion>latest</LangVersion>`)
- Use expression-bodied members for simple properties/indexers
- Prefer simple using statements
- Use pattern matching where appropriate
- Nullable reference types enabled (with polyfill attributes)

### Error Handling

```csharp
try
{
    // Risky operation
}
catch (Exception ex)
{
    LogSource.LogError($"HResult: {ex.HResult:X}, {ex.Message}");
    // Re-throw if critical
    throw;
}
```

---

## Architecture Guidelines

### Namespace Structure

```txt
KingdomMod.{ModName}           - Root namespace
KingdomMod.{ModName}.Config    - Configuration
KingdomMod.{ModName}.Gui       - UI components
KingdomMod.{ModName}.Patchers  - Harmony patches
KingdomMod.SharedLib           - Shared utilities
```

### Key Design Patterns

1. **Plugin Pattern**: Each mod has a main Plugin class inheriting from BasePlugin/BaseUnityPlugin
2. **Holder Pattern**: Static holder classes manage mod state (e.g., `OverlayMapHolder`)
3. **Patcher Pattern**: Harmony patches in separate files under `Patchers/` folder
4. **Mapper Pattern**: Component mappers for game object visualization

### IL2CPP vs Mono Compatibility

- Use conditional compilation symbols: `IL2CPP`, `MONO`, `BIE`, `BIE6`
- IL2CPP requires `RegisterTypeInIl2Cpp.RegisterAssembly()`
- IL2CPP: Inherit from `BasePlugin`, override `Load()`
- Mono: Inherit from `BaseUnityPlugin`, use `Awake()`

---

## AI Code Generation Rules (from .cursor/rules)

When generating C# code:

1. **Follow SOLID principles** and object-oriented design
2. **Single responsibility**: Each class has one clear purpose
3. **Use BepInEx logging** (ManualLogSource) - never Console.WriteLine
4. **Add XML documentation** (`///`) for public APIs
5. **Proper error handling** with try-catch blocks
6. **Follow existing patterns** for IL2CPP/Mono compatibility
7. **Use PascalCase/camelCase** per .NET conventions
8. **Include namespace declarations** and proper using order

---

## Project References

- Uses local DLL references from `../_libs/` (BepInEx, Unity, Game assemblies)
- NuGet: BepInEx.PluginInfoProps
- SharedLib referenced by all mod projects

---

## Testing

No unit test projects currently configured. Test by:

1. Build the project
2. Copy DLLs to game's BepInEx/plugins folder
3. Run the game and check BepInEx logs

---
