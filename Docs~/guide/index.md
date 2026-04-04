# AchEngine이란?

AchEngine은 Unity 게임 개발에 자주 쓰이는 기능들을 하나의 UPM 패키지로 통합한 **통합 개발 툴킷**입니다.

각 모듈은 독립적으로 사용할 수 있으며, 선택적 패키지(VContainer, MemoryPack, Addressables 등)가 없어도 Core 기능은 항상 동작합니다.

## 모듈 구성

| 모듈 | 설명 | 선택 패키지 |
|---|---|---|
| **DI** | VContainer 래퍼, ServiceLocator | `jp.hadashikick.vcontainer` |
| **UI System** | 레이어 기반 View 관리, Pool, 트랜지션 | - |
| **Table Loader** | Google Sheets → C# 데이터 파이프라인 | `com.cysharp.memorypack` |
| **Addressables** | 에셋 캐싱, 그룹 자동 관리, 원격 배포 | `com.unity.addressables` |
| **Localization** | JSON 다국어, 키 코드 생성 | `com.unity.textmeshpro` (선택) |

## 패키지 정보

- **Package ID:** `com.engine.achieve`
- **버전:** 1.0.0
- **최소 Unity 버전:** 2021.3
- **필수 의존성:** `com.unity.ugui`

## 선택적 의존성

AchEngine의 고급 기능은 아래 패키지를 설치하면 자동으로 활성화됩니다.
각 패키지가 없어도 컴파일 에러는 발생하지 않으며, 해당 기능만 비활성화됩니다.

```
jp.hadashikick.vcontainer   → DI 컨테이너 활성화 (#ACHENGINE_VCONTAINER)
com.cysharp.memorypack      → 이진 직렬화 활성화  (#ACHENGINE_MEMORYPACK)
com.unity.addressables      → Addressables 모듈  (#ACHENGINE_ADDRESSABLES)
com.unity.textmeshpro       → TMP Localization   (#ACHENGINE_LOCALIZATION_TMP)
```

:::tip
선택적 패키지 설치 여부는 **Project Settings › AchEngine** Overview 화면에서 한눈에 확인하고, 버튼 한 번으로 설치할 수 있습니다.
:::
