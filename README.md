# UnityOh

`UnityOh`는 Unity 프로젝트에서 자주 쓰는 GameObject 보조 유틸리티와 확장 메서드를 제공하는 패키지 저장소입니다.

현재 패키지 루트는 `Packages/src`이며, 패키지 이름은 `com.oojjrs.oh`입니다.

## 구성

- 런타임 및 공용 스크립트: `Packages/src`
- 에디터 스크립트: `Packages/src/Editor`
- 패키지 메타데이터: `Packages/src/package.json`
- 어셈블리 정의: `Packages/src/oojjrs.oh.asmdef`

## 설치

Unity 프로젝트의 `Packages/manifest.json`에 아래처럼 로컬 패키지 경로를 추가해 사용할 수 있습니다.

```json
{
  "dependencies": {
    "com.oojjrs.oh": "file:src"
  }
}
```

같은 저장소 기준이 아니라 외부 프로젝트에서 가져올 때는 패키지 디렉터리 자체를 복사하거나, 패키지 매니저에서 Git URL을 `Packages/src` 경로 기준으로 연결해야 합니다.

## 개발 메모

- 기존 `Assets`에 있던 스크립트와 메타 파일은 `Packages/src`로 이동했습니다.
- `.meta` 파일을 함께 유지해 Unity GUID가 바뀌지 않도록 정리했습니다.
- 현재 버전은 `1.19.0`입니다.

## 문서

- 패키지 루트 안내: [Packages/src/README.md](Packages/src/README.md)
