# AchEngine 소개

AchEngine는 Unity 개발에서 자주 쓰는 기능들을 하나의 UPM 패키지로 묶은 **통합 개발 툴킷**입니다.

각 모듈은 독립적으로 사용할 수 있고, VContainer, MemoryPack, Addressables 같은 선택 패키지가 없어도 코어 기능은 계속 동작합니다.

## 모듈 구성

| 모듈 | 설명 | 선택 패키지 |
|---|---|---|
| **DI** | VContainer 래퍼, ServiceLocator | `jp.hadashikick.vcontainer` |
| **UI System** | 레이어 기반 View 관리, 풀링, 전환 | - |
| **Table Loader** | Google Sheets에서 C# 데이터 파이프라인 생성 | `com.cysharp.memorypack` |
| **Addressables** | 에셋 캐싱, 자동 그룹 관리, 원격 배포 | `com.unity.addressables` |
| **Localization** | JSON 로컬라이제이션, 키 코드 생성 | `com.unity.textmeshpro` (선택) |

## 패키지 정보

- **Package ID:** `com.engine.achieve`
- **버전:** 1.0.1
- **최소 Unity 버전:** 6000.3
- **필수 의존성:** `com.unity.ugui`

## 선택 패키지

아래 패키지를 설치하면 고급 AchEngine 기능이 자동으로 활성화됩니다.
설치하지 않아도 컴파일 에러는 나지 않고, 해당 기능만 비활성화됩니다.

```
jp.hadashikick.vcontainer   -> DI 컨테이너 활성화        (#ACHENGINE_VCONTAINER)
com.cysharp.memorypack      -> 바이너리 직렬화 활성화    (#ACHENGINE_MEMORYPACK)
com.unity.addressables      -> Addressables 모듈 활성화   (#ACHENGINE_ADDRESSABLES)
com.unity.textmeshpro       -> TMP Localization 활성화    (#ACHENGINE_LOCALIZATION_TMP)
com.unity.entities          -> ECS 래퍼 활성화             (#ACHENGINE_ENTITIES)
```

:::tip
선택 패키지 설치 여부는 **Project Settings > AchEngine** Overview에서 한눈에 확인할 수 있고, 버튼으로 바로 설치할 수 있습니다.
:::
