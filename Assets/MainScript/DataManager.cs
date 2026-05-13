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
    // 🌟 long에서 double로 변경: 해(10^20), 자(10^24) 이상의 단위를 감당하기 위함
    public double gold = 0; 
    public int diamond = 0;

    // 🌟 추가됨: 스탯창과 실제 게임에 동시 적용될 기본 밸런스 값들
    [Header("⚙️ 기본 밸런스 설정")]
    public float baseCritDamage = 1.5f;         // 기본 크리티컬 데미지 (150%)
    public float baseDiamondDropChance = 0.05f; // 기본 다이아 드롭 확률 (5%)
    public int baseDiamondDropAmount = 1;       // 기본 다이아 드롭 양 (1개)

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
    // 1. 데이터 구조체 정의 (원본 100% 유지)
    // ==========================================

    [System.Serializable]
    public class UpgradeStat
    {
        public int level = 0;
        public int maxLevel = 100;
        
        [Header("수치 성장 (합연산 가중치)")]
        public float valuePerLevel = 0.01f; // 0.01 = 1%
        
        [Header("비용 성장 (지수 함수)")]
        public double baseCost = 100;
        public float costMultiplier = 1.15f; // 레벨당 비용 증가 배율

        public float CurrentValue => level * valuePerLevel;
        public double GetNextCost() => baseCost * Mathf.Pow(costMultiplier, level);
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
        public Sprite portraitSprite;
        public AlgorithmType algo;
        public double purchasePrice;
        public float purchaseCostMultiplier = 1.5f; // 구매할 때마다 가격이 오를 배율 (1.5배 등)

        

        [Header("기본 스탯 및 레벨업 보상(상수)")]
        public double baseGoldReward = 100; 
        public double goldRewardPerLevel = 20; 
        public float baseMoveSpeed = 3.0f;
        public float baseSearchDelay = 0.05f;
        public float baseMazeRegenTime = 3.0f; 

        [Header("로봇 레벨업 비용 (지수)")]
        public double baseLevelUpCost = 50; 
        public float levelUpCostMultiplier = 1.25f; 

        // 🌟 여기가 새로 추가되는 부분입니다! 🌟
        [Header("💎 로봇 합성 비용 (다이아/지수)")]
        public int baseMergeDiamondCost = 50; // 1성 -> 2성 갈 때의 기본 다이아 비용
        public float mergeCostMultiplier = 2.0f; // 성급이 오를 때마다 곱해지는 배율

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
        public long mazeEscapeCount = 0; // 🌟 탈출 횟수 유지
    }

    // ==========================================
    // 🌟 추가됨: UI 스탯창과 실제 게임에 쓰일 공통 계산기 
    // ==========================================
    public float GetGlobalGoldBonus() => goldUpgrades.goldEarned.CurrentValue + diaUpgrades.goldEarned.CurrentValue;
    public float GetGlobalSpeedBonus() => goldUpgrades.moveSpeed.CurrentValue + diaUpgrades.moveSpeed.CurrentValue;
    public float GetGlobalSearchDelayBonus() => goldUpgrades.searchDelay.CurrentValue + diaUpgrades.searchDelay.CurrentValue;
    public float GetGlobalRegenBonus() => goldUpgrades.mazeRegen.CurrentValue + diaUpgrades.mazeRegen.CurrentValue;
    public float GetGlobalCritChance() => goldUpgrades.critChance.CurrentValue + diaUpgrades.critChance.CurrentValue;
    public float GetGlobalCritDamage() => baseCritDamage + goldUpgrades.critDamage.CurrentValue + diaUpgrades.critDamage.CurrentValue;
    public float GetTotalDiaChance() => baseDiamondDropChance + goldUpgrades.diaDropChance.CurrentValue;
    public int GetTotalDiaAmount() => baseDiamondDropAmount + (int)diaUpgrades.diaDropAmount.CurrentValue;

    public int GetLabUpgradeCount(int robotId)
    {
        var lab = labData[robotId];
        return lab.critDamage.level + lab.critChance.level + lab.goldEarned.level + 
               lab.moveSpeed.level + lab.searchDelay.level + lab.mazeRegen.level;
    }


    // ==========================================
    // 2. 초기 세팅 및 데이터 지급 (원본 유지)
    // ==========================================
    void InitializeData()
    {
        if (myRobots.Count == 0)
        {
            myRobots.Add(new RobotInstance { robotId = 0, star = 1, level = 1 }); // 알파
            myRobots.Add(new RobotInstance { robotId = 8, star = 1, level = 1 }); // 오메가
        }
    }

    // ==========================================
    // 3. 핵심 경제 로직 (구매, 레벨업, 합성) (원본 유지)
    // ==========================================
    public void UpgradeGlobalGold(int id)
    {
        UpgradeStat target = GetGoldUpgradeStatById(id);
        if (target == null || !target.CanUpgrade) return;

        double cost = target.GetNextCost();
        if (gold >= cost)
        {
            gold -= cost;
            target.level++;
            Debug.Log($"{id}번 골드 업그레이드 성공! 현재 레벨: {target.level}");
        }
    }
    
        // ==========================================
    // 🌟 [추가] 다이아 연구소 업그레이드 실행 함수
    // ==========================================
    public void UpgradeGlobalDiamond(int id)
    {
        // 다이아 탭용 매칭 함수 사용 (이미 작성되어 있던 것)
        UpgradeStat target = GetDiaUpgradeStatById(id);
        if (target == null || !target.CanUpgrade) return;

        double cost = target.GetNextCost();
        // 🌟 다이아몬드 잔액 체크
        if (diamond >= cost)
        {
            diamond -= (int)cost; // 다이아몬드는 int이므로 형변환
            target.level++;
            Debug.Log($"{id}번 다이아 업그레이드 성공!");
        }
    }

    // ID에 따라 어떤 스탯을 강화할지 매칭해주는 헬퍼 함수
    public UpgradeStat GetGoldUpgradeStatById(int id)
    {
        switch (id)
        {
            case 0: return goldUpgrades.moveSpeed;
            case 1: return goldUpgrades.searchDelay;
            case 2: return goldUpgrades.mazeRegen;
            case 3: return goldUpgrades.goldEarned;
            case 4: return goldUpgrades.critChance;
            case 5: return goldUpgrades.critDamage;
            case 6: return goldUpgrades.diaDropChance; // 골드 탭의 마지막은 다이아 확률
            default: return null;
        }
    }
    
    // 다이아 탭용 매칭 함수 (나중에 다이아 탭 만드실 때 사용)
    public UpgradeStat GetDiaUpgradeStatById(int id)
    {
        switch (id)
        {
            case 0: return diaUpgrades.moveSpeed;
            case 1: return diaUpgrades.searchDelay;
            case 2: return diaUpgrades.mazeRegen;
            case 3: return diaUpgrades.goldEarned;
            case 4: return diaUpgrades.critChance;
            case 5: return diaUpgrades.critDamage;
            case 6: return diaUpgrades.diaDropAmount; // 다이아 탭의 마지막은 다이아 획득량
            default: return null;
        }
    }

    public void BuyRobot(int robotId)
    {
        double price = GetCurrentPurchasePrice(robotId);
        if (gold >= price)
        {
            gold -= price;
            myRobots.Add(new RobotInstance { robotId = robotId, star = 1, level = 1 });
            robotPurchaseCounts[robotId]++; // 구매 횟수 증가!
        }
    }

    public void LevelUpRobot(RobotInstance robot)
    {
        if (robot.level >= 10) return;

        double exponentialCost = robotConfigs[robot.robotId].baseLevelUpCost * Mathf.Pow(robotConfigs[robot.robotId].levelUpCostMultiplier, robot.level - 1);
        double finalCost = exponentialCost * Mathf.Pow(2.0f, robot.star - 1); 

        if (gold >= finalCost)
        {
            gold -= finalCost;
            robot.level++;
        }
    }

    public void MergeRobots(RobotInstance r1, RobotInstance r2)
    {
        if (r1.robotId == 8 || r2.robotId == 8) return;

        if (r1.robotId == r2.robotId && r1.star == r2.star && r1.level == 10 && r2.level == 10)
        {
            r1.star++;
            r1.level = 1;
            myRobots.Remove(r2);
            Debug.Log($"{robotConfigs[r1.robotId].name} {r1.star}성 합성 성공!");
        }
    }
    [Header("📊 상점 데이터")]
    // 로봇 ID별로 구매 횟수를 저장 (0~7번 로봇까지)
    public int[] robotPurchaseCounts = new int[9];

    public double GetCurrentPurchasePrice(int robotId)
    {
        var config = robotConfigs[robotId];
        // 공식: 초기 가격 * (배율 ^ 구매 횟수)
        return config.purchasePrice * Mathf.Pow(config.purchaseCostMultiplier, robotPurchaseCounts[robotId]);
    }

    // ==========================================
    // 4. 최종 스탯 계산 (🌟 공통 계산기 함수를 적용하여 깔끔하게 수정됨)
    // ==========================================

    public float GetFinalMazeRegenTime(RobotInstance robot)
    {
        float baseRegen = robotConfigs[robot.robotId].baseMazeRegenTime;
        float reducePercent = GetGlobalRegenBonus() + labData[robot.robotId].mazeRegen.CurrentValue;
        return Mathf.Max(0.5f, baseRegen * (1.0f - Mathf.Min(reducePercent, 0.9f)));
    }

    // 🌟 다이아 합성 비용 계산 함수 추가
    public int GetMergeCost(int robotId, int currentStar)
    {
        var config = robotConfigs[robotId];
        // 비용 = 기본다이아 * (배율 ^ (현재성급-1)) -> 소수점은 반올림
        return Mathf.RoundToInt(config.baseMergeDiamondCost * Mathf.Pow(config.mergeCostMultiplier, currentStar - 1));
    }

    public float GetFinalMoveSpeed(RobotInstance robot)
    {
        float baseSpeed = robotConfigs[robot.robotId].baseMoveSpeed * Mathf.Pow(robotConfigs[robot.robotId].starSpeedMultiplier, robot.star - 1);
        float bonusPercent = GetGlobalSpeedBonus() + labData[robot.robotId].moveSpeed.CurrentValue;
        return baseSpeed * (1.0f + bonusPercent);
    }

    public float GetFinalSearchDelay(RobotInstance robot)
    {
        float baseDelay = robotConfigs[robot.robotId].baseSearchDelay * Mathf.Pow(robotConfigs[robot.robotId].starDelayMultiplier, robot.star - 1);
        float reducePercent = GetGlobalSearchDelayBonus() + labData[robot.robotId].searchDelay.CurrentValue;
        return Mathf.Max(0.001f, baseDelay * (1.0f - Mathf.Min(reducePercent, 0.9f)));
    }
    // ==========================================
    // 🌟 UI 표기용 순수 스탯 계산기 (글로벌/연구소 보너스 제외)
    // ==========================================
    public double GetPureBaseGold(int robotId, int star, int level)
    {
        var config = robotConfigs[robotId];
        double baseGold = config.baseGoldReward + (config.goldRewardPerLevel * (level - 1));
        return baseGold * Mathf.Pow(config.starGoldMultiplier, star - 1);
    }

    public float GetPureBaseSpeed(int robotId, int star)
    {
        var config = robotConfigs[robotId];
        return config.baseMoveSpeed * Mathf.Pow(config.starSpeedMultiplier, star - 1);
    }

    public float GetPureBaseDelay(int robotId, int star)
    {
        var config = robotConfigs[robotId];
        return config.baseSearchDelay * Mathf.Pow(config.starDelayMultiplier, star - 1);
    }

    public double GetFinalGoldReward(RobotInstance robot)
    {
        var config = robotConfigs[robot.robotId];
        
        // 1. [상수] 기본보상 + (레벨업당 보상 * 레벨-1)
        double currentBaseGold = config.baseGoldReward + (config.goldRewardPerLevel * (robot.level - 1));
        
        // 2. [가중치] 성(Star)에 따른 뻥튀기
        currentBaseGold = currentBaseGold * Mathf.Pow(config.starGoldMultiplier, robot.star - 1);

        // 3. [합연산] 글로벌 보너스와 연구소 보너스 합산 적용
        float bonusPercent = GetGlobalGoldBonus() + labData[robot.robotId].goldEarned.CurrentValue;
        double finalReward = currentBaseGold * (1.0f + bonusPercent);

        // 4. 크리티컬 계산 (글로벌 보너스와 연구소 보너스 합산 적용)
        float critChance = GetGlobalCritChance() + labData[robot.robotId].critChance.CurrentValue;
        if (Random.value < critChance)
        {
            float critMult = GetGlobalCritDamage() + labData[robot.robotId].critDamage.CurrentValue;
            finalReward = finalReward * critMult;
        }
        return finalReward;
    }

    public void CheckDiamondDrop()
    {
        float finalChance = GetTotalDiaChance();
        if (Random.value < finalChance)
        {
            int finalAmount = GetTotalDiaAmount();
            AddDiamond(finalAmount);
            Debug.Log($"다이아 {finalAmount}개 획득!");
        }
    }

    public double CalculateCombatPower()
    {
        double totalPower = 0;
        foreach (var robot in myRobots)
        {
            float sWeight = GetFinalMoveSpeed(robot) * 100f;
            float dWeight = (1.0f / GetFinalSearchDelay(robot)) * 50f;
            double gWeight = GetFinalGoldReward(robot) * 10f;
            totalPower += (sWeight + dWeight + gWeight);
        }
        return totalPower;
    }

    public void AddGold(double amount) { gold += amount; }
    public void AddDiamond(int amount) { diamond += amount; }
}