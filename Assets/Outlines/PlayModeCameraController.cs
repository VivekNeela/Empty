using UnityEngine;

public class PlayModeCameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;
    public float fastMultiplier = 3f;

    private float rotationX;
    private float rotationY;

    void Update()
    {
        // Right mouse button must be held to look/move
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Camera rotation
            rotationX += Input.GetAxis("Mouse X") * lookSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -89f, 89f);

            transform.rotation = Quaternion.identity;

            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);

            // Movement
            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
            Vector3 direction = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );

            if (Input.GetKey(KeyCode.E)) direction.y += 1f; // Up
            if (Input.GetKey(KeyCode.Q)) direction.y -= 1f; // Down

            transform.Translate(direction * speed * Time.deltaTime, Space.Self);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
