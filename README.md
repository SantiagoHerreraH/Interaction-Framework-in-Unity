# Unity Modular Journal System


````markdown
# Interaction Machine (Unity)

An extensible **interaction state machine** for Unity that lets you chain **named interactions** over **one or many targets** with clear lifecycle hooks: **Start**, **Update**, **FixedUpdate**, and **End**. It supports automatic triggering from **2D physics** events, per-target sequencing, and clean teardown on disable/destroy.

> Built to be designer-friendly using `SerializeReference` + subclass selection (via MackySoft).

---

## Table of Contents
- [Key Features](#key-features)
- [Concepts](#concepts)
- [Getting Started](#getting-started)
- [Authoring Interactions](#authoring-interactions)
- [Sequences & Triggers](#sequences--triggers)
- [Built-in Interactions](#built-in-interactions)
- [API Surface](#api-surface)
- [Notes & Gotchas](#notes--gotchas)
- [Folder Structure](#folder-structure)

---

## Key Features
- 🧩 **Composable interactions** with lifecycle hooks:
  - `StartEndInteractions`, `StartInteractions`, `UpdateInteractions`, `FixedUpdateInteractions`, `EndInteractions`
- ⛓️ **Sequences** per target: run interaction A → B → C, and optionally kick off other sequences **when an interaction starts/ends**.
- 🎯 **Multi-target**: drive interactions on one or many targets simultaneously.
- ⚙️ **Runtime policies** for recalls (what happens if you start on an active target), target removal timing, and automatic starts.
- 🎮 **Optional Input System** usage (only for `RegisterPlayer`; not required to run).
- 🧰 **Tons of ready-made interactions** out of the box (see below).

> **Serialization & Inspector UX**: This project uses **MackySoft SerializeReference Extensions** so you can assign concrete subclasses under `SerializeReference` fields in the inspector.  
> Plugin repo: https://github.com/mackysoft/Unity-SerializeReferenceExtensions.git

---

## Concepts

### Interfaces
```csharp
public interface IInteract {
    void Interact(GameObject caller, GameObject target);
}

public interface IStartEndInteract {
    void StartInteract(GameObject caller, GameObject target);
    void EndInteract(GameObject caller, GameObject target);
}
````

* Implement **`IInteract`** for single-shot calls in Start/Update/FixedUpdate/End phases.
* Implement **`IStartEndInteract`** when you need a paired setup/teardown.

### Interaction (data)

```csharp
[Serializable]
public class Interaction {
    public float Duration;

    // Paired lifecycle
    [SerializeReference, SubclassSelector] public List<IStartEndInteract> StartEndInteractions = new();

    // Phase lists
    [SerializeReference, SubclassSelector] public List<IInteract> StartInteractions = new();
    [SerializeReference, SubclassSelector] public List<IInteract> UpdateInteractions = new();
    [SerializeReference, SubclassSelector] public List<IInteract> FixedUpdateInteractions = new();
    [SerializeReference, SubclassSelector] public List<IInteract> EndInteractions = new();
}
```

### InteractionData & Sequences

* **`InteractionData`**: `{ Name, Interaction }` — a catalog entry (unique `Name` enforced).
* **`InteractionSequence`**: ordered list of **Interaction names** + a rule for when to begin:

  * `OnStartInteraction` (when the machine starts for a target)
  * `OnReferencedInteractionStart`
  * `OnReferencedInteractionEnd`

---

## Getting Started

1. **Add the component**

   * Put `InteractionMachine` on a GameObject (the **caller**).
2. **Define a catalog**

   * Populate `_interactionData` with **unique names** and their `Interaction` definitions.
3. **Set sequences**

   * Create `_interactionSequences` listing **interaction names in order** and a **WhenToStart** rule.
   * Names are validated on `Awake`; missing names log errors with helpful context.
4. **Start interactions**

   * Call:

     * `StartInteraction(targetGameObject)`
     * or use helpers: `StartInteractionWithMainCamera()`, `StartInteractionWithAllTargets()`, etc.
   * Or let **automatic start** handle it via 2D triggers/collisions (see below).

> **Physics**: The included automatic hooks are **2D** (`OnTriggerEnter2D` / `OnCollisionEnter2D` / exits). For 3D, add the 3D counterparts if needed.

---

## Authoring Interactions

Create a new script implementing one of the interfaces, then assign it under a `SerializeReference` list (thanks to MackySoft’s subclass selector).

**Example (`IStartEndInteract`)**:

```csharp
public class PlayEffect_OnStartStop : IStartEndInteract {
    public void StartInteract(GameObject caller, GameObject target) { /* enable VFX/SFX */ }
    public void EndInteract(GameObject caller, GameObject target)   { /* disable VFX/SFX */ }
}
```

**Example (`IInteract` on Update)**:

```csharp
public class FaceTarget_Update : IInteract {
    public void Interact(GameObject caller, GameObject target) {
        if (!caller || !target) return;
        caller.transform.forward = (target.transform.position - caller.transform.position).normalized;
    }
}
```

Drop these into the corresponding lists in an `Interaction`:

* Setup/teardown → `StartEndInteractions`
* One-shot at start → `StartInteractions`
* Every frame → `UpdateInteractions`
* Every physics step → `FixedUpdateInteractions`
* One-shot at end → `EndInteractions`

---

## Sequences & Triggers

* On **start** (manual or automatic), the machine begins all sequences whose `WhenToStart == OnStartInteraction`.
* Each sequence advances through its interactions by **Duration** (if configured to remove targets after duration).
* When an interaction **starts** or **ends**, the machine can **trigger other sequences** whose `WhenToStart` references that interaction name.
* Per-target **recall policy** decides what happens if you call start again while sequences are already running:

  * Ignore, full restart, recall first, recall current, or recall current + reset time.

---

## Built-in Interactions

Already included and ready to use (names derived from the scripts in the repo):

* `AddForce_Interaction`
* `TargetedAddForce_Interaction`
* `SetVelocity_Interaction`
* `SetActive_Interaction`
* `SetBoolInAnimator_Interaction`
* `PlaySound_Interaction`
* `SpawnGameObject_Interaction`
* `TrailRenderer_Interaction`
* `UnityEvent_Interaction`
* `UnityTargetEvent_Interaction`
* `PlayerStatInteract_Interaction`
* `Interact_Interaction`

> These cover common gameplay needs: physics pushes, velocity setting, activation toggles, animator flags, SFX, spawning, trail control, UnityEvents to hook custom logic, player stat touches, and forwarding/bridging interactions. You can mix and match them inside a single `Interaction`.

---

## API Surface

### Public methods (high level)

* **Targets**

  * `AddTarget(GameObject)`, `RemoveTarget(GameObject)`, `RemoveAllTargets()`
* **Start**

  * `StartInteraction(GameObject target)`
  * `StartInteractionWithMainCamera()`
  * `StartInteractionWithAllTargets()`
  * `StartInteractionWithLastTarget()`
  * `StartInteractionWithAllButLastTarget()`
* **Stop**

  * `EndAllState()` — gracefully ends all running interactions on all targets (calls `EndInteract` + `EndInteractions`).

### Automatic start & removal

* **Add targets on** `OnTriggerEnter2D` / `OnCollisionEnter2D` if `_getTargetsOnTriggerOrCollisionEnter` is true.
* **Start when**:

  * `NoAutomaticCall`
  * `OnTriggerOrCollisionEnter`
  * `OnTargetNumberEqualRegisteredPlayerNumber` (use `RegisterPlayer(PlayerInput)` to track players)
* **Remove targets**:

  * `AfterDuration` (automatic when all sequences end for a target)
  * `OnCollisionAndTriggerExit` (on 2D exit events)

---

## Notes & Gotchas

* **Unique names**: `_interactionData` names must be unique (validated on `Awake`).
* **2D events**: Only 2D physics events are wired; add 3D versions if your game uses 3D physics.
* **Recall policies**: Be intentional—recalling Start hooks can be great for refreshing audio/VFX, but resetting timers changes pacing.
* **Sequence/State sync**: Internally maintains parallel lists (definitions + runtime data). If extending internals, keep them aligned.
* **Camera dependency**: `StartInteractionWithMainCamera()` expects `Camera.main` to be present.

---

## Folder Structure

```
/InteractionMachine (scripts you pasted)
/Interactions (ready-made interaction implementations)
  ├─ AddForce_Interaction.cs
  ├─ TargetedAddForce_Interaction.cs
  ├─ SetVelocity_Interaction.cs
  ├─ SetActive_Interaction.cs
  ├─ SetBoolInAnimator_Interaction.cs
  ├─ PlaySound_Interaction.cs
  ├─ SpawnGameObject_Interaction.cs
  ├─ TrailRenderer_Interaction.cs
  ├─ UnityEvent_Interaction.cs
  ├─ UnityTargetEvent_Interaction.cs
  ├─ PlayerStatInteract_Interaction.cs
  └─ Interact_Interaction.cs
/MackySoft (external plugin for SerializeReference inspector UX)
```

> **MackySoft SerializeReference Extensions (external plugin):**
> [https://github.com/mackysoft/Unity-SerializeReferenceExtensions.git](https://github.com/mackysoft/Unity-SerializeReferenceExtensions.git)

