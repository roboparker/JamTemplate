# CI Pipeline

## Overview

The project uses GitHub Actions with a **self-hosted runner** to build Unity projects locally. Two workflows:

- **`pr.yml`** — Validates builds on pull requests (Windows + WebGL). No artifacts uploaded, no tags created.
- **`release.yml`** — On push to `main`: bumps version, builds both targets, creates a git tag + GitHub Release, and pushes to itch.io via butler.

## Self-Hosted Runner Setup

### Prerequisites
- Windows machine with Unity 6000.3.13f1 installed at:
  `C:\Program Files\Unity\Hub\Editor\6000.3.13f1\`
- Git and Git LFS installed
- [butler](https://itch.io/docs/butler/) installed and on PATH (for itch.io pushes)

### Registering a Runner

1. Download the GitHub Actions runner from:
   https://github.com/actions/runner/releases

2. Extract to a dedicated directory (e.g. `C:\actions-runner-jamtemplate\`)

3. Generate a registration token:
   ```bash
   gh api -X POST repos/roboparker/JamTemplate/actions/runners/registration-token --jq '.token'
   ```

4. Configure the runner:
   ```cmd
   config.cmd --url https://github.com/roboparker/JamTemplate ^
     --token <TOKEN> ^
     --name <RUNNER-NAME> ^
     --labels unity-builder ^
     --work _work
   ```

5. Start the runner:
   ```cmd
   run.cmd
   ```
   Or install as a Windows service (requires admin):
   ```cmd
   config.cmd --runasservice
   ```

### Required Labels
Workflows target runners with: `[self-hosted, unity-builder]`

## GitHub Secrets

| Secret | Required By | Description |
|--------|-------------|-------------|
| `BUTLER_API_KEY` | `release.yml` | itch.io butler API key. Get it from https://itch.io/user/settings/api-keys |

`GITHUB_TOKEN` is provided automatically by GitHub Actions.

## Workflows

### pr.yml (Pull Request)

**Triggers:** `pull_request` (opened, synchronize)

**Steps:**
1. Checkout with LFS
2. Build Windows (`BatchBuild.BuildWindows`)
3. Build WebGL (`BatchBuild.BuildWebGL`)
4. Upload build logs on failure

No artifacts are uploaded on success — this is build validation only.

### release.yml (Main Branch)

**Triggers:** `push` to `main`

**Steps:**
1. Checkout with LFS (full history for tagging)
2. Read `VERSION.txt`, increment patch number, write back, commit
3. Build Windows + WebGL
4. Create git tag (`vMAJOR.MINOR.PATCH`)
5. Create GitHub Release with Windows zip artifact
6. Push Windows build to itch.io (`:windows` channel)
7. Push WebGL build to itch.io (`:html5` channel)

### VERSION.txt Format

```
MAJOR.MINOR.PATCH
```

Example: `0.1.0` → after release → `0.1.1`

The release workflow auto-increments the PATCH number on each push to main.

## itch.io Configuration

| Channel | Target | Build |
|---------|--------|-------|
| `windows` | `roboparker/jamtemplate:windows` | Windows Standalone |
| `html5` | `roboparker/jamtemplate:html5` | WebGL |

To change the itch.io target, edit the `ITCH_TARGET_WINDOWS` and `ITCH_TARGET_WEBGL` env vars in `release.yml`.

## Unity Version Branching Strategy

**Default branch:** `main` — tracks the current stable Unity version (6000.3.13f1).

### Supporting Multiple Unity Versions

When adopting a new Unity version:

1. Create a long-lived branch from `main`: e.g. `unity6-lts`, `unity2022-lts`
2. Each branch maintains its own:
   - `UNITY_EXECUTABLE` path in workflow env vars
   - `VERSION.txt` that increments independently
   - itch.io channel suffix if desired (e.g. `:windows-u6`)
3. Both `pr.yml` and `release.yml` share the same structure per branch
4. `ProjectSettings/ProjectVersion.txt` reflects the branch's Unity version

### Workflow Variable Per Branch

Update the `UNITY_EXECUTABLE` env var in both workflow files:
```yaml
env:
  UNITY_VERSION: "6000.3.13f1"
  UNITY_EXECUTABLE: "C:/Program Files/Unity/Hub/Editor/6000.3.13f1/Editor/Unity.exe"
```

The self-hosted runner must have the target Unity version installed.

## Troubleshooting

- **"Project already open"** — Close Unity Editor before the runner picks up a job, or ensure the runner's `_work` directory is separate from your dev project path.
- **WebGL build fails with `VERSION` header conflict** — The file is named `VERSION.txt` (not `VERSION`) to avoid clashing with the C++ `<version>` standard library header used by emscripten.
- **LFS files missing** — Ensure `lfs: true` is set in the checkout step.
- **Butler not found** — Install butler and ensure it's on the system PATH for the runner user.
