# Draggable & タッチオブジェクト

AchEngineは2Dワールドオブジェクトのドラッグおよびタッチ処理のために3つのコンポーネントを提供します。

## Draggable

`MonoBehaviour`に`Draggable`コンポーネントを付けると、マウス/タッチでドラッグできるようになります。
`Physics2DRaycaster`がない場合はメインカメラに自動的に追加されます。

```csharp
using AchEngine.UI;

public class CardView : Draggable
{
    protected override void Start()
    {
        base.Start(); // Physics2DRaycasterを自動追加

        OnTouchDown += () => Debug.Log("カードを掴んだ");
        OnTouching  += pos => Debug.Log($"ドラッグ中: {pos}");
        OnTouchUp   += hits => Debug.Log($"ドロップ、衝突体: {hits.Length}個");
    }
}
```

### イベント

| イベント | シグネチャ | 説明 |
|---|---|---|
| `OnTouchDown` | `Action` | ポインター押下直後 |
| `OnTouching` | `Action<Vector3>` | ドラッグ中 (ワールド座標) |
| `OnTouchUp` | `Action<Collider2D[]>` | ドロップ時、重なったCollider2Dリストを渡す |

### プロパティ

| プロパティ | 説明 |
|---|---|
| `originalPos` | ドラッグ開始時点のワールド座標 (`protected`) |

## TouchableObject

タップ/クリックのみを処理するオブジェクトに使用します。`OnTouched()`をオーバーライドしてください。

```csharp
using AchEngine.UI;

public class EnemyObject : TouchableObject
{
    protected override void OnTouched()
    {
        Debug.Log("敵をクリックした");
    }
}
```

> `TouchableObject`は`ObjectTouchManager`によって検出されます。シーンに`ObjectTouchManager`が存在している必要があります。

## ObjectTouchManager

シーンに1つだけ存在するシングルトンマネージャーです。毎フレームマウス左ボタンクリックを検出し、
2Dレイキャストで`TouchableObject`を見つけて`OnTouched()`を呼び出します。

```csharp
// シーンに空のGameObjectを作成しObjectTouchManagerコンポーネントを追加してください。
// 追加設定なしで自動的に動作します。
```

> `ObjectTouchManager`は`MonoSingleton<ObjectTouchManager>`を継承するため、
シーン内に重複インスタンスがあると自動的に削除されます。
