using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBubbles : MonoBehaviour
{
    [Serializable]
    struct Bubble
    {
        public Vector3 localPosition;
        public float scale;
        public bool isPopped;
    }

    [Header("Bubble settings")]

    public Mesh bubbleMesh;
    public Material bubbleMaterial;
    public Transform parentTransform;
    public int bubbleCount;
    public float destroyRadius;
    public float bubbleScale = 0.2f;


    // [Header("Cone spawning settings")]
    private float coneRadius = 1f;
    private float coneHeight = 2f;
    private float angularStep = 15f; // degrees between each bubble
    private float verticalStep = 0.1f; // step up in Y per full circle
    private int maxBubblesPerRing = 30;
    private int layerCount = 13;
    private int maxBubblesAtBase = 30;
    private float coneSpawnInterval = 0;
    private Coroutine spawnConeCoroutine;



    private bool isSpawningCone = false;
    private float currentAngleRad = 0f;
    private float currentConeHeight = 0f;
    // public int bubblesInCircle;


    private List<Bubble> bubbles = new List<Bubble>();
    private List<Matrix4x4> bubbleMatrices = new List<Matrix4x4>();
    private const int batchSize = 1023;

    private void Start()
    {
        StartCoroutine(MakeCone());
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     isSpawningCone = true;
        //     currentConeHeight = 0f;
        // }

        // if (isSpawningCone)
        //     SpawnConeRing();

        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     if (spawnConeCoroutine == null)
        //     {
        //         bubbles.Clear(); // Optional: clear previous
        //         spawnConeCoroutine = StartCoroutine(CreateStaggeredCone());
        //     }
        // }

        Spawn();
        DestroyBubbleClick();
        DisposeBubbles();
        bubbleCount = bubbles.Count;  //for debugging purposes
    }


    IEnumerator MakeCone()
    {
        while (true)
        {
            // SpawnInConeCircle();
            // SpawnConeRing();
            // yield return spawnConeCoroutine = StartCoroutine(SpawnConeOverTime());
            yield return new WaitForSeconds(.1f);
        }
    }


    void LateUpdate()
    {
        bubbleMatrices.Clear();

        foreach (var bubble in bubbles)
        {
            if (bubble.isPopped || bubble.scale <= 0f)
                continue;

            Vector3 worldPos = parentTransform.TransformPoint(bubble.localPosition);
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one * bubble.scale;

            bubbleMatrices.Add(Matrix4x4.TRS(worldPos, rotation, scale));
        }

        int count = bubbleMatrices.Count;
        for (int i = 0; i < count; i += batchSize)
        {
            int len = Mathf.Min(batchSize, count - i);
            Graphics.DrawMeshInstanced(bubbleMesh, 0, bubbleMaterial, bubbleMatrices.GetRange(i, len));
        }
    }


    //very wierd
    void SpawnConeRing()
    {
        if (currentConeHeight > coneHeight)
        {
            isSpawningCone = false;
            return;
        }

        float y = currentConeHeight;

        float t = y / coneHeight; // normalized height
        float radius = coneRadius * (1f - t);
        int bubblesInThisRing = Mathf.Max(1, Mathf.RoundToInt(maxBubblesPerRing * (1f - t)));

        for (int i = 0; i < bubblesInThisRing; i++)
        {
            float angle = 2f * Mathf.PI * i / bubblesInThisRing;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 localPos = new Vector3(x, y, z);

            bubbles.Add(new Bubble
            {
                localPosition = localPos,
                scale = bubbleScale,
                isPopped = false
            });
        }

        currentConeHeight += verticalStep;
    }


    void SpawnInConeCircle()
    {
        if (currentConeHeight > coneHeight)
        {
            isSpawningCone = false;
            return;
        }

        // Convert angular step to radians
        float stepRad = angularStep * Mathf.Deg2Rad;

        float radiusAtY = coneRadius * (1 - currentConeHeight / coneHeight);
        float x = Mathf.Cos(currentAngleRad) * radiusAtY;
        float z = Mathf.Sin(currentAngleRad) * radiusAtY;
        float y = currentConeHeight;

        Vector3 localPos = new Vector3(x, y, z);

        bubbles.Add(new Bubble
        {
            localPosition = localPos,
            scale = bubbleScale,
            isPopped = false
        });

        currentAngleRad += stepRad;

        if (currentAngleRad >= Mathf.PI * 2f)
        {
            currentAngleRad = 0f;
            currentConeHeight += verticalStep;
            angularStep += 15f;
        }
    }


    IEnumerator SpawnConeOverTime()
    {
        for (int i = 0; i < layerCount; i++)
        {
            float t = i / (float)(layerCount - 1); // normalized layer height
            float y = i * (coneHeight / (layerCount - 1));
            float radius = coneRadius * (1f - t);
            // int bubbleCount = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(maxBubblesAtBase, 1, t)));
            int bubbleCount = maxBubblesAtBase - i;

            float angleStep = 2f * Mathf.PI / bubbleCount;

            Debug.Log("bubble count for layer:: " + i + " is::" + bubbleCount);


            for (int j = 0; j < bubbleCount; j++)
            {
                float angle = j * angleStep;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                Vector3 localPos = new Vector3(x, y, z);

                bubbles.Add(new Bubble
                {
                    localPosition = localPos,
                    scale = bubbleScale,
                    isPopped = false
                });
            }

            yield return new WaitForSeconds(coneSpawnInterval); // ⏱️ Wait 1 sec before next layer
        }

        spawnConeCoroutine = null; // ✅ Allow re-trigger
    }


    IEnumerator CreateStaggeredCone()
    {
        float height = 2f;
        int layers = 10;
        float baseRadius = 1f;
        int maxBallsPerLayer = 24;

        for (int i = 0; i < layers; i++)
        {
            float t = (float)i / (layers - 1);
            float y = t * height;
            float radius = Mathf.Lerp(baseRadius, 0, t);


            if (radius <= 0.01f)
            {
                // Instantiate(ballPrefab, new Vector3(0, y, 0), Quaternion.identity, transform);
                bubbles.Add(new Bubble
                {
                    localPosition = new Vector3(0, y, 0),
                    scale = bubbleScale,
                    isPopped = false
                });
                continue;
            }


            // Determine number of balls in this ring based on radius
            int ballsInLayer = Mathf.Max(1, Mathf.RoundToInt(maxBallsPerLayer * (radius / baseRadius)));
            float angleStep = (Mathf.PI * 2f) / ballsInLayer;

            // Stagger odd layers by offsetting the start angle
            float angleOffset = (i % 2 == 0) ? 0f : angleStep / 2f;

            Debug.Log("balls in layer: " + i + "is::" + ballsInLayer);

            for (int j = 0; j < ballsInLayer; j++)
            {
                float angle = angleOffset + j * angleStep;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                Vector3 localPos = new Vector3(x, y, z);
                // Instantiate(ballPrefab, position, Quaternion.identity, transform);

                bubbles.Add(new Bubble
                {
                    localPosition = localPos,
                    scale = bubbleScale,
                    isPopped = false
                });
            }

            yield return new WaitForSeconds(coneSpawnInterval);
        }

        spawnConeCoroutine = null;
    }


    //normal spawning on raycast hit...
    void Spawn()
    {
        // Spawn bubbles on left click
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                float size = UnityEngine.Random.Range(0.1f, 0.2f);
                // float size = 0.2f;
                Vector3 localPos = parentTransform.InverseTransformPoint(hit.point);

                // Check for existing bubble nearby
                float minDistance = /*size  */ 0.05f; // Can be adjusted depending on overlap tolerance
                foreach (var bubble in bubbles)
                {
                    if (!bubble.isPopped && Vector3.Distance(bubble.localPosition, localPos) < minDistance)
                    {
                        return; // Skip spawning if a bubble is already nearby
                    }
                }

                bubbles.Add(new Bubble
                {
                    localPosition = localPos,
                    scale = size,
                    isPopped = false
                });

            }
        }
    }

    void DestroyBubbleClick()
    {
        // Destroy bubble near click on right click
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue, 1f);

            float sphereRadius = 0.2f;
            float maxDistance = 100000f;

            if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, maxDistance))
            {
                TryDestroyInstanceNear(hit.point, destroyRadius);
            }
        }
    }

    void DisposeBubbles()
    {
        for (int i = 0; i < bubbleMatrices.Count; i++)
        {
            if (bubbles[i].isPopped)
            {
                bubbleMatrices.RemoveAt(i);
                bubbles.RemoveAt(i);
                i--;
            }
        }
    }

    void TryDestroyInstanceNear(Vector3 center, float destroyRadius)
    {
        for (int i = 0; i < bubbles.Count; i++)
        {
            if (bubbles[i].isPopped) continue;

            Vector3 worldPos = parentTransform.TransformPoint(bubbles[i].localPosition);
            if (Vector3.Distance(worldPos, center) < destroyRadius)
            {
                Bubble b = bubbles[i];
                // b.scale = 0f;
                b.localPosition -= new Vector3(b.localPosition.x, b.localPosition.y, b.localPosition.z) * 0.1f; // Move to origin for better visibility
                b.isPopped = true;
                bubbles[i] = b;
                // break; // Remove this if you want to pop *all* nearby bubbles
            }
        }
    }


}
