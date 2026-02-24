## CI Workflow Setup for BepInEx Mods
- Created a CI workflow that handles the unique requirement of generating `ProjectSettings.custom.props` before restore.
- Used HEREDOC with quoted 'EOF' to prevent shell expansion of GitHub Actions variables.
- Ensured output directories exist before build to satisfy custom MSBuild targets.
- Updated .gitignore to exclude CI-specific temporary directories.
