using UnityEngine;
using UnityEditor;

public class AutoSetupDangerZone : Editor
{
    [MenuItem("Tools/Free Tier Sleep/Auto Setup Danger Zone")]
    public static void SetupDangerZone()
    {
        // 1. 씬에서 RisingDataFlood 찾기
        RisingDataFlood dataFlood = Object.FindFirstObjectByType<RisingDataFlood>();
        if (dataFlood == null)
        {
            Debug.LogError("씬에서 RisingDataFlood를 찾을 수 없습니다.");
            return;
        }

        // 이미 생성되어 있는지 확인
        Transform existingZone = dataFlood.transform.Find("DangerZone");
        if (existingZone != null)
        {
            Debug.LogWarning("이미 DangerZone 오브젝트가 존재합니다. 기존 것을 삭제하거나 그대로 사용해 주세요.");
            return;
        }

        // 2. 자식 오브젝트 생성
        GameObject dangerZoneObj = new GameObject("DangerZone");
        dangerZoneObj.transform.SetParent(dataFlood.transform);
        dangerZoneObj.transform.localRotation = Quaternion.identity;

        // 3. SpriteRenderer 추가 및 설정
        SpriteRenderer parentSprite = dataFlood.GetComponent<SpriteRenderer>();
        SpriteRenderer childSprite = dangerZoneObj.AddComponent<SpriteRenderer>();

        if (parentSprite != null)
        {
            childSprite.sprite = parentSprite.sprite;
            childSprite.sortingLayerID = parentSprite.sortingLayerID;
            childSprite.sortingOrder = parentSprite.sortingOrder + 1; // 부모보다 살짝 앞에 렌더링
            childSprite.drawMode = parentSprite.drawMode;
            
            // 핵심: 부모의 글리치 머티리얼을 그대로 가져와서 이질감 제거
            childSprite.material = parentSprite.sharedMaterial;

            // 4. 크기 및 위치 조정 (하단 1/3)
            if (parentSprite.drawMode == SpriteDrawMode.Simple)
            {
                // Simple 모드일 경우 Scale로 조절
                dangerZoneObj.transform.localScale = new Vector3(1f, 1f / 3f, 1f);
                // 피벗이 중앙(0.5)이라고 가정할 때, 하단 1/3의 중심은 로컬 좌표 -0.3333
                dangerZoneObj.transform.localPosition = new Vector3(0f, -0.3333f, 0f);
            }
            else
            {
                // Tiled나 Sliced 모드일 경우 Size 속성으로 조절
                dangerZoneObj.transform.localScale = Vector3.one;
                childSprite.size = new Vector2(parentSprite.size.x, parentSprite.size.y / 3f);
                // 피벗이 중앙이라고 가정할 때 위치 계산
                dangerZoneObj.transform.localPosition = new Vector3(0f, -parentSprite.size.y / 3f, 0f);
            }
        }
        else
        {
            Debug.LogWarning("RisingDataFlood에 SpriteRenderer가 없습니다. 수동으로 이미지와 크기를 설정해 주세요.");
            dangerZoneObj.transform.localScale = new Vector3(1f, 1f / 3f, 1f);
            dangerZoneObj.transform.localPosition = new Vector3(0f, -0.3333f, 0f);
        }

        // 5. 경고등 깜빡임 및 떨림 효과 스크립트 부착
        dangerZoneObj.AddComponent<DangerZonePulse>();

        // 변경 사항 저장 처리
        EditorUtility.SetDirty(dataFlood.gameObject);
        Debug.Log("DangerZone(즉사 구역 시각화) 자동 세팅이 완료되었습니다! 씬을 저장해 주세요.");
    }
}
