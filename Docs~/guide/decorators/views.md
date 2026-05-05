# Scene & Game View 오버레이

## SceneView HUD

`AchEngineSceneViewOverlay` 는 Unity 의 표준 `[Overlay(typeof(SceneView), …)]` 오버레이로 등록됩니다. SceneView 우상단의 오버레이 메뉴(`☰`) 에서 다른 기본 오버레이와 함께 켜고 끌 수 있고, **AchEngine HUD** 라는 이름으로 도킹·플로팅이 가능합니다.

### 표시 정보

| 라벨 | 내용 |
|---|---|
| **Scene** | 현재 활성 씬 이름 (`SceneManager.GetActiveScene().name`) |
| **Selection** | 선택된 GameObject 이름 + 컴포넌트 개수 |
| **Position** | 선택된 GameObject 의 world 좌표 (소수점 2자리) |
| **Stats** | 활성 씬의 루트 오브젝트 수, 현재 `Time.timeScale` |

- `Selection.selectionChanged` 와 `EditorApplication.hierarchyChanged` 에 구독하여 즉시 갱신되고, 추가로 250ms 간격 폴링으로 위치 변화도 감지합니다.
- `Preferences > AchEngine > Decorators > Scene HUD overlay` 토글이 꺼져 있으면 오버레이는 자동으로 숨겨집니다.

## GameView HUD

`AchEngineGameViewOverlay` 는 모든 GameView 의 `rootVisualElement` 좌상단에 작은 UI Toolkit 패널을 띄웁니다.

### 표시 정보

| 라벨 | 내용 |
|---|---|
| **fps** | `EditorApplication.update` 기반의 부드러운 FPS (Lerp 0.1) |
| **res** | 현재 GameView 창의 width × height |
| **ts**  | `Time.timeScale` |

- 패널은 `pickingMode = Ignore` 로 설정돼 게임 입력을 가로채지 않습니다.
- 0.5초 간격으로 throttle 된 tick 에서 GameView 인스턴스를 찾아 동기화하므로, 새 GameView 를 띄워도 자동으로 적용됩니다.
- 토글이 꺼지면 다음 tick 에서 즉시 모든 GameView 에서 제거됩니다.

## 토글

| 키 | 기본값 |
|---|---|
| Scene HUD overlay | ✅ |
| Game HUD overlay | ✅ |

## 구현 위치

- `Editor/Decorators/AchEngineSceneViewOverlay.cs`
- `Editor/Decorators/AchEngineGameViewOverlay.cs`
- USS 클래스: `.ach-scene-overlay`, `.ach-game-overlay` (`AchEngineInspectorTheme.uss`)
