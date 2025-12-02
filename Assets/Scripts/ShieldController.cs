using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ShieldController : MonoBehaviour
{
    public Transform followTransform;
    public float followSpeed = 15f;
    public float smoothDuration = 0.06f;
    Vector3 velocity;


    void Start()
    {
        if (!SpaceshipController.playerShipInstance)
            return;

        followTransform = SpaceshipController.playerShipInstance.transform;
    }


    void Update()
    {
        if (followTransform)
        {
            //transform.position = Vector3.Lerp(transform.position, followTransform.position, Time.deltaTime * followSpeed);
            transform.position = Vector3.SmoothDamp(transform.position, followTransform.position, ref velocity, smoothDuration);
        }
    }
}

