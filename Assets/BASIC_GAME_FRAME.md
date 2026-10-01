# Math High Low - 기본 게임 틀

`Assets/Scenes/PlayableExample.unity`에 기존 스크립트로 동작하는 완성 예시 화면을 배치했습니다.

## 현재 구성

- `GameManager`
  - `GameController`
  - `RoundController`
  - `PlayerController`
  - `AIController`
- `GameCanvas` + `GameView`
  - 플레이어/AI 점수와 라운드 타이머
  - 목표값 1/20 선택 버튼
  - 베팅 증가/감소 버튼
  - AI/플레이어 카드 컨테이너
  - 현재 수식과 상태 안내
  - 제출/초기화 버튼
  - 라운드 결과 패널
- `EventSystem`
  - Input System UI 모듈 연결
- `Main Camera`
  - URP 2D 카메라와 Audio Listener 연결
- `CardButton.prefab`
  - 기존 `CardView`와 UI 참조 연결
- `Art/Backgrounds/DessertMathArena.png`
  - 오리지널 과자 영웅과 디저트 마을을 사용한 예시 배경

## 실행

1. `PlayableExample` 씬을 엽니다.
2. Play를 누릅니다.
3. 목표와 베팅을 선택하고 카드를 눌러 수식을 만듭니다.
4. 이후 색상과 이미지만 교체해 원하는 스타일로 발전시킬 수 있습니다.

기존 `Assets/Scripts (1)/Scripts` 내부 스크립트는 수정하지 않았습니다.

## 비주얼 콘셉트

- 밝은 디저트 판타지 세계
- 코코아 기사와 민트 마법사 형태의 오리지널 과자 영웅
- 캐러멜, 딸기, 민트, 크림, 네이비 색상 조합
- 기존 상용 게임의 캐릭터·로고·고유 디자인은 사용하지 않음
- 상단은 AI 카드 영역, 하단은 플레이어 카드 영역, 중앙은 수식 대결 영역
- Noto Sans KR 동적 TMP 폰트를 fallback으로 사용해 한글 상태 메시지 지원
