using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public int TotalJumps { get; private set; } = 0;
    public int TotalWaves { get; private set; } = 0;
    public int ConsecutiveWaves { get; private set; } = 0;
    public int TotalEnemiesKilled { get; private set; } = 0;


    public bool[] IsUnlocked { get; private set; } = new bool[6];


    public event System.Action<int, bool> OnAchievementChanged;

    private bool playerDiedThisWave = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAchievements();
    }

    public void RegisterJump()
    {
        TotalJumps++;
        CheckAchievement(4, TotalJumps >= 50);
        SaveAchievements();
    }

    public void RegisterEnemyKill()
    {
        TotalEnemiesKilled++;
        CheckAchievement(5, TotalEnemiesKilled >= 100);
        SaveAchievements();
    }

    public void OnNewWave(int waveNumber)
    {
        if (waveNumber > 0)
        {
            TotalWaves++;

            if (!playerDiedThisWave)
                ConsecutiveWaves++;
            else
                ConsecutiveWaves = 0; 

            playerDiedThisWave = false;

            CheckAchievement(0, ConsecutiveWaves >= 10); 
            CheckAchievement(1, ConsecutiveWaves >= 20);
            CheckAchievement(2, ConsecutiveWaves >= 30);
            CheckAchievement(3, TotalWaves >= 100);       
            SaveAchievements();
        }
    }

    public void RegisterPlayerDeath()
    {
        playerDiedThisWave = true;
        ConsecutiveWaves = 0;
        OnAchievementChanged?.Invoke(-1, false);
    }

    private void CheckAchievement(int index, bool condition)
    {
        if (!IsUnlocked[index] && condition)
        {
            IsUnlocked[index] = true;
            OnAchievementChanged?.Invoke(index, true);
            Debug.Log($"[Achievement] Achievement {index + 1} débloqué !");
        }
    }

    private void SaveAchievements()
    {
        PlayerPrefs.SetInt("ACH_Jumps", TotalJumps);
        PlayerPrefs.SetInt("ACH_Waves", TotalWaves);
        PlayerPrefs.SetInt("ACH_Consec", ConsecutiveWaves);
        PlayerPrefs.SetInt("ACH_Kills", TotalEnemiesKilled);
        for (int i = 0; i < IsUnlocked.Length; i++)
            PlayerPrefs.SetInt("ACH_Unlocked_" + i, IsUnlocked[i] ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAchievements()
    {
        TotalJumps = PlayerPrefs.GetInt("ACH_Jumps", 0);
        TotalWaves = PlayerPrefs.GetInt("ACH_Waves", 0);
        ConsecutiveWaves = PlayerPrefs.GetInt("ACH_Consec", 0);
        TotalEnemiesKilled = PlayerPrefs.GetInt("ACH_Kills", 0);
        for (int i = 0; i < IsUnlocked.Length; i++)
            IsUnlocked[i] = PlayerPrefs.GetInt("ACH_Unlocked_" + i, 0) == 1;
    }

    [ContextMenu("Reset All Achievements")]
    public void ResetAllAchievements()
    {
        TotalJumps = TotalWaves = ConsecutiveWaves = TotalEnemiesKilled = 0;
        for (int i = 0; i < IsUnlocked.Length; i++) IsUnlocked[i] = false;
        SaveAchievements();
        OnAchievementChanged?.Invoke(-1, false);
        Debug.Log("[Achievement] Tout réinitialisé.");
    }
}