using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI_RobotSelectPanel 안에서 로봇 한 개를 표시하는 카드.
/// robotCardPrefab에 이 스크립트를 부착하세요.
/// UI_RobotItem의 표시 방식과 동일한 필드명 사용.
/// </summary>
public class UI_RobotSelectCard : MonoBehaviour
{
    [Header("표시 요소")]
    public Image portraitIcon;              // 로봇 초상화 (UI_RobotItem과 동일)
    public TextMeshProUGUI nameAndStarText; // "[알파] 3성" 형태
    public TextMeshProUGUI levelText;       // "Lv.5"
    public TextMeshProUGUI stageText;        // 알고리즘명 (IDA*, BFS 등)
    public TextMeshProUGUI escapesText;     // "탈출 123회"

    [Header("선택 강조")]
    public GameObject selectedHighlight;   // 선택됐을 때 켤 테두리/오버레이
    public Button cardButton;              // 카드 전체 영역 버튼

    // 외부에서 읽어가는 로봇 참조
    public DataManager.RobotInstance Robot { get; private set; }

    private Action<DataManager.RobotInstance> onSelected;

    // ─────────────────────────────────────────
    // 초기화 (UI_RobotSelectPanel.Refresh()에서 호출)
    // ─────────────────────────────────────────
    public void Setup(
        DataManager.RobotInstance robot,
        DataManager.RobotConfig config,
        Action<DataManager.RobotInstance> onSelectedCallback)
    {
        Robot = robot;
        onSelected = onSelectedCallback;

        // 초상화
        if (portraitIcon != null && config.portraitSprite != null)
            portraitIcon.sprite = config.portraitSprite;

        // 이름 + 성급 (UI_RobotItem과 동일 형식)
        if (nameAndStarText != null)
        {
            string starStr = robot.robotId == 8 ? "MAX" : $"{robot.star}성";
            nameAndStarText.text = $"[{config.name}] {starStr}";
        }

        // 레벨
        if (levelText != null)
            levelText.text = $"Lv.{robot.level}";

        // 스테이지 표시
        if (stageText != null)
        {
            string stageStr = "";
            for (int i = 0; i < 5; i++)
                stageStr += robot.stageClear[i] ? "★" : "☆";
            stageText.text = $"스테이지 {robot.currentStage}/5  {stageStr}";
        }



        // 버튼 연결
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => onSelected?.Invoke(Robot));
        }

        SetSelected(false);
    }

    // ─────────────────────────────────────────
    // 선택 강조 ON/OFF
    // ─────────────────────────────────────────
    public void SetSelected(bool isSelected)
    {
        if (selectedHighlight != null)
            selectedHighlight.SetActive(isSelected);
    }
}
