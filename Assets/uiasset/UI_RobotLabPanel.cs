using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_RobotLabPanel : MonoBehaviour
{
    public int currentRobotId; // 현재 열린 로봇 ID

    [Header("📊 6종 스탯 텍스트 배열")]
    public TextMeshProUGUI[] levelTexts; 
    public TextMeshProUGUI[] valueTexts; 

    [Header("💎 뽑기(가챠) 버튼")]
    public Button pullButton;
    public TextMeshProUGUI pullCostText;

    [Header("🚀 대각선 해금 패널")]
    public TextMeshProUGUI diagonalStatusText; 
    public Button diagonalButton;
    public TextMeshProUGUI diagonalCostText;   

    private Coroutine[] flashCoroutines = new Coroutine[6];

    private void OnEnable()
    {
        // 🌟 DataManager의 신호기에 등록
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

    public void OpenLab(int robotId)
    {
        currentRobotId = robotId;
        RefreshUI();
        gameObject.SetActive(true);
    }

    public void RefreshUI()
    {
        if (DataManager.Instance == null) return;

        var dm = DataManager.Instance;
        var lab = dm.labData[currentRobotId];

        UpdateStatUI(0, "이동속도 증가량", lab.moveSpeed);
        UpdateStatUI(1, "탐색 딜레이 감소량", lab.searchDelay);
        UpdateStatUI(2, "미로 쿨타임 감소량", lab.mazeRegen);
        UpdateStatUI(3, "골드 획득 증가량", lab.goldEarned);
        UpdateStatUI(4, "치명타 확률 증가량", lab.critChance);
        UpdateStatUI(5, "치명타 데미지 증가량", lab.critDamage);

        if (lab.IsAllMax())
        {
            pullCostText.text = "MAX";
            pullButton.interactable = false;
        }
        else
        {
            int cost = lab.GetCurrentPullCost();
            pullCostText.text = cost.ToString("N0"); 
            pullButton.interactable = dm.diamond >= cost;
        }

        if (lab.isDiagonalUnlocked)
        {
            diagonalStatusText.text = "<color=#FFFF00>해금 O</color>";
            diagonalCostText.text = "MAX";
            diagonalButton.interactable = false;
        }
        else
        {
            diagonalStatusText.text = "해금 X";
            diagonalCostText.text = lab.diagonalUnlockCost.ToString("N0");
            diagonalButton.interactable = dm.diamond >= lab.diagonalUnlockCost;
        }
    }

    private void UpdateStatUI(int index, string prefix, DataManager.UpgradeStat stat)
    {
        if (levelTexts[index] != null)
            levelTexts[index].text = $"{stat.level} / {stat.maxLevel}";

        if (valueTexts[index] != null)
        {
            string curStr = $"+{(stat.CurrentValue * 100f):F1}%";
            if (stat.level >= stat.maxLevel)
            {
                valueTexts[index].text = $"{prefix} : {curStr} (MAX)";
            }
            else
            {
                string nextStr = $"+{((stat.level + 1) * stat.valuePerLevel * 100f):F1}%";
                valueTexts[index].text = $"{prefix} : {curStr} -> <color=#FF3333>{nextStr}</color>";
            }
        }
    }

    public void OnClickPull()
    {
        string resultName = DataManager.Instance.PullRandomResearch(currentRobotId);
        
        // 🌟 강제 새로고침 호출! 이벤트 지연 무시하고 누르자마자 즉시 갱신
        RefreshUI(); 

        if (!string.IsNullOrEmpty(resultName))
        {
            int targetIndex = -1;
            if (resultName == "이동속도 증가") targetIndex = 0;
            else if (resultName == "탐색 딜레이 감소") targetIndex = 1;
            else if (resultName == "미로 재생성 쿨타임 감소") targetIndex = 2;
            else if (resultName == "골드 획득량 증가") targetIndex = 3;
            else if (resultName == "치명타 확률 증가") targetIndex = 4;
            else if (resultName == "치명타 데미지 증가") targetIndex = 5;

            if (targetIndex != -1)
            {
                if (flashCoroutines[targetIndex] != null) StopCoroutine(flashCoroutines[targetIndex]);
                flashCoroutines[targetIndex] = StartCoroutine(FlashRoutine(valueTexts[targetIndex]));
            }
        }
    }

    public void OnClickDiagonal()
    {
        DataManager.Instance.UnlockDiagonalMove(currentRobotId);
        
        // 🌟 강제 새로고침 호출! 누르자마자 텍스트와 버튼 잠금 즉시 처리
        RefreshUI();
    }

    IEnumerator FlashRoutine(TextMeshProUGUI textTarget)
    {
        Color originalColor = Color.white; 
        Color flashColor = Color.yellow;   

        for (int i = 0; i < 3; i++)
        {
            textTarget.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            textTarget.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
        textTarget.color = originalColor;
    }
}