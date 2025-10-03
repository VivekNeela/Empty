using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConeMeshGenerator : MonoBehaviour
{
    [Header("Cone Settings")]
    public float height = 5f;
    public float radius = 2f;
    public int segments = 24;
    public Material coneMat;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Vector3[] vertices;
    private Vector3 tip;


    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.mesh = GenerateCone();
        if (meshRenderer.material == null)
        {
            meshRenderer.material = coneMat;
        }
    }

    void OnValidate()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            // if (meshRenderer.material == null)
            // {
            //     meshRenderer.material = coneMat;
            // }
        }
        meshFilter.mesh = GenerateCone();
    }

    private void Update()
    {
        tip = transform.TransformPoint(vertices[0]);
        // Debug.Log("tip of cone is at " + tip);
        // coneMat.SetVector("_ConeTip", tip);
        meshRenderer.material.SetVector("_ConeTip", tip);
    }

    private void OnDrawGizmos()
    {
        // if (vertices == null || vertices.Length == 0)
        //     return;

        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(tip, 1.1f);

        // Gizmos.color = Color.green;
        // for (int i = 1; i < vertices.Length; i++)
        // {
        //     Vector3 worldPos = transform.TransformPoint(vertices[i]);
        //     Gizmos.DrawSphere(worldPos, 0.05f);
        //     Gizmos.DrawLine(tip, worldPos);
        // }
    }


    Mesh GenerateCone()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralCone";

        int vertexCount = segments + 2; // +1 apex, +1 center base
        vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int[] triangles = new int[segments * 3 * 2]; // sides + base

        // Apex (tip of cone at origin)
        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 1f);

        // var tip = transform.TransformPoint(vertices[0]);
        // Debug.Log("tip of cone is at " + tip);


        // Base circle vertices
        float angleStep = 2 * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i + 1] = new Vector3(x, -height, z);
            uv[i + 1] = new Vector2((float)i / segments, 0f);
        }

        // Base center
        vertices[segments + 1] = new Vector3(0, -height, 0);
        uv[segments + 1] = new Vector2(0.5f, 0f);

        int triIndex = 0;

        // Side triangles (apex to base circle)
        for (int i = 0; i < segments; i++)
        {
            int current = i + 1;
            int next = (i + 1) % segments + 1;

            triangles[triIndex++] = 0;      // apex
            triangles[triIndex++] = next;   // next base
            triangles[triIndex++] = current;// current base
        }

        // Base triangles
        int baseCenterIndex = segments + 1;
        for (int i = 0; i < segments; i++)
        {
            int current = i + 1;
            int next = (i + 1) % segments + 1;

            triangles[triIndex++] = baseCenterIndex;
            triangles[triIndex++] = current;
            triangles[triIndex++] = next;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }




}
