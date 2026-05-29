using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellKeyUI : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference spellAction;

    [Header("UI")]
    public TextMeshProUGUI text;

    void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// Appelé quand on veut mettre à jour l'affichage (start + rebind)
    /// </summary>
    public void Refresh()
    {
        if (spellAction == null || spellAction.action == null)
            return;

        var action = spellAction.action;

        if (action.bindings.Count == 0)
            return;

        // On prend le premier binding actif
        string path = action.bindings[0].effectivePath;

        text.text = InputControlPath.ToHumanReadableString(
            path,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }
}