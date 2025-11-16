using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleUIController : MonoBehaviour
{
    void Start()
    {
        AudioManagerController.Instance.PlayMusic(AudioManagerController.Instance.menuMusic, AudioManagerController.Instance.menuMusicVolume);
    }

    public void ClickPlay()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void ClickQuit()
    {
        Application.Quit();
    }
}
