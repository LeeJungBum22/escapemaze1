using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PurchaseItem : MonoBehaviour
{
    [Header("설정")]
    public int robotId; // 이 버튼이 어떤 로봇을 파는지 (0:알파 ~ 7:세타)
    
    [Header("UI 연결")]
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        var dm = DataManager.Instance;
        if (dm == null) return;

        // 1. 현재 가격 가져와서 표시 (CurrencyFormatter 적용)
        double currentPrice = dm.GetCurrentPurchasePrice(robotId);
        priceText.text = CurrencyFormatter.FormatKorean(currentPrice).Replace("\n", "");

        // 2. 돈이 부족하면 버튼 비활성화
        buyButton.interactable = dm.gold >= currentPrice;
    }

    public void OnClickBuy()
    {
        DataManager.Instance.BuyRobot(robotId);
        RefreshUI(); // 구매 즉시 이 버튼의 가격 상승 및 갱신
        
        // 🌟 내 로봇 리스트도 갱신해야 방금 산 로봇이 뜹니다!
        UI_RobotTab robotTab = FindObjectOfType<UI_RobotTab>();
        if (robotTab != null)
        {
            robotTab.RefreshAll();
        }
    }
}