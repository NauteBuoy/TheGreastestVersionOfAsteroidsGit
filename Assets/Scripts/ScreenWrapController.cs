using UnityEditor;
using UnityEngine;

public class ScreenWrapController : MonoBehaviour
{
    [Header("Screen Wrap Settings")]
    private Camera cam; // reference to main camera
    private float camHeight; // height of the camera view
    private float camWidth; // width of the camera view
    public ParticleSystem wrapFX; // particle effect to play on wrap
    private Vector3 lastPos; // last position of the object


    void Start()
    {
        cam = Camera.main;
        camHeight = cam.orthographicSize;
        camWidth = cam.aspect * camHeight;
        lastPos = transform.position;
    }

    Vector2 GetScreenBounds()
    {
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        return new Vector2(camWidth * 0.5f, camHeight * 0.5f);
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        Vector2 bounds = GetScreenBounds();

        if (pos.x > bounds.x)
        {
            pos.x = -bounds.x;
        }
        else if (pos.x < -bounds.x)
        {
            pos.x = bounds.x;
        }
        if (pos.y > bounds.y)
        {
            pos.y = -bounds.y;
        }
        else if (pos.y < -bounds.y)
        {
            pos.y = bounds.y;
        }

        transform.position = pos;
        
        bool wrapped = Mathf.Abs(pos.x - lastPos.x) > camWidth * 1.5f || Mathf.Abs(pos.y - lastPos.y) > camHeight * 1.5f;

        if (wrapped && wrapFX)
        {
            Instantiate(wrapFX, pos, Quaternion.identity);
        }

        lastPos = pos;
    }
}
