using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraShakeController : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    public float shakeDistance = 0.3f;
    public float shakeDuration = 0.2f;


    [Header("Camera Target Settings")]
    [SerializeField] private GameObject playerTarget;
    [SerializeField] private float cameraSmoothing = 0.5f;
    [SerializeField] private Vector3 cameraOffset;


    [Header("Camera Bounds Settings")]
    public Vector2 maxBoundary;
    public Vector2 minBoundary;


    [Header("Private Settings")]
    private Coroutine shakeRoutine;


    void Start()
    {

    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        if (!playerTarget)
            return;

        Vector3 targetPosition = playerTarget.transform.position + cameraOffset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBoundary.x, maxBoundary.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBoundary.y, maxBoundary.y);

        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSmoothing);
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
