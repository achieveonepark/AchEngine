# AchEngine — Claude 작업 규칙

## 절대 하지 말 것

### Rigidbody2D 사용 금지
- **어떤 컴포넌트에도 Rigidbody2D를 사용하지 않는다.**
- 이동·충돌·물리는 전부 `transform.position` 직접 조작 또는 자체 구현으로 처리한다.
- `[RequireComponent(typeof(Rigidbody2D))]` 금지.
- `GetComponent<Rigidbody2D>()` 금지.
- `Rigidbody2D.MovePosition()` 금지.
- AchMover가 이미 Rigidbody2D 없이 자체 충돌을 구현한 것이 그 이유다.

### RequireComponent 남용 금지
- 컴포넌트 간 의존을 강제하는 `[RequireComponent]`는 명시적으로 설계 의도가 있을 때만 붙인다.
- 단순히 "같이 쓰면 편하다"는 이유로 붙이지 않는다.

### 주석 언어
- 코드 주석(`///`, `//`)은 **한글**로 작성한다.
