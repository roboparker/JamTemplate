# Unity Game Jam Template

A reusable Unity template for game jams. Includes core systems (screen fade, music manager, scene manager, etc.), a local CI pipeline for PR and main builds, build tagging, and itch.io push integration.

## Requirements

- **Unity 6000.3.13f1** (Unity 6)
- **Git LFS** — audio, textures, and other binary assets are tracked via LFS

## Quick Start

```bash
git clone <repo-url>
cd JamTemplate
git lfs pull
```

Open the project in Unity Hub (select Unity 6000.3.13f1). The project uses the **2D (URP)** rendering pipeline.

## Folder Structure

```
Assets/
  Art/            — sprites, tilesets, visual assets
  Audio/          — music and SFX clips
  Prefabs/
    UI/           — reusable UI prefabs (HiScoreDisplay, etc.)
  Scenes/         — all game scenes
    Splash.unity  — video splash + skip (build index 0)
    Title.unity   — persistent base scene with all singletons
    Settings.unity — additive UI scene (volume sliders)
    Credits.unity  — additive UI scene (scrolling credits)
    Pause.unity    — additive UI scene (pause menu)
    Win.unity      — additive UI scene (win screen)
    Lose.unity     — additive UI scene (game over screen)
    Demo.unity     — sample gameplay scene exercising all systems
  Scripts/
    Runtime/       — game code (JamTemplate.Runtime.asmdef)
      Audio/       — IAudioBackend, AudioManager, UnityAudioBackend
      GameState/   — GameStateManager (pause, resume, game over, win)
      HiScore/     — HiScoreManager, HiScoreDisplay
      Save/        — SaveSystem (ISaveBackend, PlayerPrefs, JSON file)
      Scene/       — SceneManagerWrapper (single + additive loading)
      UI/          — ScreenFade
    Editor/        — editor-only tools (JamTemplate.Editor.asmdef)
```

## Using This Template for a New Jam

1. Clone or fork this repo
2. Replace `Assets/Scenes/Demo.unity` with your gameplay scene
3. Update `Assets/Audio/` with your music and SFX
4. Update `Assets/Art/` with your sprites and visuals
5. Update Credits scene data with your team info
6. Set your itch.io project name in the CI workflow env vars
7. Build and ship!

## Architecture

### Scene Flow
```
Splash → Title → [Settings | Credits | Gameplay]
                     ↕           ↕
                   (additive)  (additive)

Gameplay → [Pause | Win | Lose]
              ↕       ↕      ↕
           (additive) ...   ...
```

- **Title** is the persistent base scene (loaded once, never unloaded)
- All UI scenes (Settings, Credits, Pause, Win, Lose) are loaded **additively** on top
- **SceneManagerWrapper** manages scene transitions with **ScreenFade**

### Audio
- **IAudioBackend** abstraction supports swappable backends
- **UnityAudioBackend** — default, uses Unity's built-in audio (works on all platforms including WebGL)
- **WwiseAudioBackend** — optional, compiled only when `WWISE_ENABLED` scripting define is set
- Backend selection via **AudioConfig** ScriptableObject

### Save System
- **ISaveBackend** interface with two implementations:
  - **PlayerPrefsBackend** — default, works everywhere including WebGL
  - **JsonFileBackend** — writes to `Application.persistentDataPath` (not for WebGL)
- Backend selection via **SaveConfig** ScriptableObject
- Auto-falls back to PlayerPrefs on WebGL regardless of config

## CI Pipeline

See [CI.md](CI.md) for full details on:
- Self-hosted GitHub Actions runner setup
- PR build workflow (`pr.yml`)
- Main branch build + version tagging (`release.yml`)
- itch.io push via butler
- Unity version branching strategy

## Unity Version

This template uses **Unity 6000.3.13f1 LTS**.

The exact version is documented in:
- This README
- `ProjectSettings/ProjectVersion.txt`
- `VERSION` file (for CI build numbering)
- `.github/workflows/*.yml` (`UNITY_VERSION` env var)
