using UnityEditor;
using UnityEngine;

public class ScreenWrapController : MonoBehaviour
{
    private Camera cam;
    private float camHeight;
    private float camWidth;
    float wrapMargin = 0.25f;


    void Start()
    {
        cam = Camera.main;
        camHeight = cam.orthographicSize;
        camWidth = cam.aspect * camHeight;
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
    }
}
