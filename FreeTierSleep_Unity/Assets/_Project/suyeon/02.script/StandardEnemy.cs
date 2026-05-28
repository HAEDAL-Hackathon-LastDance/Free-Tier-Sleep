using System.Collections;
using UnityEngine;

public class StandardEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 1.0f;
    private float attackTimer = 100f;

    [Header("Enemy Settings")]
    public float moveSpeed = 1.0f;
    public int attackDamage = 10;

    [Header("Target Tracking")]
    public Transform coreTransform;

    private bool _isDying = false;
    private SpriteRenderer _sr;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();

        GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");
        if (realPlayer != null)
            coreTransform = realPlayer.transform;
    }

    void Update()
    {
        if (_isDying) return;

        if (coreTransform != null)
            transform.position = Vector2.MoveTowards(transform.position, coreTransform.position, moveSpeed * Time.deltaTime);

        attackTimer += Time.deltaTime;
        CheckLineCollision();
    }

    void CheckLineCollision()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.4f);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Stroke>() != null)
            {
                StartCoroutine(FadeAndDestroy());
                return;
            }
        }
    }

    IEnumerator FadeAndDestroy()
    {
        _isDying = true;

        float duration = 0.35f;
        float elapsed = 0f;
        Color original = _sr != null ? _sr.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            if (_sr != null)
                _sr.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (_isDying) return;

        if (other.CompareTag("Player"))
        {
            if (attackTimer >= attackCooldown)
            {
                PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
                if (playerHP != null)
                {
                    playerHP.TakeDamage(attackDamage);

                    AudioSource playerAudio = other.GetComponent<AudioSource>();
                    if (playerAudio != null)
                        playerAudio.Play();

                    attackTimer = 0f;
                }
            }
        }
    }
}
