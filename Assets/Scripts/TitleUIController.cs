using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleUIController : MonoBehaviour
{
    void Start()
    {
        AudioManagerController.audioManagerInstance.PlayMusic(AudioManagerController.audioManagerInstance.splashMusic, AudioManagerController.audioManagerInstance.musicVolume);
    }

    public void ClickPlay()
    {
        SceneManager.LoadScene("Game");
    }
    public void ClickQuit()
    {
        Application.Quit();
    }
}
