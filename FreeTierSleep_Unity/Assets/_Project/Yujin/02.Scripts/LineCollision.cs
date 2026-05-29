using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Stroke))]
public class LineCollision : MonoBehaviour
{
    [SerializeField] private float heavyLingerTime = 2f;

    private Stroke _stroke;

    void Awake() => _stroke = GetComponent<Stroke>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GluttonEnemy")) return;

        if (other.CompareTag("HeavyEnemy"))
        {
            StartCoroutine(DestroyAfterDelay(other.gameObject, heavyLingerTime));
            return;
        }

        if (other.CompareTag("Enemy"))
            Destroy(other.gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("GluttonEnemy"))
            _stroke.EatLastVertex();
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) Destroy(obj);
    }
}
