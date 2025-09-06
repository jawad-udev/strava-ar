using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    public float speed = 3f;           // meters per second
    public float rotationSpeed = 8f;   // how fast to rotate toward next point

    private List<Vector3> path;
    private int index = 0;
    private bool moving = false;

    public void SetPath(List<Vector3> worldPositions)
    {
        path = new List<Vector3>(worldPositions);
        index = 0;
        if (path.Count > 0)
        {
            transform.position = path[0];
            moving = true;
        }
    }

    void Update()
    {
        if (!moving || path == null || index >= path.Count - 1) return;

        Vector3 target = path[index + 1];
        Vector3 dir = (target - transform.position);
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotationSpeed);
        }

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            index++;
            if (index >= path.Count - 1)
            {
                moving = false;
                Debug.Log("Player reached destination.");
            }
        }
    }
}
