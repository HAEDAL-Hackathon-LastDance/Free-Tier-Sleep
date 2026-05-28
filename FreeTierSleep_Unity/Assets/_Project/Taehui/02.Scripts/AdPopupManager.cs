using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Taehui
{
    /// <summary>
    /// 화면에 무작위로 광고 팝업 프리팹을 생성하는 매니저
    /// </summary>
    public class AdPopupManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject[] adPrefabs;
        
        [Header("Spawn Settings")]
        [SerializeField] private RectTransform spawnArea; // 팝업이 생성될 UI 영역
        [SerializeField] private float minSpawnDelay = 0.1f;
        [SerializeField] private float maxSpawnDelay = 0.5f;

        private List<GameObject> activePopups = new List<GameObject>();
        private bool isSpawning = false;
        private AudioSource audioSource;
        private AudioClip warningClip;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.25f; // 광고 팝업이 뜰 때 너무 불쾌하지 않을 강도의 소리

            warningClip = ProceduralAudioHelper.CreateWarningSound();
        }

        public void StartSpawning()
        {
            if (isSpawning) return;
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }

        public void StopSpawning()
        {
            isSpawning = false;
            StopAllCoroutines();
        }

        public void ClearPopups()
        {
            foreach (var popup in activePopups)
            {
                if (popup != null) Destroy(popup);
            }
            activePopups.Clear();
        }

        private IEnumerator SpawnRoutine()
        {
            while (isSpawning)
            {
                SpawnAd();
                float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(delay);
            }
        }

        private void SpawnAd()
        {
            if (adPrefabs == null || adPrefabs.Length == 0) return;

            GameObject prefab = adPrefabs[Random.Range(0, adPrefabs.Length)];
            GameObject popup = Instantiate(prefab, spawnArea);

            // 랜덤 위치 설정 (UI 좌표 기준)
            Rect rect = spawnArea.rect;
            float x = Random.Range(rect.xMin, rect.xMax);
            float y = Random.Range(rect.yMin, rect.yMax);
            
            popup.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            activePopups.Add(popup);

            // 팝업 경고 SFX 재생
            if (audioSource != null && warningClip != null)
            {
                audioSource.PlayOneShot(warningClip);
            }
        }
    }
}
