# Monster Survivor

## Game Overview
A work-in-progress 2D base builder with wave defense for the Bumi Studio technical test.
The current milestone implements the economy plus one manually started combat
encounter: build a turret, then defend the generator from a waypoint-following enemy.
Clearing this encounter is not yet the full three-wave game required by the brief.

## How to Run
- Engine & version used: Unity 6000.0.75f1, C#.
- Open the project, then open `Assets/Game/Scenes/Gameplay.unity` and press Play.
- Move using WASD, arrow keys, or a gamepad's left stick.
- Start with 10 wood and 10 stone (editable on the Player prefab). Walk over a wood
  or stone pickup to collect one unit. The top-left HUD updates immediately.
- Wood and stone pickups use the authored positions under the Resources parent.
- Approach the faded Lumber Camp or Quarry and remain in its build zone for 3 seconds.
  Costs are reserved at the start: Lumber Camp = 5 wood + 3 stone; Quarry = 3 wood +
  5 stone. Leaving before completion refunds the cost and resets progress.
- A finished facility produces one resource every 3 seconds, storing at most 6.
  Walk into the collection area directly in front to collect its stock. Remaining
  there collects new output automatically. A full store restarts its timer when
  space becomes available; it does not accumulate a production backlog.
- Building costs/duration are editable in `Assets/Game/Data/Buildings`; output type,
  interval, and capacity are in `Assets/Game/Data/Production`.
- Approach the turret site to build it for 6 wood + 4 stone (3 seconds).
- Press Space when ready. Construction is disabled during combat; an unfinished
  build is cancelled and refunded. The turret attacks automatically once built.
- The enemy has 40 HP and attacks the 100-HP generator. Defeat the enemy to clear
  the encounter; generator HP reaching zero ends it in defeat.
- Press R to reload the scene and reset inventory, buildings, HP, and the encounter.
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
- ConstructionProcess and ProductionBuffer are plain C# classes in separate assemblies
  referencing the wallet contract. Unity components handle proximity and lifecycle,
  while ScriptableObjects configure costs and production.
- Trigger zones register build candidates without searching the scene. The player
  selects one nearest site and keeps it until completion or exit. Disabling the
  player, site, or build zone cancels an unfinished reservation.
- Building status UI and sprite opacity are embedded in the building prefabs. The
  authored scene hierarchy and building positions are preserved. UI refreshes from
  events, with progress updated only during active construction.
- HealthState and AttackCooldown are Unity-independent combat rules. Enemy and
  turret attack stats use ScriptableObjects under `Assets/Game/Data/Combat`; maximum
  HP is configured on each prefab Health component.
- Enemy movement follows explicit scene waypoints, then attacks the closest point
  on the generator collider. Attack range accounts for the enemy footprint. Player
  collisions are ignored for this encounter so the worker cannot body-block enemies.
- Turrets query an injected enemy-target provider, not scene searches or allocating
  physics queries. A small registered roster is sufficient for this prototype.
- Shots apply damage immediately (hitscan, without line-of-sight blocking). Each
  turret reuses one beam renderer and sprite; no projectile object is allocated per
  attack. The turret body is static so the player cannot push it.
- CombatEncounter coordinates the single encounter, actor activation, and terminal
  outcomes. Generator defeat takes priority over a clear notification. The gameplay
  scene is included in Build Settings so R can reload it in a PC build.
- No extra framework or dependency was introduced.
- Visual assets: Cainos, Pixel Art Top Down - Basic (Unity Asset Store).

## What I Would Do With More Time
Next milestones: three build/defend cycles, escalating enemy spawning, world pickup
refill at each build phase, and a run result screen. Then balance construction and
production costs, add barricades, and validate authored routes in the Windows build.

## Known Issues
- This is a single-enemy combat milestone, not the complete playable submission.
- Production continues during play, including after the encounter ends. The full
  run lifecycle and stopping economy systems will be connected with wave/game flow.
- Enemies currently attack only the generator. Turret destruction and barricade
  interaction are not implemented.
- Enemy paths are authored waypoints; dynamic obstacle avoidance is not implemented.
  Routes and visual combat feedback still need Play Mode verification.
- Camera bounds and player world bounds are not implemented.
- Wave-based pickup refill is not connected yet; waves are a later milestone.
- Unity Editor and Windows build execution have not been verified in the development
  environment used for this change. Check movement, collisions, animation direction,
  camera tracking, pickup collection, and focus changes in the specified Unity version.
- Wallet, construction, production, health, and cooldown tests are provided in `Assets/Game/Tests/EditMode`
  for the Unity Test Runner (EditMode); they have not been executed here. They cover
  mixed-cost atomicity, cancellation/refunds, completion, production timing/capacity,
  no backlog when full, partial collection, invalid input, and overflow.
- Manual resource check: confirm 10/10 at start, collect each nearby pickup, confirm
  only the matching counter increases by one and revisiting the location gives no
  additional reward. Stop and Play again to confirm the scene resets.
- Manual building check: approach a faded facility, verify the cost is deducted once,
  then leave midway and verify a full refund. Return and complete the build; leaving
  afterward must not refund it. Wait away from the output area for at least 18 seconds
  and verify stock caps at 6. Collect it, then verify the next output takes 3 seconds.
- Edge checks: try building with insufficient resources, disable the player or site
  during construction, and overlap two build zones to verify only one reservation.
- Manual combat check: build the turret, press Space, and verify an enemy travels
  along the two waypoint markers, loses HP from shots, and clears the encounter
  exactly once. Restart and press Space without a turret to verify generator defeat.
- Check that unfinished construction is refunded on Space, no construction starts
  during combat, the player cannot push the turret or body-block the enemy, and R
  fully resets the scene. The new EditMode cases include lethal/reentrant damage,
  one-time death notification, attack timing, and no burst after a large frame stall.
