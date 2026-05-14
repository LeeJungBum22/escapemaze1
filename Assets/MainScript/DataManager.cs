using System; 
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum AlgorithmType 
{ 
    AStar, IDAStar, BreadthFirstSearch, BestFirstSearch, 
    Dijkstra, JumpPointSearch, OrthogonalJumpPointSearch, Trace, ReinforcementLearning 
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public event Action OnCurrencyChanged;

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
    public double gold = 0; 
    public int diamond = 0;

    [Header("⚙️ 기본 밸런스 설정")]
    public float baseCritDamage = 1.5f;         
    public float baseDiamondDropChance = 0.05f; 
    public int baseDiamondDropAmount = 1;       

    [Header("🪙 글로벌 골드 업그레이드 (Gold Tab)")]
    public GlobalUpgrades goldUpgrades = new GlobalUpgrades();

    [Header("💎 글로벌 다이아 업그레이드 (Diamond Tab)")]
    public GlobalUpgrades diaUpgrades = new GlobalUpgrades();

    [Header("🤖 로봇 기본 설정 (로봇별 고유 스탯/가격)")]
    public RobotConfig[] robotConfigs = new RobotConfig[9];

    [Header("🧪 연구소 데이터 (로봇별 개별 업그레이드)")]
    public LabData[] labData = new LabData[9];

    [Header("🎒 내 로봇 인벤토리")]
    public List<RobotInstance> myRobots = new List<RobotInstance>();

    [Header("📊 상점 데이터")]
    public int[] robotPurchaseCounts = new int[9];

    [System.Serializable]
    public class UpgradeStat
    {
        public int level = 0;
        public int maxLevel = 100;
        public float valuePerLevel = 0.01f; 
        public double baseCost = 100;
        public float costMultiplier = 1.15f; 

        public bool isLinearCost = false; 
        public double linearCostIncrement = 10; 

        public float CurrentValue => level * valuePerLevel;
        public double GetNextCost() => isLinearCost ? baseCost + (level * linearCostIncrement) : baseCost * Mathf.Pow(costMultiplier, level);
        public bool CanUpgrade => maxLevel == 0 || level < maxLevel; 
    }

    [System.Serializable]
    public class GlobalUpgrades
    {
        public UpgradeStat moveSpeed = new UpgradeStat();
        public UpgradeStat searchDelay = new UpgradeStat();
        public UpgradeStat mazeRegen = new UpgradeStat();
        public UpgradeStat goldEarned = new UpgradeStat();
        public UpgradeStat critChance = new UpgradeStat();
        public UpgradeStat critDamage = new UpgradeStat();
        [Space(10)]
        public UpgradeStat diaDropChance = new UpgradeStat(); 
        public UpgradeStat diaDropAmount = new UpgradeStat(); 
    }

    [System.Serializable]
    public class RobotConfig
    {
        public string name = "미정";
        public Sprite portraitSprite;
        public AlgorithmType algo = AlgorithmType.AStar;
        public double purchasePrice = 1000;
        
        // 🌟 수정됨: 구매 배율은 1.1 유지
        public float purchaseCostMultiplier = 1.1f; 

        public double baseGoldReward = 10; 
        public double goldRewardPerLevel = 5; 
        public float baseMoveSpeed = 3.0f;
        public float baseSearchDelay = 0.05f;
        public float baseMazeRegenTime = 3.0f; 

        public double baseLevelUpCost = 50; 
        
        // 🌟 수정됨: 레벨업 배율은 1.05로 대폭 완화
        public float levelUpCostMultiplier = 1.05f; 

        public int baseMergeDiamondCost = 20; 
        public float mergeCostMultiplier = 2.5f; 

        public float starGoldMultiplier = 3.0f;  
        public float starSpeedMultiplier = 1.2f; 
        public float starDelayMultiplier = 0.8f; 
    }

    [System.Serializable]
    public class LabData
    {
        public UpgradeStat critDamage = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.1f };   
        public UpgradeStat critChance = new UpgradeStat { maxLevel = 20, valuePerLevel = 0.01f };  
        public UpgradeStat goldEarned = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.12f };  
        public UpgradeStat moveSpeed = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.02f };   
        public UpgradeStat searchDelay = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.02f }; 
        public UpgradeStat mazeRegen = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.01f };   
        
        public bool isDiagonalUnlocked = false;
        public int diagonalUnlockCost = 5000;

        public int pullCount = 0; 
        public int basePullCost = 50; 
        public int pullCostIncrement = 50; 

        public int GetCurrentPullCost() => basePullCost + (pullCount * pullCostIncrement);

        public bool IsAllMax()
        {
            return moveSpeed.level >= moveSpeed.maxLevel &&
                   searchDelay.level >= searchDelay.maxLevel &&
                   mazeRegen.level >= mazeRegen.maxLevel &&
                   goldEarned.level >= goldEarned.maxLevel &&
                   critChance.level >= critChance.maxLevel &&
                   critDamage.level >= critDamage.maxLevel;
        }
    }

    [System.Serializable]
    public class RobotInstance
    {
        public int robotId; 
        public int star = 1; 
        public int level = 1; 
        public long mazeEscapeCount = 0; 
    }

    [ContextMenu("Apply Starter Balance (초기 밸런스 덮어쓰기)")]
    public void ApplyStarterBalance()
    {
        goldUpgrades.moveSpeed = new UpgradeStat { baseCost = 50, costMultiplier = 1.3f, valuePerLevel = 0.01f, maxLevel = 50 };
        goldUpgrades.searchDelay = new UpgradeStat { baseCost = 50, costMultiplier = 1.3f, valuePerLevel = 0.01f, maxLevel = 50 };
        goldUpgrades.mazeRegen = new UpgradeStat { baseCost = 50, costMultiplier = 1.3f, valuePerLevel = 0.002f, maxLevel = 150 };
        goldUpgrades.goldEarned = new UpgradeStat { baseCost = 100, costMultiplier = 1.25f, valuePerLevel = 0.1505f, maxLevel = 300 }; 
        goldUpgrades.critChance = new UpgradeStat { baseCost = 250, costMultiplier = 1.4f, valuePerLevel = 0.002f, maxLevel = 175 };
        goldUpgrades.critDamage = new UpgradeStat { baseCost = 250, costMultiplier = 1.4f, valuePerLevel = 0.004f, maxLevel = 500 }; 
        goldUpgrades.diaDropChance = new UpgradeStat { baseCost = 2500, costMultiplier = 1.5f, valuePerLevel = 0.01f, maxLevel = 20 };

        diaUpgrades.moveSpeed = new UpgradeStat { isLinearCost = true, baseCost = 10, linearCostIncrement = 10, valuePerLevel = 0.01f, maxLevel = 50 };
        diaUpgrades.searchDelay = new UpgradeStat { isLinearCost = true, baseCost = 10, linearCostIncrement = 10, valuePerLevel = 0.01f, maxLevel = 50 };
        diaUpgrades.mazeRegen = new UpgradeStat { isLinearCost = true, baseCost = 10, linearCostIncrement = 10, valuePerLevel = 0.002f, maxLevel = 150 };
        diaUpgrades.goldEarned = new UpgradeStat { isLinearCost = true, baseCost = 10, linearCostIncrement = 10, valuePerLevel = 0.1505f, maxLevel = 300 };
        diaUpgrades.critChance = new UpgradeStat { isLinearCost = true, baseCost = 10, linearCostIncrement = 10, valuePerLevel = 0.002f, maxLevel = 175 };
        diaUpgrades.critDamage = new UpgradeStat { isLinearCost = true, baseCost = 10, linearCostIncrement = 10, valuePerLevel = 0.004f, maxLevel = 500 };
        diaUpgrades.diaDropAmount = new UpgradeStat { isLinearCost = true, baseCost = 10, linearCostIncrement = 10, valuePerLevel = 1f, maxLevel = 20 };

        for(int i = 0; i < 9; i++) {
            if (robotConfigs[i] == null) robotConfigs[i] = new RobotConfig();
            if (labData[i] == null) labData[i] = new LabData();
            
            labData[i].basePullCost = 50 * (i + 1);
            labData[i].pullCostIncrement = 50 * (i + 1);
            labData[i].diagonalUnlockCost = 5000 * (i + 1);

            labData[i].moveSpeed = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.02f };
            labData[i].searchDelay = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.02f };
            labData[i].mazeRegen = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.01f };
            labData[i].goldEarned = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.12f };
            labData[i].critChance = new UpgradeStat { maxLevel = 20, valuePerLevel = 0.01f };
            labData[i].critDamage = new UpgradeStat { maxLevel = 25, valuePerLevel = 0.1f };

            // 🌟 수정됨: 구매 배율은 1.1로 롤백, 레벨업 배율은 1.05로 수정!
            robotConfigs[i].purchaseCostMultiplier = 1.1f; 
            robotConfigs[i].levelUpCostMultiplier = 1.05f; 
            
            robotConfigs[i].starGoldMultiplier = 3.0f;
            robotConfigs[i].starSpeedMultiplier = 1.2f;
            robotConfigs[i].starDelayMultiplier = 0.8f;
            robotConfigs[i].mergeCostMultiplier = 2.5f;
        }

        robotConfigs[0].name = "알파"; robotConfigs[0].algo = AlgorithmType.Dijkstra; robotConfigs[0].purchasePrice = 100; robotConfigs[0].baseGoldReward = 10; robotConfigs[0].goldRewardPerLevel = 2; robotConfigs[0].baseLevelUpCost = 50; robotConfigs[0].baseMergeDiamondCost = 20;
        robotConfigs[1].name = "베타"; robotConfigs[1].algo = AlgorithmType.BreadthFirstSearch; robotConfigs[1].purchasePrice = 1000; robotConfigs[1].baseGoldReward = 40; robotConfigs[1].goldRewardPerLevel = 10; robotConfigs[1].baseLevelUpCost = 200; robotConfigs[1].baseMergeDiamondCost = 30;
        robotConfigs[2].name = "감마"; robotConfigs[2].algo = AlgorithmType.IDAStar; robotConfigs[2].purchasePrice = 10000; robotConfigs[2].baseGoldReward = 150; robotConfigs[2].goldRewardPerLevel = 40; robotConfigs[2].baseLevelUpCost = 800; robotConfigs[2].baseMergeDiamondCost = 40;
        robotConfigs[3].name = "델타"; robotConfigs[3].algo = AlgorithmType.BestFirstSearch; robotConfigs[3].purchasePrice = 100000; robotConfigs[3].baseGoldReward = 600; robotConfigs[3].goldRewardPerLevel = 150; robotConfigs[3].baseLevelUpCost = 3200; robotConfigs[3].baseMergeDiamondCost = 50;
        robotConfigs[4].name = "엡실론"; robotConfigs[4].algo = AlgorithmType.Trace; robotConfigs[4].purchasePrice = 1000000; robotConfigs[4].baseGoldReward = 2500; robotConfigs[4].goldRewardPerLevel = 600; robotConfigs[4].baseLevelUpCost = 12800; robotConfigs[4].baseMergeDiamondCost = 60;
        robotConfigs[5].name = "제타"; robotConfigs[5].algo = AlgorithmType.AStar; robotConfigs[5].purchasePrice = 10000000; robotConfigs[5].baseGoldReward = 10000; robotConfigs[5].goldRewardPerLevel = 2500; robotConfigs[5].baseLevelUpCost = 50000; robotConfigs[5].baseMergeDiamondCost = 70;
        robotConfigs[6].name = "에타"; robotConfigs[6].algo = AlgorithmType.JumpPointSearch; robotConfigs[6].purchasePrice = 100000000; robotConfigs[6].baseGoldReward = 45000; robotConfigs[6].goldRewardPerLevel = 12000; robotConfigs[6].baseLevelUpCost = 200000; robotConfigs[6].baseMergeDiamondCost = 80;
        robotConfigs[7].name = "세타"; robotConfigs[7].algo = AlgorithmType.OrthogonalJumpPointSearch; robotConfigs[7].purchasePrice = 1000000000; robotConfigs[7].baseGoldReward = 200000; robotConfigs[7].goldRewardPerLevel = 50000; robotConfigs[7].baseLevelUpCost = 800000; robotConfigs[7].baseMergeDiamondCost = 90;
        robotConfigs[8].name = "오메가"; robotConfigs[8].algo = AlgorithmType.ReinforcementLearning; robotConfigs[8].purchasePrice = 100000000000; robotConfigs[8].baseGoldReward = 1000000; robotConfigs[8].goldRewardPerLevel = 250000; robotConfigs[8].baseLevelUpCost = 5000000; robotConfigs[8].baseMergeDiamondCost = 100;
    }

    void InitializeData()
    {
        if (robotConfigs[0] == null || string.IsNullOrEmpty(robotConfigs[0].name) || robotConfigs[0].name == "미정")
        {
            ApplyStarterBalance();
        }

        if (myRobots.Count == 0)
        {
            myRobots.Add(new RobotInstance { robotId = 0, star = 1, level = 1 });
            myRobots.Add(new RobotInstance { robotId = 8, star = 1, level = 1 });
        }
    }

    private void NotifyCurrencyChanged()
    {
        OnCurrencyChanged?.Invoke();
    }

    public void AddGold(double amount) 
    { 
        gold += amount; 
        NotifyCurrencyChanged(); 
    }
    
    public void AddDiamond(int amount) 
    { 
        diamond += amount; 
        NotifyCurrencyChanged(); 
    }

    public void UpgradeGlobalGold(int id)
    {
        UpgradeStat target = GetGoldUpgradeStatById(id);
        if (target == null || !target.CanUpgrade) return;

        double cost = target.GetNextCost();
        if (gold >= cost) 
        { 
            gold -= cost; 
            target.level++; 
            NotifyCurrencyChanged(); 
        }
    }
    
    public void UpgradeGlobalDiamond(int id)
    {
        UpgradeStat target = GetDiaUpgradeStatById(id);
        if (target == null || !target.CanUpgrade) return;

        double cost = target.GetNextCost();
        if (diamond >= cost) 
        { 
            diamond -= (int)cost; 
            target.level++; 
            NotifyCurrencyChanged(); 
        }
    }

    public string PullRandomResearch(int robotId)
    {
        LabData lab = labData[robotId];
        int cost = lab.GetCurrentPullCost();

        if (diamond >= cost && !lab.IsAllMax())
        {
            diamond -= cost;
            lab.pullCount++;

            List<UpgradeStat> availableStats = new List<UpgradeStat>();
            List<string> availableNames = new List<string>();

            if (lab.moveSpeed.CanUpgrade) { availableStats.Add(lab.moveSpeed); availableNames.Add("이동속도 증가"); }
            if (lab.searchDelay.CanUpgrade) { availableStats.Add(lab.searchDelay); availableNames.Add("탐색 딜레이 감소"); }
            if (lab.mazeRegen.CanUpgrade) { availableStats.Add(lab.mazeRegen); availableNames.Add("미로 재생성 쿨타임 감소"); }
            if (lab.goldEarned.CanUpgrade) { availableStats.Add(lab.goldEarned); availableNames.Add("골드 획득량 증가"); }
            if (lab.critChance.CanUpgrade) { availableStats.Add(lab.critChance); availableNames.Add("치명타 확률 증가"); }
            if (lab.critDamage.CanUpgrade) { availableStats.Add(lab.critDamage); availableNames.Add("치명타 데미지 증가"); }

            if (availableStats.Count > 0)
            {
                int randomIndex = Random.Range(0, availableStats.Count);
                availableStats[randomIndex].level++; 
                NotifyCurrencyChanged(); 
                return availableNames[randomIndex]; 
            }
        }
        return null;
    }

    public void UnlockDiagonalMove(int robotId)
    {
        LabData lab = labData[robotId];
        if (diamond >= lab.diagonalUnlockCost && !lab.isDiagonalUnlocked)
        {
            diamond -= lab.diagonalUnlockCost;
            lab.isDiagonalUnlocked = true;
            NotifyCurrencyChanged();
        }
    }

    public UpgradeStat GetGoldUpgradeStatById(int id) { switch (id) { case 0: return goldUpgrades.moveSpeed; case 1: return goldUpgrades.searchDelay; case 2: return goldUpgrades.mazeRegen; case 3: return goldUpgrades.goldEarned; case 4: return goldUpgrades.critChance; case 5: return goldUpgrades.critDamage; case 6: return goldUpgrades.diaDropChance; default: return null; } }
    public UpgradeStat GetDiaUpgradeStatById(int id) { switch (id) { case 0: return diaUpgrades.moveSpeed; case 1: return diaUpgrades.searchDelay; case 2: return diaUpgrades.mazeRegen; case 3: return diaUpgrades.goldEarned; case 4: return diaUpgrades.critChance; case 5: return diaUpgrades.critDamage; case 6: return diaUpgrades.diaDropAmount; default: return null; } }

    public void BuyRobot(int robotId) 
    { 
        double price = GetCurrentPurchasePrice(robotId); 
        if (gold >= price) 
        { 
            gold -= price; 
            myRobots.Add(new RobotInstance { robotId = robotId, star = 1, level = 1 }); 
            robotPurchaseCounts[robotId]++; 
            NotifyCurrencyChanged();
        } 
    }

    // 🌟 핵심 로직: 성급이 증가할수록 비용 10% 상승, 레벨업 배율은 +0.005씩 증가!
    public double GetLevelUpCost(int robotId, int star, int level)
    {
        var config = robotConfigs[robotId];
        
        double currentBaseCost = config.baseLevelUpCost * Mathf.Pow(1.1f, star - 1);
        float currentMultiplier = config.levelUpCostMultiplier + ((star - 1) * 0.005f);

        return currentBaseCost * Mathf.Pow(currentMultiplier, level - 1);
    }

    public void LevelUpRobot(RobotInstance robot) 
    { 
        if (robot.level >= 10) return; 
        
        double finalCost = GetLevelUpCost(robot.robotId, robot.star, robot.level); 
        
        if (gold >= finalCost) 
        { 
            gold -= finalCost; 
            robot.level++; 
            NotifyCurrencyChanged();
        } 
    }
    
    public void MergeRobots(RobotInstance r1, RobotInstance r2) { if (r1.robotId == 8 || r2.robotId == 8) return; if (r1.robotId == r2.robotId && r1.star == r2.star && r1.level == 10 && r2.level == 10) { r1.star++; r1.level = 1; myRobots.Remove(r2); } }

    public double GetCurrentPurchasePrice(int robotId) { var config = robotConfigs[robotId]; return config.purchasePrice * Mathf.Pow(config.purchaseCostMultiplier, robotPurchaseCounts[robotId]); }

    public float GetGlobalGoldBonus() => goldUpgrades.goldEarned.CurrentValue + diaUpgrades.goldEarned.CurrentValue;
    public float GetGlobalSpeedBonus() => goldUpgrades.moveSpeed.CurrentValue + diaUpgrades.moveSpeed.CurrentValue;
    public float GetGlobalSearchDelayBonus() => goldUpgrades.searchDelay.CurrentValue + diaUpgrades.searchDelay.CurrentValue;
    public float GetGlobalRegenBonus() => goldUpgrades.mazeRegen.CurrentValue + diaUpgrades.mazeRegen.CurrentValue;
    public float GetGlobalCritChance() => goldUpgrades.critChance.CurrentValue + diaUpgrades.critChance.CurrentValue;
    public float GetGlobalCritDamage() => baseCritDamage + goldUpgrades.critDamage.CurrentValue + diaUpgrades.critDamage.CurrentValue;
    public float GetTotalDiaChance() => baseDiamondDropChance + goldUpgrades.diaDropChance.CurrentValue;
    public int GetTotalDiaAmount() => baseDiamondDropAmount + (int)diaUpgrades.diaDropAmount.CurrentValue;

    public int GetLabUpgradeCount(int robotId) { var lab = labData[robotId]; return lab.critDamage.level + lab.critChance.level + lab.goldEarned.level + lab.moveSpeed.level + lab.searchDelay.level + lab.mazeRegen.level; }
    public float GetFinalMazeRegenTime(RobotInstance robot) { float baseRegen = robotConfigs[robot.robotId].baseMazeRegenTime; float reducePercent = GetGlobalRegenBonus() + labData[robot.robotId].mazeRegen.CurrentValue; return Mathf.Max(0.5f, baseRegen * (1.0f - Mathf.Min(reducePercent, 0.9f))); }
    public int GetMergeCost(int robotId, int currentStar) { var config = robotConfigs[robotId]; return Mathf.RoundToInt(config.baseMergeDiamondCost * Mathf.Pow(config.mergeCostMultiplier, currentStar - 1)); }
    public float GetFinalMoveSpeed(RobotInstance robot) { float baseSpeed = robotConfigs[robot.robotId].baseMoveSpeed * Mathf.Pow(robotConfigs[robot.robotId].starSpeedMultiplier, robot.star - 1); float bonusPercent = GetGlobalSpeedBonus() + labData[robot.robotId].moveSpeed.CurrentValue; return baseSpeed * (1.0f + bonusPercent); }
    public float GetFinalSearchDelay(RobotInstance robot) { float baseDelay = robotConfigs[robot.robotId].baseSearchDelay * Mathf.Pow(robotConfigs[robot.robotId].starDelayMultiplier, robot.star - 1); float reducePercent = GetGlobalSearchDelayBonus() + labData[robot.robotId].searchDelay.CurrentValue; return Mathf.Max(0.001f, baseDelay * (1.0f - Mathf.Min(reducePercent, 0.9f))); }
    public double GetPureBaseGold(int robotId, int star, int level) { var config = robotConfigs[robotId]; double baseGold = config.baseGoldReward + (config.goldRewardPerLevel * (level - 1)); return baseGold * Mathf.Pow(config.starGoldMultiplier, star - 1); }
    public float GetPureBaseSpeed(int robotId, int star) { var config = robotConfigs[robotId]; return config.baseMoveSpeed * Mathf.Pow(config.starSpeedMultiplier, star - 1); }
    public float GetPureBaseDelay(int robotId, int star) { var config = robotConfigs[robotId]; return config.baseSearchDelay * Mathf.Pow(config.starDelayMultiplier, star - 1); }

    public double GetFinalGoldReward(RobotInstance robot, out bool isCrit)
    {
        var config = robotConfigs[robot.robotId];
        double currentBaseGold = config.baseGoldReward + (config.goldRewardPerLevel * (robot.level - 1));
        currentBaseGold = currentBaseGold * Mathf.Pow(config.starGoldMultiplier, robot.star - 1);
        float bonusPercent = GetGlobalGoldBonus() + labData[robot.robotId].goldEarned.CurrentValue;
        double finalReward = currentBaseGold * (1.0f + bonusPercent);

        float critChance = GetGlobalCritChance() + labData[robot.robotId].critChance.CurrentValue;
        isCrit = (UnityEngine.Random.value < critChance);
        if (isCrit) { float critMult = GetGlobalCritDamage() + labData[robot.robotId].critDamage.CurrentValue; finalReward = finalReward * critMult; }
        return finalReward;
    }

    public int CheckDiamondDropAmount() { float finalChance = GetTotalDiaChance(); if (UnityEngine.Random.value < finalChance) { int finalAmount = GetTotalDiaAmount(); AddDiamond(finalAmount); return finalAmount; } return 0; }
    public double CalculateCombatPower() { double totalPower = 0; foreach (var robot in myRobots) { float sWeight = GetFinalMoveSpeed(robot) * 100f; float dWeight = (1.0f / GetFinalSearchDelay(robot)) * 50f; bool dummyCrit; double gWeight = GetFinalGoldReward(robot, out dummyCrit) * 10f; totalPower += (sWeight + dWeight + gWeight); } return totalPower; }
}