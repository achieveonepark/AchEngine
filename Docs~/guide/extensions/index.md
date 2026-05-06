# 확장 메서드 (Extensions)

`AchEngine.Extensions` 어셈블리에는 Unity 개발에서 자주 쓰이는 60개 이상의 확장 메서드가 포함되어 있습니다.

## 카테고리

### 컬렉션
| 클래스 | 주요 기능 |
|---|---|
| `ArrayExt` | 배열 유틸리티 |
| `ListExt` | 리스트 유틸리티 |
| `DictionaryExt` | 딕셔너리 헬퍼 |
| `IEnumerableExt` / `IListExt` | LINQ 보조 |
| `LinqE` | 고급 LINQ 확장 |

### Unity 타입
| 클래스 | 주요 기능 |
|---|---|
| `GameObjectExt` | `GetOrAddComponent`, `SetActiveIfNotNull` |
| `Vector2Ext` / `Vector3Ext` | 벡터 연산 |
| `ComponentExt` | 컴포넌트 유틸리티 |
| `RectExt` | Rect 연산 |

### UI
| 클래스 | 주요 기능 |
|---|---|
| `RectTransformExt` | 레이아웃 조작 |
| `ImageExt` / `RawImageExt` | 이미지 헬퍼 |
| `GraphicExt` | Graphic 유틸리티 |
| `ButtonClickedEventExt` | 버튼 이벤트 헬퍼 |

### 기본 타입
| 클래스 | 주요 기능 |
|---|---|
| `StringExt` / `StringParseExt` | 문자열 처리 |
| `IntExt` / `FloatExt` / `BoolExt` | 숫자·불리언 확장 |
| `EnumExt` | 열거형 유틸리티 |
| `ColorExt` | 색상 변환 |

### 유틸리티 클래스
| 클래스 | 설명 |
|---|---|
| `Selectable<T>` / `SelectableList<T>` | 변경 감지 가능한 값/리스트 |
| `MultiDictionary<K,V>` | 한 키에 여러 값을 저장하는 딕셔너리 |
| `StringAppender` | StringBuilder 래퍼 |
| `RandomUtils` | 편의 랜덤 유틸리티 |
| `CameraUtils` | 카메라 좌표 변환 |

## 사용 예

```csharp
// GameObjectExt
var rb = gameObject.GetOrAddComponent<Rigidbody>();
gameObject.SetActiveIfNotNull(false);

// RectTransformExt
rectTransform.SetLeft(10f);

// StringExt
string result = "Hello".Repeat(3); // "HelloHelloHello"

// Selectable<T>
var hp = new Selectable<int>(100);
hp.OnValueChanged += (prev, next) => UpdateHpBar(next);
hp.Value = 80;
```
