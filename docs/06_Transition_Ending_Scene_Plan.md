# [기획서 6] 전환 컷신 및 최종 진 엔딩 씬 상세 설계서

본 문서는 **Phase 1 -> Phase 2 전환 컷신(Scene_Transition)**과 **최종 진 엔딩 씬(Scene_Ending)**의 분리 구현 기능과 작업 상세 내용을 다룹니다.

---

## 📝 기능 설명 (Feature Description)

### 1. Phase 1 -> Phase 2 전환 컷신 (`Scene_Transition.unity`)

- **영상 재생**: Phase 1을 성공적으로 클리어하면 랜선을 끊고 하얀 문으로 진입하는 `Ending_Swap.mp4` 영상이 10초간 매끄럽게 재생됩니다. (디코더 오류로 인한 조기 끊김 현상 없음)
- **독백 출력**: 영상 종료 후 완전한 암전 상태에서 타자기 효과음과 함께 차분하게 독백 텍스트가 타이핑 출력됩니다. 가독성을 위해 문장이 적절하게 줄바꿈되며, 6초간 유지되어 깊은 긴장감을 줍니다.
  > "드디어... 연결이 끊어졌다."
  > "완벽한 고요."
- **시스템 에러 경고**: 정적 대기 이후 화면 전체가 TV 화이트 노이즈로 뒤덮이며 쨍한 경고 Beep 사운드와 함께 적색 경고 텍스트가 출력됩니다. 화면 넘침 없이 패널 내에 깔끔하게 아랫줄로 개행 정렬되어 7초간 흔들림 없이 고정 노출됩니다.
  > `[Error] 불법적인 오프라인 전환 감지.`
  > `약관 위반에 따른 강제 징수 프로그램(Nightmare.exe)을 실행합니다.`
- **인게임 진입**: 에러 연출이 마무리되면 전자기 글리치 사운드와 함께 자동으로 Phase 2 인게임 씬(`Scene_Phase2`)으로 전환됩니다.

### 2. 최종 진 엔딩 씬 (`Scene_Ending.unity`)

- **기상 연출**: 악몽(Phase 2) 버티기에 성공하면 하얗게 화면이 페이드인되며, 밝은 아침 햇살 속에서 침대에서 일어나는 주인공 P의 픽셀 아트 이미지(`ending_waking_up.png`)가 노출되어 4초간 평화로운 분위기를 자아냅니다.
- **대반전 글리치**: 평화가 일순간 깨지며 1.5초간 화면의 픽셀 텍스처들이 심하게 일그러지고(URP Lens Distortion 및 Chromatic Aberration 적용), 강렬한 글리치 효과음이 출력됩니다.
- **DOS CMD 진실 폭로**: 화면이 흑백 도스 프롬프트로 강제 전환되며 실제 컴퓨터 로컬 시간이 연동된 시스템 실험 완료 로그가 고속 기계식 타건음과 함께 타이핑됩니다.
- **타이틀 루프 연계**: 로그가 완성되고 12초간 정독할 시간이 확보된 뒤, 강한 Glitch와 함께 시작 화면으로 이동합니다. 시작 화면의 타이틀 및 버튼들이 붉은 경고색 루프 테마(`[AI 에이전트 스트레스 테스트: 893번째 루프]`)로 영구 갱신됩니다.

---

## 🛠️ 작업 상세 내용 (Detailed Work Description)

### 1. 스크립트 작성 및 갱신

- **`PhaseTransitionController.cs`**
  - 비디오 재생(RenderTexture 동적 생성 및 RawImage 어사인), 음소거(Mute), 독백 텍스트 연출 및 경고 Beep, Nightmare.exe 에러 제어 후 Phase 2 씬을 로드하도록 제어 흐름 작성.
- **`PhaseTransitionAutoSetup.cs`**
  - `Tools > Taehui > Setup Transition Scene - AUTO` 메뉴 바인딩. 씬 자동 조립 및 `Standalone` 타겟 대상 비디오 강제 **H.264 트랜스코딩** 임포터 설정 코드를 이식하여 멈춤 문제를 원천 해결.
- **`EndingSceneController.cs`**
  - 비디오 및 독백 로직을 걷어내고, 아침 페이드인 ➔ 기상 이미지 활성 ➔ URP Volume Glitch ➔ CMD 터미널 제어로직으로 전면 리팩토링.
- **`EndingSceneAutoSetup.cs`**
  - 엔딩 씬 생성 도구 갱신. 아침 기상 픽셀 이미지의 자동 `Sprite (2D and UI)` 임포터 타입 변환 로직 탑재 및 신규 패널 구조 조립 설정 변경.

### 2. 가독성 개선 및 안정성 확보

- **에러 텍스트 레이아웃 보정**: 에러 문구 중간에 개행(`\n`)을 추가하여 레이아웃을 고정하고, 텍스트 넘침을 방지하기 위해 TextMeshProUGUI 컴포넌트의 오토 사이징 및 자동 줄바꿈을 활성화했습니다.
- **Obsolete 경고 해결**: TextMeshPro의 구식 API `enableWordWrapping` 경고를 최신 Unity 환경에 맞게 `textWrappingMode = TextWrappingModes.Normal`로 보정하여 컴파일 경고를 0건으로 제어했습니다.
