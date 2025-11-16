using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraShakeController : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    public float shakeDistance = 0.3f;
    public float shakeDuration = 0.2f;


    [Header("Private Settings")]
    private Coroutine shakeRoutine;


    void Start()
    {

    }

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
        Vector3 startPos = transform.position;

        float timeElapsed = 0f;
        while (timeElapsed < shakeDuration)
        {
            timeElapsed += Time.deltaTime;
            float shakeIntensity = shakeDistance * shakeMultiplier * (1f - timeElapsed / shakeDuration);
            Vector2 shakeOffset = Random.insideUnitCircle * shakeIntensity;
            transform.position = startPos + new Vector3(shakeOffset.x, shakeOffset.y, 0);
            yield return null;
        }

        transform.position = startPos;
        shakeRoutine = null;
    }
}
