using UnityEngine;
using UnityEngine.UI;

namespace Taehui
{
    /// <summary>
    /// 광케이블을 따라 흐르는 발광 데이터 패킷 효과를 연출하는 스크립트
    /// </summary>
    public class CableDataFlow : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float flowSpeed = 150f; // 흐름 속도
        [SerializeField] private int packetCount = 3;     // 화면에 보일 패킷 개수
        [SerializeField] private Color packetColor = new Color(0f, 1f, 0.9f, 1f); // 쨍한 네온 시안 컬러

        private RectTransform[] packets;
        private float parentHeight;

        private void Start()
        {
            RectTransform parentRect = GetComponent<RectTransform>();
            parentHeight = parentRect.rect.height;

            packets = new RectTransform[packetCount];
            for (int i = 0; i < packetCount; i++)
            {
                GameObject packetObj = new GameObject($"Packet_{i}");
                packetObj.transform.SetParent(transform, false);
                
                Image img = packetObj.AddComponent<Image>();
                img.color = packetColor;
                
                // 패킷의 발광 연출을 돕기 위해 은은한 잔상 느낌의 사각형으로 크기 설정
                RectTransform rt = packetObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(parentRect.rect.width, 16f); // 케이블 굵기에 맞추고 세로로 살짝 길게
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                
                // 간격을 두고 초기 위치 분배
                float startY = (parentHeight / packetCount) * i;
                rt.anchoredPosition = new Vector2(0f, startY);

                packets[i] = rt;
            }
        }

        private void Update()
        {
            if (packets == null) return;

            for (int i = 0; i < packets.Length; i++)
            {
                if (packets[i] == null) continue;

                Vector2 pos = packets[i].anchoredPosition;
                // 머리 뒤(0)에서 시작해 화면 밖(parentHeight)으로 흘러가는 데이터 흐름
                pos.y += flowSpeed * Time.deltaTime;

                // 끝에 다다르면 다시 시작 위치(0)로 루프
                if (pos.y > parentHeight)
                {
                    pos.y = 0f;
                }

                packets[i].anchoredPosition = pos;
            }
        }
    }
}
