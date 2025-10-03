using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private Vector2 movement;

    public float lineLength = 30f;
    public int numberOfLines = 60;
    public float fovAngle = 60f;
    public LineRenderer[] lineRenderers;
    public LineRenderer[] verticalLineRenderers;
    public Transform verticalLines;
    public float wallHeightFactor = 10f;

    public float rotationSpeed = 100f; // Speed of rotation
    private float currentRotationAngle = 0f; // Stores current rotation angle




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Initialize line renderers for multiple lines
        lineRenderers = new LineRenderer[numberOfLines];
        for (int i = 0; i < numberOfLines; i++)
        {
            GameObject lineObj = new GameObject("LineRenderer" + i);
            lineObj.transform.SetParent(transform);
            lineRenderers[i] = lineObj.AddComponent<LineRenderer>();
            lineRenderers[i].positionCount = 2;
            lineRenderers[i].startWidth = .05f;
            lineRenderers[i].endWidth = .05f;
            lineRenderers[i].gameObject.layer = 6;   //2d layer...
        }

        // Initialize line renderers for multiple lines
        verticalLineRenderers = new LineRenderer[numberOfLines];
        for (int i = 0; i < numberOfLines; i++)
        {
            GameObject lineObj = new GameObject("VerticalLineRenderer" + i);
            lineObj.transform.SetParent(verticalLines);
            verticalLineRenderers[i] = lineObj.AddComponent<LineRenderer>();
            verticalLineRenderers[i].positionCount = 2;
            verticalLineRenderers[i].startWidth = .2f;
            verticalLineRenderers[i].endWidth = .2f;
            verticalLineRenderers[i].startColor = Color.red;
            verticalLineRenderers[i].endColor = Color.red;
            verticalLineRenderers[i].material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        }
    }

    void Update()
    {

        // Get movement input
        float moveX = Input.GetAxisRaw("Horizontal"); // A (-1) | D (1)
        float moveY = Input.GetAxisRaw("Vertical");   // W (1) | S (-1)

        // Move relative to rotation
        movement = GetRelativeMovement(moveX, moveY);

        // Rotate the direction vector using left/right arrow keys
        if (Input.GetKey(KeyCode.L))
        {
            currentRotationAngle += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.J))
        {
            currentRotationAngle -= rotationSpeed * Time.deltaTime;
        }

        // Get the rotated vector
        Vector3 rotatedVector = RotateVector(currentRotationAngle);
        Debug.DrawRay(transform.position, rotatedVector * 2, Color.green); // Debug ray for visualization

        DrawLinesInFOV();
    }

    void FixedUpdate()
    {
        // Apply movement to the Rigidbody
        rb.linearVelocity = movement * moveSpeed;
    }

    // Converts movement input into world-relative movement
    Vector2 GetRelativeMovement(float inputX, float inputY)
    {
        // Forward direction based on rotation
        Vector3 forward = RotateVector(currentRotationAngle);
        Vector3 right = Quaternion.Euler(0, 0, 90) * forward; // Right is 90 degrees rotated from forward

        // Compute movement relative to rotation
        return (forward * inputY + right * inputX).normalized;
    }

    // Function to rotate a vector pointing up around the Z-axis
    Vector3 RotateVector(float angle)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        return rotation * Vector3.up; // Return the rotated vector
    }

    void DrawLinesInFOV()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        // Vector3 baseDirection = (mousePosition - transform.position).normalized;
        Vector3 baseDirection = RotateVector(currentRotationAngle);

        float startAngle = -fovAngle / 2;
        float angleStep = fovAngle / (numberOfLines - 1);

        for (int i = 0; i < numberOfLines; i++)
        {
            float angle = startAngle + (angleStep * i);

            Vector3 rotatedDirection = Quaternion.Euler(0, 0, angle) * baseDirection;

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, rotatedDirection, lineLength);

            // Vector3 endPosition = hit.collider != null ? hit.point : transform.position + rotatedDirection * lineLength;

            Vector3 endPosition = Vector3.zero;
            Color lineColor = Color.clear;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != boxCollider2D)
                {
                    endPosition = hit.point; // Set end position at first hit
                    // Debug.Log("Hit object: " + hit.collider.name);

                    // Check the normal direction
                    Vector2 normal = hit.normal;
                    if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x))
                    {
                        // Debug.Log("Hit a HORIZONTAL side of " + hit.collider.name);
                        Color col = new Color(.7f, 0, 0, 1);
                        lineColor = col;
                    }
                    else
                    {
                        // Debug.Log("Hit a VERTICAL side of " + hit.collider.name);
                        lineColor = Color.red;
                    }

                    break; // Use first valid hit for now (can modify if needed)
                }
            }

            // Wall height based on the distance (closer = taller)
            float distanceToWall = Vector3.Distance(transform.position, endPosition);

            // Fix fisheye: Adjust distance using cosine of angle relative to player's view direction
            float correctedDistance = distanceToWall * Mathf.Cos(Mathf.Deg2Rad * angle);

            float wallHeight = wallHeightFactor / correctedDistance;

            // Now set the position of the vertical line renderer
            // Mapping the screen coordinates (X) for the vertical lines
            float normalizedX = (i / (float)(numberOfLines - 1)); // From 0 to 1 across the screen

            // Convert this normalized X to screen space
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            float screenWidth = Screen.width;
            float screenX = normalizedX * screenWidth;

            // Calculate the position of the wall slice in world space
            // To simulate a 3D effect, we place the line at the correct height based on the wallHeight
            float halfScreenHeight = Screen.height / 2;
            float startY = halfScreenHeight - (wallHeight / 2);
            float endY = halfScreenHeight + (wallHeight / 2);

            // Set the start and end positions of the vertical line renderer
            Vector3 wallPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenX, startY, screenPosition.z));
            Vector3 wallEndPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenX, endY, screenPosition.z));


            verticalLineRenderers[i].SetPosition(0, wallPosition);
            verticalLineRenderers[i].SetPosition(1, wallEndPosition);

            verticalLineRenderers[i].startColor = lineColor;
            verticalLineRenderers[i].endColor = lineColor;


            lineRenderers[i].SetPosition(0, transform.position);
            lineRenderers[i].SetPosition(1, endPosition);
        }
    }

}
