# Extension Methods

The `AchEngine.Extensions` assembly includes 60+ extension methods for everyday Unity development.

## Categories

### Collections
`ArrayExt`, `ListExt`, `DictionaryExt`, `IEnumerableExt`, `LinqE`

### Unity Types
`GameObjectExt`, `Vector2Ext`, `Vector3Ext`, `ComponentExt`, `RectExt`

### UI
`RectTransformExt`, `ImageExt`, `RawImageExt`, `GraphicExt`, `ButtonClickedEventExt`

### Primitive Types
`StringExt`, `StringParseExt`, `IntExt`, `FloatExt`, `BoolExt`, `EnumExt`, `ColorExt`

### Utility Classes
`Selectable<T>`, `SelectableList<T>`, `MultiDictionary<K,V>`, `StringAppender`, `RandomUtils`, `CameraUtils`

## Usage Examples

```csharp
// GameObjectExt
var rb = gameObject.GetOrAddComponent<Rigidbody>();

// Selectable<T> — observable value
var hp = new Selectable<int>(100);
hp.OnValueChanged += (prev, next) => UpdateHpBar(next);
hp.Value = 80;

// MultiDictionary
var skills = new MultiDictionary<string, Skill>();
skills.Add("warrior", new Skill("Slash"));
skills.Add("warrior", new Skill("Block"));
```
