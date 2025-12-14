using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashController : MonoBehaviour
{
    public Image logo;
    public float fadeTime = 0.6f;
    public float holdTime = 2f;
    public string nextScene = "Title";

    void Start()
    {
        StartCoroutine(SplashRoutine());
    }

    IEnumerator SplashRoutine()
    {
        Color logoColour = logo.color;
        logoColour.a = 0f;
        logo.color = logoColour;

        float timeElapsed = 0f;
        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime / fadeTime;
            logoColour.a = timeElapsed;
            logo.color = logoColour;
            logo.transform.localScale = Vector3.Lerp(Vector3.one * 0.95f, Vector3.one, timeElapsed);
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        timeElapsed = 0f;
        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime / fadeTime;
            logoColour.a = 1f - timeElapsed;
            logo.color = logoColour;
            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }
}
