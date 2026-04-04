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

AchEngine 설치 후 **Project Settings › AchEngine** 을 열면 Overview 탭에서
각 모듈의 설치 상태를 확인하고 바로 설치 버튼을 누를 수 있습니다.

| 패키지 | 설치 방법 |
|---|---|
| `com.unity.addressables` | Overview에서 **설치** 버튼 클릭 (Package Manager 자동 추가) |
| `jp.hadashikick.vcontainer` | Overview에서 **GitHub** 버튼 → GitHub 페이지에서 설치 |
| `com.cysharp.memorypack` | Overview에서 **GitHub** 버튼 → GitHub 페이지에서 설치 |

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

## 설치 확인

설치가 완료되면 Unity 콘솔에 에러가 없어야 하며,
메뉴에 **Tools › AchEngine** 항목이 보여야 합니다.
