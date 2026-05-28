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

        _lr.startWidth = 0.07f;
        _lr.endWidth = 0.04f;
        // URP 호환 셰이더 우선, 없으면 레거시 사용
        Shader lineShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                         ?? Shader.Find("Sprites/Default");
        _lr.material = new Material(lineShader);
        // 탁한 흰색 그라데이션 - 어두운 배경에 자연스럽게 보이는 오프화이트
        _lr.startColor = new Color(0.85f, 0.85f, 0.83f, 0.88f);
        _lr.endColor   = new Color(0.60f, 0.60f, 0.58f, 0.20f);
        _lr.useWorldSpace = true;
        _lr.positionCount = 0;

        _col.isTrigger = false;

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
