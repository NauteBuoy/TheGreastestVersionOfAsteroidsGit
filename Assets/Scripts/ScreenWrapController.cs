using UnityEditor;
using UnityEngine;

public class ScreenWrapController : MonoBehaviour
{
    private Camera cam;
    private float camHeight;
    private float camWidth;
    float wrapMargin = 0.25f;
    public ParticleSystem wrapFX;
    private Vector3 lastPos;


    void Start()
    {
        cam = Camera.main;
        camHeight = cam.orthographicSize;
        camWidth = cam.aspect * camHeight;
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        if (pos.x > camWidth + wrapMargin)
        {
            pos.x = -camWidth - wrapMargin;
        }
        else if (pos.x < -camWidth - wrapMargin)
        {
            pos.x = camWidth + wrapMargin;
        }

        if (pos.y > camHeight + wrapMargin)
        {
            pos.y = -camHeight - wrapMargin;
        }
        else if (pos.y < -camHeight - wrapMargin)
        {
            pos.y = camHeight + wrapMargin;
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
