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
- `MyAnimator`에서 enum 값을 Action integer 파라미터로 재생하는 `aaPlayAction<T>`, `aaPlayActionOnce<T>` 오버로드 제공
- `Object.DestroySafety`, `GameObject.DestroySafety`, `Component.DestroyObjectSafety`로 null-safe 파괴 호출을 구분
- `Object` 계열 prefab에 대해 `prefab.Instantiate()` 형태의 생성 보조 메서드 제공
- `TransformExtensions`에서 `RotationSafety`, `LocalRotationSafety`로 Quaternion 회전 설정을 안전하게 처리
- `LoaderT`, `TableT`, `FinderT`, `FinderInParentT` 기반의 참조 탐색 보조 타입
- `AnimatorFinderFromVariable`, `MyAnimatorFinderFromVariable`로 Inspector 지정 Animator 참조 제공
- `SingletonMonoBehaviourT`, `MyUpdater`, `LifeTime` 같은 실행 흐름 보조 타입
- `SimpleLog`를 통한 Unity 생명주기와 애플리케이션 상태 로그 출력
- 코루틴 기반 컴포넌트에서 `yield return` 복귀 후 `this`와 캐시된 Unity 객체 접근 방지
- `MyUpdater` named invoker의 자연 종료, 명시 취소, 즉시 완료 시 실행 이름 등록 정리
- `EntityModelBindingT`를 통한 Entity-Model 연결 관리
- `MyStableEnumAttribute`로 문자열 필드에 enum 이름을 저장하는 에디터 드롭다운 제공
- `WindowSizeDetector`로 화면 크기 변경 시 너비와 높이를 콜백에 전달
- `InputDetector`로 키보드, 마우스, 게임패드 버튼 입력 경로를 콜백에 전달
- `DeviceDetector`로 장치 연결 상태와 현재 입력 장치를 키보드, 마우스, PlayStation, Xbox, 기타 게임패드 콜백으로 구분
- `DevelopmentBuildPlayerPrefsResetter`로 개발 빌드의 애플리케이션 버전 변경 시 `PlayerPrefs` 초기화
- 단일 동작만 허용하는 런타임 컴포넌트의 동일 GameObject 중복 부착 방지

## InputDetector

`InputDetector`는 키보드, 마우스, 게임패드에서 눌린 버튼의 Input System 경로를 같은 GameObject의 `CallbackInterface` 구현체에 전달한다.

```csharp
using oojjrs.oh;
using UnityEngine;

[RequireComponent(typeof(InputDetector))]
public class InputDetectorReceiver : MonoBehaviour, InputDetector.CallbackInterface
{
    void InputDetector.CallbackInterface.Update(string path)
    {
        Debug.Log(path);
    }
}
```

- 콜백 구현체는 `InputDetector`와 같은 GameObject에 추가한다.
- 게임패드 경로의 장치 이름은 `Gamepad`로 통일된다.
- 프로젝트의 Active Input Handling은 `Input System Package (New)` 또는 `Both`로 설정해야 한다.

## DeviceDetector

`DeviceDetector`는 `PlayerInput` 없이 장치 연결 상태와 실제 입력으로 바뀐 현재 장치를 감지하고, 같은 GameObject의 단일 `CallbackInterface` 구현체에 세분화된 콜백을 전달한다.

```csharp
using oojjrs.oh;
using UnityEngine;

[RequireComponent(typeof(DeviceDetector))]
public class DeviceDetectorReceiver : MonoBehaviour, DeviceDetector.CallbackInterface
{
    void DeviceDetector.CallbackInterface.OnCurrentDeviceChanged(DeviceDetector.DeviceEnum? previousDevice, DeviceDetector.DeviceEnum currentDevice) { }
    void DeviceDetector.CallbackInterface.OnDeviceConnected(DeviceDetector.DeviceEnum device, int deviceCount, int gamepadCount, bool isInitialState) { }
    void DeviceDetector.CallbackInterface.OnDeviceDisconnected(DeviceDetector.DeviceEnum device, int deviceCount, int gamepadCount) { }
    void DeviceDetector.CallbackInterface.OnGamepadInput(DeviceDetector.DeviceEnum gamepad) { }
    void DeviceDetector.CallbackInterface.OnKeyboardExtendedInput() { }
    void DeviceDetector.CallbackInterface.OnKeyboardInput() { }
    void DeviceDetector.CallbackInterface.OnKeyboardUnavailable() { }
    void DeviceDetector.CallbackInterface.OnMouseButtonInput() { }
    void DeviceDetector.CallbackInterface.OnMouseMove() { }
    void DeviceDetector.CallbackInterface.OnMouseUnavailable() { }
}
```

- 콜백 구현체는 `DeviceDetector`와 같은 GameObject에 하나만 추가한다.
- 사용자 컴포넌트의 `Start()`가 끝난 다음 프레임에 연결된 물리 장치를 `OnDeviceConnected()`로 전달하며 `isInitialState`를 `true`로 설정한다. 초기 장치 열거 순서는 실제 연결 순서가 아니므로, 마지막 연결 장치 정책에는 `isInitialState`가 `false`인 실행 중 연결만 사용한다. 키보드나 마우스가 없으면 각 전용 콜백으로 초기 상태를 알리고, 초기 스냅샷 전에는 입력 콜백을 호출하지 않는다.
- 초기화 이후에는 물리 장치가 연결되거나 해제될 때마다 `OnDeviceConnected()`와 `OnDeviceDisconnected()`를 호출한다. `deviceCount`에는 변경 후 같은 `DeviceEnum` 장치 수를, `gamepadCount`에는 변경 후 전체 게임패드 수를 전달하므로 공개 API에 `InputDevice`를 노출하지 않고도 해당 종류가 완전히 사라졌는지와 남은 전체 패드 수를 모두 판단할 수 있다.
- 입력으로 현재 물리 장치가 바뀌면 `OnCurrentDeviceChanged()`에 이전 장치 종류와 현재 장치 종류를 전달한 뒤 해당 입력 종류 콜백을 호출한다.
- 공개 API에는 Unity Input System의 `InputDevice`를 노출하지 않고 패키지 자체 `DeviceEnum`만 사용한다.
- PlayStation 계열은 `DualShockGamepad`, Xbox 계열은 `XInputController` 레이아웃 상속으로 판별한다.
- 게임패드 입력은 `OnGamepadInput()` 하나로 전달하며 인자의 `DeviceEnum`으로 PlayStation, Xbox, 기타 계열을 구분한다.
- 키보드 입력은 `OnKeyboardInput()`으로 전달한다.
- 마우스 이동은 `OnMouseMove()`로 전달해 커서 표시만 처리할 수 있고, 같은 마우스의 첫 버튼 입력은 `OnMouseButtonInput()`으로 따로 전달해 UI 표시 전환을 처리할 수 있다.
- 패드나 키보드로 현재 장치가 바뀌면 마우스 버튼 활성 상태를 초기화하며, 다시 마우스를 움직인 뒤 버튼을 누를 때 같은 순서로 콜백한다.
- 키보드와 마우스의 사용 가능 상태를 각각 추적하고, 없어지면 `OnKeyboardUnavailable()`과 `OnMouseUnavailable()`을 따로 호출한다.
- 일반 문자·숫자·기호, Ctrl·Alt·Shift, 탐색키, CapsLock·NumLock, 숫자 패드, F1~F12만 키보드 전환 입력으로 허용한다.
- PrintScreen·ScrollLock·Pause, Meta·Windows, ContextMenu, OEM, F13~F24, 미디어·IME 키와 이후 추가되는 미분류 키는 `OnKeyboardExtendedInput()`으로 한 번만 전달하되 현재 장치를 키보드로 바꾸지 않는다. 일반 키나 다른 장치 입력이 들어오거나 키보드가 해제되면 다시 호출할 수 있게 초기화한다.
- 입력 크기 `0.1` 이상의 실제 상태 변화만 처리하므로 장치 연결만으로는 콜백하지 않는다.
- Inspector의 `Debug Log`를 켜면 컴포넌트 활성화 상태, 공개 콜백 호출, 장치 종류와 레이아웃·ID, 변경 후 게임패드 수, 현재 장치 전환을 추적할 수 있다. 기본값은 꺼짐이며 입력 이벤트가 무시되는 핫패스에는 로그를 남기지 않는다.
- `InputDetector`, `WindowSizeDetector`, `MyUpdater`, `ChronoInterfaceMachine`, `SimpleBgmer`, `AutoDisabler`, `LifeTime`도 Inspector의 `Debug Log`를 켰을 때만 입력 전달, 상태 전환, 예약·취소·완료 같은 상세 진단 로그를 출력한다.
- 씬 전환, Singleton 수명, 영구 오브젝트 존속, 개발 빌드 데이터 초기화처럼 시스템 흐름 복원에 필요한 기존 로그와 모든 경고는 디버그 설정과 관계없이 항상 출력한다.
- 프로젝트의 Active Input Handling은 `Input System Package (New)` 또는 `Both`로 설정해야 한다.

## DevelopmentBuildPlayerPrefsResetter

`DevelopmentBuildPlayerPrefsResetter`는 개발 빌드에서 `Application.version`을 기록하고, 이전 실행에서 기록한 버전과 달라졌을 때 `PlayerPrefs.DeleteAll()`을 호출하는 일회성 컴포넌트이다.

- 시작 Scene의 전용 GameObject에 추가한다. 처리가 끝나면 해당 GameObject를 제거하므로 다른 컴포넌트를 함께 추가하지 않는다.
- 최초 실행에는 기존 값을 유지하고 현재 버전만 기록한다.
- Inspector에서 `Delete On First Run`을 활성화하면 최초 실행에도 전체 값을 삭제한다.
- Unity Editor 또는 `Debug.isDebugBuild`가 참인 빌드에서만 `PlayerPrefs`를 변경한다.
- 개별 데이터 마이그레이션이나 이전 버전 보존이 필요하면 애플리케이션의 저장 로직에서 별도로 처리한다.

## AnimatorExtensions

`AnimatorExtensions`는 null-safe Animator 호출을 제공한다. `SetIntegerSafety<T>`는 enum 값을 `int`로 변환해 Animator integer 파라미터에 저장하고, `GetIntegerSafety<T>`는 Animator에 저장된 정수를 enum 값으로 되돌린다.

```csharp
animator.SetIntegerSafety(ActionHash, ActionEnum.Attack);
var action = animator.GetIntegerSafety(ActionHash, ActionEnum.None);
```

- `Animator`가 null이면 setter는 아무 동작도 하지 않고, enum getter는 전달된 기본값을 반환한다.
- 저장된 정수는 enum 정의 여부를 별도로 검증하지 않고 해당 enum 타입의 값으로 복원한다.

## MyAnimator

`MyAnimator`는 `Action` integer 파라미터를 사용해 액션 애니메이션을 재생하는 보조 컴포넌트이다. enum 오버로드는 enum 값을 `int`로 변환해 기존 Action 재생 경로를 그대로 사용한다.

```csharp
myAnimator.aaPlayAction(ActionEnum.Attack);
myAnimator.aaPlayActionOnce(ActionEnum.Attack);
myAnimator.aaPlayActionSafety(ActionEnum.Attack);
myAnimator.aaPlayActionOnceSafety(ActionEnum.Attack);
```

- `aaPlayAction<T>`는 현재 액션을 중단한 뒤 새 Action 값을 설정한다.
- `aaPlayActionOnce<T>`는 기존 1회성 액션 종료 감지와 중복 호출 방지 흐름을 유지한다.
- safety extension은 `MyAnimator`가 null이면 기존 int overload와 같은 경고 로그를 출력한다.

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
- 저장된 문자열이 enum 목록에 없으면 Inspector에서 `<Missing: 기존값>`으로 표시하며, 사용자가 새 enum 값을 선택하기 전까지 기존 문자열을 보존한다.

## 사용 환경

- Unity `6000.3`
- Package name: `com.oojjrs.oh`

## 참고

- `Packages/packages-lock.json`에서는 `com.oojjrs.oh`가 `file:src` 패키지로 반영된다.
- `MonoBehaviour` 상속 스크립트는 파일명과 클래스명이 같아야 한다.
- Unity GUID 보존이 필요한 파일 이동 시 `.meta`를 함께 유지해야 한다.
