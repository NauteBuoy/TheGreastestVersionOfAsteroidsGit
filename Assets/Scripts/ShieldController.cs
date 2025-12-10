using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ShieldController : MonoBehaviour
{
    [Header("Reference Settings")]
    public SpaceshipController playerShip;
    public Transform followTransform;
    public SpriteRenderer shieldVisual;
    public CircleCollider2D shieldCollider;


    [Header("Immunity Settings")]
    public float imunityDuration = 0.2f;
    public bool isImmune = false;


    [Header("Follow Settings")]
    public float followSpeed = 30f; //LERP
    public float smoothFollowDuration = 0.03f; //DAMP
    private Vector3 followCentreVel;


    void Start()
    { 
        playerShip = SpaceshipController.playerShipInstance;
        followTransform = playerShip.transform;
        shieldVisual = GetComponent<SpriteRenderer>();
        shieldCollider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        HandleFollow();
        HandleShieldVisual();
    }
    
    public void HandleFollow()
    {
        if (!playerShip)
            return;

        transform.position = Vector3.SmoothDamp(transform.position, followTransform.position, ref followCentreVel, smoothFollowDuration);
    }

    public void HandleShieldVisual()
    {
        shieldCollider.enabled = playerShip.currentShields > 0;
        shieldVisual.enabled = playerShip.currentShields > 0;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (isImmune)
            return;

        if (playerShip.currentShields <= 0)
            return;

        if (!collision.CompareTag("Asteroid"))
            return;

        playerShip.ShieldDamage();
        StartCoroutine(collisionImmunityRoutine());
    }


    IEnumerator collisionImmunityRoutine()
    {
        isImmune = true;
        yield return new WaitForSeconds(imunityDuration);
        isImmune = false;
    }
}



