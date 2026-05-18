# Button Extras

`AchButtonCooldown` and `AchButtonHold` are additive components that extend an existing `Button`.
Both require a `Button` component on the same GameObject to function.

## AchButtonCooldown

Disables the button immediately after a click and re-enables it once the cooldown period has elapsed.
Includes a built-in countdown label to display the remaining time.

### Inspector Fields

| Field | Description |
|---|---|
| `Cooldown` | Minimum wait time in seconds between clicks. Default: `1` |
| `Show Countdown` | Whether to display the remaining cooldown time on a label |
| `Countdown Label` | `Text` component to show the countdown (optional) |
| `On Cooldown Start` | `UnityEvent` fired when the cooldown begins |
| `On Cooldown End` | `UnityEvent` fired when the cooldown expires |

### API

| Member | Description |
|---|---|
| `IsCoolingDown` | Whether the button is currently cooling down |
| `StartCooldown()` | Starts the cooldown manually. Restarts the timer if already cooling down |
| `ResetCooldown()` | Cancels the cooldown immediately and re-enables the button |

### Usage Example

```csharp
// Control the cooldown from code
var cooldown = GetComponent<AchButtonCooldown>();

// Lock the button while waiting for a server response
cooldown.StartCooldown();

// Unlock immediately once the response arrives
cooldown.ResetCooldown();

// Check availability
if (!cooldown.IsCoolingDown)
    Debug.Log("Button is available");
```

Connect a loading spinner's activation to `On Cooldown Start` and its deactivation to `On Cooldown End`
in the Inspector for visual feedback without any extra code.

---

## AchButtonHold

Fires an event repeatedly at a fixed interval while the button is held down.
Useful for continuously changing a value — such as volume or quantity — while the button is pressed.

### Inspector Fields

| Field | Description |
|---|---|
| `Initial Delay` | Time in seconds before the first repeat fires. Default: `0.5` |
| `Repeat Interval` | Time in seconds between each repeat after the initial delay. Default: `0.1` |
| `On Hold Fire` | `UnityEvent` invoked on each repeat |

### API

| Member | Description |
|---|---|
| `IsHolding` | Whether the button is currently being held |

### Usage Example

Attaching `AchButtonHold` to a volume increase button:

```csharp
// Attach AchButtonHold to the volume + Button GameObject
// and connect On Hold Fire to the method below.

public void IncreaseVolume()
{
    AudioManager.Volume = Mathf.Clamp01(AudioManager.Volume + 0.05f);
}
```

```
[Button — Volume +]
 └── [AchButtonHold]
       Initial Delay   : 0.5
       Repeat Interval : 0.1
       On Hold Fire    → IncreaseVolume()
```

After pressing and holding for 0.5 seconds, `IncreaseVolume()` fires every 0.1 seconds until released.

---

## Using Both Together

`AchButtonCooldown` and `AchButtonHold` are independent and can be used on the same button.

```
[Button GameObject]
 ├── Button
 ├── AchButtonCooldown   (2-second cooldown after click)
 └── AchButtonHold       (repeat fire while held)
```

## Related Docs

- [UI System Overview](/en/guide/ui/)
- [AchTimer](/en/guide/timer)
