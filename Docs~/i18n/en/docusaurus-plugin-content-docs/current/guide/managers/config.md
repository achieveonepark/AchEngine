# ConfigManager

PlayerPrefs-backed key-value settings store. Use it for simple option values like app volume or language settings.

## API

```csharp
var cfg = ServiceLocator.Resolve<ConfigManager>();

// Register a key (set initial value)
cfg.AddKey("bgmVolume", 1.0f);
cfg.AddKey("language",  "en");

// Read a value
float vol = cfg.GetConfig<float>("bgmVolume", fallback: 1f);

// Write a value (automatically persisted to PlayerPrefs)
cfg.SetConfig("bgmVolume", 0.5f);
```

> `SetConfig` does nothing for keys that have not been registered with `AddKey`.
