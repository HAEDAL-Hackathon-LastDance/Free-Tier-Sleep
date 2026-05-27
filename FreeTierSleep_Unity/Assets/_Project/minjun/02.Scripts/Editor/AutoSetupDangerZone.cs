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
            childSprite.sortingLayerID = parentSprite.sortingLayerID;
            childSprite.sortingOrder = parentSprite.sortingOrder + 1; // 부모보다 살짝 앞에 렌더링
            childSprite.drawMode = SpriteDrawMode.Simple;

            // 4. 크기 및 위치 조정 (하단 1/3)
            // DangerZonePulse에서 64x64 (PPU 100) 텍스처를 생성하므로 기본 크기는 0.64 x 0.64 유닛입니다.
            // 이를 부모의 실제 크기에 맞게 스케일링합니다.
            float parentWidth = parentSprite.bounds.size.x / dataFlood.transform.lossyScale.x;
            float parentHeight = parentSprite.bounds.size.y / dataFlood.transform.lossyScale.y;
            
            float targetWidth = parentWidth;
            float targetHeight = parentHeight / 3f;

            dangerZoneObj.transform.localScale = new Vector3(targetWidth / 0.64f, targetHeight / 0.64f, 1f);
            
            // 피벗이 중앙이라고 가정할 때 하단 1/3 위치로 이동
            dangerZoneObj.transform.localPosition = new Vector3(0f, -parentHeight / 3f, 0f);
        }
        else
        {
            Debug.LogWarning("RisingDataFlood에 SpriteRenderer가 없습니다. 수동으로 크기를 설정해 주세요.");
        }

        // 5. 다채로운 가로줄 글리치 생성 스크립트 부착
        dangerZoneObj.AddComponent<DangerZonePulse>();

        // 변경 사항 저장 처리
        EditorUtility.SetDirty(dataFlood.gameObject);
        Debug.Log("DangerZone(즉사 구역 시각화) 자동 세팅이 완료되었습니다! 씬을 저장해 주세요.");
    }
}
