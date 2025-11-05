using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneUIController: MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ClickStart()
    {

    }

    public void ClickExit()
    {
        Application.Quit();
    }
}
