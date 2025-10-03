using UnityEngine;
using UnityEngine.UI;

public class ColoringManager : MonoBehaviour
{
    [Header("Canvas")]
    public int canvasWidth = 1024;
    public int canvasHeight = 1024;
    public Material brushMaterial; // Material with BrushStampNoTex shader
    public Material spriteMaterial; // Material with BrushStampNoTex shader
    public RawImage display;       // UI RawImage to show the canvas
    public SpriteRenderer sprite;
    // public Texture2D tex;


    [Header("Brush Settings")]
    public Color brushColor = Color.black;
    [Range(1, 200)] public float brushSizePixels = 32;
    [Range(0, 1)] public float opacity = 1f;
    [Range(0, 1)] public float hardness = 0.8f;
    public float stepSize = 0.01f;

    private RenderTexture rtA, rtB;
    [SerializeField] private bool toggle;



    void Start()
    {
        // Create two RenderTextures for ping-pong painting
        rtA = new RenderTexture(canvasWidth, canvasHeight, 0, RenderTextureFormat.ARGB32);
        rtB = new RenderTexture(canvasWidth, canvasHeight, 0, RenderTextureFormat.ARGB32);
        rtA.Create();
        rtB.Create();

        // Clear canvas to white
        ClearCanvas(Color.white);

        // sprite.material = new Material(brushMaterial);

        // Debug.Log("SPRITE MAT::" + sprite.material.name);

        sprite.material = spriteMaterial;

        // Show the current render texture in UI
        if (display) display.texture = GetCurrentRT();

        sprite.material.SetTexture("_SubTex", GetCurrentRT());

        // sprite.sprite = Texture2DToSprite(RenderTextureToTexture2D(GetCurrentRT()));
    }

    private Vector2? lastUV = null;
    void Update()
    {
        if (Input.GetMouseButton(0)) // hold left mouse
        {
            //for display
            Vector2 uv = new Vector2(
                Input.mousePosition.x / Screen.width,
                Input.mousePosition.y / Screen.height
            );

            //for sprite
            // Vector2 uv = new Vector2(
            //     Input.mousePosition.x / sprite.size.x,
            //     Input.mousePosition.y / sprite.size.y
            // );

            //interpolation logic for smooth painting
            if (lastUV == null)
                PaintAt(uv);
            else
            {
                // interpolate between lastUV and current uv
                Vector2 prev = lastUV.Value;
                float dist = Vector2.Distance(prev, uv);

                // choose spacing relative to brush size
                // float step = 0.01f; // smaller = denser stamps
                int steps = Mathf.CeilToInt(dist / stepSize);

                for (int i = 1; i <= steps; i++)
                {
                    Vector2 lerped = Vector2.Lerp(prev, uv, i / (float)steps);
                    PaintAt(lerped);
                }
            }

            lastUV = uv;

        }
        else
            lastUV = null;


        // Update UI
        if (display) display.texture = GetCurrentRT();

        //sprite stuff
        sprite.material.SetTexture("_SubTex", GetCurrentRT());
    }


    void PaintAt(Vector2 uv)
    {
        // Set brush params
        brushMaterial.SetTexture("_MainTex", GetCurrentRT());
        brushMaterial.SetColor("_BrushColor", brushColor);
        brushMaterial.SetVector("_BrushCenter", new Vector4(uv.x, uv.y, 0, 0));
        brushMaterial.SetFloat("_BrushSize", brushSizePixels / (float)canvasWidth);
        brushMaterial.SetFloat("_Opacity", opacity);
        brushMaterial.SetFloat("_Hardness", hardness);

        // Blit from current to next RT
        Graphics.Blit(GetCurrentRT(), GetNextRT(), brushMaterial);

        //setting the sprite main tex
        spriteMaterial.SetTexture("_MainTex", GetCurrentRT());

        toggle = !toggle;
    }

    RenderTexture GetCurrentRT() => toggle ? rtB : rtA;
    RenderTexture GetNextRT() => toggle ? rtA : rtB;


    [ContextMenu("Clear Page")]
    public void ClearCanvas(Color color)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = GetCurrentRT();
        GL.Clear(true, true, color);
        RenderTexture.active = prev;
    }

    void OnDestroy()
    {
        if (rtA) rtA.Release();
        if (rtB) rtB.Release();
    }


}
