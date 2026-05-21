# HttpLink

`HttpLink`は、Unityの`UnityWebRequest`をビルダーパターンでラップしたHTTPクライアントです。
GET/POSTリクエスト、JSONシリアライズ、ヘッダー設定、タイムアウトを簡潔に処理します。

## GET リクエスト

```csharp
var result = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/users/1")
    .AddHeader("Authorization", "Bearer token123")
    .SetTimeout(10)
    .GetAsync<UserData>();

if (result != null)
    Debug.Log($"User: {result.Name}");
```

## POST リクエスト

```csharp
string json = JsonConvert.SerializeObject(new LoginRequest { UserId = "abc", Password = "pw" });

bool success = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/login")
    .SetJsonBody(json)
    .PostAsync();

Debug.Log(success ? "ログイン成功" : "ログイン失敗");
```

## 直接送信 (`SendAsync`)

応答本文または状態を直接確認したい場合は`Build().SendAsync()`を使用します。

```csharp
var link = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/data")
    .SetMethod(UnityWebRequest.kHttpVerbGET)
    .Build()
    .SendAsync();

if (link.Success)
{
    Debug.Log(link.ReceiveDataString);
}
```

## Builder API

| メソッド | 説明 |
|---|---|
| `SetUrl(string)` | リクエストURLを設定 |
| `SetMethod(string)` | HTTPメソッド (`GET`または`POST`のみサポート、他は`NotSupportedException`) |
| `SetTimeout(int)` | タイムアウト（秒） |
| `SetJsonBody(string)` | JSONリクエストボディを設定 (Content-Type: application/json) |
| `AddHeader(string, string)` | リクエストヘッダーを追加 |
| `GetAsync<T>()` | GETリクエスト後にJSONを逆シリアライズ |
| `PostAsync()` | POSTリクエスト後に成功可否を返す |
| `Build()` | `HttpLink`インスタンスを生成 |

## HttpLink プロパティ

| プロパティ | 説明 |
|---|---|
| `Success` | リクエスト成功可否 |
| `Result` | `UnityWebRequest.Result`値 |
| `ReceiveDataString` | 応答本文（文字列） |
| `ReceiveData` | 応答バイト配列 |
| `DownloadSize` | ダウンロード済みバイト数 |
| `DownloadProgress` | ダウンロード進捗（0〜1） |

> JSONシリアライズ/逆シリアライズは`Newtonsoft.Json`の`JsonConvert`を使用します。
