# UIView & 수명 주기

`UIView`는 AchEngine UI System에서 모든 화면의 기본 클래스입니다.

## 수명 주기

```
Pool에서 꺼냄 / 첫 생성
       ↓
OnInitialize()   ← 최초 생성 시 단 1회
       ↓
    Show() 호출
       ↓
OnBeforeOpen()   ← 트랜지션 시작 전
       ↓
  [트랜지션 재생]
       ↓
OnOpened()       ← 트랜지션 완료 후 (완전히 표시됨)
       ↓
   Close() 호출
       ↓
OnBeforeClose()  ← 트랜지션 시작 전
       ↓
  [트랜지션 재생]
       ↓
OnClosed()       ← 트랜지션 완료 후 → Pool 반환
```

## UIView 구현

```csharp
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailView : UIView
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _descText;
    [SerializeField] private Button _closeButton;

    private ItemData _item;

    // 최초 1회 초기화
    protected override void OnInitialize()
    {
        _closeButton.onClick.AddListener(Close);
    }

    // 외부에서 데이터 주입
    public void SetItem(ItemData item)
    {
        _item = item;
    }

    // 화면이 열릴 때마다 호출
    protected override void OnOpened()
    {
        _nameText.text = _item.Name;
        _descText.text = _item.Description;
    }

    // 화면이 닫힌 후 호출 (데이터 초기화 권장)
    protected override void OnClosed()
    {
        _item = null;
    }
}
```

## View 등록 (UIViewCatalog)

1. `UIViewCatalog` ScriptableObject를 생성합니다.
   - **Create › AchEngine › UI View Catalog**
2. 프리팹을 catalog에 등록합니다.
3. `UIRoot`의 **Catalog** 필드에 연결합니다.

## View 열기

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// 타입으로 열기
ui.Show<ItemDetailView>();

// 타입 + 초기화 콜백
ui.Show<ItemDetailView>(view => view.SetItem(item));

// 문자열 ID로 열기 (Catalog에 등록된 이름)
ui.Show("ItemDetail");
```

## 단일 인스턴스 모드

같은 View를 여러 번 열어도 하나만 유지하려면 `SingleInstance` 플래그를 사용합니다.

```csharp
public class LoadingView : UIView
{
    public override bool SingleInstance => true;
}
```

## Close 버튼 자동 연결

`UICloseButton` 컴포넌트를 버튼에 추가하면 가장 가까운 부모 `UIView`를 자동으로 닫습니다.

```
[ItemDetailView]
  └── [CloseButton]  ← UICloseButton 컴포넌트 추가
```
