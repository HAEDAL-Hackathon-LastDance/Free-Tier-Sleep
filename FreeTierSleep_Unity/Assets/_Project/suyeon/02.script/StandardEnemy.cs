using UnityEngine;

public class StandardEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 1.0f; // 1�ʸ��� �������� ��
    private float attackTimer = 100f;   // ù Ÿ���� ���ڸ��� �ٷ� �������� ū ���ڷ� ����
    [Header("Enemy Settings")]
    public float moveSpeed = 1.0f;     // ���� �ӵ��� ���� �̵�
    public int attackDamage = 10;      // �ھ �� ������

    [Header("Target Tracking")]
    public Transform coreTransform;    // �߾� �ھ� Transform ����
    void Start()
    {
        // [�߿�] ���� ���� ��, ���� �ִ� "Player"��� �±׸� ���� ��¥ ������Ʈ�� ã�Ƽ� Ÿ������ ��ƶ�!
        GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");
        if (realPlayer != null)
        {
            coreTransform = realPlayer.transform;
        }
    }
    void Update()
    {
        // �ھ ���ӿ� ������ ���� �̵�
        if (coreTransform != null)
        {
            // ������ �䱸����: Vector2.MoveTowards ���
            // ���� ��ġ���� �ھ� ��ġ�� ���� moveSpeed��ŭ �����ϰ� ����
            transform.position = Vector2.MoveTowards(transform.position, coreTransform.position, moveSpeed * Time.deltaTime);
        }
        // �� �����Ӹ��� Ÿ�̸� �ð��� deltaTime�� ������
        attackTimer += Time.deltaTime;
    }

    // Enter ��� Stay�� ���� ����ִ� ���� ��� �����!
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Ÿ�̸Ӱ� ��Ÿ��(1��)�� �Ѱ��� ���� �������� ��
            if (attackTimer >= attackCooldown)
            {
                PlayerHealth playerHP = other.GetComponent<PlayerHealth>();

                if (playerHP != null)
                {
                    playerHP.TakeDamage(attackDamage);

                    // ?? �÷��̾� ������Ʈ�� �پ��ִ� AudioSource(������)�� ã�� ����ض�!
                    AudioSource playerAudio = other.GetComponent<AudioSource>();
                    if (playerAudio != null)
                    {
                        playerAudio.Play();
                    }

                    attackTimer = 0f; // �������ϱ� Ÿ�̸Ӹ� �ٽ� 0���� �ʱ�ȭ!
                }
            }
        }
    }
}
