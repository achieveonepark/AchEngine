# HttpLink

`HttpLink` is a builder-pattern HTTP client wrapping Unity's `UnityWebRequest`.
It handles GET/POST requests, JSON deserialization, custom headers, and timeouts concisely.

## GET Request

```csharp
var result = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/users/1")
    .AddHeader("Authorization", "Bearer token123")
    .SetTimeout(10)
    .GetAsync<UserData>();

if (result != null)
    Debug.Log($"User: {result.Name}");
```

## POST Request

```csharp
string json = JsonConvert.SerializeObject(new LoginRequest { UserId = "abc", Password = "pw" });

bool success = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/login")
    .SetJsonBody(json)
    .PostAsync();

Debug.Log(success ? "Login succeeded" : "Login failed");
```

## Raw Send (`SendAsync`)

Use `Build().SendAsync()` to inspect the response directly.

```csharp
var link = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/data")
    .SetMethod(UnityWebRequest.kHttpVerbGET)
    .Build()
    .SendAsync();

if (link.Success)
    Debug.Log(link.ReceiveDataString);
```

## Builder API

| Method | Description |
|---|---|
| `SetUrl(string)` | Request URL |
| `SetMethod(string)` | HTTP method (`GET`, `POST`, etc.) |
| `SetTimeout(int)` | Timeout in seconds |
| `SetJsonBody(string)` | JSON request body (sets Content-Type: application/json) |
| `AddHeader(string, string)` | Add a request header |
| `GetAsync<T>()` | Send GET and deserialize JSON response |
| `PostAsync()` | Send POST and return success flag |
| `Build()` | Create an `HttpLink` instance |

## HttpLink Properties

| Property | Description |
|---|---|
| `Success` | Whether the request succeeded |
| `Result` | `UnityWebRequest.Result` value |
| `ReceiveDataString` | Response body as string |
| `ReceiveData` | Response body as byte array |
| `DownloadSize` | Bytes downloaded |
| `DownloadProgress` | Download progress (0–1) |

> JSON serialization and deserialization use `JsonConvert` from `Newtonsoft.Json`.
