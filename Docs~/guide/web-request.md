# HttpLink

`HttpLink`는 Unity의 `UnityWebRequest`를 빌더 패턴으로 감싼 HTTP 클라이언트입니다.
GET/POST 요청, JSON 직렬화, 헤더 설정, 타임아웃을 간결하게 처리합니다.

## GET 요청

```csharp
var result = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/users/1")
    .AddHeader("Authorization", "Bearer token123")
    .SetTimeout(10)
    .GetAsync<UserData>();

if (result != null)
    Debug.Log($"User: {result.Name}");
```

## POST 요청

```csharp
string json = JsonConvert.SerializeObject(new LoginRequest { UserId = "abc", Password = "pw" });

bool success = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/login")
    .SetJsonBody(json)
    .PostAsync();

Debug.Log(success ? "로그인 성공" : "로그인 실패");
```

## 직접 전송 (`SendAsync`)

응답 본문 또는 상태를 직접 확인하려면 `Build().SendAsync()`를 사용하세요.

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

| 메서드 | 설명 |
|---|---|
| `SetUrl(string)` | 요청 URL 설정 |
| `SetMethod(string)` | HTTP 메서드 (`GET`, `POST` 등) |
| `SetTimeout(int)` | 타임아웃 (초) |
| `SetJsonBody(string)` | JSON 요청 바디 설정 (Content-Type: application/json) |
| `AddHeader(string, string)` | 요청 헤더 추가 |
| `GetAsync<T>()` | GET 요청 후 JSON 역직렬화 |
| `PostAsync()` | POST 요청 후 성공 여부 반환 |
| `Build()` | `HttpLink` 인스턴스 생성 |

## HttpLink 프로퍼티

| 프로퍼티 | 설명 |
|---|---|
| `Success` | 요청 성공 여부 |
| `Result` | `UnityWebRequest.Result` 값 |
| `ReceiveDataString` | 응답 본문 (문자열) |
| `ReceiveData` | 응답 바이트 배열 |
| `DownloadSize` | 다운로드된 바이트 수 |
| `DownloadProgress` | 다운로드 진행률 (0~1) |

> JSON 직렬화/역직렬화는 `Newtonsoft.Json`의 `JsonConvert`를 사용합니다.
