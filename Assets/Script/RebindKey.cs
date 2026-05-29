using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindKey : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI keyboardText;
    [SerializeField] private Image mouseIcon;

    [Header("Sprites")]
    [SerializeField] private Sprite leftClickSprite;
    [SerializeField] private Sprite rightClickSprite;
    [SerializeField] private Sprite spaceSprite;


    [Header("Input Action")]
    public InputActionReference actionReference;

    [Tooltip("Index du binding dans l'InputAction (important pour composites Move)")]
    [SerializeField] private int bindingIndex = 0;

    private bool waitingForKey = false;

    public void StartRebind()
    {
        waitingForKey = true;

        keyboardText.gameObject.SetActive(false);
        mouseIcon.gameObject.SetActive(false);
    }

    private const string BindingsKey = "save_input_bindings";

    void Awake()
    {
        LoadBindingOverrides();
    }

    void Start()
    {
        UpdateVisual();
    }

    void LoadBindingOverrides()
    {
        var asset = actionReference != null && actionReference.action != null
            ? actionReference.action.actionMap?.asset
            : null;
        if (asset == null) return;

        string json = PlayerPrefs.GetString(BindingsKey, "");
        if (!string.IsNullOrEmpty(json))
            asset.LoadBindingOverridesFromJson(json);
    }

    void SaveBindingOverrides()
    {
        var asset = actionReference.action.actionMap?.asset;
        if (asset == null) return;

        PlayerPrefs.SetString(BindingsKey, asset.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (!waitingForKey) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ApplyBinding("<Mouse>/leftButton");
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            ApplyBinding("<Mouse>/rightButton");
            return;
        }

        foreach (Key key in System.Enum.GetValues(typeof(Key)))
        {
            if (key == Key.None) continue;

            if (Keyboard.current[key].wasPressedThisFrame)
            {
                ApplyBinding($"<Keyboard>/" +key);
                return;
            }
        }
    }

    void ApplyBinding(string path)
    {
        if (!waitingForKey) return;

        waitingForKey = false;

        if (actionReference == null || actionReference.action == null)
        {
            Debug.LogWarning($"RebindKey on '{name}': Input Action reference is not assigned.", this);
            return;
        }

        var action = actionReference.action;

        action.ApplyBindingOverride(bindingIndex, path);

        SaveBindingOverrides();

        UpdateVisual();

    }

    void UpdateVisual()
    {
        if (actionReference == null || actionReference.action == null)
        {
            Debug.LogWarning($"RebindKey on '{name}': Input Action reference is not assigned.", this);
            return;
        }

        var action = actionReference.action;

        if (action.bindings.Count <= bindingIndex)
            return;

        string binding = action.bindings[bindingIndex].effectivePath;

        if (binding.Contains("leftButton"))
        {
            keyboardText.gameObject.SetActive(false);
            mouseIcon.gameObject.SetActive(true);
            mouseIcon.sprite = leftClickSprite;
        }
        else if (binding.Contains("rightButton"))
        {
            keyboardText.gameObject.SetActive(false);
            mouseIcon.gameObject.SetActive(true);
            mouseIcon.sprite = rightClickSprite;
        }
        else if (binding.Contains("<Keyboard>/space"))
        {
            keyboardText.gameObject.SetActive(false);
            mouseIcon.gameObject.SetActive(true);
            mouseIcon.sprite = spaceSprite;
        }
        else
        {
            keyboardText.gameObject.SetActive(true);
            mouseIcon.gameObject.SetActive(false);

            keyboardText.text = InputControlPath.ToHumanReadableString(
                binding,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        }
    }
}