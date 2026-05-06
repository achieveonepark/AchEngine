# Hierarchy 데코레이터

`AchEngineHierarchyDecorator` 는 Unity Hierarchy 창의 각 행 위에 보조 정보를 그립니다. 모든 항목은 `Preferences > AchEngine > Decorators > Hierarchy` 에서 토글할 수 있습니다.

## 기능

### 줄무늬 (Row stripes)
짝수/홀수 행에 매우 옅은 배경을 깔아서 시선을 정렬합니다. 길이가 긴 트리에서 동일한 깊이의 항목을 따라가기가 쉬워집니다.

### 섹션 헤더 (Section headers)
이름이 다음 중 하나로 시작하는 GameObject 는 풀폭(Full‑width) 헤더로 그려집니다.

```
--- Lighting
### UI Layer
>>> Spawners
```

- 접두사와 공백은 자동으로 잘라낸 뒤 대문자로 표시합니다.
- 헤더 행에는 컴포넌트 아이콘·뱃지·토글이 그려지지 않아 시각적으로 깔끔하게 분리됩니다.

### 컴포넌트 아이콘
GameObject 의 주요 컴포넌트(최대 5개)를 우측에 미니 아이콘으로 표시합니다.

- `Transform` 은 항상 제외됩니다.
- 아이콘을 좌클릭하면 해당 컴포넌트를 Ping 합니다.

### 활성 토글 (Active toggle)
행의 가장 오른쪽에 작은 체크박스가 추가되어 `GameObject.activeSelf` 를 한 번에 토글할 수 있습니다. Undo 가 등록되므로 `Ctrl+Z` 로 되돌릴 수 있습니다.

### Tag · Layer · Static 뱃지
- **Tag**: `Untagged` 가 아닌 경우에 초록색 pill 로 표시
- **Layer**: `Default(0)` 가 아닌 경우에 파란색 pill 로 표시
- **Static**: `isStatic == true` 일 때 황색 `S` 뱃지

## 토글

| 키 | 기본값 |
|---|---|
| Enabled | ✅ |
| Row stripes | ✅ |
| Section header rows  (`---`, `###`, `>>>`) | ✅ |
| Component icons | ✅ |
| Active toggle | ✅ |
| Tag / Layer / Static badges | ✅ |

## 구현 위치

`Editor/Decorators/AchEngineHierarchyDecorator.cs`

`EditorApplication.hierarchyWindowItemOnGUI` 콜백에서 IMGUI 로 직접 그리며, 행 단위로 한 프레임에 한 번만 호출되므로 오버헤드가 거의 없습니다. 컴포넌트 목록은 호출마다 재사용 버퍼(`List<Component>`)로 GC 를 피합니다.
