using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_BookItem : MonoBehaviour
{
    [Header("설정")]
    public int robotId; // 0:알파 ~ 7:세타, 8:오메가(강화학습)

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
        if (dm == null) return;

        // 🌟 오메가(8번): 강화학습 분기
        if (robotId == 8)
        {
            RefreshOmegaTrain(dm);
            return;
        }

        // ─── 기존 0~7번 도감 보상 시스템 ───
        if (robotId > 8) return;

        int nextStarToClaim = dm.claimedRewardStars[robotId] + 1;
        int currentMaxStar = dm.maxAchievedStars[robotId];

        // 🌟 보상을 받을 수 있는 상태
        if (nextStarToClaim <= currentMaxStar)
        {
            int baseReward = 50 + (robotId * 25);
            int rewardAmount = baseReward * nextStarToClaim;

            rewardContentText.gameObject.SetActive(true);
            rewardContentText.text = $"{nextStarToClaim}성 해금\n보상 : <color=#FF3333>{rewardAmount}</color>";

            rewardButton.interactable = true;
        }
        else // 🌟 보상을 모두 받았거나 조건 미달인 상태
        {
            rewardContentText.gameObject.SetActive(false);
            rewardButton.interactable = false;
        }
    }

    // 🌟 오메가 강화학습 UI 갱신
    void RefreshOmegaTrain(DataManager dm)
    {
        rewardContentText.gameObject.SetActive(true);

        if (dm.IsOmegaMaxTrained())
        {
            rewardContentText.text = $"강화학습 {dm.omegaTrainLevel}강\n<color=#FFD700>MAX</color>";
            rewardButton.interactable = false;
        }
        else
        {
            int cost = dm.GetOmegaTrainCost();
            rewardContentText.text = $"강화학습 {dm.omegaTrainLevel}강\n💎 <color=#FF3333>{cost}</color>";
            rewardButton.interactable = dm.diamond >= cost;
        }
    }

    public void OnClickReward()
    {
        var dm = DataManager.Instance;
        if (dm == null) return;

        // 🌟 오메가: 다이아 차감 + 강화학습 실행
        if (robotId == 8)
        {
            bool success = dm.TrainOmega();
            if (!success)
                Debug.Log("[BookItem] 오메가 강화 실패 (다이아 부족 또는 MAX)");
            RefreshUI();
            return;
        }

        // ─── 기존 도감 보상 수령 ───
        dm.ClaimBookReward(robotId);
        RefreshUI();
    }
}