using System.Collections.Generic;
using UnityEngine;

public class Stroke : MonoBehaviour
{
    private LineRenderer _lr;
    private EdgeCollider2D _col;
    private float _vertexLifetime;
    private bool _initialized;
    private bool _hasPoints;

    private struct VertexData
    {
        public Vector3 position;
        public float spawnTime;
    }

    private Queue<VertexData> _vertices = new Queue<VertexData>();

    public int VertexCount => _vertices.Count;

    public void Initialize(float lifetime)
    {
        _vertexLifetime = lifetime;

        LineRenderer existingLr = gameObject.GetComponent<LineRenderer>();
        _lr = existingLr != null ? existingLr : gameObject.AddComponent<LineRenderer>();

        EdgeCollider2D existingCol = gameObject.GetComponent<EdgeCollider2D>();
        _col = existingCol != null ? existingCol : gameObject.AddComponent<EdgeCollider2D>();

        _lr.startWidth = 0.15f;
        _lr.endWidth = 0.08f;
        _lr.material = new Material(Shader.Find("Sprites/Default"));
        // 모노크롬 픽셀아트에 녹아드는 desaturated 블루-화이트 (디지털 방화벽 느낌)
        _lr.startColor = new Color(0.65f, 0.83f, 0.97f, 0.95f);
        _lr.endColor   = new Color(0.50f, 0.70f, 0.90f, 0.45f);
        _lr.useWorldSpace = true;
        _lr.positionCount = 0;

        _col.isTrigger = true;

        _initialized = true;
    }

    void Update()
    {
        if (!_initialized) return;

        RemoveExpiredVertices();
        SyncRenderers();

        if (_hasPoints && _vertices.Count == 0)
            Destroy(gameObject);
    }

    public void TryAddPoint(Vector3 newPos, float minDist, int remainingInk)
    {
        if (!_initialized || remainingInk <= 0) return;

        if (_vertices.Count > 0)
        {
            VertexData[] arr = _vertices.ToArray();
            if (Vector3.Distance(arr[arr.Length - 1].position, newPos) < minDist) return;
        }

        _vertices.Enqueue(new VertexData { position = newPos, spawnTime = Time.time });
        _hasPoints = true;
    }

    public void EatLastVertex()
    {
        if (_vertices.Count == 0) return;
        VertexData[] arr = _vertices.ToArray();
        _vertices.Clear();
        for (int i = 0; i < arr.Length - 1; i++)
            _vertices.Enqueue(arr[i]);
    }

    private void RemoveExpiredVertices()
    {
        while (_vertices.Count > 0 && Time.time - _vertices.Peek().spawnTime > _vertexLifetime)
            _vertices.Dequeue();
    }

    private void SyncRenderers()
    {
        VertexData[] arr = _vertices.ToArray();
        _lr.positionCount = arr.Length;

        Vector2[] colPoints = new Vector2[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            _lr.SetPosition(i, arr[i].position);
            colPoints[i] = new Vector2(arr[i].position.x, arr[i].position.y);
        }

        _col.points = arr.Length >= 2 ? colPoints : new Vector2[0];
    }
}
