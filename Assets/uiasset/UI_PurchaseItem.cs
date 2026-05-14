using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PurchaseItem : MonoBehaviour
{
    [Header("설정")]
    public int robotId; // 이 버튼이 어떤 로봇을 파는지 (0:알파 ~ 8:오메가)
    
    [Header("UI 연결")]
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private void OnEnable()
    {
        // 🌟 DataManager의 신호기(이벤트)에 등록하여 실시간 갱신 활성화
        if (DataManager.Instance != null)
            DataManager.Instance.OnCurrencyChanged += RefreshUI;
            
        RefreshUI();
    }

    // 🌟 추가됨: 창이 꺼질 때 메모리 누수 방지
    private void OnDisable()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnCurrencyChanged -= RefreshUI;
    }

    public void RefreshUI()
    {
        var dm = DataManager.Instance;
        if (dm == null) return;

        // 1. 현재 가격 가져와서 표시 (CurrencyFormatter 적용)
        double currentPrice = dm.GetCurrentPurchasePrice(robotId);
        priceText.text = CurrencyFormatter.FormatKorean(currentPrice).Replace("\n", "");

        // 2. 돈이 충분한지 실시간으로 판단하여 버튼 활성화/비활성화
        buyButton.interactable = dm.gold >= currentPrice;
    }

    public void OnClickBuy()
    {
        DataManager.Instance.BuyRobot(robotId);
        
        // 💡 이제 DataManager 내부에서 BuyRobot 실행 시 자동으로 NotifyCurrencyChanged()를 
        // 발송하므로, 여기서 RefreshUI()를 직접 호출하지 않아도 즉시 갱신됩니다!

        // 🌟 내 로봇 리스트도 갱신해야 방금 산 로봇이 뜹니다!
        UI_RobotTab robotTab = FindFirstObjectByType<UI_RobotTab>();
        if (robotTab != null)
        {
            robotTab.RefreshAll();
        }
    }
}