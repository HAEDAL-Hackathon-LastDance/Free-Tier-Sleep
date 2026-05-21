using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
    [SerializeField] private float minPointDistance = 0.1f;

    private LineRenderer _lr;
    private bool _isDrawing;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = 0;
        _lr.startWidth = 0.1f;
        _lr.endWidth = 0.1f;
        _lr.material = new Material(Shader.Find("Sprites/Default"));
        _lr.startColor = Color.cyan;
        _lr.endColor = Color.cyan;
        _lr.useWorldSpace = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartDrawing();

        if (Input.GetMouseButton(0) && _isDrawing)
            ContinueDrawing();

        if (Input.GetMouseButtonUp(0))
            _isDrawing = false;

        if (Input.GetMouseButtonDown(1))   // 우클릭 → 선 전체 삭제
            Clear();
    }

    private void StartDrawing()
    {
        _isDrawing = true;
        _lr.positionCount = 0;
        AddPoint(GetWorldPosition());
    }

    private void ContinueDrawing()
    {
        Vector3 current = GetWorldPosition();
        Vector3 last = _lr.GetPosition(_lr.positionCount - 1);

        if (Vector3.Distance(current, last) >= minPointDistance)
            AddPoint(current);
    }

    protected virtual void AddPoint(Vector3 point)
    {
        _lr.positionCount++;
        _lr.SetPosition(_lr.positionCount - 1, point);
    }

    protected Vector3 GetWorldPosition()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;
        return pos;
    }

    public void Clear()
    {
        _lr.positionCount = 0;
        _isDrawing = false;
    }

    public LineRenderer GetLineRenderer() => _lr;
}
