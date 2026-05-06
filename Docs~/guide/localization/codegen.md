# 키 상수 코드 생성

문자열 키를 하드코딩하면 오타가 발생하기 쉽습니다.
AchEngine은 JSON 키를 **타입-세이프 중첩 클래스**로 변환하는 코드 생성기를 제공합니다.

## 변환 예시

JSON 키:
```json
{
  "menu.start": "게임 시작",
  "menu.settings": "설정",
  "dialog.confirm": "확인",
  "item.sword.name": "철 검"
}
```

생성된 C# 클래스:
```csharp
// 자동 생성 — 직접 수정하지 마세요
public static class L
{
    public static class Menu
    {
        public const string Start    = "menu.start";
        public const string Settings = "menu.settings";
    }

    public static class Dialog
    {
        public const string Confirm = "dialog.confirm";
    }

    public static class Item
    {
        public static class Sword
        {
            public const string Name = "item.sword.name";
        }
    }
}
```

## 코드 생성 설정

**Project Settings › AchEngine › Localization › 키 상수 코드 생성** 에서:

| 항목 | 기본값 |
|---|---|
| **클래스 이름** | `L` |
| **네임스페이스** | (비어있으면 전역 네임스페이스) |
| **출력 경로** | `Assets/Generated/` |

**키 상수 생성** 버튼을 클릭하면 `{출력경로}/{클래스명}.cs` 파일이 생성됩니다.

## LocalizedString 컴포넌트

Inspector에서 키를 지정할 때는 `LocalizedString` 타입을 사용합니다.

```csharp
public class ItemNameDisplay : MonoBehaviour
{
    [SerializeField] private LocalizedString _nameKey;

    private void Start()
    {
        GetComponent<Text>().text = _nameKey.Value;
    }
}
```

Inspector에서 `_nameKey` 필드에 키를 입력하면 커스텀 PropertyDrawer가
현재 로케일의 번역 미리보기를 보여줍니다.
