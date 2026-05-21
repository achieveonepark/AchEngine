# HttpLink

`HttpLink` 是一个用构建器模式封装 Unity `UnityWebRequest` 的 HTTP 客户端。
它能简洁地处理 GET/POST 请求、JSON 序列化、请求头设置和超时。

## GET 请求

```csharp
var result = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/users/1")
    .AddHeader("Authorization", "Bearer token123")
    .SetTimeout(10)
    .GetAsync<UserData>();

if (result != null)
    Debug.Log($"User: {result.Name}");
```

## POST 请求

```csharp
string json = JsonConvert.SerializeObject(new LoginRequest { UserId = "abc", Password = "pw" });

bool success = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/login")
    .SetJsonBody(json)
    .PostAsync();

Debug.Log(success ? "로그인 성공" : "로그인 실패");
```

## 直接发送 (`SendAsync`)

如需直接检查响应正文或状态,请使用 `Build().SendAsync()`。

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

| 方法 | 说明 |
|---|---|
| `SetUrl(string)` | 设置请求 URL |
| `SetMethod(string)` | HTTP 方法(仅支持 `GET` 或 `POST`,其他方法会抛出 `NotSupportedException`) |
| `SetTimeout(int)` | 超时(秒) |
| `SetJsonBody(string)` | 设置 JSON 请求体(Content-Type: application/json) |
| `AddHeader(string, string)` | 添加请求头 |
| `GetAsync<T>()` | 发送 GET 请求后进行 JSON 反序列化 |
| `PostAsync()` | 发送 POST 请求后返回是否成功 |
| `Build()` | 创建 `HttpLink` 实例 |

## HttpLink 属性

| 属性 | 说明 |
|---|---|
| `Success` | 请求是否成功 |
| `Result` | `UnityWebRequest.Result` 值 |
| `ReceiveDataString` | 响应正文(字符串) |
| `ReceiveData` | 响应字节数组 |
| `DownloadSize` | 已下载的字节数 |
| `DownloadProgress` | 下载进度 (0~1) |

> JSON 序列化/反序列化使用 `Newtonsoft.Json` 的 `JsonConvert`。
