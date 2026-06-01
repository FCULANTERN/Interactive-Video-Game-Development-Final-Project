using UnityEngine;

/// <summary>
/// �Ĩ뫬�ĤH�G���ʧ֡B�ˮ`���A�i�J�d���|�W�O�᩹�e���A
/// ���ɼ�������S�ġ]�p VFX_Piercing_Ice�^�A�R�����a�y���ˮ`�C
/// �ݷf�t Damageable�BRigidbody�BCollider�C
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DashEnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Move (Chase)")]
    [Tooltip("�@��l���t�ס]��ĳ��@��ĤH�֡^")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;
    [Tooltip("�Z�����a�h���ɶ}�l�ǳƽĨ�")]
    public float dashTriggerDistance = 6f;
    [Tooltip("�����a�h��N�����������νĨ�")]
    public float meleeDistance = 1.5f;

    [Header("Dash")]
    [Tooltip("�Ĩ�e���W�O�ɶ�")]
    public float dashChargeTime = 0.4f;
    [Tooltip("�Ĩ�t��")]
    public float dashSpeed = 18f;
    [Tooltip("�Ĩ����ɶ�")]
    public float dashDuration = 0.35f;
    [Tooltip("�⦸�Ĩ붡�j")]
    public float dashCooldown = 2.5f;

    [Header("Damage")]
    [Tooltip("�Ĩ�R���Ϊ񨭧������ˮ`")]
    public int attackDamage = 4;
    [Tooltip("�Ĩ�R���P�w�b�|")]
    public float dashHitRadius = 1.2f;

    [Header("VFX")]
    [Tooltip("�������Ĩ�S�� Prefab�A�Ҧp VFX_Piercing_Ice")]
    public GameObject attackVFX;
    [Tooltip("�S���Y��")]
    public float vfxScale = 1f;
    [Tooltip("�S�Ĭ۹��ĤH���첾�]�q�` z �]���ȩ�b����e��^")]
    public Vector3 vfxLocalOffset = new Vector3(0, 1f, 0.5f);
    [Tooltip("�S�����[���ਤ�]�کԨ��^�AVFX_Piercing_Ice �¦V -X�A�q�` (0,90,0) �� (0,-90,0)")]
    public Vector3 vfxRotationOffset = new Vector3(0f, -90f, 0f);
    [Tooltip("���S�����ج�ʱ����V���ӡ��ʹL����y���y180")]
    public bool flipDirection = false;
    [Tooltip("�ʹ��H�ҌØ�ʊ�ڍ��b�ʹL�����­� ��0,0,0�^")]
    public bool zeroChildLocalPositions = true;

    private Rigidbody rb;
    private PlayerHealth playerHealth;
    private Damageable selfDamageable;
    private EnemyMouthAnimator mouthAnimator;

    private float stunTimer;
    private float dashCdTimer;

    private enum State { Chase, Charging, Dashing }
    private State state = State.Chase;
    private float stateTimer;
    private Vector3 dashDirection;
    private bool dashHasHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        selfDamageable = GetComponent<Damageable>();
        mouthAnimator = GetComponentInChildren<EnemyMouthAnimator>();
        FindPlayer();
    }

    public void Stun(float duration) { stunTimer = duration; }

    void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsDead) return;
        if (selfDamageable != null && selfDamageable.isDead) { enabled = false; return; }
        if (target == null) { FindPlayer(); return; }

        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            return;
        }

        dashCdTimer -= Time.fixedDeltaTime;

        switch (state)
        {
            case State.Chase: TickChase(); break;
            case State.Charging: TickCharging(); break;
            case State.Dashing: TickDashing(); break;
        }
    }

    // �w�w �l�� �w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w
    void TickChase()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        Vector3 dir = distance > 0.001f ? toTarget / distance : transform.forward;

        // ���V���a
        Quaternion targetRot = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));

        // �񨭪�����
        if (distance <= meleeDistance)
        {
            DealDamage();
            stunTimer = 0.6f; // �u�Ȱ��y�קK�C�V����
            return;
        }

        // �i�J�Ĩ�Z�� + �N�o�n�F �� �W�O
        if (distance <= dashTriggerDistance && dashCdTimer <= 0f)
        {
            EnterCharging();
            return;
        }

        // ���q�l��
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    // �w�w �W�O �w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w
    void EnterCharging()
    {
        state = State.Charging;
        stateTimer = dashChargeTime;
    }

    void TickCharging()
    {
        // �W�O������a�C�t���V���a
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * 2f * Time.fixedDeltaTime));
        }

        stateTimer -= Time.fixedDeltaTime;
        if (stateTimer <= 0f)
            EnterDashing();
    }

    // �w�w �Ĩ� �w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w
    void EnterDashing()
    {
        state = State.Dashing;
        stateTimer = dashDuration;
        dashHasHit = false;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        dashDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.forward;

        SetIgnorePlayerCollision(true);

        SpawnAttackVFX();

        if (mouthAnimator != null)
            mouthAnimator.PlayAttackAnimation();
    }

    void TickDashing()
    {
        // ���u���
        rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);

        // �Ĩ뤤�Ĥ@���I�쪱�a�N�y���ˮ`
        if (!dashHasHit && playerHealth != null && !playerHealth.IsDead)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= dashHitRadius + meleeDistance)
            {
                DealDamage();
                dashHasHit = true;
            }
        }

        stateTimer -= Time.fixedDeltaTime;
        if (stateTimer <= 0f)
        {
            EndDash();
        }
    }

    void EndDash()
    {
        state = State.Chase;
        dashCdTimer = dashCooldown;
        SetIgnorePlayerCollision(false);
    }

    /// <summary>
    /// �Ĩ�ɼȮɩ������a�I���A�קK���a�Q���_�Ϊ��_���Y�a
    /// </summary>
    void SetIgnorePlayerCollision(bool ignore)
    {
        if (target == null) return;
        Collider[] myCols = GetComponentsInChildren<Collider>();
        Collider[] playerCols = target.GetComponentsInChildren<Collider>();
        foreach (Collider mc in myCols)
        {
            if (mc == null) continue;
            foreach (Collider pc in playerCols)
            {
                if (pc == null) continue;
                Physics.IgnoreCollision(mc, pc, ignore);
            }
        }
    }

    void OnDisable()
    {
        // �קK�ƥ��Q��^�ɫO���L���I����
        if (state == State.Dashing)
            SetIgnorePlayerCollision(false);
    }

    // �w�w �u�� �w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w�w
    void DealDamage()
    {
        if (playerHealth != null)
            playerHealth.TakeDamage(attackDamage);
    }

    void SpawnAttackVFX()
    {
        if (attackVFX == null) return;

        Vector3 dir = flipDirection ? -dashDirection : dashDirection;

        Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(vfxRotationOffset);
        Vector3 pos = transform.position
                    + transform.right * vfxLocalOffset.x
                    + transform.up * vfxLocalOffset.y
                    + transform.forward * vfxLocalOffset.z;

        GameObject fx = Instantiate(attackVFX, pos, rot);
        fx.transform.localScale = Vector3.one * vfxScale;

        // 重置子物件的 LocalPosition，避免原 prefab 內部偏移造成「特效從旁邊飛來」
        if (zeroChildLocalPositions)
        {
            foreach (Transform child in fx.transform)
            {
                child.localPosition = Vector3.zero;
            }
        }

        ParticleSystem ps = fx.GetComponentInChildren<ParticleSystem>();
        float life = 2f;
        if (ps != null)
            life = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(fx, life);
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    // �s�边��ܻ��U�d��
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashTriggerDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeDistance);
    }
}