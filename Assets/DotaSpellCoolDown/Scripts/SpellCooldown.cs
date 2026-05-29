using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellCooldown : MonoBehaviour
{
    [Header("UI items for Spell Cooldown")]
    [Tooltip("Tooltip example")]
    [SerializeField]
    private Image imageCooldown;
    [SerializeField]
    private TMP_Text textCooldown;
    [SerializeField]
    private Image imageEdge;

    private bool isCoolDown = false;
    private int cooldownTime = 1;
    private float cooldownTimer = 0.0f;

    void Start()
    {
        textCooldown.gameObject.SetActive(false);
        imageEdge.gameObject.SetActive(false);
        imageCooldown.fillAmount = 0.0f;
    }

    void Update()
    {
        if(isCoolDown)
        {
            ApplyCooldown();
        }
    }

    public bool StartCooldown(int duration)
    {
        cooldownTime = duration;
        return UseSpell();
    }

    void ApplyCooldown()
    {
        cooldownTimer -= Time.deltaTime;
        if(cooldownTimer < 0.0f)
        {
            isCoolDown = false;
            textCooldown.gameObject.SetActive(false);
            imageEdge.gameObject.SetActive(false);
            imageCooldown.fillAmount = 0.0f;
        }
        else
        {
            textCooldown.text = Mathf.CeilToInt(cooldownTimer).ToString();
            imageCooldown.fillAmount = cooldownTimer / cooldownTime;
            imageEdge.transform.localEulerAngles = new Vector3(0, 0, 360.0f * (cooldownTimer / cooldownTime));
        }
    }

    public bool UseSpell()
    {
        if(isCoolDown)
        {
            return false;
        }
        else
        {
            isCoolDown = true;
            textCooldown.gameObject.SetActive(true);
            cooldownTimer = cooldownTime;
            textCooldown.text = Mathf.CeilToInt(cooldownTimer).ToString();
            imageCooldown.fillAmount = 1.0f;
            imageEdge.gameObject.SetActive(true);
            return true;
        }
    }
}
