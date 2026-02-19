# Plan: Refactor OverlayMap Architecture (Notifiers & Mappers)

## TL;DR

> **Quick Summary**: Decouple event notification (Harmony patches) from visual mapping logic. Introduce `NotifierType` to distinguish component sources. Move generic patches to `Notifiers/` directory.
>
> **Deliverables**:
> - `NotifierType` enum.
> - Refactored `IComponentMapper` interface.
> - New `Notifiers/` directory with `ScaffoldingNotifier`, `PayableNotifier`, etc.
> - Refactored `Mappers/` to handle specific logic based on `NotifierType`.
> - Centralized `Teleporter` marker logic in `TeleporterMapper`.
>
> **Estimated Effort**: Medium
> **Parallel Execution**: YES - 2 waves
> **Critical Path**: Interface Change -> TopMapView Update -> Notifiers Extraction -> Mappers Refactor

---

## Context

### Original Request
The current architecture mixes mapping logic and event patching in `*Mapper.cs` files.
Specifically, `ScaffoldingMapper` and `PayableMapper` trigger marker creation for other components (like `Teleporter`), causing logic fragmentation.
The goal is to separate "Notification" (Notifiers) from "Mapping" (Mappers) and use `NotifierType` to clarify the context in `Map()` methods.

### Strategy
1.  **Separation of Concerns**: Notifiers only patch game methods and call `TopMapView`. Mappers only handle UI logic.
2.  **Context Passing**: Pass `NotifierType` to `Map()` so the Mapper knows if it's drawing a "Construction" state (from Scaffolding) or "Active" state (from Payable).
3.  **Logic Consolidation**: Move Teleporter logic from `ScaffoldingMapper` into `TeleporterMapper`.

---

## Work Objectives

### Core Objective
Refactor `OverlayMap` to separate Harmony patches into `Notifiers` and standardize `IComponentMapper`.

### Concrete Deliverables
- [ ] `OverlayMap/NotifierType.cs`
- [ ] `OverlayMap/Notifiers/ScaffoldingNotifier.cs`
- [ ] `OverlayMap/Notifiers/PayableNotifier.cs`
- [ ] `OverlayMap/Notifiers/PayableBlockerNotifier.cs`
- [ ] `OverlayMap/Notifiers/WorkableBuildingNotifier.cs`
- [ ] Updated `IComponentMapper.cs`
- [ ] Updated `TopMapView.cs`
- [ ] Updated `ScaffoldingMapper.cs` (logic only, delegation)
- [ ] Updated `TeleporterMapper.cs` (centralized logic)

---

## Execution Strategy

### Parallel Execution Waves

```
Wave 1 (Infrastructure & Interface):
├── Task 1: Create NotifierType Enum [quick]
├── Task 2: Update IComponentMapper Interface [quick]
├── Task 3: Update TopMapView to support NotifierType [quick]
└── Task 4: Create Notifiers Directory Structure [quick]

Wave 2 (Migration & Refactoring - Serialized due to file overlaps):
├── Task 5: Extract ScaffoldingNotifier & Refactor ScaffoldingMapper [deep]
├── Task 6: Extract PayableNotifier & Refactor PayableMapper [deep]
├── Task 7: Extract PayableBlockerNotifier & Refactor PayableBlockerMapper [quick]
├── Task 8: Extract WorkableBuildingNotifier & Refactor WorkableBuildingMapper [quick]
└── Task 9: Centralize Teleporter Logic in TeleporterMapper [deep]
```

---

## TODOs

- [x] 1. Create NotifierType Enum
- [x] 2. Update IComponentMapper Interface
- [x] 3. Update TopMapView
- [x] 4. Extract ScaffoldingNotifier

  **What to do**:
  - Create `OverlayMap/Notifiers/ScaffoldingNotifier.cs`.
  - Move `AwakePatch` and `CompleteAndRemovePatch` from `ScaffoldingMapper.cs`.
  - Update patches to call `view.OnComponentCreated(__instance, NotifierType.Scaffolding)`.
  - Clean up `ScaffoldingMapper.cs` (remove patches).
  - Update `ScaffoldingMapper.Map` signature to match interface.

  **Recommended Agent Profile**:
  - **Category**: `deep`
  - **Skills**: [`unity-il2cpp-modding`, `csharp`]

- [x] 5. Refactor TeleporterMapper Logic
- [x] 6. Extract PayableNotifier
- [x] 7. Extract PayableBlockerNotifier
- [x] 8. Extract WorkableBuildingNotifier
- [x] 9. Bulk Update Remaining Mappers

  **What to do**:
  - Update all other implementations of `IComponentMapper` to match the new signature `Map(Component, NotifierType)`.
  - Most will just ignore the `NotifierType` parameter.
  - Use `grep` or `ast-grep` to find all classes implementing the interface.

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: [`csharp`, `ast_grep_replace`]

---

## Success Criteria

### Verification Commands
```bash
dotnet build -c Debug
```

### Final Checklist
- [ ] All Patches moved to `Notifiers/`.
- [ ] `IComponentMapper` uses `NotifierType`.
- [ ] `TeleporterMapper` handles both construction and active states.
- [ ] Build succeeds.
