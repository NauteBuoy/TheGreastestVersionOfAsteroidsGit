using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    public float shakeDistance = 0.3f;
    public float shakeDuration = 0.2f;


    [Header("Motion Settings")]
    public float motionAmount = 0.1f;
    public float motionSpeed = 2f;


    [Header("Private Settings")]
    private Vector3 basePos;
    private Vector3 targetShakeOffset = Vector3.zero;
    private Coroutine shakeRoutine;



    void Start()
    {
        basePos = transform.localPosition;
    }

    void Update()
    {

    }

    void LateUpdate()
    {
        if (!SpaceshipController.playerInstance)
        {
            transform.localPosition = basePos + targetShakeOffset;
            return;
        }

        float velocity = SpaceshipController.playerInstance.GetVelocity();
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
}
