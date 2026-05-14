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

        DataManager.UpgradeStat stat = isDiamondUpgrade 
            ? dm.GetDiaUpgradeStatById(upgradeId) 
            : dm.GetGoldUpgradeStatById(upgradeId);

        if (stat == null) return;

        // 🌟 핵심 방어 코드: 인스펙터 연결이 누락된 버튼이 하나라도 있으면 에러 뿜고 멈추는 걸 방지!
        if (levelText == null || statText == null || costText == null || upgradeButton == null) return;

        levelText.text = $"Lv.{stat.level}";

        bool isAmountStat = (isDiamondUpgrade && upgradeId == 6);

        string curStr = "";
        string nextStr = "";

        if (isAmountStat)
        {
            curStr = $"+{stat.CurrentValue:F0}개";
            nextStr = $"+{((stat.level + 1) * stat.valuePerLevel):F0}개";
        }
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
        FindFirstObjectByType<UI_RobotTab>()?.RefreshAll();
    }
}