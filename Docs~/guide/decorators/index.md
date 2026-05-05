# 에디터 데코레이터

AchEngine는 Unity 기본 에디터 창인 `Hierarchy`, `Project`, `Scene`, `Game` 위에 가벼우면서도 실용적인 장식 레이어를 더해, 평소 작업 흐름을 더 빠르고 보기 좋게 만듭니다.

목표는 Unity를 완전히 다른 에디터로 바꾸는 것이 아닙니다. 매일 보는 기본 에디터를 더 정돈되고, 더 빨리 스캔되고, 더 도움 되는 방향으로 다듬는 것입니다.

:::warning 요구 사항
데코레이터는 최신 UI Toolkit API를 사용하므로 **Unity 6000.3 이상**에서만 활성화됩니다. 더 낮은 버전에서는 `#if`로 제외되고 Unity 기본 UI가 그대로 사용됩니다.
:::

## 한눈에 보기

| 영역 | 핵심 기능 |
|---|---|
| [Hierarchy](./hierarchy) | 줄무늬, 섹션 헤더, 컴포넌트 아이콘, 활성 토글, Tag / Layer / Static 배지 |
| [Project](./project) | 줄무늬, 파일 크기 배지, 폴더 항목 수 |
| [Scene & Game](./views) | Scene HUD 오버레이, Game 뷰 FPS / 해상도 / time scale 오버레이 |

## Preferences

`Edit > Preferences > AchEngine > Decorators`에서 관리합니다.

- 각 영역마다 마스터 토글이 있습니다.
- 대부분의 영역은 세부 토글도 제공합니다.
- 값을 바꾸면 관련 뷰가 즉시 다시 그려집니다.
- 값은 `EditorPrefs`에 저장되므로 사용자 / 머신 단위로 유지됩니다.

## 동작 방식

| 데코레이터 | 연결 지점 |
|---|---|
| Hierarchy | `EditorApplication.hierarchyWindowItemOnGUI` |
| Project | `EditorApplication.projectWindowItemOnGUI` |
| SceneView | `[Overlay(typeof(SceneView), ...)]` |
| GameView | `GameView.rootVisualElement`에 UI Toolkit 오버레이 주입 |

모든 데코레이터는 `Editor/Decorators/` 아래에 있으며 `[InitializeOnLoad]`로 자동 등록됩니다.
