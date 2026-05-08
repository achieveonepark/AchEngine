# 설치

## UPM — Git URL로 설치 (권장)

Unity 에디터에서 **Window › Package Manager** 를 열고,
**`+` 버튼 → Add package from git URL...** 을 선택한 뒤 아래 URL을 입력하세요.

```
https://github.com/achieveonepark/AchEngine.git
```

특정 버전을 고정하려면 태그를 붙입니다.

```
https://github.com/achieveonepark/AchEngine.git#1.0.0
```

## manifest.json으로 설치

프로젝트의 `Packages/manifest.json` 을 직접 수정하는 방법입니다.

```json
{
  "dependencies": {
    "com.engine.achieve": "https://github.com/achieveonepark/AchEngine.git",
    ...
  }
}
```

## 선택적 패키지 설치

AchEngine은 아래 패키지를 선택적으로 지원합니다. 패키지를 설치하면 해당 기능이 자동으로 활성화됩니다.
**Window › AchEngine › AchEngine Info** 창에서 각 패키지의 설치 여부와 활성화 기능을 확인할 수 있습니다.

| 패키지 | Package ID | 활성화 기능 |
|---|---|---|
| VContainer | `jp.hadashikick.vcontainer` | DI 컨테이너 (AchEngineScope, ServiceLocator) |
| MemoryPack | `com.cysharp.memorypack` | QuickSave 직렬화 (`USE_QUICK_SAVE`) |
| Addressables | `com.unity.addressables` | AddressableManager, RemoteContentManager |
| R3 | `com.cysharp.r3` | UIBindingManager (Reactive pub/sub) |

### VContainer 수동 설치

`Packages/manifest.json` 의 `scopedRegistries` 에 아래를 추가하세요.

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": ["jp.hadashikick.vcontainer"]
    }
  ],
  "dependencies": {
    "jp.hadashikick.vcontainer": "1.15.0"
  }
}
```

### MemoryPack 수동 설치

[MemoryPack GitHub](https://github.com/Cysharp/MemoryPack) 의 설치 가이드를 참고하세요.

### R3 설치

[R3 GitHub](https://github.com/Cysharp/R3) 의 설치 가이드를 참고하세요.

## 설치 확인

설치가 완료되면 Unity 콘솔에 에러가 없어야 하며,
메뉴에 **Window › AchEngine › AchEngine Info** 항목이 보여야 합니다.
패키지 설치 상태는 이 창에서 한눈에 확인할 수 있습니다.
