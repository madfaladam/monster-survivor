# Monster Survivor

## Game Overview
A work-in-progress 2D base builder with wave defense for the Bumi Studio technical test.
The current milestone implements player movement, camera follow, starting wood/stone,
and automatic resource collection. Construction, production, waves, and win/lose
conditions are not implemented yet.

## How to Run
- Engine & version used: Unity 6000.0.75f1, C#.
- Open the project, then open `Assets/Game/Scenes/Gameplay.unity` and press Play.
- Move using WASD, arrow keys, or a gamepad's left stick.
- Start with 10 wood and 10 stone (editable on the Player prefab). Walk over a wood
  or stone pickup to collect one unit. The top-left HUD updates immediately.
- Four sample pickups are placed near the player; the existing stone elsewhere
  in the scene is also collectible.
- Build location: not available yet.

## Technical Decisions
- Player input reads a private clone of the existing Input System Move action, keeping
  enable/disable ownership separate from other consumers of the shared asset.
- Movement uses the prefab's dynamic Rigidbody2D in FixedUpdate, with gravity disabled,
  rotation frozen, and interpolation enabled. Input magnitude is capped to prevent
  faster diagonal movement while retaining analog stick control.
- Animation is a separate component using the existing visual asset's Direction and
  IsMoving parameters. Gameplay movement and camera scripts are project-owned.
- Camera follow runs in LateUpdate, keeps its existing Z position, and exposes offset
  and smoothing in the Inspector. It snaps to the target on scene start.
- The wallet is a plain C# class in a Unity-independent assembly. Mixed wood/stone
  costs are validated before either balance changes, and observers receive one
  notification per successful transaction. Negative values and overflow are rejected.
- PlayerInventory owns the wallet; PlayerCollector handles trigger contacts; each
  ResourcePickup prevents duplicate collection and deactivates instead of destroying.
  Restore reuses a collected pickup for the future wave refill system.
- The HUD subscribes to wallet changes instead of polling every frame.
- No extra framework or dependency was introduced.
- Visual assets: Cainos, Pixel Art Top Down - Basic (Unity Asset Store).

## What I Would Do With More Time
Next milestones: proximity construction, production buffers, wave-based world pickup
refill, then three complete build/defend cycles with victory and defeat.

## Known Issues
- This is the movement milestone, not the complete playable submission.
- Camera bounds and player world bounds are not implemented.
- Wave-based pickup refill is not connected yet; waves are a later milestone.
- Unity Editor and Windows build execution have not been verified in the development
  environment used for this change. Check movement, collisions, animation direction,
  camera tracking, pickup collection, and focus changes in the specified Unity version.
- Wallet tests are provided in `Assets/Game/Tests/EditMode` for the Unity Test Runner
  (EditMode); they have not been executed here. They cover mixed-cost atomicity,
  exact-balance spending, notifications, invalid amounts, and overflow.
- Manual resource check: confirm 10/10 at start, collect each nearby pickup, confirm
  only the matching counter increases by one and revisiting the location gives no
  additional reward. Stop and Play again to confirm the scene resets.
