using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private float vertexLifetime = 1f;
    [SerializeField] private int maxInk = 15;
    [SerializeField] private float minPointDistance = 0.1f;

    // ⭐️ 추가 1: 쿨타임 설정을 위한 변수들
    [SerializeField] private float drawCooldown = 3.0f; // 3초 제한 (인스펙터에서 수정 가능!)
    private float lastDrawTime = -10f; // 마지막으로 선을 그은 시간 (시작하자마자 그릴 수 있게 음수로 넉넉히 세팅)

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

        // ⭐️ 수정 2: 마우스를 누를 때 쿨타임이 지났는지 확인하는 조건 추가
        if (Input.GetMouseButtonDown(0))
        {
            // 현재 시간이 (마지막으로 그은 시간 + 3초)를 넘었을 때만 그리기 허용
            if (Time.time >= lastDrawTime + drawCooldown)
            {
                StartNewStroke();
                lastDrawTime = Time.time; // ⭐️ 선을 그은 현재 시간을 기록
            }
            else
            {
                // (확인용) 아직 3초가 안 지났으면 유니티 콘솔 창에 메시지 띄우기
                Debug.Log("쿨타임 중! 남은 시간: " + (lastDrawTime + drawCooldown - Time.time).ToString("F1") + "초");
            }
        }

        // 이 아래는 원래 코드와 동일합니다!
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
