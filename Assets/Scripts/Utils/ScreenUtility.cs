using UnityEngine;

public class ScreenUtility : MonoBehaviour
{
    public Camera cam;

    [SerializeField] protected float widthToMaintain = 5f;


    /// <summary>
    /// Left side of the screen in world coordinates
    /// </summary>
    public float Left
    {
        get
        {
            if (cam)
                return cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;

            return 0.0f;
        }
    }

    /// <summary>
    /// Right side of the screen in world coordinates
    /// </summary>
    public float Right
    {
        get
        {
            if (cam)
                return cam.ViewportToWorldPoint(new Vector3(1.0f, 0f, 0f)).x;

            return 0.0f;
        }
    }

    /// <summary>
    /// Top side of the screen in world coordinates
    /// </summary>
    public float Top
    {
        get
        {
            if (cam)
                return cam.ViewportToWorldPoint(new Vector3(0f, 1.0f, 0f)).y;

            return 0.0f;
        }
    }

    /// <summary>
    /// Bottom side of the screen in world coordinates
    /// </summary>
    public float Bottom
    {
        get
        {
            if (cam)
                return cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y;

            return 0.0f;
        }
    }

    /// <summary>
    /// Middle of the screen in world coordinates
    /// </summary>
    public Vector3 Middle
    {
        get
        {
            if (cam)
            {
                return cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            }

            return Vector3.zero;
        }
    }

    public float Height
    {
        get
        {
            return cam.orthographicSize * 2f;
        }
    }

    public float Width
    {
        get
        {
            return (cam.orthographicSize * 2f) * cam.aspect;
        }
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        SetupScreen();
    }

    public void SetupScreen()
    {
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        /*if (width != widthToMaintain)
        {
            cam.orthographicSize = widthToMaintain / cam.aspect;
        }*/
    }
}