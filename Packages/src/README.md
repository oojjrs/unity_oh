# Packages/src 안내

`Packages/src`는 현재 `com.oojjrs.oh`의 실제 패키지 루트입니다.

## 현재 구성

- 패키지 메타데이터 `package.json`
- 런타임 및 공용 스크립트
- 어셈블리 정의 `oojjrs.oh.asmdef`
- 에디터 전용 스크립트 `Editor/`
- 패키지 문서 `README.md`

## 이번 정리 내용

기존 `Assets`에 있던 아래 항목을 `Packages/src`로 옮겼습니다.

- C# 스크립트와 대응 `.meta`
- `oojjrs.oh.asmdef`와 대응 `.meta`
- `Editor` 폴더와 대응 `.meta`
- 패키지 메타 파일 `package.json`

## 참고 사항

- `Packages/packages-lock.json`에는 `com.oojjrs.oh`가 `file:src` 임베디드 패키지로 반영됩니다.
- 파일 이동 시 `.meta`를 함께 옮겨 Unity GUID를 유지했습니다.
- 현재 `Assets` 루트는 비어 있습니다.
