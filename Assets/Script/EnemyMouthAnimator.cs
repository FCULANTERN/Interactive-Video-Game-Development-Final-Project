using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class EnemyMouthAnimator : MonoBehaviour
{
    // 攻擊動畫的 Offset 順序：左上(預設) -> 右上 -> 左下 -> 右下
    private static readonly Vector2[] attackOffsets = new Vector2[]
    {
        new Vector2(0f,   0.5f), // 左上：大嘴張開（預設）
        new Vector2(0.5f, 0.5f), // 右上：小嘴張開
        new Vector2(0f,   0f),   // 左下：閉嘴
        new Vector2(0.5f, 0f),   // 右下：緊閉
    };

    public float frameRate = 0.1f; // 每幀播放速度（秒）

    private DecalProjector decalProjector;
    private bool isPlaying = false;

    void Start()
    {
        decalProjector = GetComponentInChildren<DecalProjector>();
        // 設定預設狀態為左上
        SetOffset(attackOffsets[0]);
    }

    public void PlayAttackAnimation()
    {
        if (!isPlaying)
            StartCoroutine(AnimateAttack());
    }

    IEnumerator AnimateAttack()
    {
        isPlaying = true;

        foreach (var offset in attackOffsets)
        {
            SetOffset(offset);
            yield return new WaitForSeconds(frameRate);
        }

        // 回到預設（左上）
        SetOffset(attackOffsets[0]);
        isPlaying = false;
    }

    void SetOffset(Vector2 offset)
    {
        if (decalProjector == null) return;
        // DecalProjector 的 uvBias 對應 Offset
        decalProjector.uvBias = offset;
    }
}
