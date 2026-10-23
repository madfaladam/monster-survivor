# Monster Survivor

## Game Overview
A work-in-progress 2D base builder with wave defense for the Bumi Studio technical test.
The current milestone implements player movement and camera follow. Resource collection,
construction, waves, and win/lose conditions are not implemented yet.

## How to Run
- Engine & version used: Unity 6000.0.75f1, C#.
- Open the project, then open `Assets/Game/Scenes/Gameplay.unity` and press Play.
- Move using WASD, arrow keys, or a gamepad's left stick.
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
- No extra framework or dependency was introduced.
- Visual assets: Cainos, Pixel Art Top Down - Basic (Unity Asset Store).

## What I Would Do With More Time
Next milestones: wood/stone pickups and wallet, proximity construction, production
buffers, then three complete build/defend cycles with victory and defeat.

## Known Issues
- This is the movement milestone, not the complete playable submission.
- Camera bounds and player world bounds are not implemented.
- Unity Editor and Windows build execution have not been verified in the development
  environment used for this change. Check movement, collisions, animation direction,
  camera tracking, and focus changes in the specified Unity version.
