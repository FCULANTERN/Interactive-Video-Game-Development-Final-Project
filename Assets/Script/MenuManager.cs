using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject achievementsPanel;
    public GameObject settingsPanel;
    public GameObject leavesParticle;


    void Start()
    {
        ShowHome();
    }

    public void ShowHome()
    {
        homePanel.SetActive(true);
        settingsPanel.SetActive(false);
        achievementsPanel.SetActive(false);

        leavesParticle.SetActive(true);
    }

    public void ShowAchievements()
    {
        homePanel.SetActive(false);
        settingsPanel.SetActive(false);
        achievementsPanel.SetActive(true);

        leavesParticle.SetActive(false);
    }

    public void ShowSettings()
    {
        homePanel.SetActive(false);
        settingsPanel.SetActive(true);
        achievementsPanel.SetActive(false);

        leavesParticle.SetActive(false);
    }
}