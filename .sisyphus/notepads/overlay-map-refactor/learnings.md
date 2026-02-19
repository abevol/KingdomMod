# Refactoring Summary\n\nAll notifiers have been extracted to 'Notifiers/' directory.\nAll mappers now implement 'Map(Component, NotifierType)'.\nTeleporter logic is consolidated.

## Redundant Code Removal
- In C# 8.0+, interface default implementations allow us to avoid boilerplate code in implementing classes.
- Removed redundant `public void Map(Component component, NotifierType notifierType) => Map(component);` from multiple mappers in `OverlayMap/Gui/TopMap/Mappers/`.
- These mappers now rely on the default implementation in `IComponentMapper`.
