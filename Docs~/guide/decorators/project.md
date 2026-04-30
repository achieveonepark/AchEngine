# Project 데코레이터

`AchEngineProjectDecorator` 는 Project 창의 **단일 컬럼(One Column) 리스트 뷰**에 보조 정보를 그립니다. 아이콘 그리드 뷰에서는 자동으로 비활성화됩니다.

## 기능

### 줄무늬 (Row stripes)
Hierarchy 와 동일하게 옅은 alternating background 를 깔아서 긴 폴더를 따라 시선을 정렬해 줍니다.

### 파일 사이즈 뱃지
폴더가 아닌 모든 에셋의 행 우측에 디스크 파일 크기를 작은 검정 pill 로 표시합니다. (`B / KB / MB / GB`)

### 폴더 항목 수
폴더 행에는 즉시 안의 항목 개수가 파란 pill 로 표시됩니다.

- `.meta` 파일은 카운트에서 제외됩니다.
- 폴더 카운트는 5초 캐시되므로, 폴더가 많아도 매 프레임 디스크를 다시 스캔하지 않습니다.
- 데코레이터 설정이 바뀌면 캐시는 즉시 비워집니다.

## 토글

| 키 | 기본값 |
|---|---|
| Enabled | ✅ |
| Row stripes | ✅ |
| File size badge | ✅ |
| Folder item count | ✅ |

## 구현 위치

`Editor/Decorators/AchEngineProjectDecorator.cs`

훅: `EditorApplication.projectWindowItemOnGUI(string guid, Rect rect)`. 행 높이를 검사하여 단일 컬럼 리스트 뷰일 때만 그립니다.
