using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraShakeController : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    public float shakeDistance = 0.3f;
    public float shakeDuration = 0.2f;

    [Header("Private Settings")]
    private Vector3 originalPos;
    private Coroutine shakeRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartSceenShake(float shakeMultiplier)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(shakeMultiplier));
    }

    private IEnumerator ShakeRoutine(float shakeMultiplier)
    {
        float timeElapsed = 0f;

        while (timeElapsed < shakeDuration)
        {
            timeElapsed += Time.deltaTime;
            float shakePercent = timeElapsed / shakeDuration;
            float currentShakeIntensity = shakeDistance * shakeMultiplier * (1f - shakePercent);

            Vector3 shakePos = Random.insideUnitCircle * currentShakeIntensity;
            transform.localPosition = originalPos + new Vector3(shakePos.x, shakePos.y, 0);
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeRoutine = null;
    }
}
