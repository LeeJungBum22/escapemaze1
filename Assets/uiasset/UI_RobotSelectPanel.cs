using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Button (2) → MenuButton → UIManager.ToggleMenu() 패턴으로 열리는 캐릭터 선택 창.
///
/// [씬 연결 방법]
/// 1. Button (2)에 MenuButton 컴포넌트 부착
///    - uiManager  = UIManager 오브젝트
///    - targetPanel = 이 스크립트가 붙은 패널 오브젝트
///    - xSprite    = 닫기 X 아이콘
/// 2. Button (2)의 OnClick() → MenuButton.OnClickThisButton()
/// 3. 이 패널 오브젝트에 UI_RobotSelectPanel 스크립트 부착 후 Inspector 슬롯 연결
/// </summary>
public class UI_RobotSelectPanel : MonoBehaviour
{
    [Header("카드 목록")]
    public GameObject robotCardPrefab;   // UI_RobotSelectCard 프리팹
    public Transform cardContainer;      // ScrollView > Viewport > Content

    [Header("하단 선택 정보")]
    public TextMeshProUGUI selectedInfoText; // 선택된 로봇 정보 한 줄 표시
    public Button confirmButton;             // 확인 버튼 (선택사항)

    // 현재 선택된 로봇
    private DataManager.RobotInstance selectedRobot = null;

    // 생성된 카드 목록
    private readonly List<UI_RobotSelectCard> spawnedCards = new();

    // ─────────────────────────────────────────
    // UIManager가 targetPanel.SetActive(true) 호출하면 자동 실행
    // ─────────────────────────────────────────
    void OnEnable()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnCurrencyChanged += Refresh;

        selectedRobot = null;
        Refresh();
        UpdateSelectedDisplay();
    }

    void OnDisable()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnCurrencyChanged -= Refresh;
    }

    // ─────────────────────────────────────────
    // 카드 목록 갱신
    // ─────────────────────────────────────────
    void Refresh()
    {
        foreach (var card in spawnedCards)
            if (card != null) Destroy(card.gameObject);
        spawnedCards.Clear();

        if (DataManager.Instance == null) return;

        // UI_RobotTab과 동일한 정렬 기준 사용
        var sortedList = DataManager.Instance.myRobots
            .GroupBy(r => r.robotId)
            .Select(g => g.OrderByDescending(r => r.star)
                          .ThenByDescending(r => r.level)
                          .First())
            .OrderBy(r => r.robotId)
            .ToList();

        foreach (var robot in sortedList)
        {
            GameObject go = Instantiate(robotCardPrefab, cardContainer);
            var card = go.GetComponent<UI_RobotSelectCard>();
            if (card == null) continue;

            var config = DataManager.Instance.robotConfigs[robot.robotId];
            card.Setup(robot, config, OnCardClicked);
            spawnedCards.Add(card);
        }
    }

    // ─────────────────────────────────────────
    // 카드 클릭 콜백
    // ─────────────────────────────────────────
    void OnCardClicked(DataManager.RobotInstance robot)
    {
        selectedRobot = robot;

        foreach (var card in spawnedCards)
            card.SetSelected(card.Robot == robot);

        UpdateSelectedDisplay();
    }

    // ─────────────────────────────────────────
    // 하단 선택 정보 갱신
    // ─────────────────────────────────────────
    void UpdateSelectedDisplay()
    {
        if (selectedInfoText == null) return;

        if (selectedRobot == null)
        {
            selectedInfoText.text = "로봇을 선택하세요";
            if (confirmButton != null) confirmButton.interactable = false;
            return;
        }

        var dm = DataManager.Instance;
        var config = dm.robotConfigs[selectedRobot.robotId];
        string starStr = selectedRobot.robotId == 8 ? "MAX" : $"{selectedRobot.star}성";

        selectedInfoText.text =
            $"[{config.name}]  {starStr}  Lv.{selectedRobot.level}" +
            $"  |  탈출 {selectedRobot.mazeEscapeCount}회";

        if (confirmButton != null) confirmButton.interactable = true;
    }

    // ─────────────────────────────────────────
    // 확인 버튼 Inspector OnClick()에 연결
    // ─────────────────────────────────────────
    public void OnClickConfirm()
    {
        if (selectedRobot == null) return;

        var stageMap = GetComponent<StageMap>();
        if (stageMap != null)
            stageMap.StartStage(selectedRobot);
    }
}
