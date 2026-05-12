using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    [Header("💰 재화")]
    public long gold = 1000;
    public int diamond = 100;

    [Header("🪙 글로벌 골드 업그레이드")]
    public GlobalUpgrades goldUpgrades;

    [Header("💎 글로벌 다이아 업그레이드")]
    public GlobalUpgrades diaUpgrades;

    [Header("🤖 로봇 기본 설정 (가격, 스탯, 증가량 등)")]
    public RobotConfig[] robotConfigs = new RobotConfig[9];

    [Header("🧪 연구소 설정 및 데이터 (로봇별)")]
    public LabData[] labData = new LabData[9];

    [Header("🎒 내 로봇 인벤토리 (보유 중인 로봇들)")]
    public List<RobotInstance> myRobots = new List<RobotInstance>();

    // ==========================================
    // 1. 데이터 구조체 정의 (에디터에서 수정 가능)
    // ==========================================

    [System.Serializable]
    public class UpgradeStat
    {
        public int level = 0;
        public int maxLevel = 100;
        [Tooltip("레벨당 오르는 수치 (예: 0.01 = 1%)")] public float valuePerLevel = 0.01f;
        [Tooltip("업그레이드 기본 비용")] public long baseCost = 100;
        [Tooltip("레벨당 비용 증가 배율 (예: 1.1 = 10%씩 증가)")] public float costMultiplier = 1.1f;

        // 현재 수치 및 다음 레벨 비용 계산 (자동)
        public float CurrentValue => level * valuePerLevel;
        public long NextCost => (long)(baseCost * Mathf.Pow(costMultiplier, level));
        public bool CanUpgrade => level < maxLevel;
    }

    [System.Serializable]
    public class GlobalUpgrades
    {
        public UpgradeStat moveSpeed;
        public UpgradeStat searchDelay;
        public UpgradeStat mazeRegen;
        public UpgradeStat goldEarned;
        public UpgradeStat critChance;
        public UpgradeStat critDamage;
        public UpgradeStat diaDropChanceAmount; // 골드는 확률, 다이아는 드롭량으로 사용
    }

    [System.Serializable]
    public class RobotConfig
    {
        public string name;
        public AlgorithmType algo;
        public long purchasePrice; // 상점에서 살 때 가격

        [Header("상수 증가 수치")]
        public long baseGoldReward = 100; 
        public long goldRewardPerLevel = 15; // 레벨업 시 오르는 골드량 (상수)
        public float baseMoveSpeed = 3.0f;
        public float baseSearchDelay = 0.05f;

        [Header("비용 및 가중치")]
        public long baseLevelUpCost = 50;
        public float levelUpCostMultiplier = 1.2f; // 레벨당 비용 증가

        [Header("🌟 성(Star) 진화 스탯 배율")]
        [Tooltip("합성해서 성이 오를 때 곱해지는 획득 골드 가중치")] public float starGoldMultiplier = 2.5f;
        [Tooltip("성이 오를 때 속도 가중치")] public float starSpeedMultiplier = 1.5f;
        [Tooltip("성이 오를 때 딜레이 감소 가중치")] public float starDelayMultiplier = 0.8f;
    }

    [System.Serializable]
    public class LabData
    {
        public UpgradeStat critDamage;
        public UpgradeStat critChance;
        public UpgradeStat goldEarned;
        public UpgradeStat moveSpeed;
        public UpgradeStat searchDelay;
        public UpgradeStat mazeRegen;
        public bool isDiagonalUnlocked;
    }

    [System.Serializable]
    public class RobotInstance
    {
        public int robotId; // 0:알파 ~ 8:오메가
        public int star = 1; // 기본 1성
        public int level = 1; // 기본 1레벨 (최대 10)
    }

    // ==========================================
    // 2. 핵심 로직: 로봇 구매, 레벨업, 합성
    // ==========================================

    // 상점에서 로봇 구매
    public void BuyRobot(int robotId)
    {
        long price = robotConfigs[robotId].purchasePrice;
        if (gold >= price)
        {
            gold -= price;
            myRobots.Add(new RobotInstance { robotId = robotId, star = 1, level = 1 });
            Debug.Log($"{robotConfigs[robotId].name} 1성 1레벨 구매 완료!");
        }
    }

    // 인벤토리의 특정 로봇 레벨업
    public void LevelUpRobot(RobotInstance robot)
    {
        if (robot.level >= 10) return; // 10레벨 만렙

        long cost = (long)(robotConfigs[robot.robotId].baseLevelUpCost * Mathf.Pow(robotConfigs[robot.robotId].levelUpCostMultiplier, robot.level - 1));
        
        // 성(Star)이 높으면 레벨업 비용도 가중치(예: 성당 2배) 적용
        cost = (long)(cost * Mathf.Pow(2.0f, robot.star - 1)); 

        if (gold >= cost)
        {
            gold -= cost;
            robot.level++;
            Debug.Log($"레벨업! 현재 {robot.level}레벨");
        }
    }

    // 10레벨 로봇 2개 합성
    public void MergeRobots(RobotInstance r1, RobotInstance r2)
    {
        if (r1.robotId == r2.robotId && r1.star == r2.star && r1.level == 10 && r2.level == 10)
        {
            // r1을 다음 성(Star) 1레벨로 진화시키고, r2는 인벤토리에서 삭제
            r1.star++;
            r1.level = 1;
            myRobots.Remove(r2);
            Debug.Log($"합성 성공! {robotConfigs[r1.robotId].name} {r1.star}성 달성!");
        }
        else
        {
            Debug.LogWarning("합성 조건(동일 로봇, 동일 성, 둘 다 10레벨)이 맞지 않습니다.");
        }
    }

    // ==========================================
    // 3. 최종 스탯 계산 함수
    // ==========================================

    public float GetFinalMoveSpeed(RobotInstance robot)
    {
        var config = robotConfigs[robot.robotId];
        var lab = labData[robot.robotId];

        // 기본속도 * (성별 속도 가중치)
        float speed = config.baseMoveSpeed * Mathf.Pow(config.starSpeedMultiplier, robot.star - 1);
        
        // 업그레이드 합연산
        float bonusPercent = goldUpgrades.moveSpeed.CurrentValue + diaUpgrades.moveSpeed.CurrentValue + lab.moveSpeed.CurrentValue;
        return speed * (1.0f + bonusPercent);
    }

    public float GetFinalSearchDelay(RobotInstance robot)
    {
        var config = robotConfigs[robot.robotId];
        var lab = labData[robot.robotId];

        float delay = config.baseSearchDelay * Mathf.Pow(config.starDelayMultiplier, robot.star - 1);
        float reducePercent = goldUpgrades.searchDelay.CurrentValue + diaUpgrades.searchDelay.CurrentValue + lab.searchDelay.CurrentValue;
        
        return Mathf.Max(0.001f, delay * (1.0f - Mathf.Min(reducePercent, 0.9f)));
    }

    public long GetFinalGoldReward(RobotInstance robot)
    {
        var config = robotConfigs[robot.robotId];
        var lab = labData[robot.robotId];

        // 1. [상수] 기본 골드 + (레벨업당 오르는 상수 골드 * 레벨)
        long baseGold = config.baseGoldReward + (config.goldRewardPerLevel * (robot.level - 1));
        
        // 2. [가중치] 합성으로 성(Star)이 오를 때 뻥튀기
        baseGold = (long)(baseGold * Mathf.Pow(config.starGoldMultiplier, robot.star - 1));

        // 3. [합연산] 글로벌 업그레이드 및 연구소 % 보너스 적용
        float bonusPercent = goldUpgrades.goldEarned.CurrentValue + diaUpgrades.goldEarned.CurrentValue + lab.goldEarned.CurrentValue;
        long finalReward = (long)(baseGold * (1.0f + bonusPercent));

        // 4. 크리티컬 계산 (합연산)
        float critChance = goldUpgrades.critChance.CurrentValue + diaUpgrades.critChance.CurrentValue + lab.critChance.CurrentValue;
        if (Random.value < critChance)
        {
            float critMult = 1.5f + goldUpgrades.critDamage.CurrentValue + diaUpgrades.critDamage.CurrentValue + lab.critDamage.CurrentValue;
            finalReward = (long)(finalReward * critMult);
        }

        return finalReward;
    }
}