using UnityEngine;

public class ProjectileTrajectory : MonoBehaviour
{
    public Transform planet; // The planet's position
    public float initialVelocity; // Initial velocity of the projectile
    public float initialAngle; // Launch angle (in degrees)
    public int steps = 100; // Number of steps to simulate the trajectory

    private LineRenderer lineRenderer;
    private Vector3 velocity;
    private Vector3 position;
    private float gravitationalConstant = 6.67430e-11f; // Gravitational constant (in units of Unity's physics)
    private float planetMass = 5.972e24f; // Mass of the planet (in kg)

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Set initial position and velocity
        position = transform.position;
        velocity = new Vector3(Mathf.Cos(Mathf.Deg2Rad * initialAngle) * initialVelocity, 0, Mathf.Sin(Mathf.Deg2Rad * initialAngle) * initialVelocity);

        DrawTrajectory();
        
    }

    void DrawTrajectory()
    {
        lineRenderer.positionCount = steps;

        // Simulate projectile motion for 'steps' iterations
        for (int i = 0; i < steps; i++)
        {
            // Calculate the gravitational force towards the planet
            Vector3 gravitationalForce = GetGravitationalForce(position);
            velocity += gravitationalForce * Time.deltaTime; // Update velocity based on the gravitational force
            position += velocity * Time.deltaTime; // Update position based on the velocity

            // Update the line renderer to show the trajectory
            lineRenderer.SetPosition(i, position);
        }
    }

    // Function to calculate the gravitational force towards the planet
    Vector3 GetGravitationalForce(Vector3 position)
    {
        Vector3 directionToPlanet = (planet.position - position).normalized; // Get the direction towards the planet
        float distanceSquared = (planet.position - position).sqrMagnitude; // Get the square of the distance between the object and the planet
        float forceMagnitude = gravitationalConstant * planetMass / distanceSquared; // Calculate the magnitude of the gravitational force
        return directionToPlanet * forceMagnitude; // Return the gravitational force vector
    }

    void Update()
    {
        // Move the object towards the planet by applying gravitational force each frame
        Vector3 gravitationalForce = GetGravitationalForce(position);
        velocity += gravitationalForce * Time.deltaTime;
        position += velocity * Time.deltaTime;

        // Apply the new position and velocity to the object's transform
        transform.position = position;
    }
}
