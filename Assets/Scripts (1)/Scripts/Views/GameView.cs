using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using MathHighLow.Models;
using MathHighLow.Services;

namespace MathHighLow.Views
{
    /// <summary>
    /// ✅ 수정: 연산자 버튼 완전 제거
    /// 
    /// Canvas에 부착되어 모든 UI 요소를 관리하고 이벤트를 구독/발행합니다.
    /// 이제 연산자는 카드로만 처리합니다!
    /// </summary>
    public class GameView : MonoBehaviour
    {
        [Header("프리팹")]
        [SerializeField] private CardView cardPrefab;

        [Header("UI 컨테이너 (손패)")]
        [SerializeField] private Transform playerHandContainer;
        [SerializeField] private Transform aiHandContainer;

        [Header("상태 텍스트")]
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI aiScoreText;
        [SerializeField] private TextMeshProUGUI playerExpressionText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("베팅 UI")]
        [SerializeField] private TextMeshProUGUI betText;
        [SerializeField] private Button betIncreaseButton;
        [SerializeField] private Button betDecreaseButton;
        [SerializeField] private Button betSetFiveButton;
        private int currentBetDisplay;

        [Header("베팅 칩 애니메이션")]
        [SerializeField] private GameObject oneChipObject1;
        [SerializeField] private GameObject oneChipObject2;
        [SerializeField] private GameObject oneChipObject3;
        [SerializeField] private GameObject oneChipObject4;
        [SerializeField] private GameObject fiveChipObject;
        private GameObject[] oneChipObjects;
        private readonly Vector2[] oneChipStartPositions =
        {
            new Vector2(-761f, -287f), new Vector2(-762f, -295f),
            new Vector2(-761.4304f, -298f), new Vector2(-761.4304f, -298f)
        };
        private readonly Vector2[] oneChipBoardPositions =
        {
            new Vector2(-385f, 49f), new Vector2(-369f, 27f),
            new Vector2(-353f, 5f), new Vector2(-337f, -17f)
        };
        private static readonly Vector2 FiveChipStartPosition = new Vector2(-881.31604f, -274f);
        private static readonly Vector2 FiveChipBoardPosition = new Vector2(-333f, -37f);
        private const float ChipMoveDuration = 1f;
        private bool isFiveChipDisplayed;

        [Header("목표값 버튼")]
        [SerializeField] private List<Button> targetButtons;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.blue;
        private int currentSelectedTarget = -1;

        [Header("기본 버튼")]
        [SerializeField] private Button submitButton;
        [SerializeField] private Button resetButton;

        [Header("결과 패널")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultSummaryText;
        [SerializeField] private TextMeshProUGUI resultDetailText;

        // 생성된 카드 뷰 오브젝트들을 관리
        private List<GameObject> spawnedCards = new List<GameObject>();

        #region 이벤트 구독 (OnEnable / OnDisable)

        void OnEnable()
        {
            // 점수
            GameEvents.OnScoreChanged += UpdateScoreText;

            // 라운드 진행
            GameEvents.OnRoundStarted += HandleRoundStarted;
            GameEvents.OnCardAdded += HandleCardAdded;
            GameEvents.OnRoundEnded += HandleRoundEnded;

            // 플레이어 입력
            GameEvents.OnExpressionUpdated += UpdateExpressionText;
            GameEvents.OnBetChanged += UpdateBetText;
            GameEvents.OnTimerUpdated += UpdateTimerText;

            // 제출 가능 여부
            GameEvents.OnSubmitAvailabilityChanged += UpdateSubmitAvailability;

            // ✅ 추가: 상태 텍스트
            GameEvents.OnStatusTextUpdated += UpdateStatusText;

            // 게임 종료
            GameEvents.OnGameOver += HandleGameOver;
        }

        void OnDisable()
        {
            GameEvents.OnScoreChanged -= UpdateScoreText;
            GameEvents.OnRoundStarted -= HandleRoundStarted;
            GameEvents.OnCardAdded -= HandleCardAdded;
            GameEvents.OnRoundEnded -= HandleRoundEnded;
            GameEvents.OnExpressionUpdated -= UpdateExpressionText;
            GameEvents.OnBetChanged -= UpdateBetText;
            GameEvents.OnTimerUpdated -= UpdateTimerText;
            GameEvents.OnSubmitAvailabilityChanged -= UpdateSubmitAvailability;
            GameEvents.OnStatusTextUpdated -= UpdateStatusText; // ✅ 추가
            GameEvents.OnGameOver -= HandleGameOver;
        }

        #endregion

        #region 버튼 리스너 (Start)

        void Start()
        {
            // 기본 버튼
            submitButton.onClick.AddListener(() => GameEvents.InvokeSubmit());
            resetButton.onClick.AddListener(() => GameEvents.InvokeReset());

            // 베팅 버튼
            betIncreaseButton.onClick.AddListener(HandleBetIncrease);
            betDecreaseButton.onClick.AddListener(HandleBetDecrease);
            betSetFiveButton.onClick.AddListener(HandleBetSetFive);
            InitializeBetChipAnimations();

            // 목표값 버튼
            if (targetButtons.Count > 0 && targetButtons[0] != null)
                targetButtons[0].onClick.AddListener(() => SelectTarget(0, 1));
            if (targetButtons.Count > 1 && targetButtons[1] != null)
                targetButtons[1].onClick.AddListener(() => SelectTarget(1, 20));

            // 초기화
            resultPanel.SetActive(false);
            UpdateScoreText(0, 0);
            UpdateExpressionText("");
            UpdateTimerText(0, 180);
        }

        #endregion

        #region 입력 핸들러 (UI -> Event)

        private void HandleBetIncrease()
        {
            int previousBet = currentBetDisplay;
            currentBetDisplay++;

            // ✅ 추가: 최대 5원 제한
            if (currentBetDisplay > 5)
            {
                currentBetDisplay = 5;
            }

            if (currentBetDisplay > previousBet)
            {
                if (currentBetDisplay == 5)
                {
                    StopAllCoroutines();
                    StartCoroutine(AddFourthChipAndSwitchToFive(previousBet));
                }
                else
                {
                    int chipIndex = currentBetDisplay - 2;
                    if (chipIndex >= 0 && chipIndex < oneChipObjects.Length)
                    {
                        PlayChipForward(chipIndex);
                    }
                }
            }

            GameEvents.InvokeBetChanged(currentBetDisplay);
        }

        private void HandleBetSetFive()
        {
            int previousBet = currentBetDisplay;
            currentBetDisplay = 5;
            StopAllCoroutines();
            StartCoroutine(SwitchToFiveChip(previousBet));
            GameEvents.InvokeBetChanged(currentBetDisplay);
        }

        private void HandleBetDecrease()
        {
            int previousBet = currentBetDisplay;
            currentBetDisplay--;
            if (currentBetDisplay < 1) currentBetDisplay = 1;

            if (previousBet == 5 && isFiveChipDisplayed)
            {
                StopAllCoroutines();
                StartCoroutine(SwitchFiveChipToFourCookies());
            }
            else if (currentBetDisplay < previousBet)
            {
                int chipIndex = previousBet - 2;
                if (chipIndex >= 0 && chipIndex < oneChipObjects.Length)
                {
                    PlayChipReverse(chipIndex);
                }
            }

            GameEvents.InvokeBetChanged(currentBetDisplay);
        }

        private void InitializeBetChipAnimations()
        {
            oneChipObjects = new GameObject[4];
            oneChipObjects[0] = oneChipObject1;
            oneChipObjects[1] = oneChipObject2;
            oneChipObjects[2] = oneChipObject3;
            oneChipObjects[3] = oneChipObject4;
            for (int i = 0; i < oneChipObjects.Length; i++)
            {
                if (oneChipObjects[i] != null)
                {
                    oneChipObjects[i].GetComponent<RectTransform>().anchoredPosition = oneChipStartPositions[i];
                    oneChipObjects[i].SetActive(false);
                }
            }

            if (fiveChipObject != null)
            {
                fiveChipObject.GetComponent<RectTransform>().anchoredPosition = FiveChipStartPosition;
                fiveChipObject.SetActive(false);
            }
        }

        private void PlayChipForward(int index)
        {
            GameObject chip = oneChipObjects[index];
            if (chip == null) return;

            StartCoroutine(MoveChip(chip, oneChipStartPositions[index],
                oneChipBoardPositions[index], false));
        }

        private void PlayChipReverse(int index)
        {
            GameObject chip = oneChipObjects[index];
            if (chip == null) return;

            StartCoroutine(MoveChip(chip, oneChipBoardPositions[index],
                oneChipStartPositions[index], true));
        }

        private IEnumerator MoveChip(GameObject chip, Vector2 from, Vector2 to, bool hideAfter)
        {
            if (chip == null) yield break;

            chip.SetActive(true);
            chip.transform.SetAsLastSibling();
            RectTransform rect = chip.GetComponent<RectTransform>();
            rect.anchoredPosition = from;

            float elapsed = 0f;
            while (elapsed < ChipMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / ChipMoveDuration);
                t = t * t * (3f - 2f * t);
                rect.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
                yield return null;
            }

            rect.anchoredPosition = to;
            if (hideAfter) chip.SetActive(false);
        }

        private IEnumerator SwitchToFiveChip(int previousBet)
        {
            isFiveChipDisplayed = true;

            int activeOneChipCount = Mathf.Clamp(previousBet - 1, 0, oneChipObjects.Length);
            for (int i = activeOneChipCount - 1; i >= 0; i--)
            {
                PlayChipReverse(i);
            }

            // 1Cookie 칩들이 모두 빠진 다음 5Cookie 칩을 표시합니다.
            yield return new WaitForSeconds(ChipMoveDuration);

            if (fiveChipObject != null)
            {
                yield return MoveChip(fiveChipObject, FiveChipStartPosition,
                    FiveChipBoardPosition, false);
            }
        }

        private IEnumerator AddFourthChipAndSwitchToFive(int previousBet)
        {
            // + 버튼으로 5Cookie가 될 때 마지막 1Cookie 칩까지 이동시킵니다.
            PlayChipForward(oneChipObjects.Length - 1);
            yield return new WaitForSeconds(ChipMoveDuration);
            yield return SwitchToFiveChip(previousBet + 1);
        }

        private IEnumerator SwitchFiveChipToFourCookies()
        {
            isFiveChipDisplayed = false;

            if (fiveChipObject != null)
            {
                yield return MoveChip(fiveChipObject, FiveChipBoardPosition,
                    FiveChipStartPosition, true);
            }

            // 5Cookie 칩이 빠진 뒤 1Cookie 칩 4개를 다시 표시합니다.
            for (int i = 0; i < oneChipObjects.Length; i++)
            {
                PlayChipForward(i);
            }
        }

        private void SelectTarget(int buttonIndex, int targetValue)
        {
            // ✅ 수정: 바로 색상 변경
            currentSelectedTarget = targetValue;

            // 즉시 버튼 색상 업데이트
            for (int i = 0; i < targetButtons.Count; i++)
            {
                if (targetButtons[i] == null) continue;

                // 버튼에 연결된 목표값 확인 (0번=1, 1번=20)
                int buttonTargetValue = (i == 0) ? 1 : 20;

                // 선택된 버튼은 파란색, 나머지는 흰색
                ColorBlock colors = targetButtons[i].colors;
                colors.normalColor = (buttonTargetValue == targetValue) ? selectedColor : normalColor;
                targetButtons[i].colors = colors;
            }

            // 이벤트 발행 (다른 시스템에 알림)
            GameEvents.InvokeTargetSelected(targetValue);
        }

        #endregion

        #region 로직 핸들러 (Event -> UI)

        private void UpdateScoreText(int playerScore, int aiScore)
        {
            playerScoreText.text = $"Player: ${playerScore}";
            aiScoreText.text = $"AI: ${aiScore}";
        }

        private void HandleRoundStarted()
        {
            // 기존 카드 오브젝트 모두 파괴
            foreach (var card in spawnedCards)
            {
                Destroy(card);
            }
            spawnedCards.Clear();

            // 패널 초기화
            resultPanel.SetActive(false);
            UpdateExpressionText("");

            // ✅ 수정: 초기 상태는 RoundController에서 설정
            // statusText는 OnStatusTextUpdated 이벤트로 업데이트됨

            // ✅ 추가: 제출 버튼 초기 비활성화
            submitButton.interactable = false;

            UpdateTimerText(0, 180);
        }

        private void HandleCardAdded(Card card, bool isPlayer)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("[GameView] CardButton 프리팹이 연결되지 않았습니다.");
                return;
            }

            // 1. 프리팹 생성
            Transform parent = isPlayer ? playerHandContainer : aiHandContainer;
            if (parent == null)
            {
                Debug.LogError($"[GameView] {(isPlayer ? "Player" : "AI")} 카드 컨테이너가 연결되지 않았습니다.");
                return;
            }

            CardView newCardView = Instantiate(cardPrefab, parent);
            RectTransform cardRect = newCardView.GetComponent<RectTransform>();
            cardRect.localScale = Vector3.one;
            cardRect.localRotation = Quaternion.identity;
            cardRect.sizeDelta = new Vector2(112f, 144f);
            cardRect.SetAsLastSibling();

            // 2. 프리팹 초기화 (데이터 주입)
            newCardView.Initialize(card, isPlayer);
            newCardView.gameObject.SetActive(true);

            // 3. 리스트에 추가하여 관리
            spawnedCards.Add(newCardView.gameObject);
            Canvas.ForceUpdateCanvases();
        }

        private void HandleRoundEnded(RoundResult result)
        {
            // 결과 패널 표시
            resultSummaryText.text = result.GetSummary();
            resultDetailText.text = result.GetDetail();
            resultPanel.SetActive(true);
        }

        private void UpdateExpressionText(string expressionText)
        {
            playerExpressionText.text = string.IsNullOrEmpty(expressionText) ? "..." : expressionText;
        }



        private void UpdateBetText(int newBet)
        {
            currentBetDisplay = newBet;
            betText.text = $"{currentBetDisplay}Cookie";

            // ✅ 추가: 최대값에 도달하면 버튼 비활성화
            if (currentBetDisplay >= 5)
            {
                betIncreaseButton.interactable = false;
                if (betSetFiveButton != null) betSetFiveButton.interactable = false;
            }
            else
            {
                betIncreaseButton.interactable = true;
                if (betSetFiveButton != null) betSetFiveButton.interactable = true;
            }

            // 최소값에 도달하면 감소 버튼 비활성화
            if (currentBetDisplay <= 1)
            {
                betDecreaseButton.interactable = false;
            }
            else
            {
                betDecreaseButton.interactable = true;
            }
        }

        private void UpdateTimerText(float currentTime, float maxTime)
        {
            float remainingTime = maxTime - currentTime;

            if (remainingTime < 0)
            {
                timerText.text = "00:00";
                timerText.color = Color.red;
            }
            else
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";

                // 30초 이하면 빨간색으로 경고
                if (remainingTime <= 30)
                {
                    timerText.color = Color.red;
                }
                else
                {
                    timerText.color = Color.white;
                }
            }
        }

        private void HandleGameOver(string winner)
        {
            statusText.text = $"게임 종료! 최종 승자: {winner}";
        }

        /// <summary>
        /// ✅ 수정: 제출 가능 여부 업데이트 (버튼만 제어)
        /// </summary>
        private void UpdateSubmitAvailability(bool canSubmit)
        {
            // 제출 버튼만 활성화/비활성화
            submitButton.interactable = canSubmit;
        }

        /// <summary>
        /// ✅ 추가: 상태 텍스트 업데이트
        /// </summary>
        private void UpdateStatusText(string message)
        {
            statusText.text = message;

            // 메시지별 색상 설정
            if (message.Contains("분배"))
            {
                statusText.color = Color.white;
            }
            else if (message.Contains("완성하세요"))
            {
                statusText.color = Color.cyan;
            }
            else if (message.Contains("제출"))
            {
                statusText.color = Color.green;
            }
            else if (message.Contains("결과"))
            {
                statusText.color = Color.yellow;
            }
            else
            {
                statusText.color = Color.white;
            }
        }

        #endregion
    }
}
