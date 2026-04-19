# UnityOh

`UnityOh`는 Unity 프로젝트에서 자주 쓰는 GameObject 보조 유틸리티와 확장 메서드를 제공하는 패키지 저장소입니다.

현재 패키지 루트는 `Packages/src`이며, 패키지 이름은 `com.oojjrs.oh`입니다.

## 구성

- 런타임 및 공용 스크립트: `Packages/src`
- 에디터 스크립트: `Packages/src/Editor`
- 패키지 메타데이터: `Packages/src/package.json`
- 어셈블리 정의: `Packages/src/oojjrs.oh.asmdef`

## 개발 메모

- 기존 `Assets`에 있던 스크립트와 메타 파일은 `Packages/src`로 이동했습니다.
- `.meta` 파일을 함께 유지해 Unity GUID가 바뀌지 않도록 정리했습니다.
- 현재 버전은 `1.20.1`입니다.

## 문서

- 패키지 루트 안내: [Packages/src/README.md](Packages/src/README.md)
