# EldritchGames.InputSystem

Genre-agnostic input pipeline for Unity. Converts raw input (or AI decisions) into typed commands dispatched to pawns via capability interfaces — no casts, no genre-specific knowledge in the core layer.

---

## Installation

### Via Unity Package Manager

Open **Window → Package Manager → + → Add package from git URL** and enter:

```
https://github.com/<org>/<repo>.git?path=Assets/EldritchGames/InputSystem
```

Or add directly to `Packages/manifest.json`:

```json
"com.eldritchgames.inputsystem": "https://github.com/<org>/<repo>.git?path=Assets/EldritchGames/InputSystem"
```

Pin to a specific commit by appending `#<sha>` to the URL.

**Requires:** `com.unity.inputsystem` 1.7.0+

---

## Architecture

```
PlayerInput / AI
      │
      ▼
IInputContext          ← defines what actions generate which commands
(on IInputContextStack)
      │  CollectCommands(buffer)
      ▼
IController (PlayerController MonoBehaviour)
      │  ExecuteCommand per command
      ▼
IPawnController (your pawn)
      │  command.Execute(this)  →  TryGet<IMovable>()
      ▼
Capability interface (IMovable, IInteractor, …)
```

The key invariant: **commands never reference a concrete pawn type**. They query `ICapabilityProvider.TryGet<T>()` — if the pawn supports the capability, it executes; otherwise it silently no-ops.

---

## Type Hierarchy

```
ICommandSource
  └── IInputContext
        ├── ActionMapInputContext       (abstract class — owns an InputActionMap, auto Enable/Disable)
        ├── ITickableContext            (adds Tick(deltaTime) — IController auto-ticks each frame)
        └── ICompletableInputContext    (adds OnCompleted event + ForceComplete)

ICapabilityProvider
  └── IPawnController
        └── PawnController             (abstract MonoBehaviour — provides TryGet<T>)
```

`ICompletableInputContext` can be combined freely with `ActionMapInputContext` or `ITickableContext` — just extend/implement both.

---

## Core Concepts

| Type | Role |
|---|---|
| `ICommand` | One discrete action (Move, Jump, Attack). Queries a capability, executes if supported. |
| `ICapabilityProvider` | Implemented by pawns. Returns `this as T` — zero allocation, no reflection. |
| `IPawnController` | A pawn that can receive commands. Extends `ICapabilityProvider`. |
| `ICommandSource` | Anything that fills a command buffer — player input, AI, replay. |
| `IInputContext` | A `ICommandSource` with push/pop/pause/resume lifecycle hooks. |
| `ActionMapInputContext` | Abstract base for contexts backed by a Unity `InputActionMap`. Auto-handles Enable/Disable lifecycle; subclasses only implement `CollectCommands`. |
| `ITickableContext` | An `IInputContext` that needs per-frame time advancement via `Tick(deltaTime)`. `IController` auto-ticks the active context if it implements this. |
| `ICompletableInputContext` | An `IInputContext` that fires `OnCompleted(bool)` exactly once and supports `ForceComplete(bool)` for external cancellation. |
| `IInputContextStack` | Manages active context. Push for overlays (menu), Swap for mode changes. |
| `IController` | Per-player driver: drains the active context into a buffer, dispatches to the pawn. Exposes `CurrentContext` and `PushCompletable`. |
| Capability interfaces | `IMovable`, `IInteractor`, `ICharacterSwitchable`, … Declare what a pawn can do. |
| `PawnController` | Abstract `MonoBehaviour` base. Provides the default `TryGet<T>` implementation. |

---

## Context Lifecycle

Stack operations fire lifecycle methods in this order:

```
Push(A)           Push(B)          Pop(B)           Pop(A)
   │                 │                │                │
A.OnPush()      A.OnPause()      B.OnPop()        A.OnPop()
                B.OnPush()       A.OnResume()
```

```
Swap(B)  [replaces A]
   │
A.OnPop()     ← A is gone permanently, no OnResume
B.OnPush()
```

**Only the top context's `CollectCommands` runs each frame.** Paused contexts go silent — their input is neither read nor executed until they resume.

---

## Adding a New Pawn (New Genre)

**1. Extend `PawnController` and declare only the capabilities your pawn supports.**

```csharp
// FPS pawn: can move, look, jump, crouch
public class FPSPawn : PawnController, IMovable, ILookable, IJumpable, ICrouchable
{
    public void Move(Vector2 direction) { /* strafe / walk logic */ }
    public void Look(Vector2 delta)     { /* camera rotation */ }
    public void Jump()                  { /* jump force */ }
    public void Crouch(bool active)     { /* crouch toggle */ }

    public override void ExecuteCommand(ICommand command) => command.Execute(this);
}
```

No `TryGet<T>` needed — `PawnController` base class provides it.

**2. If a command is sent that the pawn does not support, it silently skips.** `MoveCommand` works on any pawn implementing `IMovable`. No existing command needs modification.

---

## Adding a New Command

```csharp
public class AttackCommand : ICommand
{
    public void Execute(ICapabilityProvider provider)
    {
        if (provider.TryGet<IAttackable>(out var attackable))
            attackable.Attack();
    }
}
```

---

## Adding a New Input Context

### Choosing a base type

| My context… | Use |
|---|---|
| reads a Unity ActionMap | extend `ActionMapInputContext` |
| has no ActionMap (AI, code-driven, replay) | implement `IInputContext` directly |
| completes and reports a result | also implement `ICompletableInputContext` |
| advances an internal timer | also implement `ITickableContext` |
| reads an ActionMap AND is completable | extend `ActionMapInputContext`, implement `ICompletableInputContext` |

### With an ActionMap (preferred for player input)

Extend `ActionMapInputContext` and pass the specific map. Enable/Disable lifecycle is handled automatically.

```csharp
public class CombatInputContext : ActionMapInputContext
{
    public CombatInputContext(PlayerInput input)
        : base(input.actions.FindActionMap("Combat", throwIfNotFound: true)) { }

    public override void CollectCommands(IList<ICommand> buffer)
    {
        if (ActionMap["Attack"].triggered)
            buffer.Add(new AttackCommand());

        Vector2 move = ActionMap["Move"].ReadValue<Vector2>();
        if (move != Vector2.zero)
            buffer.Add(new MoveCommand(move));
    }
}
```

Pass the **specific map**, not `input.actions` (the whole asset) — this prevents accidentally enabling unrelated maps such as UI bindings.

### With an ActionMap AND completable

Extend `ActionMapInputContext` and also implement `ICompletableInputContext`. ActionMap lifecycle is still automatic; add `ForceComplete` and the `OnCompleted` event.

```csharp
public class ConfirmPromptContext : ActionMapInputContext, ICompletableInputContext
{
    private bool _done;
    public event Action<bool> OnCompleted;

    public ConfirmPromptContext(PlayerInput input)
        : base(input.actions.FindActionMap("Player", throwIfNotFound: true)) { }

    public override void CollectCommands(IList<ICommand> buffer)
    {
        if (_done) return;
        if (ActionMap["Confirm"].WasPerformedThisFrame())
            ForceComplete(true);
    }

    public void ForceComplete(bool success)
    {
        if (_done) return;
        _done = true;
        OnCompleted?.Invoke(success);
    }
}
```

### Without an ActionMap

Implement `IInputContext` directly when the context is not driven by a Unity ActionMap (battle UI driven by code, AI, replay).

```csharp
public class NullInputContext : IInputContext
{
    public void CollectCommands(IList<ICommand> buffer) { }
    public void OnPush()   { }
    public void OnPop()    { }
    public void OnResume() { }
    public void OnPause()  { }
}
```

### AI input

`IInputContext` is device-agnostic — AI decisions flow through the same pipeline as player input. Swap an AI context onto any controller to drive its pawn with the same command/capability system.

```csharp
public class EnemyAIContext : IInputContext
{
    private readonly IEnemyBrain _brain;
    public EnemyAIContext(IEnemyBrain brain) => _brain = brain;

    public void CollectCommands(IList<ICommand> buffer)
    {
        var decision = _brain.Decide();
        if (decision.Move != Vector2.zero) buffer.Add(new MoveCommand(decision.Move));
        if (decision.Attack)               buffer.Add(new AttackCommand());
    }

    public void OnPush()   => _brain.Activate();
    public void OnPop()    => _brain.Deactivate();
    public void OnResume() { }
    public void OnPause()  { }
}
```

### Time-advancing contexts

Implement `ITickableContext` when the context needs to advance an internal timeline (e.g. QTE beat windows). `IController` automatically calls `Tick(deltaTime)` on the active context before `CollectCommands` each frame — no external tick management needed.

```csharp
public class TimedPromptContext : ITickableContext, ICompletableInputContext
{
    private float _elapsed;
    private readonly float _limit;
    private bool _done;

    public event Action<bool> OnCompleted;

    public TimedPromptContext(float timeLimit) => _limit = timeLimit;

    public void Tick(float deltaTime)
    {
        if (_done) return;
        _elapsed += deltaTime;
        if (_elapsed >= _limit) ForceComplete(false);
    }

    public void ForceComplete(bool success)
    {
        if (_done) return;
        _done = true;
        OnCompleted?.Invoke(success);
    }

    public void CollectCommands(IList<ICommand> buffer) { /* read input here */ }
    public void OnPush()   { }
    public void OnPop()    { }
    public void OnResume() { }
    public void OnPause()  { }
}
```

---

## Completable Contexts

Use `ICompletableInputContext` for bounded interactions — QTE windows, prompts, minigame phases — that know when they are done.

**Rules:**
- `OnCompleted` fires exactly once (`true` = success, `false` = fail/timeout).
- `ForceComplete(bool)` allows external cancellation (timeout coroutine, state machine abort). No-ops if already completed.
- Stop generating commands after completion.

**Prefer `PushCompletable` over manual Push + subscribe + Pop** to avoid lifecycle bugs:

```csharp
// Manual (error-prone — easy to forget the PopContext):
var ctx = new ShieldPromptContext(input);
ctx.OnCompleted += success => { HandleResult(success); controller.PopContext(); };
controller.PushContext(ctx);

// Preferred — pop is automatic:
controller.PushCompletable(ctx, success => HandleResult(success));
```

**Always pair `PushCompletable` with a timeout** for any context that can fail to complete (player disconnects, game abort). Call `ForceComplete(false)` from a timeout coroutine — without it, a context that never fires `OnCompleted` stays on the stack permanently.

---

## Context Stack: Push vs Swap

| Operation | Use case | Stack depth |
|---|---|---|
| `Push` / `PushCompletable` | Overlay a new context (open menu, QTE prompt) | +1 |
| `Pop` | Dismiss overlay, resume previous context | -1 |
| `Swap` | Replace active context permanently (exploration → combat) | unchanged |

```csharp
// Open inventory (pause gameplay input, activate UI input)
controller.PushContext(inventoryContext);

// Close inventory (resume gameplay)
controller.PopContext();

// Completable overlay — auto-pops when the context fires OnCompleted:
controller.PushCompletable(shieldPrompt, success => ApplyDefenseResult(success));

// Enter vehicle (replace exploration with driving, no context to return to)
controller.SwitchContext(drivingContext);
```

`CurrentContext` exposes the active top-of-stack context for read-only queries (e.g. checking what type of QTE is running) without exposing the stack itself.

---

## Wire-Up (Scene Setup)

```csharp
// GameStateManagerBehaviour.Start()
var stack   = new InputContextStack();
var context = new PlayerInputContext(input);

controller.Initialize(input.playerIndex, pawn, stack);
manager.SwitchState(GameStateType.Exploration, controller, pawn, context);
```

`PlayerController` (MonoBehaviour) calls `Tick(Time.deltaTime)` in `Update` automatically once initialized. Each tick:
1. If the active context implements `ITickableContext`, `Tick(deltaTime)` is called first.
2. `CollectCommands` fills the buffer.
3. Each command is dispatched to the current pawn.

---

## Local Multiplayer

Instantiate one `PlayerController` + one `InputContextStack` per player. Unity's `PlayerInputManager` assigns a `PlayerInput` per device; pass `input.playerIndex` to `Initialize`.

```csharp
// P1
controllerP1.Initialize(inputP1.playerIndex, pawnP1, new InputContextStack());

// P2
controllerP2.Initialize(inputP2.playerIndex, pawnP2, new InputContextStack());
```

---

## Common Mistakes

**1. Enable the whole asset instead of a specific map.**
`input.actions.Enable()` enables every map — Player, UI, and anything else in the asset. Always scope to the map you own. `ActionMapInputContext` enforces this by design; pass `FindActionMap("X", throwIfNotFound: true)`.

**2. Fire `OnCompleted` more than once.**
Guard with a `_done` bool set to `true` before invoking the event. Every example above follows this pattern.

**3. Use `PushContext` for completable contexts without a timeout.**
If `OnCompleted` never fires, the context stays on the stack forever. Use `PushCompletable` and, for any context that can time out, call `ForceComplete(false)` from a timeout coroutine or game state callback.

Exception: if the pop must happen *after* a command is dispatched through the pawn pipeline (e.g. a context whose `OnCompleted` fires before `CollectCommands` has had a chance to run), manage the pop manually via a pawn callback instead.

**4. Read input in `OnPush` or `OnPop`.**
Input state is mid-frame during lifecycle callbacks. Read input only inside `CollectCommands`.

**5. Use `Swap` when you need to return to the previous context.**
`Swap` calls `OnPop` on the replaced context — it is gone permanently, with no path back via `Pop`. Use `Push`/`Pop` when you need to resume a paused context later.

---

## Testing Patterns

Contexts are pure C# — no `MonoBehaviour`, no scene required. All tests run in Unity's EditMode test runner.

```csharp
// Stack lifecycle — mock IInputContext to verify call order
var ctx = new Mock<IInputContext>();
var stack = new InputContextStack();

stack.Push(ctx.Object);
ctx.Verify(c => c.OnPush(), Times.Once);

stack.Pop();
ctx.Verify(c => c.OnPop(), Times.Once);
```

```csharp
// Command execution — use CapabilityProviderStub (Tests assembly)
var stub = new CapabilityProviderStub(movable: mockMovable.Object);
new MoveCommand(Vector2.right).Execute(stub);
mockMovable.Verify(m => m.Move(Vector2.right), Times.Once);
```

See `Assets/Tests/EditMode/Control/` for full examples: `InputContextStackTest`, `CommandTests`, `SequenceQTEContextTests`.

---

## Assembly Notes

- `EldritchGames.InputSystem.asmdef` contains interfaces, `ActionMapInputContext`, and the `PawnController` base class.
- References `com.unity.inputsystem` — required by `ActionMapInputContext` (`InputActionMap`). Concrete input reading beyond the base class stays in the `Control` assembly.
- To add a new capability interface (e.g. `ILookable`), add a file to this package only. No other files change.

---

## Namespace Reference (Control Assembly)

The `Control` assembly implements the contracts defined in this package. Namespaces are grouped by concern:

| Namespace | Contents |
|---|---|
| `Control.Capabilities` | `IMovable`, `IInteractor`, `ICharacterSwitchable`, `IBattleControllable` |
| `Control.Commands` | `MoveCommand`, `ChangeCharacterCommand`, `InteractCommand`, `ActivateCommand` |
| `Control.Commands.Battle` | `SubmitBattleActionCommand` |
| `Control.Components` | `PlayerController`, `ExplorationPawn`, `BattlePawn` |
| `Control.InputContext` | `PlayerInputContext`, `NullInputContext`, `InputContextStack` |
| `Control.Battle` | `BattleInputContext`, `ShieldDefenseContext`, `CoopSyncContext` |
| `Control.QTE` | `ITickableQTEContext`, `SequenceQTEContext`, `SequenceQTE` |

**Typical using block for battle input wiring:**

```csharp
using Control.Components;      // PlayerController, BattlePawn
using Control.InputContext;    // InputContextStack, PlayerInputContext
using Control.Battle;          // BattleInputContext, ShieldDefenseContext, CoopSyncContext
using Control.QTE;             // SequenceQTE, SequenceQTEContext, ITickableQTEContext
```

**Separation rationale:**

- `Control.InputContext` — general-purpose contexts driven by `PlayerInput`. No battle dependency.
- `Control.Battle` — contexts that exist only during an active battle (defense, coop sync). Depend on `EldritchGames.RPG.BattleSystem`.
- `Control.QTE` — QTE execution layer: the context that tracks beat timing (`SequenceQTEContext`) and the `IBattleQTE` implementation that creates it (`SequenceQTE`). Kept together because the two types are inseparable.
