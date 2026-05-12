using System.Collections.Generic;
using UnityEngine;

// 알고리즘 종류 (기존 코드와 호환)
public enum AlgorithmType 
{ 
    AStar, IDAStar, BreadthFirstSearch, BestFirstSearch, 
    Dijkstra, JumpPointSearch, OrthogonalJumpPointSearch, Trace 
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeData();
        }
        else Destroy(gameObject);
    }

    [Header("💰 재화")]
    public long gold = 0; 
    public int diamond = 0;

    [Header("🪙 글로벌 골드 업그레이드 (Gold Tab)")]
    public GlobalUpgrades goldUpgrades;

    [Header("💎 글로벌 다이아 업그레이드 (Diamond Tab)")]
    public GlobalUpgrades diaUpgrades;

    [Header("🤖 로봇 기본 설정 (로봇별 고유 스탯/가격)")]
    public RobotConfig[] robotConfigs = new RobotConfig[9];

    [Header("🧪 연구소 데이터 (로봇별 개별 업그레이드)")]
    public LabData[] labData = new LabData[9];

    [Header("🎒 내 로봇 인벤토리")]
    public List<RobotInstance> myRobots = new List<RobotInstance>();

    // ==========================================
    // 1. 데이터 구조체 정의
    // ==========================================

    [System.Serializable]
    public class UpgradeStat
    {
        public int level = 0;
        public int maxLevel = 100;
        
        [Header("수치 성장 (합연산 가중치)")]
        public float valuePerLevel = 0.01f; // 0.01 = 1%
        
        [Header("비용 성장 (지수 함수)")]
        public long baseCost = 100;
        public float costMultiplier = 1.15f; // 레벨당 비용 증가 배율

        public float CurrentValue => level * valuePerLevel;
        public long GetNextCost() => (long)(baseCost * Mathf.Pow(costMultiplier, level));
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
        
        [Space(10)]
        public UpgradeStat diaDropChance; // 골드 탭에서 사용: 다이아 획득 확률
        public UpgradeStat diaDropAmount; // 다이아 탭에서 사용: 다이아 획득량
    }

    [System.Serializable]
    public class RobotConfig
    {
        public string name;
        public AlgorithmType algo;
        public long purchasePrice;

        [Header("기본 스탯 및 레벨업 보상(상수)")]
        public long baseGoldReward = 100; 
        public long goldRewardPerLevel = 20; // 레벨업 시 오르는 고정 골드량
        public float baseMoveSpeed = 3.0f;
        public float baseSearchDelay = 0.05f;
        public float baseMazeRegenTime = 3.0f; 

        [Header("로봇 레벨업 비용 (지수)")]
        public long baseLevelUpCost = 50;
        public float levelUpCostMultiplier = 1.25f; 

        [Header("🌟 성(Star) 진화 가중치 (곱연산)")]
        public float starGoldMultiplier = 3.0f;  
        public float starSpeedMultiplier = 1.5f; 
        public float starDelayMultiplier = 0.7f; 
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
        public int star = 1; 
        public int level = 1; 
    }

    // ==========================================
    // 2. 초기 세팅 및 데이터 지급
    // ==========================================
    void InitializeData()
    {
        // 리스트가 비어있다면 초기 로봇 지급 (알파 & 오메가)
        if (myRobots.Count == 0)
        {
            myRobots.Add(new RobotInstance { robotId = 0, star = 1, level = 1 }); // 알파
            myRobots.Add(new RobotInstance { robotId = 8, star = 1, level = 1 }); // 오메가
        }
    }

    // ==========================================
    // 3. 핵심 경제 로직 (구매, 레벨업, 합성)
    // ==========================================

    public void BuyRobot(int robotId)
    {
        long price = robotConfigs[robotId].purchasePrice;
        if (gold >= price)
        {
            gold -= price;
            myRobots.Add(new RobotInstance { robotId = robotId, star = 1, level = 1 });
        }
    }

    public void LevelUpRobot(RobotInstance robot)
    {
        if (robot.level >= 10) return;

        // 비용 = 기본비용 * (비용배율 ^ 레벨-1) * (성급보정 2^성-1)
        double exponentialCost = robotConfigs[robot.robotId].baseLevelUpCost * Mathf.Pow(robotConfigs[robot.robotId].levelUpCostMultiplier, robot.level - 1);
        long finalCost = (long)(exponentialCost * Mathf.Pow(2.0f, robot.star - 1)); 

        if (gold >= finalCost)
        {
            gold -= finalCost;
            robot.level++;
        }
    }

    public void MergeRobots(RobotInstance r1, RobotInstance r2)
    {
        // 오메가는 합성 불가 예외처리
        if (r1.robotId == 8 || r2.robotId == 8) return;

        if (r1.robotId == r2.robotId && r1.star == r2.star && r1.level == 10 && r2.level == 10)
        {
            r1.star++;
            r1.level = 1;
            myRobots.Remove(r2);
            Debug.Log($"{robotConfigs[r1.robotId].name} {r1.star}성 합성 성공!");
        }
    }

    // ==========================================
    // 4. 최종 스탯 계산 (합연산 공식 적용)
    // ==========================================

    // 미로 재생성 쿨타임 계산
    public float GetFinalMazeRegenTime(RobotInstance robot)
    {
        float baseRegen = robotConfigs[robot.robotId].baseMazeRegenTime;
        // % 감소치 합산
        float reducePercent = goldUpgrades.mazeRegen.CurrentValue + diaUpgrades.mazeRegen.CurrentValue + labData[robot.robotId].mazeRegen.CurrentValue;
        return Mathf.Max(0.5f, baseRegen * (1.0f - Mathf.Min(reducePercent, 0.9f)));
    }

    // 이동 속도 계산
    public float GetFinalMoveSpeed(RobotInstance robot)
    {
        float baseSpeed = robotConfigs[robot.robotId].baseMoveSpeed * Mathf.Pow(robotConfigs[robot.robotId].starSpeedMultiplier, robot.star - 1);
        float bonusPercent = goldUpgrades.moveSpeed.CurrentValue + diaUpgrades.moveSpeed.CurrentValue + labData[robot.robotId].moveSpeed.CurrentValue;
        return baseSpeed * (1.0f + bonusPercent);
    }

    // 탐색 딜레이 계산
    public float GetFinalSearchDelay(RobotInstance robot)
    {
        float baseDelay = robotConfigs[robot.robotId].baseSearchDelay * Mathf.Pow(robotConfigs[robot.robotId].starDelayMultiplier, robot.star - 1);
        float reducePercent = goldUpgrades.searchDelay.CurrentValue + diaUpgrades.searchDelay.CurrentValue + labData[robot.robotId].searchDelay.CurrentValue;
        return Mathf.Max(0.001f, baseDelay * (1.0f - Mathf.Min(reducePercent, 0.9f)));
    }

    // 골드 보상 계산 (상수 증가 + 성급 배수 + 합연산 %)
    public long GetFinalGoldReward(RobotInstance robot)
    {
        var config = robotConfigs[robot.robotId];
        
        // 1. [상수] 기본보상 + (레벨업당 보상 * 레벨-1)
        long currentBaseGold = config.baseGoldReward + (config.goldRewardPerLevel * (robot.level - 1));
        
        // 2. [가중치] 성(Star)에 따른 뻥튀기
        currentBaseGold = (long)(currentBaseGold * Mathf.Pow(config.starGoldMultiplier, robot.star - 1));

        // 3. [합연산] 모든 골드 획득량 % 합산
        float bonusPercent = goldUpgrades.goldEarned.CurrentValue + diaUpgrades.goldEarned.CurrentValue + labData[robot.robotId].goldEarned.CurrentValue;
        long finalReward = (long)(currentBaseGold * (1.0f + bonusPercent));

        // 4. 크리티컬 계산 (합연산)
        float critChance = goldUpgrades.critChance.CurrentValue + diaUpgrades.critChance.CurrentValue + labData[robot.robotId].critChance.CurrentValue;
        if (Random.value < critChance)
        {
            float critMult = 1.5f + goldUpgrades.critDamage.CurrentValue + diaUpgrades.critDamage.CurrentValue + labData[robot.robotId].critDamage.CurrentValue;
            finalReward = (long)(finalReward * critMult);
        }
        return finalReward;
    }

    // 💎 다이아 드롭 체크 (확률-골드업글 / 획득량-다이아업글)
    public void CheckDiamondDrop()
    {
        float baseChance = 0.05f; // 기본 5%
        float finalChance = baseChance + goldUpgrades.diaDropChance.CurrentValue;

        if (Random.value < finalChance)
        {
            int baseAmount = 1;
            int finalAmount = baseAmount + (int)diaUpgrades.diaDropAmount.CurrentValue;
            AddDiamond(finalAmount);
            Debug.Log($"다이아 {finalAmount}개 획득!");
        }
    }

    // 전체 전투력 계산
    public long CalculateCombatPower()
    {
        long totalPower = 0;
        foreach (var robot in myRobots)
        {
            float sWeight = GetFinalMoveSpeed(robot) * 100f;
            float dWeight = (1.0f / GetFinalSearchDelay(robot)) * 50f;
            float gWeight = GetFinalGoldReward(robot) * 10f;
            totalPower += (long)(sWeight + dWeight + gWeight);
        }
        return totalPower;
    }

    public void AddGold(long amount) { gold += amount; }
    public void AddDiamond(int amount) { diamond += amount; }
}