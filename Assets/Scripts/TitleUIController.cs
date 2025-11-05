using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleUIController : MonoBehaviour
{
    public void ClickPlay()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void ClickQuit()
    {
        Application.Quit();
    }
}
