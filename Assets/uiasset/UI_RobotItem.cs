using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_RobotItem : MonoBehaviour
{
    private DataManager.RobotInstance myRobot;
    private UI_RobotTab parentTab;

    [Header("UI 연결")]
    public Image portraitIcon;                
    public TextMeshProUGUI portraitLevelText; 

    public TextMeshProUGUI nameAndStarText;   
    public TextMeshProUGUI goldRewardText;    
    public TextMeshProUGUI moveSpeedText;     
    public TextMeshProUGUI searchDelayText;   

    [Header("업그레이드 버튼 영역")]
    public Button upgradeButton;              
    public TextMeshProUGUI costText;          
    public TextMeshProUGUI buttonLabel;       

    // 강조용 밝은 빨간색 컬러 코드
    private readonly string highlightColor = "#FF3333"; 

    public void Setup(DataManager.RobotInstance robot, UI_RobotTab tab)
    {
        myRobot = robot;
        parentTab = tab;
        RefreshUI();
    }

    public void RefreshUI()
    {
        var dm = DataManager.Instance;
        int id = myRobot.robotId;
        var config = dm.robotConfigs[id];

        if (config.portraitSprite != null) portraitIcon.sprite = config.portraitSprite;
        portraitLevelText.text = $"Lv.{myRobot.level}";

        string starStr = id == 8 ? "MAX" : $"{myRobot.star}성";
        nameAndStarText.text = $"[{config.name}] {starStr}";

        double curGold = dm.GetPureBaseGold(id, myRobot.star, myRobot.level);
        float curSpeed = dm.GetPureBaseSpeed(id, myRobot.star);
        string curDelayStr = dm.GetPureBaseDelay(id, myRobot.star).ToString("F2"); 

        // -----------------------------------------------------
        // [모드 1] 레벨업 상태 (Lv.1 ~ 9)
        // -----------------------------------------------------
        if (myRobot.level < 10)
        {
            double nextGold = dm.GetPureBaseGold(id, myRobot.star, myRobot.level + 1);

            goldRewardText.text = $"골드획득량 : {FormatGold(curGold)} -> <color={highlightColor}>{FormatGold(nextGold)}</color>";
            moveSpeedText.text = $"이동속도 : {curSpeed}";
            searchDelayText.text = $"탐색속도 : {curDelayStr}";

            double cost = config.baseLevelUpCost * Mathf.Pow(config.levelUpCostMultiplier, myRobot.level - 1);
            
            buttonLabel.text = "레벨업";
            costText.text = CurrencyFormatter.FormatKorean(cost).Replace("\n", ""); 
            
            upgradeButton.interactable = dm.gold >= cost; 
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => OnLevelUpClick());
        }
        // -----------------------------------------------------
        // [모드 2] 합성 대기 상태 (Lv.10)
        // -----------------------------------------------------
        else
        {
            if (id == 8) // 오메가
            {
                goldRewardText.text = $"골드획득량 : {FormatGold(curGold)} (MAX)";
                moveSpeedText.text = $"이동속도 : {curSpeed} (MAX)";
                searchDelayText.text = $"탐색속도 : {curDelayStr} (MAX)";
                costText.text = "MAX";
                buttonLabel.text = ""; 
                upgradeButton.interactable = false;
            }
            else
            {
                int nextStar = myRobot.star + 1;
                double nextGold = dm.GetPureBaseGold(id, nextStar, 1); 
                float nextSpeed = dm.GetPureBaseSpeed(id, nextStar);
                string nextDelayStr = dm.GetPureBaseDelay(id, nextStar).ToString("F2");

                nameAndStarText.text = $"[{config.name}] {myRobot.star}성 -> <color={highlightColor}>{nextStar}성</color>";
                goldRewardText.text = $"골드획득량 : {FormatGold(curGold)} -> <color={highlightColor}>{FormatGold(nextGold)}</color>";
                moveSpeedText.text = $"이동속도 : {curSpeed} -> <color={highlightColor}>{nextSpeed}</color>";
                searchDelayText.text = $"탐색속도 : {curDelayStr} -> <color={highlightColor}>{nextDelayStr}</color>";

                buttonLabel.text = ""; 
                
                int mergeCost = dm.GetMergeCost(id, myRobot.star);
                // 🌟 "합성" 글자 제거, 조건과 가격만 남김!
                costText.text = $"{myRobot.star}성{config.name}Lv.10\n+\n<color={highlightColor}>{mergeCost}</color>";

                bool canMerge = HasMergePartner();
                if (canMerge && dm.diamond >= mergeCost)
                {
                    upgradeButton.interactable = true;
                    upgradeButton.onClick.RemoveAllListeners();
                    upgradeButton.onClick.AddListener(() => OnMergeClick());
                }
                else
                {
                    upgradeButton.interactable = false;
                }
            }
        }
    }

    private string FormatGold(double gold)
    {
        if (gold >= 10000) return CurrencyFormatter.FormatKorean(gold).Replace("\n", "");
        else return gold.ToString("F0");
    }

    private bool HasMergePartner()
    {
        int count = 0;
        foreach (var r in DataManager.Instance.myRobots)
        {
            if (r.robotId == myRobot.robotId && r.star == myRobot.star && r.level == 10) count++;
        }
        return count >= 2; 
    }

    private void OnLevelUpClick()
    {
        DataManager.Instance.LevelUpRobot(myRobot);
        parentTab.RefreshAll(); 
    }

    private void OnMergeClick()
    {
        DataManager.RobotInstance partner = null;
        foreach (var r in DataManager.Instance.myRobots)
        {
            if (r != myRobot && r.robotId == myRobot.robotId && r.star == myRobot.star && r.level == 10)
            {
                partner = r; break;
            }
        }

        if (partner != null)
        {
            int mergeCost = DataManager.Instance.GetMergeCost(myRobot.robotId, myRobot.star);
            DataManager.Instance.diamond -= mergeCost;
            DataManager.Instance.MergeRobots(myRobot, partner);
            parentTab.RefreshAll();
        }
    }
}