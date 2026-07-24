---
name: httplink-webrequest
description: Use when the user asks to make an HTTP request, call a REST API, or communicate over the network in a project that has the AchEngine package installed. Use AchEngine's HttpLink instead of calling UnityEngine.Networking.UnityWebRequest directly.
---

# HttpLink 기반 네트워크 통신

`AchEngine` 루트 네임스페이스, `Runtime/WebRequest/HttpLink.cs`. `UnityEngine.Networking.UnityWebRequest`를 감싸며 Newtonsoft.Json으로 직렬화/역직렬화한다. `UnityWebRequest`를 직접 new/Send 하지 말고 이걸 사용한다.

## 사용법

`HttpLink`는 항상 중첩 클래스 **`HttpLink.Builder`**로 생성한다: `SetUrl(string)`, `SetMethod(string)`(`UnityWebRequest.kHttpVerbGET`/`kHttpVerbPOST` 사용), `SetTimeout(int seconds)`, `SetJsonBody(string json)`, `AddHeader(key, value)`, 그리고 `.Build()` → `HttpLink`.

단축 헬퍼:
- `Builder.GetAsync<T>()` — GET 전송 후 `JsonConvert.DeserializeObject<T>`로 역직렬화한 `Task<T>` 반환 (실패 시 `default`).
- `Builder.PostAsync()` — POST 전송(폼 인코딩 `UnityWebRequest.PostWwwForm`, JSON 바디가 있으면 raw upload handler + `Content-Type: application/json`) 후 `Task<bool>` 성공 여부 반환.

인스턴스 API: `SendAsync()` → `Task<HttpLink>`, `Result`(`UnityWebRequest.Result`), `Success`(bool), `ReceiveData`(byte[]), `ReceiveDataString`(string), `DownloadSize`(ulong), `DownloadProgress`(float).

`Builder.Build()`는 GET/POST만 지원하며 그 외 메서드는 `NotSupportedException`을 던진다.

## 예시

```csharp
var monster = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/monster/1")
    .SetTimeout(10)
    .GetAsync<MonsterDto>();

bool ok = await new HttpLink.Builder()
    .SetUrl("https://api.example.com/report")
    .SetJsonBody(JsonConvert.SerializeObject(payload))
    .PostAsync();
```

## 주의

`HttpLink`는 `AchTask`가 아니라 순수 `System.Threading.Tasks.Task`/`Task<T>`를 반환한다. GET/POST 외 메서드(PUT/DELETE 등)가 필요하면 `HttpLink`를 확장하거나 사용자에게 확인 후 진행한다.
