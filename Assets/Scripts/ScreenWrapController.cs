using UnityEditor;
using UnityEngine;

public class ScreenWrapController : MonoBehaviour
{
    private Camera cam;
    private float camHeight;
    private float camWidth;

    void Start()
    {
        cam = Camera.main;
        camHeight = cam.orthographicSize;
        camWidth = cam.aspect * camHeight;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        if (pos.x > camWidth) pos.x = -camWidth;
        else if (pos.x < -camWidth) pos.x = camWidth;

        if (pos.y > camHeight) pos.y = -camHeight;
        else if (pos.y < -camHeight) pos.y = camHeight;

        transform.position = pos;
    }
}
