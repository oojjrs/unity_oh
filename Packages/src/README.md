# OOJJRS' Unity GameObject Helper

`com.oojjrs.oh`는 Unity 프로젝트에서 자주 반복되는 GameObject 처리, 컴포넌트 탐색, 로딩, 수명 관리, 에디터 보조 작업을 모아 둔 유틸리티 패키지이다.

## 구성

- 런타임 확장 메서드와 보조 컴포넌트
- 로더, 테이블, 파인더, 싱글톤 기반 유틸리티 타입
- 에디터 전용 보조 스크립트
- 패키지 메타데이터와 어셈블리 정의

## 포함 기능

- `Component`, `GameObject`, `Transform`, `Animator`, `AudioMixer` 등을 위한 확장 메서드
- `LoaderT`, `TableT`, `FinderT`, `FinderInParentT` 기반의 참조 탐색 보조 타입
- `SingletonMonoBehaviourT`, `MyUpdater`, `LifeTime` 같은 실행 흐름 보조 타입
- `EntityModelBindingT`를 통한 Entity-Model 연결 관리

## LoaderT와 TableT

기존 `LoaderT<T>`는 `MonoBehaviour` 기반 참조 컨테이너이고, 신규 `TableT<T>`는 같은 인덱스 조회 규약을 `ScriptableObject`로 옮긴 자산형 컨테이너이다.

- 두 타입 모두 `0` 인덱스를 null 대체값처럼 사용한다.
- `Get(int)`와 `GetNull()` 인터페이스를 동일하게 유지해 사용처를 점진적으로 옮길 수 있다.
- 기존 `LoaderT` 계열은 유지되고, 새 프로젝트나 교체 대상에는 `TableT` 계열을 사용할 수 있다.

현재 함께 제공되는 `Table` 타입은 아래와 같다.

- `PrefabTable`
- `SpriteTable`
- `Texture2DTable`
- `AudioSourceTable`
- `ChronoEffectTable`
- `SimpleBgmerTable`

## EntityModelBindingT

`EntityModelBindingT<IdType, EntityType, ModelType>`는 외부에서 생성한 `ModelType`을 `EntityType`과 연결해 관리하는 바인딩 도우미이다.

- 같은 `Entity`에 모델이 중복 연결되지 않도록 바인딩을 교체한다.
- 같은 `Id`, `Entity`, `Model`이 이미 등록되어 있으면 경고 로그를 남긴다.
- `TryGetModel`, `TryGetEntity`, `HasEntity`로 연결 상태를 조회할 수 있다.
- `Remove`, `Clear`는 필요 시 연결된 모델 오브젝트 삭제까지 함께 처리할 수 있다.

## 사용 환경

- Unity `6000.3`
- Package name: `com.oojjrs.oh`

## 참고

- `Packages/packages-lock.json`에서는 `com.oojjrs.oh`가 `file:src` 패키지로 반영된다.
- `MonoBehaviour` 상속 스크립트는 파일명과 클래스명이 같아야 한다.
- Unity GUID 보존이 필요한 파일 이동 시 `.meta`를 함께 유지해야 한다.