using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows a short "Not enough mana / 法力不足" message above the spell bar
/// when the player tries to cast a spell without enough mana.
/// Call NotEnoughManaUI.Instance?.Show() to flash the message.
/// </summary>
public class NotEnoughManaUI : MonoBehaviour
{
    public static NotEnoughManaUI Instance;

    [Header("UI")]
    [Tooltip("The message label. Defaults to the TextMeshProUGUI on this object.")]
    public TextMeshProUGUI label;

    [Header("Timing")]
    [Tooltip("How long the message stays fully visible before fading out.")]
    public float holdDuration = 0.8f;
    [Tooltip("Fade-out time after the hold.")]
    public float fadeDuration = 0.4f;

    private Coroutine routine;

    void Awake()
    {
        Instance = this;
        if (label == null)
            label = GetComponent<TextMeshProUGUI>();
        SetAlpha(0f);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Flash the message: show it fully, hold, then fade out.
    /// </summary>
    public void Show()
    {
        if (label == null)
            return;

        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        SetAlpha(1f);
        yield return new WaitForSeconds(holdDuration);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, t / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);
        routine = null;
    }

    void SetAlpha(float a)
    {
        if (label == null)
            return;

        Color c = label.color;
        c.a = a;
        label.color = c;
    }
}
