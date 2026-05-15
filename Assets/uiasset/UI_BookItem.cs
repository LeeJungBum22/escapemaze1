using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_BookItem : MonoBehaviour
{
    [Header("설정")]
    public int robotId; // 0:알파 ~ 7:세타
    
    [Header("UI 연결 (보상 관련만 연결)")]
    public Button rewardButton;
    public TextMeshProUGUI rewardContentText; // "?성 해금 \n 보상 : 50"

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
        if (dm == null || robotId >= 8) return; 

        int nextStarToClaim = dm.claimedRewardStars[robotId] + 1;
        int currentMaxStar = dm.maxAchievedStars[robotId];

        // 🌟 보상을 받을 수 있는 상태
        if (nextStarToClaim <= currentMaxStar)
        {
            int baseReward = 50 + (robotId * 25);
            int rewardAmount = baseReward * nextStarToClaim;

            // 텍스트 보이기 및 갱신
            rewardContentText.gameObject.SetActive(true);
            rewardContentText.text = $"{nextStarToClaim}성 해금\n보상 : <color=#FF3333>{rewardAmount}</color>";
            
            // 버튼 활성화
            rewardButton.interactable = true;
        }
        else // 🌟 보상을 모두 받았거나 조건 미달인 상태
        {
            // 요청하신 대로 글자를 가리고 버튼 비활성화
            rewardContentText.gameObject.SetActive(false);
            rewardButton.interactable = false;
        }
    }

    public void OnClickReward()
    {
        // 1. 데이터 상에서 보상 수령 처리
        DataManager.Instance.ClaimBookReward(robotId);

        // 2. 🌟 중요: 수령 직후 즉시 UI 리셋 (버튼 비활성화 또는 다음 성급 보상 노출)
        RefreshUI();
    }
}