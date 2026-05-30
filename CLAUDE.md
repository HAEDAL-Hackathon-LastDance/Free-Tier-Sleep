# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**Free-Tier-Sleep**은 Unity 6 (6000.3.15f1) 기반 2D 메타픽션 아케이드 게임입니다. 플레이어는 AI 스트레스 테스트(루프 #893) 시뮬레이션이라는 설정 속에서 두 개의 게임플레이 페이즈를 경험합니다:

- **Phase 1 — "정보 과부하"**: 절차적으로 생성된 플랫폼을 오르면서 상승하는 데이터 홍수와 적 웨이브를 피해 3분(AM 03:00→06:00)을 버티는 수직 플랫포머
- **Phase 2 — "디지털 공허"**: 마우스로 방화벽 선을 그려 적을 처치하되, 적 수가 늘수록 입력 지연이 증가하는 타워 디펜스 혼합형
- **인트로 씬**: 글리치 효과, 타이핑 연출, URP 후처리로 구성된 서사 시퀀스

설계 문서는 `docs/`와 `Phase{1,2}_Develop_Guide.md`에 있습니다.

## 브랜치 & 커밋 컨벤션

- `main` — 릴리즈 전용
- `dev` — 통합 기본 브랜치 (PR 대상)
- 기능 브랜치: `{type}/{이슈번호}-{설명}` (예: `feat/#12-player-move`)
- 커밋 접두사: `feat : `, `fix : `, `docs : `, `chore : ` (콜론 양쪽에 공백)

## 프로젝트 구조

에셋은 `Assets/_Project/{개발자명}/` 아래 개발자별로 분리되어 있습니다:

| 개발자 | 담당 시스템 | 주요 스크립트 |
|---|---|---|
| `minjun/` | Phase 1 핵심 | `PlayerController`, `LevelGenerator`, `RisingDataFlood`, `ObjectPooler`, `CameraFollow` |
| `suyeon/` | 적 시스템 | Phase 1 `GameManager`(웨이브 스포너), Phase 2 `StandardEnemy`, `PlayerHealth` |
| `Yujin/` | Phase 2 드로잉 | `LineDrawer`, `Stroke`, `LineCollision`, `LagCursor` |
| `taehui/` | 인트로 씬 | `IntroSceneController`, `TypingEffect`, `AdPopupManager`, `ProceduralAudioHelper` |

## 아키텍처

### 싱글턴 매니저
`ObjectPooler` (minjun)가 핵심 싱글턴입니다. `ObjectPooler.Instance`로 접근하며, 플랫폼·적·발사체의 재사용 큐를 관리합니다. 풀 소진 시 동적으로 확장되며, `ReturnToPool()`에 이중 반환 방지 로직이 내장되어 있습니다.

### Phase 1 시스템
- `LevelGenerator` — `ObjectPooler`와 `CameraFollow`를 통해 뷰포트 기준으로 플랫폼을 생성·제거합니다.
- `RisingDataFlood` — 플레이어 고도 300, 600에서 상승 속도가 단계적으로 가속되며, 사망 시 카메라 하단에 고정됩니다.
- `PlayerController` — 코요테 타임(0.15 s), 더블 점프, 가변 점프 높이를 구현합니다. 피격 시 머티리얼의 `_Intensity` 프로퍼티로 글리치 셰이더를 구동합니다.

### Phase 2 시스템
- `LineDrawer` — 마우스 입력을 `Stroke` 객체로 기록합니다. 각 `Stroke`는 버텍스 생존시간 큐를 관리하고, `LineCollision`이 `EdgeCollider2D`를 붙여 적과의 충돌을 처리합니다.
- `LagCursor` — 실제 마우스 위치를 향해 lerp하는 가상 커서입니다. 지연 계수가 적 수에 비례해 증가하며, 드로잉은 항상 지연된 커서 위치를 사용합니다.

### 공통 패턴
- 체력 변화 이벤트는 C# `Action`(`OnHealthChanged`)으로 UI와 분리합니다.
- 피격 플래시·페이드·글리치 전환 등 시간 기반 연출은 코루틴으로 처리합니다.
- 디버그 로그는 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`로 조건부 컴파일합니다.
- 게임 오버 시 `Time.timeScale = 0`으로 일시정지합니다.

## 빌드 & 씬

별도의 빌드 스크립트 없음. Unity 에디터의 기본 Build Settings를 사용합니다.

| 씬 | 경로 |
|---|---|
| 인트로 | `Assets/_Project/taehui/01.Scenes/Scene_Intro.unity` |
| Phase 1 | `Assets/_Project/minjun/01.Scenes/Phase1_Movement.unity` |
| Phase 2 | `Assets/_Project/Yujin/` 또는 `suyeon/` 씬 폴더 |

입력은 새 Input System 에셋(`InputSystem_Actions.inputactions`)으로 처리합니다.
