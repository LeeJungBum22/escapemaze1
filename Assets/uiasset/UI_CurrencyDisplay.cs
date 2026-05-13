using UnityEngine;
using TMPro; // TextMeshPro를 사용한다고 가정합니다.

public class UI_CurrencyDisplay : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diamondText;

    private void Update()
    {
        if (DataManager.Instance == null) return;

        UpdateGoldDisplay();
        UpdateDiamondDisplay();
    }

    private void UpdateGoldDisplay()
    {
        if (goldText != null)
        {
            // 🌟 long 타입인 gold를 double로 변환해서 넘겨줌
            goldText.text = CurrencyFormatter.FormatKorean((double)DataManager.Instance.gold);
        }
    }

    private void UpdateDiamondDisplay()
    {
        if (diamondText != null)
        {
            // 다이아몬드도 동일하게 처리
            diamondText.text = CurrencyFormatter.FormatKorean((double)DataManager.Instance.diamond);
        }
    }
}