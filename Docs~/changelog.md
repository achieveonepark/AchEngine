# 변경 내역

## 1.0.3

- `SaveManager`, `ISaveService`, `LocalSaveService`를 추가했습니다. 저장 로직을 `PlayerManager`에서 분리한 추상화 레이어로, 동기/비동기 API를 모두 제공하며 향후 Firestore, AWS 등 클라우드 백엔드로 교체할 수 있도록 설계했습니다.
- `PlayerManager`에서 저장·불러오기 로직을 제거했습니다. 이제 타입별 데이터 컨테이너 관리(`Add`, `Get`, `Remove`)만 담당합니다.
- `AchProjectile`을 추가했습니다. Rigidbody2D가 필요 없는 직선·유도탄 통합 발사체 컴포넌트입니다.
- `AchFollower`를 완전한 독립 컴포넌트로 리팩터했습니다. `AchMover` 의존성을 제거했습니다.
- FontAsset Maker에 다국어 FontAsset 빌드 기능을 추가했습니다. 한국어·영어·일어를 멀티체크로 선택해 각각 별도의 `*_TMP.asset` 파일을 생성할 수 있습니다.
- 모든 런타임 비동기 API를 `System.Threading.Tasks.Task`로 통일했습니다. 중간 추상화인 `AchTask`를 제거했습니다.

## 1.0.2

- Unity Entities용 선택 ECS 헬퍼를 추가했습니다. World, CommandBuffer, Baker, System, DI 래퍼를 포함합니다.
- Managers, Singleton, Log, WebRequest, PlayerData, QuickSave 등 게임 프레임워크 런타임 모듈을 추가했습니다.
- Unity 오브젝트, UI 컴포넌트, 컬렉션, 문자열, 델리게이트, Task, 공통 유틸리티를 다루는 Runtime Extensions 어셈블리를 추가했습니다.
- A* Pathfinding 유틸리티와 Grid Baker를 추가했습니다.
- AchMover 기반 이동 헬퍼를 추가했습니다.
- RedDot 알림 배지 런타임 기능을 추가했습니다.
- Drag, Object Touch, Binding, Open/Close Button 등 UI 컴포넌트 헬퍼를 추가했습니다.
- AchEngine 주요 시스템을 함께 보여주는 3개 씬 구성의 Full Sample을 추가했습니다.
- Addressables, DI, Localization, Table, UI 문서를 한국어/영어 양쪽에서 보강했습니다.
- Domain Reload가 꺼진 Enter Play Mode에서도 정적 상태가 초기화되도록 처리했습니다.
- 문서 사이트, Mermaid 다이어그램, 교차 링크, JSON 처리 관련 문제를 수정했습니다.
- Editor Decorators 모듈과 관련 문서를 제거했습니다.
- 루트 README를 문서 링크 중심의 간단한 랜딩 페이지로 정리했습니다.

## 1.0.1

- Table JSON 데이터를 Google Sheets 임포트용 CSV로 내보내는 도구를 추가했습니다. 개별 파일과 폴더 단위 변환을 지원합니다.
