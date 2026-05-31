using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject achievementsPanel;
    public GameObject settingsPanel;

    void Start()
    {
        ShowHome();
    }

    public void ShowHome()
    {
        homePanel.SetActive(true);
        settingsPanel.SetActive(false);
        achievementsPanel.SetActive(false);

    }

    public void ShowAchievements()
    {
        homePanel.SetActive(false);
        settingsPanel.SetActive(false);
        achievementsPanel.SetActive(true);

    }

    public void ShowSettings()
    {
        homePanel.SetActive(false);
        settingsPanel.SetActive(true);
        achievementsPanel.SetActive(false);

    }
}