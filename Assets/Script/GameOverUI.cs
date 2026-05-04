using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;

    // 當 GameOver 畫面被啟用時，自動顯示分數
    void OnEnable()
    {
        if (scoreText != null && ScoreManager.Instance != null)
        {
            scoreText.text = "SCORE : " + ScoreManager.Instance.GetScore();
        }
    }

    // 綁定到 Play 按鈕
    public void OnPlayButton()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 綁定到 Quit 按鈕
    public void OnQuitButton()
    {
        SceneManager.LoadScene("HomeScene");
    }
}
