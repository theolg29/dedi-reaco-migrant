using UnityEngine;

public class CanvasFaceCamera : MonoBehaviour
{
    void Update()
    {
        if (Camera.main == null) return;

        Vector3 direction = Camera.main.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(-direction);
    }
}
