# AchEngine

Unity development package for DI, UI, tables, Addressables, Localization, and ECS helpers.

## Documentation

- [한국어](https://achieveonepark.github.io/AchEngine/)
- [English](https://achieveonepark.github.io/AchEngine/en/)
- [日本語](https://achieveonepark.github.io/AchEngine/ja/)
- [中文](https://achieveonepark.github.io/AchEngine/zh/)

## Changelog

- [English](CHANGELOG.md)
- [한국어](CHANGELOG.ko.md)

## 기존 Unity 게임에 AchEngine 적용하기

아래 프롬프트를 Unity 프로젝트를 다룰 수 있는 코딩 에이전트에게 그대로 입력하세요. 프로젝트의 기존 구조를 먼저 분석하고, 필요한 AchEngine 기능만 단계적으로 적용하도록 작성되어 있습니다.

```text
이 Unity 게임 프로젝트에 AchEngine을 안전하게 도입해줘.

목표:
- 현재 게임의 공개 API, 씬, 프리팹, 직렬화 데이터, 저장 데이터와 플레이 감각을 최대한 보존한다.
- 기존에 정상 동작하는 시스템을 무조건 교체하지 말고, AchEngine이 실질적인 이점을 주는 부분만 적용한다.
- 한 번에 전면 마이그레이션하지 말고 컴파일 가능한 작은 단계로 나눠 진행한다.

진행 규칙:
1. 먼저 코드를 수정하지 말고 프로젝트 구조, Unity 버전, 렌더 파이프라인, 입력 백엔드, 사용 중인 패키지, asmdef, 부트스트랩 씬, 서비스/매니저, UI, 저장, 테이블, 로컬라이제이션, Addressables 사용 여부를 조사한다.
2. 현재 Git 변경 사항을 확인하고 사용자의 기존 변경은 보존한다. 생성물이나 캐시 폴더는 직접 수정하지 않는다.
3. 조사 결과를 바탕으로 다음 형식의 대응표를 먼저 제시한다.
   - 기존 시스템
   - 대응 가능한 AchEngine 모듈
   - 도입 이점
   - 호환성 위험
   - 적용/유지/보류 권장안
4. VContainer DI 또는 ECS/DOTS 도입은 아키텍처에 큰 영향을 주므로, 현재 프로젝트가 이미 사용 중이지 않다면 적용 전에 반드시 나에게 물어본다.
5. AchEngine 내부 정책상 Rigidbody2D를 추가하거나 사용하지 않는다. 이동과 충돌은 transform 기반 또는 AchEngine의 자체 구현을 사용한다.
6. 기존 Input Manager, Input System 또는 Both 중 현재 설정을 감지하고 그 설정과 호환되게 연결한다. 입력 방식을 임의로 바꾸지 않는다.
7. 기존 MonoBehaviour와 외부 코드가 참조하는 public/protected 멤버, SerializeField 이름, 컴포넌트 타입은 가능한 한 유지한다. 변경이 필요하면 어댑터, 래퍼, FormerlySerializedAs 또는 단계적 폐기를 우선 검토한다.
8. 씬·프리팹·ScriptableObject를 변경하기 전 참조 관계와 중복 초기화 가능성을 확인한다. 영속 오브젝트, EventSystem, AudioListener, UI Root를 중복 생성하지 않는다.
9. 저장 시스템을 변경할 때는 기존 파일을 덮어쓰기 전에 버전과 마이그레이션 경로를 마련한다. 실제 저장 데이터 삭제는 내 명시적 승인 없이는 하지 않는다.
10. Addressables를 적용할 때 주소 중복, 핸들 해제, 씬 언로드, 원격 카탈로그 실패 경로를 검증한다. 기존 Resources 참조는 한 번에 제거하지 않는다.
11. Table과 Localization을 적용할 때 스키마, 고유 int Id, locale 코드, 누락 키와 잘못된 JSON/CSV를 검증하고 오류가 있는 데이터를 성공으로 베이크하지 않는다.
12. 코드 주석은 한글로 작성한다. 기존 코드 스타일과 네임스페이스, asmdef 경계를 따른다.

실행 순서:
1. 읽기 전용 분석과 마이그레이션 계획 제시
2. 가장 위험이 낮은 모듈 하나를 선택해 최소 변경으로 적용
3. Unity 컴파일과 관련 EditMode/PlayMode 테스트 실행
4. 오류와 경고를 수정하고 변경 내용을 요약
5. 다음 모듈로 반복

각 단계의 완료 조건:
- Unity 콘솔 컴파일 오류 0개
- 새 obsolete 경고와 중복 메뉴/컴포넌트 없음
- 관련 테스트 통과
- 씬과 프리팹 참조 유지
- 적용 전후의 사용자 인터페이스와 플레이 동작이 의도치 않게 변하지 않음

최종 보고에는 적용한 AchEngine 모듈, 유지한 기존 시스템, 호환 어댑터, 설정 방법, 검증 결과, 남은 위험과 되돌리는 방법을 포함해줘.
```
