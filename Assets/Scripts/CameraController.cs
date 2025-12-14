using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCam; //reference to main camera

    [Header("Camera Shake Settings")]
    public float shakeDistance = 0.3f; //maximum shake distance
    public float shakeDuration = 0.2f; //duration of shake effect

    [Header("Motion Settings")]
    public float motionAmount = 0.1f; //amount of motion based on velocity
    public float motionSpeed = 2f; //speed of motion oscillation

    [Header("Private Settings")]
    private Vector3 basePos; //base local position of camera
    private Vector3 targetShakeOffset = Vector3.zero; //current shake offset
    private Coroutine shakeRoutine; //reference to shake coroutine

    [Header("Zoom Settings")]
    public float orthoMin = 4.05f; //minimum orthographic size
    public float orthoMax = 3.95f; //maximum orthographic size
    public float cameraLerpSpeed = 1f; //speed of camera size lerping

    [Header("Post Processing Settings")]
    public Volume globalVolume; //reference to global volume
    ChromaticAberration chromaticAberration; //reference to chromatic aberration effect

    void Start()
    {
        if (!mainCam)
        {
            mainCam = Camera.main;
        }
        if (!globalVolume)
        {
            globalVolume = Object.FindAnyObjectByType<Volume>();
        }

        basePos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (!SpaceshipController.playerInstance)
        {
            transform.localPosition = basePos + targetShakeOffset;
            return;
        }

        float velocity = SpaceshipController.playerInstance.GetVelocityNorm();
        float offsetX = Mathf.Sin(Time.time * motionSpeed) * motionAmount * velocity;
        float offsetY = Mathf.Cos(Time.time * motionSpeed * 1.3f) * motionAmount * velocity;
        Vector3 velocityOffset = new Vector3(offsetX, offsetY, 0f);

        transform.localPosition = basePos + velocityOffset + targetShakeOffset;
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
            float shakeIntensity = shakeDistance * shakeMultiplier * (1f - timeElapsed / shakeDuration);
            Vector2 shakeOffset = Random.insideUnitCircle * shakeIntensity;
            targetShakeOffset = new Vector3(shakeOffset.x, shakeOffset.y, 0f);
            yield return null;
        }

        targetShakeOffset = Vector3.zero;
        shakeRoutine = null;
    }

    public void SetOrthographicSize(float thermalNorm)
    {
        if (!mainCam)
            return;

        float targetSize = Mathf.Lerp(orthoMin, orthoMax, thermalNorm);
        mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, targetSize, Time.deltaTime * cameraLerpSpeed);
    }

    //public IEnumerator DeathChromaticRoutine()
    //{
    //    if (chromaticAberration == null)
    //        yield break;

    //    float timeElapsed = 0f;
    //    float peak = 0.2f;

    //    while (timeElapsed < 1f)
    //    {
    //        timeElapsed += Time.unscaledDeltaTime * 6f;
    //        chromaticAberration.intensity.value = Mathf.Lerp(0f, peak, timeElapsed);
    //        yield return null;
    //    }

    //    yield return new WaitForSecondsRealtime(0.1f);

    //    timeElapsed = 0f;
    //    while (timeElapsed < 1f)
    //    {
    //        timeElapsed += Time.unscaledDeltaTime * 4f;
    //        chromaticAberration.intensity.value = Mathf.Lerp(peak, 0.02f, timeElapsed);
    //        yield return null;
    //    }
    //}
}
