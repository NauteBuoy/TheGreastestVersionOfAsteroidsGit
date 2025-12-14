using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManagerController : MonoBehaviour
{
    [Header("Reference Settings")]
    public static GameManagerController gameManagerInstance; //singleton instance
    public GameOverUIController gameOverUI; //reference to game over UI controller

    [Header("Score")]
    public int score = 0; //current score
    public ScoreUIController scoreUI; //reference to score UI controller           
    public GameObject floatingText; //floating text prefab for score popups

    [Header("Time-Based Score Settings")]
    public float timeScoreRate = 1f; //points per second for time-based score            
    public float timeScoreMultiplier = 1f; //multiplier for time-based score
    public float timeScore = 0f; //accumulated time-based score 


    void Awake()
    {
        if (gameManagerInstance) 
        { 
            Destroy(gameObject); 
            return; 
        }

        gameManagerInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (!gameOverUI)
        {
            gameOverUI = Object.FindAnyObjectByType<GameOverUIController>();
        }
        if (!scoreUI)
        {
            scoreUI = Object.FindAnyObjectByType<ScoreUIController>();
        }
        if (AudioManagerController.audioManagerInstance)
        {
            AudioManagerController.audioManagerInstance.PlayMusic(AudioManagerController.audioManagerInstance.gameMusic, AudioManagerController.audioManagerInstance.sfxVolume);
        }
        scoreUI.UpdateScoreDisplay(score);
    }

    void Update()
    {
        if (!SpaceshipController.playerInstance)
            return;

        if (timeScoreRate > 0f)
        {
            float heatBonus = 1f;
            heatBonus += SpaceshipController.playerInstance.GetThermalNorm();

            timeScore += timeScoreRate * heatBonus * Time.deltaTime * timeScoreMultiplier;
            if (timeScore >= 1f)
            {
                int scoreIncrease = Mathf.FloorToInt(timeScore);
                AddScore(scoreIncrease);
                timeScore -= scoreIncrease;
            }
        }
    }

    public void AddScore(int scoreIncrease, Vector3? asteroidPos = null)
    {
        if (scoreIncrease <= 0) 
            return;

        score += scoreIncrease;
        scoreUI.UpdateScoreDisplay(score);
        AudioManagerController.audioManagerInstance.PlayScoreTick();


        if (asteroidPos.HasValue && floatingText)
        {
            var floatingTextFX = Instantiate(this.floatingText, asteroidPos.Value, Quaternion.identity);
            var floatingText = floatingTextFX.GetComponent<ScorePopupController>();
            if (floatingText)
            {
                floatingText.SetText("+" + scoreIncrease.ToString());
            }
        }
    }

    public void ResetScore()
    {
        score = 0;
        timeScore = 0f;
        scoreUI.UpdateScoreDisplay(score);
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public void SetHighScore(int highScore)
    {
        PlayerPrefs.SetInt("HighScore", highScore);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    public IEnumerator GameOverRoutine()
    {
        bool newHighScore = false;
        if (score > GetHighScore())
        {
            SetHighScore(score);
            newHighScore = true;
        }

        if (gameOverUI)
        {
            gameOverUI.Show(newHighScore);
        }
        yield return null;
    }
}

