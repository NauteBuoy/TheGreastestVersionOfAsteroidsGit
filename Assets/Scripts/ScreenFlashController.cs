using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlashController : MonoBehaviour
{
    public float flashDuration = 0.33f;
    private Image flashImage;
    private Color flashColor;

    void Start()
    {
        flashImage = GetComponent<Image>();
        flashColor = flashImage.color;
    }

    void Update()
    {
        
    }

    public IEnumerator FlashRoutine()
    {

        float elapsedTime = 0f;
        float flashtimer = 0f;

        while (elapsedTime < flashDuration) // repeats while condition is true
        {
            elapsedTime += Time.deltaTime;
            flashtimer = Mathf.Clamp01(elapsedTime / flashDuration);
            float alpha = Mathf.Lerp(0.8f, 0f, flashtimer);
            Color col = flashColor;
            col.a = alpha;
            flashImage.color = col;
            yield return null;
        }
    }

    public void HideFlash()
    {
        Color col = flashColor;
        col.a = 0f;
        flashImage.color = col;
        StopAllCoroutines(); // stop any ongoing flash
    }
}
