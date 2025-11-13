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

    private SpaceshipController playerShip;


    void Start()
    {
        playerShip = Object.FindAnyObjectByType<SpaceshipController>();
        Hide();
    }

    public void Show(bool newHighScore)
    {
        scoreTextBox.text = playerShip.score.ToString();
        highScoreTextBox.text = playerShip.GetHighScore().ToString();

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
        SceneManager.LoadScene("TitleScene");
    }
}
