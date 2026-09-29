# Tiny Wizard — Project Context

Handoff document for any agent or developer picking this project up.
Last updated: after commit `1ea3941` (tag `v0.5.0`).

---

## 1. Project Basics

| Item | Value |
|------|-------|
| Project | Tiny Wizard |
| Path | `C:\Users\harsh\Tiny Wizard` |
| Unity version | 6000.3.11f1 |
| Render pipeline | URP (Universal Render Pipeline) |
| Input | NEW Input System (`PlayerInput` + `TinyWizardControls.inputactions`) |
| UI | **Legacy `UnityEngine.UI`** (`Image`, `Text`) — NOT TextMeshPro |
| Repo | `git@github.com:Tron-xR/Tiny-Wizard.git`, branch `master` |
| Tags | `v0.3.0` (7d71650), `v0.4.0` (8d62523), `v0.5.0` (0118573) |
| Scene | `Assets/Scenes/TinyWizard.unity` (the only scene in the project) |
| Build settings | Only `TinyWizard.unity` is in the build list |

Scale of the game is "tiny" — a small wizard in a giant kitchen.
Enemies use small NavMesh agent radii (0.15) and short step heights (0.1).

---

## 2. Git History (what was built, in order)

```
1ea3941  Fix font reference: Arial.ttf -> LegacyRuntime.ttf for Unity 6000
45cd78c  Fix ManaUI/PauseManager field access (private -> public)
0118573  Wire new systems into scene + create SystemSetupWindow editor tool   [v0.5.0]
8d62523  Add 12 missing systems + rename PushSpell -> AttackSpell             [v0.4.0]
7d71650  Add health system + fix animation clips                             [v0.3.0]
e126ad3  Add enemy AI system (Cockroach, Spider, Fly) + modular state machine
4f277e2  Add spell system (Attack, Freeze, Bounce) + prefabs + editor tool
b078197  Refactor to kinematic MovePosition character controller
0e1ea5f  Add modular interaction system (pickup, push, raycast)
184ad68  Rewrite README
...      jump/ground-check debugging commits
```

---

## 3. Script Inventory

### 3.1 Player — `Assets/Scripts/Player/`
| Script | Purpose |
|--------|---------|
| `PlayerController.cs` | Kinematic movement, `MovePosition`/`SlideMove`, jump, gravity, sprint, rotation. Public API: `GetCurrentSpeed()`, `IsGrounded`, `Launch(Vector3)`, `GetVelocity()` |
| `GroundChecker.cs` | OverlapSphere-based ground detection (rewritten several times) |
| `PlayerInputHandler.cs` | Wraps `PlayerInput`. C# events: `JumpPressed`, `InteractPressed`, `CastSpellPressed`, `PausePressed`, `SpellSlotPressed(int)`. UnityEvents: `OnJump`, `OnInteract`, `OnCastSpell`, `OnPause` |
| `PlayerHealth.cs` | Implements `IDamageable`. HP, i-frames, hit flash, death/respawn. Event `OnHealthChanged(float,float)`, `OnPlayerDeath`, `OnPlayerRespawn` |
| `PlayerAnimation.cs` | **NEW (v0.4.0)**. Smooth-damps speed into Animator float `Speed`, sets `IsGrounded` |
| `FootstepAudio.cs` | **NEW (v0.4.0)**. `OnFootstep()` public method for animation events; picks clip by ground tag, random pitch, `PlayOneShot` |

### 3.2 Camera — `Assets/Scripts/Camera/`
| Script | Purpose |
|--------|---------|
| `ThirdPersonCamera.cs` | Orbit follow with pivot, zoom, collision, smoothing |
| `CameraShake.cs` | **NEW (v0.4.0)**. Singleton (`Instance`), `Shake()` / `Shake(duration, magnitude)`, offsets `localPosition` in `Update` |

### 3.3 Spells — `Assets/Scripts/Spells/`
| Script | Purpose |
|--------|---------|
| `SpellBase.cs` | Abstract: cooldown, mana cost, cast delay, VFX/SFX hooks, `StartCast`/`ExecuteCast` |
| `AttackSpell.cs` | **Renamed from `PushSpell.cs` in v0.4.0.** Projectile or radial push force |
| `FreezeSpell.cs` | Raycast target → `FreezeableObject.Freeze()` + ice platform |
| `BounceSpell.cs` | Raycast ground → bounce pad + `Rigidbody.linearVelocity` override |
| `SpellManager.cs` | Owns spell list, input, cooldown/mana. **Instance** events `OnSpellSwitched(int)`, `OnManaChanged(float)`, `OnCooldownUpdated(float)`. Public: `ActiveSpell`, `ActiveSpellIndex`, `SpellCount`, `CurrentMana`, `MaxMana`, `HasMana`, `CastOrigin`, `UseMana(float)`, `SwitchToSpell(int)` |
| `SpellProjectile.cs` | Simple kinematic projectile |
| `FreezeableObject.cs` | Freezes RB constraints, swaps material |
| `IDamageable.cs` | Shared damage interface (used by both player and enemy health) |
| `ISpellTarget.cs` | `OnPushSpell`, `OnFreezeSpell`, `OnBounceSpell` |

Prefabs: `SpellProjectile.prefab`, `BouncePad.prefab`, `IcePlatform.prefab` (materials not yet assigned).

### 3.4 Enemy — `Assets/Scripts/Enemy/` (15 files)
`EnemyBase`, `EnemyStateMachine`, `EnemyPatrol`, `EnemyDetection`, `EnemyAttack`,
`EnemyHealth`, `EnemyAnimator`, `EnemyAudioHandler`, `EnemyVFXHandler`,
`EnemySpawner`, plus movement/types: `FlyingEnemyMovement`, `SpiderWallMovement`,
`CockroachEnemy`, `SpiderEnemy`, `FlyEnemy`.

State flow: `Idle → Patrol → DetectPlayer → Chase → Attack → LosePlayer → ReturnToPatrol → Patrol`

`EnemyHealth.OnDeath` is an instance `System.Action`. `EnemySpawner` subscribes per-instance
and advances waves when `aliveCount` hits 0, with difficulty scaling.

**No enemy prefabs exist yet** — only the scripts. They must be built per the hierarchy in `AGENTS.md`.

### 3.5 Interaction — `Assets/Scripts/Interaction/`
`IInteractable`, `InteractableObject`, `InteractionController`, `InteractionUI`,
`PickupObject`, `PushableObject`. SphereCast radius 0.5 + OverlapCapsule fallback,
distance checked from the **player** (zoom-proof), `GetComponentInParent<IInteractable>()`.

### 3.6 UI — `Assets/Scripts/UI/`
| Script | Purpose | Notes |
|--------|---------|-------|
| `HealthUI.cs` | Filled `Image` bar, colour tiers, text, death overlay | subscribes `PlayerHealth.OnHealthChanged` |
| `ManaUI.cs` | **NEW (v0.4.0)**. Smoothed mana fill + text + spell name | `manaFill`/`manaText` are **public** (see §6) |
| `PauseManager.cs` | **NEW (v0.4.0)**. `Time.timeScale`, cursor, canvas toggle, `Restart()`, `MainMenu()`, `Quit()` | `pauseCanvas` is **public**; `pauseKey` field is currently unused (it listens to `PlayerInputHandler.PausePressed`) |
| `MainMenuController.cs` | **NEW (v0.4.0)**. `Play()`, `Continue()`, `OpenSettings()`, `CloseSettings()`, `Quit()` | loads scene `gameSceneName = "TinyWizard"` |
| `TutorialManager.cs` | **NEW (v0.4.0)**. `TutorialPrompt[]` with optional dismiss-on-move / dismiss-on-interact |
| `InteractionUI.cs` | Interaction prompt text (legacy `Text`) | |

### 3.7 Gameplay / Utilities
| Script | Purpose |
|--------|---------|
| `Gameplay/Collectible.cs` | **NEW**. `CollectibleType { HealthOrb, ManaOrb, Key }`, rotate + bob idle motion, optional VFX/SFX |
| `Gameplay/LevelGoal.cs` | **NEW**. Trigger → show win canvas → load next scene after delay |
| `Utilities/ObjectPool.cs` | **NEW**. Generic `ObjectPool<T> where T : Component`; ctor `(prefab, initialSize, parent)`, `Get(pos,rot)`, `Return(obj)`, `Clear()` |
| `Utilities/SaveManager.cs` | **NEW**. Static JSON save. `SaveData { playerX/Y/Z, playerHealth, activeSpellIndex }`, `Save()`, `Load()`, `ApplySave()`, `DeleteSave()`, `HasSave()` |

### 3.8 Editor tooling — `Assets/Scripts/Editor/`
| Script | Menu |
|--------|------|
| `TinyWizardSceneSetup.cs` | `Tiny Wizard/Setup Scene` |
| `AnimatorSetupHelper.cs` | `Tiny Wizard/Setup Player Animations`, `Tiny Wizard/Replace Player Model` |
| `GiantKitchenBuilder.cs` | `Tiny Wizard/Build Giant Kitchen` |
| `InteractionSetupHelper.cs` | `Tiny Wizard/Setup Interaction System` |
| `SpellSetupWindow.cs` | `Tools/Spell System Setup` |
| `HealthSetupWindow.cs` | `Tools/Health System Setup` |
| `SystemSetupWindow.cs` | **`Tiny Wizard/System Setup`** (new in v0.5.0) |

`SystemSetupWindow` buttons:
1. Create ManaUI on Canvas
2. Create Pause Canvas
3. Wire PauseManager
4. Fix PlayerHealth Renderer
Plus "Do All Steps" which runs all four.

---

## 4. Scene Wiring — `Assets/Scenes/TinyWizard.unity`

Key fileIDs (stable anchors for YAML editing):

| Object | fileID |
|--------|--------|
| Player GameObject | `1017788641` |
| Player Animator | `1017788645` |
| Player PlayerController | `1017788642` |
| Player PlayerInputHandler | `1017788644` |
| Player SpellManager | `1017788650` |
| Player PlayerHealth | `1017788652` |
| Player FootstepAudio | `1017788653` |
| Player PlayerAnimation | `1017788654` |
| Player AudioSource | `1017788655` |
| Main Camera GameObject | `849415858` |
| Main Camera ThirdPersonCamera | `849415863` |
| Main Camera CameraShake | `849415864` |
| AttackSpell component | `81287849` |

Wired in v0.5.0 (direct YAML edits):
- AttackSpell GUID fixed: `0ac53a8744c808645b182561f061b9de` → `e246c95641386874ebe1e4576183486a`
  and class identifier `PushSpell` → `AttackSpell`
- `CameraShake` added to Main Camera
- `GameManager` GameObject created with `PauseManager` (fileID `2000000000`/`1`/`2`)
- `AudioSource` + `FootstepAudio` + `PlayerAnimation` added to Player

Created later by running `Tiny Wizard > System Setup`:
- `PauseCanvas` (inactive by default) with `Overlay` + `PauseText`, wired to `PauseManager.pauseCanvas`
- `ManaUI` under Canvas with `ManaBar_Fill` + `ManaText`

Script GUIDs for new scripts:
```
AttackSpell     e246c95641386874ebe1e4576183486a
CameraShake     94ad5cde1f7ecc443985d460e833583e
FootstepAudio   6dc97f7e9361e15479fd9f97c2843f80
PlayerAnimation 839606f70f350884f94ec18ba1e7d698
PauseManager    537200b33af156b4ea5a7b1acbb189b0
ManaUI          ed692bb80109c0443bd062ebdb2118df
```

---

## 5. Animation Setup

- Source clip FBX: `Assets/Animations/PlayerAnimation.fbx` (Generic rig)
  - idle `0-56`, walk `56-130`, run `130-190`, jump `190-247`
- Mesh FBX: `Assets/Animations/animated_wizard_final.fbx`
- Controller: `Assets/Animations/PlayerController.controller`
  - States: Idle, Walk, Run, Jump, Fall
  - Params: `Speed` (float), `IsGrounded` (bool), `JumpTrigger` (trigger)
- `AnimatorSetupHelper.SetupAnimations()` calls, in order:
  `ResetModelFBXClips()` → `ConfigureFBXClips()` → `CreateAnimatorController()` →
  `AssignClipsToController()` → `AssignControllerToPlayer()`
- **Important fix:** only the animation FBX gets custom clips. Earlier versions also
  re-imported clips on the mesh FBX and set an explicit `takeName`; both caused empty
  `curves: []` in the meta and broken animation. Do not reintroduce either.

---

## 6. Gotchas and Known Issues

1. **Built-in font renamed in Unity 6.** `Resources.GetBuiltinResource<Font>("Arial.ttf")`
   throws `ArgumentException`. Use `"LegacyRuntime.ttf"`. This bit us in `SystemSetupWindow`.
2. **`SystemSetupWindow` is not idempotent.** It has no duplicate guard, so running
   "Do All Steps" more than once creates duplicate `ManaUI` / `ManaText` /
   `ManaBar_Fill` objects. The scene currently has **2 of each** — clean up in the
   Hierarchy, and add a `GameObject.Find` guard to the script.
3. **No `MainMenu` scene exists.** Both `PauseManager.MainMenu()` and
   `MainMenuController` reference a scene named `MainMenu`; loading it will fail.
   Only `TinyWizard.unity` is in the build list.
4. **`SpellManager.useMana` is `0` in the scene.** The mana bar will sit full and
   never drain until this is enabled. `ManaUI` is already wired and will respond
   once toggled.
5. **`FootstepAudio.OnFootstep()` will NRE if `defaultSounds` is null** — it reads
   `defaultSounds.pitchMin` unguarded. The scene instance has it serialised, but a
   freshly added component will crash on the first footstep animation event.
6. **`PlayerHealth.playerRenderer` was `{fileID: 0}`** in the scene YAML; the
   "Fix PlayerHealth Renderer" button in `SystemSetupWindow` assigns it.
7. **`PauseManager.pauseKey` is dead config** — pause is driven by
   `PlayerInputHandler.PausePressed` only.
8. **No enemy prefabs.** `EnemySpawner` has empty `enemyPrefabs`/`spawnPoints`.
9. **Bounce spell uses `Rigidbody.linearVelocity`** (Unity 6 API). Player is kinematic,
   so bounce only affects non-kinematic rigidbodies unless `applyToPlayer` is set.
10. **Scene YAML is edited by hand in this project.** It is legitimate to patch it
    directly, but always reuse the fileID anchors in §4 and verify component lists
    (`m_Component`) match the added MonoBehaviour blocks.

---

## 7. How to Continue

Typical order for a new agent:

1. Open `Assets/Scenes/TinyWizard.unity` in Unity 6000.3.11f1 and let it compile.
2. Clean up the duplicate `ManaUI` objects in the Hierarchy; enable
   `SpellManager.useMana`; assign `PlayerHealth.playerRenderer`.
3. Build the three enemy prefabs using the hierarchy and stats in `AGENTS.md`, then
   assign them to an `EnemySpawner`.
4. Create a `MainMenu` scene and add it to build settings so `MainMenu` /
   `Continue` work.
5. Assign materials to `SpellProjectile`, `BouncePad`, `IcePlatform` prefabs.
6. Assign footstep audio clips to `FootstepAudio.defaultSounds` and add
   `OnFootstep()` animation events to the walk/run clips.
7. Add a mana bar and spell name text — `spellIcon` / `spellNameText` on `ManaUI`
   are still unassigned.
8. Play-test: movement, jump, animation, spell casting, pause, mana bar.
9. Commit and tag the next version (`v0.6.0`).

Useful commands: `Tiny Wizard/Setup Scene`, `Tiny Wizard/Setup Player Animations`,
`Tiny Wizard/Build Giant Kitchen`, `Tiny Wizard/Setup Interaction System`,
`Tiny Wizard/System Setup`, `Tools/Spell System Setup`, `Tools/Health System Setup`.
