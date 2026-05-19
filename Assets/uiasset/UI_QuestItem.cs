using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_QuestItem : MonoBehaviour
{
    // 퀘스트 종류를 선택할 수 있는 메뉴
    public enum QuestType 
    { 
        Escape,          // 미로 탈출
        Purchase,        // 상점 구매
        GlobalUpgrade,   // 글로벌 업그레이드
        Merge,           // 로봇 합성
        DiamondDrop      // 다이아 드롭
    }

    [Header("퀘스트 설정")]
    public QuestType questType;
    public int robotId; // Escape(미로 탈출) 퀘스트일 때만 사용 (0:알파 ~ 7:세타)

    [Header("UI 연결")]
    public TextMeshProUGUI questNameText;  // "로봇 군단 양성" 등 직접 적어둔 이름
    public TextMeshProUGUI questDescText;  // "상점에서 로봇 N회 구매" 등 직접 적어둔 설명
    public TextMeshProUGUI progressText;   // 진행도 텍스트 (예: 1000/100)
    public Button rewardButton;            // 다이아 보상 버튼
    public TextMeshProUGUI rewardAmountText; // 보상 버튼 안의 "10" 텍스트

    private readonly int REWARD_AMOUNT = 10; // 모든 퀘스트의 보상은 10 다이아로 고정

    private void OnEnable()
    {
        // 실시간 업데이트 구독
        if (DataManager.Instance != null)
            DataManager.Instance.OnCurrencyChanged += RefreshUI;
        
        RefreshUI();
    }

    private void OnDisable()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnCurrencyChanged -= RefreshUI;
    }

    public void RefreshUI()
    {
        var dm = DataManager.Instance;
        if (dm == null) return;

        long currentProgress = 0;
        long targetProgress = 0;

        // 🌟 퀘스트 종류에 따라 목표치와 현재 진행도를 가져옵니다.
        switch (questType)
        {
            case QuestType.Escape:
                if (robotId >= 8) return; // 오메가 제외
                currentProgress = dm.GetTotalEscapeCount(robotId);
                // 1레벨(처음)이면 100, 2레벨이면 200...
                targetProgress = (dm.questLevel_Escape[robotId] + 1) * 100L; 
                break;

            case QuestType.Purchase:
                currentProgress = dm.GetTotalRobotPurchaseCount();
                targetProgress = (dm.questLevel_Purchase + 1) * 30L;
                break;

            case QuestType.GlobalUpgrade:
                currentProgress = dm.totalGlobalUpgradeCount;
                targetProgress = (dm.questLevel_GlobalUpgrade + 1) * 50L;
                break;

            case QuestType.Merge:
                currentProgress = dm.totalMergeCount;
                targetProgress = (dm.questLevel_Merge + 1) * 15L;
                break;

            case QuestType.DiamondDrop:
                currentProgress = dm.totalDiamondDropCount;
                targetProgress = (dm.questLevel_DiamondDrop + 1) * 100L;
                break;
        }

        // 텍스트 업데이트
        progressText.text = $"{currentProgress} / {targetProgress}";
        if (rewardAmountText != null) rewardAmountText.text = REWARD_AMOUNT.ToString();

        // 목표 달성 시 버튼 활성화
        rewardButton.interactable = currentProgress >= targetProgress;
    }

    // 보상버튼 On Click () 이벤트에 연결하세요!
    public void OnClickReward()
    {
        var dm = DataManager.Instance;
        
        // 1. 다이아 10개 지급
        dm.AddDiamond(REWARD_AMOUNT);

        // 2. 퀘스트 레벨(단계) 상승
        switch (questType)
        {
            case QuestType.Escape: dm.questLevel_Escape[robotId]++; break;
            case QuestType.Purchase: dm.questLevel_Purchase++; break;
            case QuestType.GlobalUpgrade: dm.questLevel_GlobalUpgrade++; break;
            case QuestType.Merge: dm.questLevel_Merge++; break;
            case QuestType.DiamondDrop: dm.questLevel_DiamondDrop++; break;
        }

        // 3. UI 즉시 갱신 (목표치가 다음 단계로 올라감)
        RefreshUI();
    }
}