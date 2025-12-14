using TMPro;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    [Header("Score Reference Settings")]
    public static ScoreUIController scoreUIInstance;
    public TMP_Text scoreText;

    void Awake()
    {
        scoreUIInstance = this;
    }

    public void UpdateScoreDisplay(int score)
    {
        scoreText.text = score.ToString();
    }
}