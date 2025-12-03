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

    [Header("Follow Settings")]
    public float followSpeed = 30f;
    public float smoothDuration = 0.03f;
    private Vector3 followVel;


    [Header("Collision Settings")]
    public float collisionImunityDuration = 0.2f;
    public bool isImmune = false;

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
        if (followTransform)
        {
            transform.position = Vector3.SmoothDamp(transform.position, followTransform.position, ref followVel, smoothDuration);
        }
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
        yield return new WaitForSeconds(collisionImunityDuration);
        isImmune = false;
    }
}



