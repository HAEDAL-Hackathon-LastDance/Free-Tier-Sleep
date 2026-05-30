using UnityEngine;

// 카메라 전체를 덮는 몽환적 배경 (URP 언릿 셰이더 기반)
// - 카메라에 따라 위치·크기가 자동 추적됨
// - Custom/DreamyBackground 셰이더를 런타임에 찾아 Material 생성
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DreamyBackground : MonoBehaviour
{
    [Header("색상 (셰이더 프로퍼티)")]
    public Color colorA = new Color(0.05f, 0.05f, 0.30f);
    public Color colorB = new Color(0.15f, 0.02f, 0.45f);
    public Color colorC = new Color(0.00f, 0.08f, 0.35f);
    public Color colorD = new Color(0.10f, 0.00f, 0.28f);

    [Header("애니메이션")]
    [Range(0.05f, 3f)]  public float waveSpeed       = 0.50f;
    [Range(1f,   10f)]  public float waveFreq        = 3.00f;
    [Range(0f,   0.4f)] public float waveAmp         = 0.15f;
    [Range(0.1f, 3f)]   public float shimmerSpeed    = 1.20f;
    [Range(1f,   12f)]  public float shimmerScale    = 5.00f;
    [Range(0f,   0.4f)] public float shimmerIntensity = 0.08f;
    [Range(0.02f, 1f)]  public float colorCycleSpeed = 0.18f;

    private Camera      cam;
    private MeshRenderer mr;
    private Material    mat;

    private void Awake()
    {
        cam = Camera.main;
        CreateMesh();
        CreateMaterial();
    }

    private void CreateMesh()
    {
        var mesh = new Mesh { name = "DreamyBG_Quad" };
        mesh.vertices  = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f),
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void CreateMaterial()
    {
        mr = GetComponent<MeshRenderer>();

        Shader shader = Shader.Find("Custom/DreamyBackground");
        if (shader == null)
        {
            mr.enabled = false;
            return;
        }

        mat = new Material(shader) { name = "DreamyBackground_Runtime" };
        ApplyProperties();

        mr.sharedMaterial   = mat;
        mr.sortingLayerName = "Default";
        mr.sortingOrder     = -9999;
        mr.shadowCastingMode    = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows       = false;
        mr.lightProbeUsage      = UnityEngine.Rendering.LightProbeUsage.Off;
        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }

    private void ApplyProperties()
    {
        if (mat == null) return;
        mat.SetColor("_ColorA",           colorA);
        mat.SetColor("_ColorB",           colorB);
        mat.SetColor("_ColorC",           colorC);
        mat.SetColor("_ColorD",           colorD);
        mat.SetFloat("_WaveSpeed",        waveSpeed);
        mat.SetFloat("_WaveFreq",         waveFreq);
        mat.SetFloat("_WaveAmp",          waveAmp);
        mat.SetFloat("_ShimmerSpeed",     shimmerSpeed);
        mat.SetFloat("_ShimmerScale",     shimmerScale);
        mat.SetFloat("_ShimmerIntensity", shimmerIntensity);
        mat.SetFloat("_ColorCycleSpeed",  colorCycleSpeed);
    }

    private void LateUpdate()
    {
        if (cam == null) { cam = Camera.main; return; }

        // 정확한 카메라 뷰 크기 계산을 시도하지 않고, 충분히 크게 유지.
        // SortingOrder = -9999 이므로 크기가 커도 다른 오브젝트에 영향 없음.
        // orthographicSize × 4 = 화면 높이의 2배 → 어떤 종횡비·설정에서도 커버 보장.
        float h = cam.orthographicSize * 4f;
        float w = h * Mathf.Max(cam.aspect, (float)Screen.width / Screen.height);

        Vector3 camPos = cam.transform.position;
        transform.position   = new Vector3(camPos.x, camPos.y, camPos.z + 10f);
        transform.localScale = new Vector3(w, h, 1f);
    }

    private void OnValidate()
    {
        ApplyProperties();
    }
}
