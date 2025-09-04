using UnityEngine;

public class PlayerRunner : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 90f; // degrees per turn
    public float turnInterval = 3f; // seconds between turns
    public float randomTurnChance = 0.5f; // 50% chance to turn

    private float timer = 0f;

    void Update()
    {
        // Always move forward
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // Update timer
        timer += Time.deltaTime;

        // Check if it's time to consider turning
        if (timer >= turnInterval)
        {
            timer = 0f;

            // Random chance
            if (Random.value < randomTurnChance)
            {
                // Pick left (-1) or right (1)
                int dir = Random.value < 0.5f ? -1 : 1;

                // Rotate smoothly
                StartCoroutine(Turn(dir));
            }
        }
    }

    private System.Collections.IEnumerator Turn(int direction)
    {
        float rotated = 0f;

        while (rotated < turnSpeed)
        {
            float step = 180f * Time.deltaTime; // turn speed (degrees/sec)
            transform.Rotate(Vector3.up, step * direction);
            rotated += step;
            yield return null;
        }
    }
}
