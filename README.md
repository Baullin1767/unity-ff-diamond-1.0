# FF Diamond 1.0

FF Diamond 1.0 is a Unity 2D companion app built for players who want quick access to redeem codes, strategy guides, quizzes, and lightweight minigames (daily rewards, scratch cards, wheel fortune, calculator). All live content is pulled from Dropbox at boot, cached locally, and rendered through a custom Zenject-driven UI view system that targets mobile hardware.

## Table of Contents
- [Highlights](#highlights)
- [Tech Stack](#tech-stack)
- [Project Layout](#project-layout)
- [Remote Content Pipeline](#remote-content-pipeline)
- [Gameplay & UI Systems](#gameplay--ui-systems)
- [Configuration](#configuration)
- [Running the Project](#running-the-project)
- [Building for Release](#building-for-release)
- [Troubleshooting & Notes](#troubleshooting--notes)

## Highlights
- Remote-first data layer that downloads JSON + sprites from Dropbox, caches them under `Application.temporaryCachePath`, and feeds every screen through strongly typed parsers.
- Zenject-powered architecture (`GameRootInstaller`, `UIViewInstaller`, scriptable installers) keeps UI controllers, popups, and services decoupled.
- Rich content catalog: redeem codes, “free diamond” tips, characters, pets, weapons, vehicles, and quiz definitions are mapped to dedicated UI item views.
- Minigame suite: wheel fortune (DOTween-powered spinner), lucky scratch cards, daily rewards timeline, quizzes with result popups, currency counter & calculator.
- Virtualized scroll lists (fixed-height `PooledScroll` & dynamic `VariablePooledScroll`) efficiently render large remote feeds with pooling and item-height feedback.
- Connectivity UX: `Bootstrapper` gates startup on Dropbox initialization, shows loading progress, and loops a connection error popup until the device is back online.

## Tech Stack
- **Engine:** Unity `2022.3.44f1` (`ProjectSettings/ProjectVersion.txt`)
- **Primary packages:** TextMeshPro, UGUI, Newtonsoft JSON, Timeline, Visual Scripting (from `Packages/manifest.json`)
- **Embedded plugins:** Zenject (DI + installers), Cysharp UniTask (async workflows), DOTween (animations), NativeShare (Android/iOS sharing), custom Dropbox client.
- **Languages:** C# for runtime scripts, Unity serialized assets for configs.

## Project Layout
| Path | Description |
| --- | --- |
| `Assets/Scripts/Core/` | Bootstrap flow (`Bootstrapper`), Zenject root installer, Dropbox cache helpers. |
| `Assets/Scripts/Data/` | Data models, Dropbox path builder, manual JSON parsers, sprite caching & fetch utilities. |
| `Assets/Scripts/UI/` | Menu utilities, calculator, share buttons, wifi indicator loop, custom scroll systems, minigames, and the full view/popup controller stack. |
| `Assets/Plugins/` | Vendor plugins: Demigiant DOTween, Dropbox helper, NativeShare, UniTask, Zenject. |
| `Assets/Resources/` | ScriptableObject installers (`MinigameCurrencyInstaller.asset`) and wheel fortune configuration used by Zenject at runtime. |
| `Assets/Scenes/SampleScene.unity` | Main scene that wires Bootstrapper, UI roots, Zenject context, and prefabs. |
| `ProjectSettings/` & `Packages/` | Unity editor/project settings and package manifests. |

## Remote Content Pipeline
1. **Bootstrapper (`Assets/Scripts/Core/Bootstrapper.cs`)** hides the base UI, ensures connectivity, initializes Dropbox, and iterates through `DataType` entries to download/cached JSON, updating the loading progress bar the entire time.
2. **DropboxHelper (`Assets/Plugins/Dropbox/DropboxHelper.cs`)** authenticates via refresh token and exposes text/image download helpers used throughout the game.
3. **DropboxJsonCache (`Assets/Scripts/Core/DropboxJsonCache.cs`)** mirrors every JSON payload into the temp cache (and a small in-memory dictionary) so repeated visits are instant.
4. **DataManager (`Assets/Scripts/Data/DataManager.cs`)** abstracts JSON retrieval & parsing, sprite downloads, and sprite PNG caching under `DropboxSpriteCache`.
5. **Content parsers** (`Assets/Scripts/Data/DataParsing/*`) decode the obfuscated keys coming from Dropbox into strongly typed models before the UI consumes them.

If Dropbox is unreachable, `Bootstrapper` loops a reconnection popup until the player retries or dismisses it, then continues once a net connection returns.

## Gameplay & UI Systems
- **View system (`Assets/Scripts/UI/ViewSystem/*`)** — `UIViewController` keeps a registry of screen/popup bindings, toggles loading overlays, and routes transitions; `UIView` / `PopupUIView` give each screen a uniform show/hide contract. `UIViewInstaller` exposes the controller via Zenject so any component can request transitions.
- **Screens** — Menu, quizzes, redeem codes, calculators, minigames, etc. (`Assets/Scripts/UI/ViewSystem/UIViews/Screens/`). Examples:
  - `QuizScreenUIView` displays remote quiz questions, locks buttons once answered, yields rewards, and shows a `QuizResultsPopupUIView`.
  - `DailyRewardsScreenUIView` orchestrates tiles, cooldown gating, and currency rewards through `PlayerPrefsDailyRewardsService`.
  - `WheelFortuneUIView` and `LuckyCardMinigameView` wrap the minigame logic and report results to the Win popup + currency service.
- **Custom scrolls** (`Assets/Scripts/UI/CustomScrollRect/*`) — Prefab-pooling scroll rects minimize allocations while binding remote data; `OpenableItemView` demonstrates communicating height/state changes back to the scroll owner.
- **Minigame currency (`Assets/Scripts/UI/Mimigames/Currency/*`)** — Scriptable installer binds `PlayerPrefsMinigameCurrencyService`, `MinigameCurrencyText` shows the current balance, and all mini-games pay out through the service.
- **External actions** — `MenuExternalActions` hooks up share buttons via NativeShare and opens policy/support links.
- **Utility UI** — `FFCalculator` pops a purchase cost estimator, `ButtonSpriteToggleGroup` maintains nav tile states, `WifiIconLoop` animates the dancing connectivity glyph.

## Configuration
- **Dropbox credentials:** `Assets/Plugins/Dropbox/DropboxHelper.cs` contains the app key, secret, and refresh token. Replace these before shipping and keep secrets out of public repos.
- **Content folders:** `Assets/Scripts/Data/DataParsing/PathBuilder.cs` maps every `DataType` to Dropbox directories and file names. Adjust if your folder IDs change.
- **Scriptable installers:** Update `Assets/Resources/MinigameCurrencyInstaller.asset` (PlayerPrefs key & starting balance) and `Assets/Resources/WheelFortuneConfig.asset` (segment weights, reward amounts, spin parameters) as needed.
- **Scenes & prefabs:** The default `SampleScene` hosts the Bootstrapper, Zenject context, and UI hierarchy. If you create additional scenes, copy these root objects or convert them into reusable prefabs.

## Running the Project
1. **Clone or pull** the repository.
2. **Open with Unity Hub** using editor version `2022.3.44f1` (otherwise Unity will import/upgrade all assets).
3. Wait for packages/import to finish. If DOTween reports missing setup, run **Tools ▸ Demigiant ▸ DOTween Utility Panel ▸ Setup DOTween**.
4. Load `Assets/Scenes/SampleScene.unity`, press Play, and watch the loading overlay pull data from Dropbox.
5. Use the menu to navigate between guides, quizzes, and minigames. The console logs remote download/caching events which helps when debugging Dropbox connectivity.

## Building for Release
- **Switch platform** inside Build Settings (Android/iOS). NativeShare + Dropbox both require Internet permissions; ensure the manifest/plist includes them.
- **Player Settings:** Update company/product names, bundle identifiers, and icons before building.
- **Testing offline handling:** Simulate loss of connectivity to confirm the reconnection popup loop behaves correctly.
- **Clear caches between builds** if you change remote content structures (delete the `DropboxJsonCache` / `DropboxSpriteCache` folders under your device’s cache directory).

## Troubleshooting & Notes
- Dropbox failures will surface in the console. `HasInternetConnection` inside `Bootstrapper` gates both startup and a background connectivity watcher.
- Tokens currently live in source control for convenience; rotate them before distributing any binaries.
- Because large scroll views reuse item views aggressively, always remove listeners in `OnDestroy` and guard async callbacks against recycled views (see the pattern inside `GameVehiclesItemView` / `GameWeaponsItemView`).
- Minigame currency & daily rewards both use PlayerPrefs, so clearing player data resets balances and cooldowns.

---

Feel free to adapt section titles/wording, but this README captures the current architecture, setup workflow, and the notable systems inside the project.
