# Monster Survivor

## Game Overview
A 2D base builder with wave defense for the Bumi Studio technical test.
The current milestone implements the economy and a wave-based combat encounter:
build defenses, manage the preparation timer, and defend the generator from
waypoint-following enemies across multiple phases.

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
- The preparation phase has an auto-start countdown. Press Space to skip the timer
and start the combat wave immediately. Construction is disabled during combat; an unfinished
  build is cancelled and refunded. The turret attacks automatically once built.
- The enemy has 40 HP and attacks the 100-HP generator. Defeat the enemy to clear
  the encounter; generator HP reaching zero ends it in defeat.
- Press R to reload the scene and reset inventory, buildings, HP, and the encounter.
- Build location: [Build](https://drive.google.com/drive/folders/1UQLU6X_NUuXNY6BGojg5aHBo4ifZdQWn?usp=sharing)

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
- Building status UI and sprite opacity are embedded in the building prefabs. The
  authored scene hierarchy and building positions are preserved. UI refreshes from
  events, with progress updated only during active construction.
- HealthState and AttackCooldown are Unity-independent combat rules. Enemy and
  turret attack stats use ScriptableObjects under `Assets/Game/Data/Combat`; maximum
  HP is configured on each prefab Health component.
- Enemy spawning utilizes a custom object pool (Queue and HashSet) to reuse instances
across waves, preventing runtime instantiation overhead.
- Enemy targeting uses Physics2D.OverlapCircle with ContactFilter2D and a shared
static Collider2D[] buffer. This guarantees zero-allocation physics in Unity 6,
avoiding GC spikes during wave defense.
- Math calculations in FixedUpdate use sqrMagnitude instead of Vector2.Distance
to bypass expensive CPU square root operations.
- AudioManager auto-generates its hierarchy on Awake and uses O(1) dictionary lookups.
It implements SFX throttling (50ms cooldown per clip) to prevent audio peaking and
voice stealing when multiple turrets fire or enemies die simultaneously.
- Shots apply damage immediately (hitscan). Each turret reuses one beam renderer and
sprite; no projectile object is allocated per attack.
- No extra framework or dependency was introduced.

## What I Would Do With More Time
Next milestones: world pickup refill at each build phase, and a comprehensive run
result screen showing wave stats. Then balance construction and production costs,
add barricades, and validate authored routes in the Windows build.

## Known Issues
- This is a single-enemy combat milestone, not the complete playable submission.roduction continues during play, including after the encounter ends. The full
run lifecycle and stopping economy systems will be connected with game flow.
- Enemies currently prioritize the generator but will attack blocking structures.
Full barricade interaction is not fully polished.
- Enemy paths are authored waypoints; dynamic obstacle avoidance is not implemented.
- Camera bounds and player world bounds are not implemented.
- Wallet, construction, production, health, and cooldown tests are provided in Assets/Game/Tests/EditMode
for the Unity Test Runner (EditMode). They cover mixed-cost atomicity, cancellation,
production timing/capacity, no backlog when full, invalid input, and overflow.

## Assets & Credits
- Visual assets: [Cainos, Pixel Art Top Down - Basic](https://assetstore.unity.com/packages/p/pixel-art-top-down-basic-187605)
- Generated by ChatGPT and GoogleFlow
  - [Buildings](https://chatgpt.com/s/m_6ae04bc38be481919d24026edf026f00)
  - [Enemy Gate](https://chatgpt.com/s/m_6ae04adf29388191a5a89a4f21023bf8)
  - [Barricade](https://flow.google.com/shared/image/9a116149-29d2-45a3-ad97-c1268de692fe)
- BGM: [Kevin MacLeod](https://incompetech.com/music/royalty-free/music.html)
- SFX: [opengameart](https://opengameart.org/content/sfx-0), [torshid.itch.io](https://torshid.itch.io/western-front-lite), [pixabay](https://pixabay.com/sound-effects/)
- VFX: [cartoon-fx-remaster-free](https://assetstore.unity.com/packages/vfx/particles/cartoon-fx-remaster-free-109565)
