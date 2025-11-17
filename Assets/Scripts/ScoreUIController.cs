using TMPro;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    [Header("Score Reference Settings")]
    public static ScoreUIController Instance;
    public TMP_Text scoreText;
    Vector3 baseScale;

    [Header("Animation Settings")]
    public float animaitonScale = 1.35f;     // how big the pop is
    public float animationSpeed = 16f;       // speed of pop animation


    void Start()
    {
        Instance = this;
        AudioManagerController.Instance.PlayMusic(AudioManagerController.Instance.gameMusic, AudioManagerController.Instance.gameMusicVolume);
        baseScale = transform.localScale;
    }

    void Update()
    {

    }

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
