using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOverUIController : MonoBehaviour
{
    public TMP_Text scoreTextBox;
    public TMP_Text highScoreTextBox;
    public GameObject gameOverPanel;
    public GameObject NewHighScore;

    void Start()
    {
        Hide();
    }

    public void Show(bool newHighScore)
    {
        scoreTextBox.text = GameManagerController.gameManagerInstance.score.ToString();
        highScoreTextBox.text = GameManagerController.gameManagerInstance.GetHighScore().ToString();

        gameOverPanel.SetActive(true);
        NewHighScore.SetActive(newHighScore);
    }

    public void Hide()
    {
        gameOverPanel.SetActive(false);
    }

    public void ClickPlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
    public void ClickMainMenu()
    {
        SceneManager.LoadScene("Splash");
    }
}
