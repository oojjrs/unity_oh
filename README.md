# UnityOh

Unity 프로젝트에서 반복되는 GameObject 처리, 참조 관리, 입력·화면 감지와 실행 흐름을 보조하는 유틸리티 패키지입니다.

## 설치

Unity Package Manager의 `Add package from git URL...`에 다음 주소를 입력합니다.

```text
https://github.com/oojjrs/unity_oh.git?path=/Packages/src
```

## 구성

| 구성 요소 | 종류 | 용도 |
| --- | --- | --- |
| `*Extensions` | 확장 메서드 | `GameObject`, `Component`, `Transform`, `Animator`, `AudioMixer` 등의 반복 작업 보조 |
| `LoaderT<T>`, `TableT<T>`, `FinderT<T>` | 참조 도우미 | 컴포넌트 또는 `ScriptableObject` 기반 참조 조회 |
| `InputDetector`, `DeviceDetector`, `DisplayDetector`, `WindowSizeDetector` | 런타임 컴포넌트 | 입력 장치, 디스플레이, 창 크기 변화 전달 |
| `Ticker`, `ApplicationMonitor`, `CoreSingleton` | 런타임 컴포넌트 | Tick, 애플리케이션 생명주기, 초기화 흐름 구성 |
| `GameViewFullscreen`, `DevTool` | 에디터 도구 | Game View 전체화면과 프로젝트별 개발 도구 기반 제공 |

## 사용

```csharp
var instance = prefab.Instantiate(parent);
instance.SetActiveSafety(true);
instance.DestroySafety();
```

## 제약

- Unity `6000.3` 이상과 Input System `1.19.0`을 사용합니다.
- `GameViewFullscreen`은 Windows Unity Editor에서만 제공되며 Unity Editor 내부 API가 바뀌면 비활성화될 수 있습니다.

## 문서

- [패키지 상세 문서](Packages/src/README.md)
