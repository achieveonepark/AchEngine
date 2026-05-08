# RedDot 시스템

`RedDot`은 알림 뱃지(빨간 점)를 관리하는 정적 파사드입니다.
키 계층 구조(`/` 구분자)를 지원하여 자식 노드의 카운트가 부모로 자동 집계됩니다.

## API

```csharp
namespace AchEngine.UI
{
    public static class RedDot
    {
        // 특정 노드의 카운트 설정
        public static void Set(string key, int count);

        // 카운트에 delta 값 추가
        public static void Add(string key, int delta);

        // 집계된 카운트 조회 (자신 + 모든 자식 합산)
        public static int Get(string key);

        // 카운트가 0보다 크면 true
        public static bool HasDot(string key);

        // 카운트를 0으로 초기화
        public static void Clear(string key);

        // 카운트 변경 이벤트 구독
        public static void Subscribe(string key, Action<int> handler);

        // 구독 해제
        public static void Unsubscribe(string key, Action<int> handler);
    }
}
```

## 계층 구조

키에 `/`를 사용하면 자동으로 트리가 구성됩니다.
자식 노드의 합산 카운트가 부모에 자동으로 반영됩니다.

```
"Shop"          → "Shop/New" + "Shop/Sale" 의 합산
"Shop/New"      → 직접 설정한 카운트
"Shop/Sale"     → 직접 설정한 카운트
```

```csharp
RedDot.Set("Shop/New", 3);    // Shop/New = 3
RedDot.Set("Shop/Sale", 1);   // Shop/Sale = 1

RedDot.Get("Shop");           // → 4 (3 + 1 자동 집계)
RedDot.HasDot("Shop");        // → true
```

## 사용 예시

```csharp
// 새 아이템 입수
RedDot.Set("Shop/New", newItemCount);

// 퀘스트 완료
RedDot.Add("Quest/Daily", 1);

// 읽음 처리
RedDot.Clear("Quest/Daily");

// 메인 메뉴 버튼 — Shop 또는 Quest 중 하나라도 있으면 표시
bool showOnMainMenu = RedDot.HasDot("Shop") || RedDot.HasDot("Quest");
```

## RedDotBadge 컴포넌트

`RedDotBadge`는 UI GameObject에 붙이는 MonoBehaviour입니다.
지정한 키의 카운트를 자동으로 감지해 dot 오브젝트를 활성화·비활성화합니다.

| 필드 | 설명 |
|---|---|
| `Key` | 구독할 RedDot 키 (`"Shop"`, `"Quest/Daily"` 등) |
| `Dot` | 카운트 > 0 일 때 활성화할 GameObject |
| `Count Label` | (선택) 카운트를 표시할 `Text` 컴포넌트. 2 이상일 때만 표시 |

```
[Button GameObject]
 └── [RedDotBadge]  Key = "Shop"
      └── [DotImage]  (카운트 > 0이면 활성화)
           └── [Text]  (선택: "3" 등 숫자 표시)
```

`OnEnable` 시점에 자동으로 구독하고, `OnDisable` 시 해제합니다.
씬 전환이나 오브젝트 비활성화에도 안전합니다.

## 코드에서 직접 구독

컴포넌트 없이 직접 구독할 수도 있습니다.

```csharp
private void OnEnable()
{
    RedDot.Subscribe("Shop", OnShopChanged);
}

private void OnDisable()
{
    RedDot.Unsubscribe("Shop", OnShopChanged);
}

private void OnShopChanged(int count)
{
    _shopButton.SetDotVisible(count > 0);
}
```

## EnterPlayMode 지원

`RedDot`은 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`으로
도메인 리로드 없이 에디터 재생 시 자동 초기화됩니다.

## 관련 문서

- [UI 시스템 개요](/guide/ui/)
- [UIView & 수명 주기](/guide/ui/views)
