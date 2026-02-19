
## Explicit Implementation of Map(Component, NotifierType)

- **Date**: 2026-02-17
- **Decision**: Bulk update all remaining mappers implementing `IComponentMapper` to have an explicit `public void Map(Component component, NotifierType notifierType) => Map(component);` implementation.
- **Rationale**: While the interface provides a default implementation, explicit implementation ensures uniformity across the codebase and prevents reliance on "magic" interface features that might be removed or changed in the future. It also serves as a clear indicator that these mappers currently do not care about the notifier type.
- **Method**: Used a Python script to insert the explicit implementation before the existing `Map(Component)` method in 41 mapper files. Verified with a successful build of the OverlayMap project.
