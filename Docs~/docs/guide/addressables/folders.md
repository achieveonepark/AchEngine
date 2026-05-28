# 감시 폴더 & 그룹

감시 폴더를 등록하면 해당 폴더의 에셋이 지정한 Addressables 그룹에 자동으로 추가됩니다.

**Project Settings › AchEngine › Addressables › 감시 폴더** 섹션에서
**+ 폴더 추가** 버튼을 눌러 항목을 추가합니다.

## 감시 폴더 항목 구성

| 항목 | 설명 |
|---|---|
| **폴더 경로** | `Assets/` 로 시작하는 상대 경로 (예: `Assets/Art/Icons`) |
| **그룹 이름** | Addressables 그룹 이름 (없으면 자동 생성) |
| **주소 생성 방식** | 파일명 / 전체 경로 / GUID 중 선택 |
| **하위 폴더 포함** | 재귀적으로 하위 폴더 스캔 여부 |
| **라벨** | 쉼표로 구분된 Addressables 라벨 목록 |

## 주소 생성 방식 (AddressNamingMode)

| 값 | 생성되는 주소 예시 |
|---|---|
| `FileName` | `icon_sword` |
| `FullPath` | `Assets/Art/Icons/icon_sword.png` |
| `GUID` | `a1b2c3d4e5f6...` |

:::tip 자동 스캔
에셋을 추가하거나 삭제하면 Unity의 AssetPostprocessor가 자동으로 스캔을 트리거합니다.
:::
