using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ShipVFXController : MonoBehaviour
{
    [Header("Reference Settings")]
    public static ShipVFXController shipVFXInstance; //singleton instance
    public CameraController cameraController; //reference to camera controller
    public ParticleSystem thrusterFX; //thruster particle system

    [Header("Fire and Ice Settings")]
    public SpriteRenderer iceSprite; //ice sprite renderer
    public ParticleSystem flameFX; //flame visual effect 

    float iceTargetScale; //target scale for ice effect
    float flameTargetScale; //target scale for flame effect
    Vector3 iceScaleVel; //velocity for smooth damp
    Vector3 flameScaleVel; //velocity for smooth damp
    Vector3 iceBaseScale = Vector3.one;
    Vector3 flameBaseScale = Vector3.one;

    [Header("Juice Settings")]
    public float smoothDuration = 0.08f; //smoothing time for punch and wobble effects
    public float punchMultiplier = 1.25f; //punch scale multiplier
    public float wobbleSpeed = 6f; //speed of wobble effect
    public float wobbleAmount = 2f; //amount of wobble effect

    [Header("Hit Stop Settings")]
    public float hitstopMinDuration = 0.04f; //minimum hitstop duration
    public float hitstopMaxDuration = 0.08f; //maximum hitstop duration

    [Header("Immunity Flash Settings")]
    public SpriteRenderer immunityVisual; //immunity flash sprite renderer
    Coroutine immunityRoutine; //reference to immunity flash coroutine

    [Header("Death Explosion Settings")]
    public GameObject deathExplosionFX; //death explosion effect prefab


    void Awake()
    {
        if (!shipVFXInstance)
        {
            shipVFXInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (iceSprite)
        {
            iceSprite.transform.localScale = Vector3.zero;
            iceSprite.gameObject.SetActive(false);
        }
        if (flameFX)
        {
            flameFX.transform.localScale = Vector3.zero;
            flameFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (immunityVisual)
        {
            immunityVisual.enabled = false;
        }
    }

    void LateUpdate()
    {
        UpdateIceVisual();
        UpdateFlameVisual();
    }

    public void UpdateThruster(bool isThrusting, float thermalNorm)
    {
        if (!thrusterFX)
            return;

        var emission = thrusterFX.emission;
        emission.rateOverTime = Mathf.Lerp(10f, 80f, thermalNorm);

        var main = thrusterFX.main;
        main.startSizeMultiplier = Mathf.Lerp(0.05f, 0.25f, thermalNorm);

        if (isThrusting)
        {
            if (!thrusterFX.isPlaying)
            {
                thrusterFX.Play();
            }
        }
        else
        {
            thrusterFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void UpdateThermalCamera(float thermalNorm)
    {
        if (cameraController)
        { 
            cameraController.SetOrthographicSize(thermalNorm);
        }
    }

    public void UpdateThermalStateChange(SpaceshipController.ThermalState state, float thermalNorm)
    {
        iceTargetScale = GetIceScale(state);
        flameTargetScale = GetFlameScale(state);

        if (iceTargetScale > 0 && iceSprite)
        {
            iceSprite.transform.localScale = Vector3.one * iceTargetScale * punchMultiplier;
        }
        if (flameTargetScale > 0 && flameFX)
        {
            flameFX.transform.localScale = Vector3.one * flameTargetScale * punchMultiplier;
        }
    }

    void UpdateIceVisual()
    {
        if (!iceSprite)
            return;

        if (iceTargetScale <= 0f)
        {
            iceSprite.transform.localScale = Vector3.SmoothDamp(iceSprite.transform.localScale, Vector3.zero, ref iceScaleVel, smoothDuration
);
            if (iceSprite.transform.localScale.magnitude < 0.01f)
            {
                iceSprite.gameObject.SetActive(false);
            }
        }
        else
        {
            if(!iceSprite.gameObject.activeSelf)
            {
                iceSprite.gameObject.SetActive(true);
            }

            Vector3 targetScale = Vector3.one * iceTargetScale;

            iceSprite.transform.localScale = Vector3.SmoothDamp(iceSprite.transform.localScale, targetScale, ref iceScaleVel, smoothDuration);

            float iceWobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
            iceSprite.transform.localRotation = Quaternion.Euler(0f, 0f, iceWobble);
        }
    }

    float GetIceScale(SpaceshipController.ThermalState state)
    {
        switch (state)
        {
            case SpaceshipController.ThermalState.Frozen1: 
                return 0.4f;
            case SpaceshipController.ThermalState.Frozen2: 
                return 0.8f;
            case SpaceshipController.ThermalState.Frozen3: 
                return 1.2f;
            default: 
                return 0f;
        }
    }

    void UpdateFlameVisual()
    {
        if (!flameFX)
            return;

        if (flameTargetScale <= 0f)
        {
            flameFX.transform.localScale = Vector3.SmoothDamp(flameFX.transform.localScale, Vector3.zero, ref flameScaleVel, smoothDuration);

            if (flameFX.transform.localScale.magnitude < 0.01f)
            {
                flameFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        else
        {
            if (!flameFX.isPlaying)
            {
                flameFX.Play();
            }

            Vector3 targetScale = Vector3.one * flameTargetScale;

            flameFX.transform.localScale = Vector3.SmoothDamp(flameFX.transform.localScale, targetScale, ref flameScaleVel, smoothDuration);
        }
    }

    float GetFlameScale(SpaceshipController.ThermalState state)
    {
        switch (state)
        {
            case SpaceshipController.ThermalState.Hot1: 
                return 2f;
            case SpaceshipController.ThermalState.Hot2: 
                return 3f;
            case SpaceshipController.ThermalState.Hot3: 
                return 4f;
            case SpaceshipController.ThermalState.Critical:
                return 8f;
            default: 
                return 0f;
        }
    }

    public void PlayImmunityFlash(float immunityDuration)
    {
        if (!immunityVisual)
            return;

        if (immunityRoutine != null)
        {
            StopCoroutine(immunityRoutine);
        }
        immunityRoutine = StartCoroutine(ImmunityFlashRoutine(immunityDuration));
    }

    IEnumerator ImmunityFlashRoutine(float immunityDuration)
    {
        immunityVisual.enabled = true;

        Color immunityColour = immunityVisual.color;
        immunityColour.a = 0f;
        immunityVisual.color = immunityColour;

        float fadeInDuration = immunityDuration * 0.25f;
        float fadeOutDuration = immunityDuration * 0.25f;

        float timeElapsed = 0f;
        while (timeElapsed < fadeInDuration)
        {
            timeElapsed += Time.deltaTime;
            immunityColour.a = Mathf.Lerp(0f, 1f, timeElapsed / fadeInDuration);
            immunityVisual.color = immunityColour;
            yield return null;
        }

        yield return new WaitForSeconds(immunityDuration * 0.5f);

        timeElapsed = 0f;
        while (timeElapsed < fadeOutDuration)
        {
            timeElapsed += Time.deltaTime;
            immunityColour.a = Mathf.Lerp(1f, 0f, timeElapsed / fadeOutDuration);
            immunityVisual.color = immunityColour;
            yield return null;
        }

        immunityVisual.enabled = false;
        immunityRoutine = null;
    }

    public void PlayHitstop(float thermalNorm)
        {
            StartCoroutine(HitstopRoutine(thermalNorm));
        }

    IEnumerator HitstopRoutine(float thermalNorm)
    {
        float prevTimeScale = Time.timeScale;
        float prevFixedDelta = Time.fixedDeltaTime;

        float stopScale = Mathf.Lerp(hitstopMinDuration, hitstopMaxDuration, thermalNorm);
        float stopDuration = Mathf.Lerp(hitstopMinDuration, hitstopMaxDuration, thermalNorm);

        Time.timeScale = stopScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(stopDuration);

        Time.timeScale = prevTimeScale;
        Time.fixedDeltaTime = prevFixedDelta;
    }

    public void PlayDeathExplosion(Vector2 position)
    {
        if (deathExplosionFX)
        {
            Instantiate(deathExplosionFX, position, Quaternion.identity);
        }
    }
}
