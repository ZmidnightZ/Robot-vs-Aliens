using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            -100
        );
    }
}
