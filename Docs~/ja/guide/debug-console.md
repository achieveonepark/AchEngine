# AchDebugConsole

`AchDebugConsole`は、Android・iOSのネイティブUIレイヤーにレンダリングされるデバッグコンソールです。
Unityメインスレッドから完全に独立しているため、パフォーマンスプロファイリング中でも安全に使用できます。
エディター上ではIMGUIフォールバックとして動作します。

## 概要

`Application.logMessageReceivedThreaded`を通じて、Unityのすべてのログ(`Log`、`Warning`、`Error`、`Exception`、`Assert`)を自動的に受信します。
別途の初期化コードは不要で、`[RuntimeInitializeOnLoadMethod]`によって自動登録されるため、
ゲーム開始と同時にログ収集が始まります。

| 項目 | 内容 |
|---|---|
| 最大保管ログ数 | 500件(リングバッファ) |
| ログの色 | Error・Exception・Assert → 赤 / Warning → 黄 / Log → 白 |
| スレッドセーフ | すべてのプラットフォームでUIスレッドへのディスパッチ処理 |

## プラットフォーム別の動作

### Android

`WindowManager`を使用したフローティングオーバーレイウィンドウとして表示されます。

- Unityのレンダリング上に独立して描画され、ゲームループに影響を与えません。
- ツールバーをドラッグしてウィンドウ位置を自由に移動できます。
- 画面の95%幅、50%高さで上部に表示されます。
- **`SYSTEM_ALERT_WINDOW`権限が必要です。**([権限セクション](#android-権限)を参照)

### iOS

`UIWindowLevelAlert + 100`レベルの別途`UIWindow`として表示されます。

- Unityのキーウィンドウから独立して動作し、Unityのレンダリングに影響を与えません。
- ドラッグジェスチャーでウィンドウ位置を移動できます。
- 画面の95%幅、45%高さで表示されます。
- UITableViewベースでログを表示するため、大量のログでもスムーズにスクロールします。
- 追加の権限は必要ありません。

### Editor

Unityエディターにはすでにコンソールウィンドウがあるため、`Show()`/`Hide()`は内部フラグのみをトグルします。
ゲームビューにオーバーレイを描画したい場合は、[エディターフォールバック](#エディターフォールバック)セクションを参照してください。

## API

```csharp
namespace AchEngine
{
    public static class AchDebugConsole
    {
        // 콘솔이 현재 표시 중인지 여부
        public static bool IsVisible { get; }

        // 콘솔 표시
        public static void Show();

        // 콘솔 숨기기
        public static void Hide();

        // 표시/숨김 토글
        public static void Toggle();

        // 로그 항목 전체 삭제
        public static void Clear();
    }
}
```

## クイックスタート

4本指タップでコンソールを開閉する例です。

```csharp
using UnityEngine;
using AchEngine;

public class DebugConsoleTrigger : MonoBehaviour
{
    private void Update()
    {
        // 4손가락 탭 감지
        if (Input.touchCount == 4)
        {
            bool allBegan = true;
            for (int i = 0; i < 4; i++)
            {
                if (Input.GetTouch(i).phase != TouchPhase.Began)
                {
                    allBegan = false;
                    break;
                }
            }

            if (allBegan)
                AchDebugConsole.Toggle();
        }
    }
}
```

シーンに空のGameObjectを作成し、このコンポーネントをアタッチしてください。
キーボードショートカットを使いたい場合は、`Input.GetKeyDown(KeyCode.BackQuote)`などに置き換えてください。

## エディターフォールバック

エディターのゲームビューにオーバーレイを表示するには、`DrawEditorGUI()`を`OnGUI()`から呼び出すMonoBehaviourが必要です。

```csharp
using UnityEngine;
using AchEngine;

public class AchDebugConsoleEditorOverlay : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnGUI()
    {
        AchDebugConsole.DrawEditorGUI();
    }
#endif
}
```

このコンポーネントをシーン内の任意のGameObjectにアタッチしておけば、エディター再生中にゲームビューのオーバーレイとしてコンソールを確認できます。

:::warning Android権限
Androidでオーバーレイウィンドウを表示するには、`SYSTEM_ALERT_WINDOW`権限が必要です。
`AchDebugConsoleManifest.xml`がパッケージに含まれているため、`AndroidManifest.xml`を別途編集しなくても自動的に権限が宣言されます。

ただし、Android 6.0(API 23)以上では、ユーザーへの**ランタイム権限リクエスト**が必要です。
`Show()`を呼び出す前に、以下のように権限を確認・リクエストしてください。

```csharp
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;

if (!Permission.HasUserAuthorizedPermission("android.permission.SYSTEM_ALERT_WINDOW"))
{
    Permission.RequestUserPermission("android.permission.SYSTEM_ALERT_WINDOW");
    // 권한 승인 후 Show() 호출
}
else
{
    AchDebugConsole.Show();
}
#endif
```
:::

:::info パフォーマンス
AndroidおよびiOSのどちらでも、ネイティブUIスレッド上でレンダリングされます。
Unityメインスレッドやレンダースレッドへの追加負荷がないため、
パフォーマンスプロファイリング中にコンソールを開いたままにしても問題ありません。
:::

## 関連ドキュメント

- [UIシステム概要](/ja/guide/ui/)
