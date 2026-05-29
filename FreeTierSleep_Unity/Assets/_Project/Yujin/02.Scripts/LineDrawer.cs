using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private float vertexLifetime = 6f;
    [SerializeField] private int maxInk = 300;
    [SerializeField] private float minPointDistance = 0.1f;

    private List<Stroke> _activeStrokes = new List<Stroke>();
    private Stroke _currentStroke;

    public int CurrentInk
    {
        get
        {
            int total = 0;
            foreach (var s in _activeStrokes)
                if (s != null) total += s.VertexCount;
            return total;
        }
    }
    public int MaxInk => maxInk;

    void Update()
    {
        _activeStrokes.RemoveAll(s => s == null);

        if (Input.GetMouseButtonDown(0))
            StartNewStroke();

        if (Input.GetMouseButton(0) && _currentStroke != null)
            _currentStroke.TryAddPoint(GetDrawPosition(), minPointDistance, maxInk - CurrentInk);

        if (Input.GetMouseButtonUp(0))
            _currentStroke = null;

        if (Input.GetMouseButtonDown(1))
            Clear();
    }

    private void StartNewStroke()
    {
        GameObject obj = new GameObject("Stroke");
        _currentStroke = obj.AddComponent<Stroke>();
        obj.AddComponent<LineCollision>();
        _currentStroke.Initialize(vertexLifetime);
        _activeStrokes.Add(_currentStroke);
    }

    private Vector3 GetDrawPosition()
    {
        if (LagCursor.Instance != null)
            return LagCursor.Instance.WorldPosition;

        Vector3 pos = Input.mousePosition;
        pos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 world = Camera.main.ScreenToWorldPoint(pos);
        world.z = 0f;
        return world;
    }

    public void Clear()
    {
        foreach (var s in _activeStrokes)
            if (s != null) Destroy(s.gameObject);
        _activeStrokes.Clear();
        _currentStroke = null;
    }
}
