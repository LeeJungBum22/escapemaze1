using UnityEngine;
using TMPro;
using System.Text;

public class UI_StatsPanel : MonoBehaviour
{
    [Header("📊 글로벌 스탯 표시 (데이터매니저 최종값)")]
    public TextMeshProUGUI totalRobotCountText;
    public TextMeshProUGUI goldBonusText;
    public TextMeshProUGUI speedBonusText;
    public TextMeshProUGUI mazeRegenText;
    public TextMeshProUGUI critChanceText;
    public TextMeshProUGUI critDamageText;
    public TextMeshProUGUI diaChanceText;
    public TextMeshProUGUI diaAmountText;

    [Header("🤖 로봇별 상세 정보 (목록형)")]
    public TextMeshProUGUI robotDetailsText;

    void OnEnable() => RefreshStats();

    public void RefreshStats()
    {
        var dm = DataManager.Instance;
        if (dm == null) return;

        // --- 1. 글로벌 정보 표시 ---
        if (totalRobotCountText != null) totalRobotCountText.text = $"{dm.myRobots.Count} 대";
        if (goldBonusText != null) goldBonusText.text = $"+{(dm.GetGlobalGoldBonus() * 100):F1}%";
        if (speedBonusText != null) speedBonusText.text = $"+{(dm.GetGlobalSpeedBonus() * 100):F1}%";
        if (mazeRegenText != null) mazeRegenText.text = $"-{(dm.GetGlobalRegenBonus() * 100):F1}%";
        
        if (critChanceText != null) critChanceText.text = $"{(dm.GetGlobalCritChance() * 100):F1}%";
        if (critDamageText != null) critDamageText.text = $"{(dm.GetGlobalCritDamage() * 100):F0}%"; 
        
        if (diaChanceText != null) diaChanceText.text = $"{(dm.GetTotalDiaChance() * 100):F1}%";   
        if (diaAmountText != null) diaAmountText.text = $"{dm.GetTotalDiaAmount()} 개";             

        // --- 2. 로봇별 리스트 생성 ---
        StringBuilder sb = new StringBuilder();
        
        for (int i = 0; i < 9; i++)
        {
            int maxStar = 0;
            int maxLevel = 0;
            long totalEscapes = 0;
            bool hasRobot = false;

            foreach (var r in dm.myRobots)
            {
                if (r.robotId == i)
                {
                    hasRobot = true;
                    if (r.star > maxStar) maxStar = r.star;
                    if (r.level > maxLevel) maxLevel = r.level;
                    totalEscapes += r.mazeEscapeCount;
                }
            }

            if (hasRobot)
            {
                string rName = dm.robotConfigs[i].name;
                if (string.IsNullOrEmpty(rName)) rName = $"로봇_{i}";

                int labBuyCount = dm.GetLabUpgradeCount(i);

                // 🌟 핵심 수정: <pos=값em> 을 이용해 시작 위치를 픽스하고, 끝에 \n을 넣어 줄바꿈을 두 배로 늘림
                if (i == 8) 
                {
                    sb.AppendLine($"[{rName}]<pos=6em>Lv.{maxLevel}<pos=12em>| 연구:{labBuyCount}회<pos=19em>| 탈출:{totalEscapes:N0}회\n\n");
                }
                else 
                {
                    sb.AppendLine($"[{rName}]<pos=6em>{maxStar}성 Lv.{maxLevel}<pos=12em>| 연구:{labBuyCount}회<pos=19em>| 탈출:{totalEscapes:N0}회\n\n");
                }
            }
        }
        if (robotDetailsText != null) robotDetailsText.text = sb.ToString();
    }
}