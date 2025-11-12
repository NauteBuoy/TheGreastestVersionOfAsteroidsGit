using TMPro;
using UnityEngine;

public class HighScoreUIController : MonoBehaviour
{
    public TMP_Text scoreTextBox;
    public GameObject scorePanel;
    private SpaceshipController playerShip;


    void Start()
    {
        playerShip = Object.FindAnyObjectByType<SpaceshipController>();
    }

    void Update()
    {
        if (playerShip)
        {
            scoreTextBox.text = playerShip.score.ToString();
        }
    }
}
