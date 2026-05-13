using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_GoldUpgradeItem : MonoBehaviour
{
    [Header("설정")]
    public int upgradeId; // 0:이동속도 ~ 6:다이아확률/획득량
    public bool isDiamondUpgrade; 

    [Header("UI 연결")]
    public TextMeshProUGUI nameText;      
    public TextMeshProUGUI levelText;     
    public TextMeshProUGUI statText;      
    public Button upgradeButton;
    public TextMeshProUGUI costText;      

    private readonly string highlightColor = "#FF3333";

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        var dm = DataManager.Instance;
        if (dm == null) return;

        DataManager.UpgradeStat stat = isDiamondUpgrade 
            ? dm.GetDiaUpgradeStatById(upgradeId) 
            : dm.GetGoldUpgradeStatById(upgradeId);

        if (stat == null) return;

        levelText.text = $"Lv.{stat.level}";

        // 🌟 핵심 수정: 다이아 업그레이드 6번(획득 갯수)인지 확인
        bool isAmountStat = (isDiamondUpgrade && upgradeId == 6);

        string curStr = "";
        string nextStr = "";

        // 개수 스탯일 때는 100을 곱하지 않고 '개'를 붙임
        if (isAmountStat)
        {
            curStr = $"+{stat.CurrentValue:F0}개";
            nextStr = $"+{((stat.level + 1) * stat.valuePerLevel):F0}개";
        }
        // 확률/비율 스탯일 때는 100을 곱하고 '%'를 붙임
        else
        {
            curStr = $"+{(stat.CurrentValue * 100f):F1}%";
            nextStr = $"+{((stat.level + 1) * stat.valuePerLevel * 100f):F1}%";
        }

        if (!stat.CanUpgrade)
        {
            statText.text = $"{curStr} (MAX)";
            costText.text = "MAX";
            upgradeButton.interactable = false;
        }
        else
        {
            statText.text = $"{curStr} -> <color={highlightColor}>{nextStr}</color>";
            
            double cost = stat.GetNextCost();
            
            if (isDiamondUpgrade)
            {
                costText.text = cost.ToString("N0"); 
                upgradeButton.interactable = dm.diamond >= cost;
            }
            else
            {
                costText.text = CurrencyFormatter.FormatKorean(cost).Replace("\n", "");
                upgradeButton.interactable = dm.gold >= cost;
            }
        }
    }

    public void OnClickUpgrade()
    {
        if (isDiamondUpgrade)
        {
            DataManager.Instance.UpgradeGlobalDiamond(upgradeId);
        }
        else
        {
            DataManager.Instance.UpgradeGlobalGold(upgradeId);
        }
        
        RefreshUI();
        FindObjectOfType<UI_RobotTab>()?.RefreshAll();
        // 스탯창 갱신도 필요하다면 아래 줄을 활성화하세요
        // FindObjectOfType<UI_StatsPanel>()?.RefreshStats();
    }
}