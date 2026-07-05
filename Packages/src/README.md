# OOJJRS' Unity GameObject Helper

`com.oojjrs.oh`는 Unity 프로젝트에서 자주 반복되는 GameObject 처리, 컴포넌트 탐색, 로딩, 수명 관리, 에디터 보조 작업을 모아 둔 유틸리티 패키지이다.

## 구성

- 런타임 확장 메서드와 보조 컴포넌트
- 로더, 테이블, 파인더, 싱글톤 기반 유틸리티 타입
- 에디터 전용 보조 스크립트
- 패키지 메타데이터와 어셈블리 정의

## 포함 기능

- `Component`, `GameObject`, `Transform`, `Animator`, `AudioMixer` 등을 위한 확장 메서드
- `AnimatorExtensions`에서 enum 값을 Animator integer 파라미터로 저장하고 다시 enum으로 읽는 `SetIntegerSafety<T>`, `GetIntegerSafety<T>` 오버로드 제공
- `Object.DestroySafety`, `GameObject.DestroySafety`, `Component.DestroyObjectSafety`로 null-safe 파괴 호출을 구분
- `Object` 계열 prefab에 대해 `prefab.Instantiate()` 형태의 생성 보조 메서드 제공
- `TransformExtensions`에서 `RotationSafety`, `LocalRotationSafety`로 Quaternion 회전 설정을 안전하게 처리
- `LoaderT`, `TableT`, `FinderT`, `FinderInParentT` 기반의 참조 탐색 보조 타입
- `AnimatorFinderFromVariable`, `MyAnimatorFinderFromVariable`로 Inspector 지정 Animator 참조 제공
- `SingletonMonoBehaviourT`, `MyUpdater`, `LifeTime` 같은 실행 흐름 보조 타입
- `SimpleLog`를 통한 Unity 생명주기와 애플리케이션 상태 로그 출력
- `EntityModelBindingT`를 통한 Entity-Model 연결 관리
- `MyStableEnumAttribute`로 문자열 필드에 enum 이름을 저장하는 에디터 드롭다운 제공

## AnimatorExtensions

`AnimatorExtensions`는 null-safe Animator 호출을 제공한다. `SetIntegerSafety<T>`는 enum 값을 `int`로 변환해 Animator integer 파라미터에 저장하고, `GetIntegerSafety<T>`는 Animator에 저장된 정수를 enum 값으로 되돌린다.

```csharp
animator.SetIntegerSafety(ActionHash, ActionEnum.Attack);
var action = animator.GetIntegerSafety(ActionHash, ActionEnum.None);
```

- `Animator`가 null이면 setter는 아무 동작도 하지 않고, enum getter는 전달된 기본값을 반환한다.
- 저장된 정수는 enum 정의 여부를 별도로 검증하지 않고 해당 enum 타입의 값으로 복원한다.

## SimpleLog

`SimpleLog`는 컴포넌트가 붙은 GameObject의 주요 Unity 메시지 시점에 Inspector에서 지정한 로그를 출력하는 보조 컴포넌트이다. 각 메시지는 Unity 기본 `LogType`을 사용해 `Log`, `Warning`, `Error` 등 출력 타입을 선택할 수 있다. 출력 로그에는 GameObject 이름과 이벤트 이름이 `GameObjectName> EventName: 메시지` 형식으로 자동 포함된다.

지원하는 시점은 아래와 같다.

- `Awake`
- `OnEnable`
- `Start`
- `OnDisable`
- `OnDestroy`
- `OnApplicationFocus` / `OnApplicationBlur`
- `OnApplicationPause` / `OnApplicationResume`
- `OnApplicationQuit`

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

## MyStableEnumAttribute

`MyStableEnumAttribute`는 enum 값을 정수 대신 문자열 이름으로 직렬화해야 하는 필드에 붙이는 속성이다. 대상 필드는 `string`이어야 하며, Unity Inspector에서는 지정한 enum 타입의 이름 목록을 드롭다운으로 보여 준다.

```csharp
[MyStableEnum(typeof(MyState))]
public string StateName;
```

- enum 멤버 순서가 바뀌어도 저장된 값은 기존 이름을 유지한다.
- Inspector 변경 감지와 Prefab override 표시가 Unity 기본 프로퍼티 처리 흐름을 따른다.
- Inspector 라벨 영역을 Unity 기본 규칙에 맞춰 분리해 드롭다운 정렬을 유지한다.
- 저장된 문자열이 enum 목록에 없으면 Inspector에서는 첫 번째 항목을 기본 선택값으로 표시한다.

## 사용 환경

- Unity `6000.3`
- Package name: `com.oojjrs.oh`

## 참고

- `Packages/packages-lock.json`에서는 `com.oojjrs.oh`가 `file:src` 패키지로 반영된다.
- `MonoBehaviour` 상속 스크립트는 파일명과 클래스명이 같아야 한다.
- Unity GUID 보존이 필요한 파일 이동 시 `.meta`를 함께 유지해야 한다.
