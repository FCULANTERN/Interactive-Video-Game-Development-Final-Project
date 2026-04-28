using UnityEngine;
using UnityEngine.InputSystem;

public class SlashCircleCaster : MonoBehaviour
{
    [Header("References")]
    public GameObject slashCirclePrefab;
    public Transform spawnPoint;

    [Header("Cast Settings")]
    public Key castKey = Key.O;
    public float cooldown = 0.5f;
    public float destroyAfter = 3f;

    [Header("Rotation Offset")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    private float cooldownTimer = 0f;

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[castKey].wasPressedThisFrame && cooldownTimer <= 0f)
        {
            CastSlashCircle();
            cooldownTimer = cooldown;
        }
    }

    void CastSlashCircle()
    {
        if (slashCirclePrefab == null)
        {
            Debug.LogWarning("SlashCircleCaster: slashCirclePrefab 沒有指定。");
            return;
        }

        Transform pointToUse = spawnPoint != null ? spawnPoint : transform;

        Quaternion finalRotation =
            pointToUse.rotation * Quaternion.Euler(rotationOffset);

        GameObject fx = Instantiate(
            slashCirclePrefab,
            pointToUse.position,
            finalRotation
        );

        if (destroyAfter > 0f)
        {
            Destroy(fx, destroyAfter);
        }
    }
}