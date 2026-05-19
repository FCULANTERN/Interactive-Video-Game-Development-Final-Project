using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabBrushTool : EditorWindow
{
    List<GameObject> prefabs = new List<GameObject>();
    float brushRadius = 3f;
    float minSpacing = 2f;
    float minScale = 0.8f;
    float maxScale = 1.2f;
    bool randomYRotation = true;
    bool ringMode   = false;
    int  ringLayers = 1;
    Transform paintParent;

    // Positions placed during the current mouse drag (cleared on MouseDown).
    // Used to enforce spacing before Physics catches up with newly spawned colliders.
    readonly List<Vector3> strokePositions = new List<Vector3>();

    [MenuItem("Tools/Prefab Brush")]
    public static void ShowWindow() => GetWindow<PrefabBrushTool>("Prefab Brush");

    void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    void OnGUI()
    {
        EditorGUILayout.LabelField("Prefab Brush", EditorStyles.boldLabel);
        GUILayout.Space(4);

        brushRadius     = EditorGUILayout.Slider("Brush Radius",  brushRadius,  0.5f, 50f);
        minSpacing      = EditorGUILayout.Slider("Min Spacing",   minSpacing,   0.1f, 15f);
        minScale        = EditorGUILayout.Slider("Min Scale",     minScale,     0.1f, 5f);
        maxScale        = EditorGUILayout.Slider("Max Scale",     maxScale,     minScale, 5f);
        randomYRotation = EditorGUILayout.Toggle("Random Y Rotation", randomYRotation);
        ringMode = EditorGUILayout.Toggle("Ring Mode", ringMode);
        if (ringMode)
            ringLayers = EditorGUILayout.IntSlider("Ring Layers", ringLayers, 1, 8);

        GUILayout.Space(4);
        paintParent = (Transform)EditorGUILayout.ObjectField(
            "Parent Transform", paintParent, typeof(Transform), true);

        GUILayout.Space(4);
        if (GUILayout.Button("Reset Settings"))
            ResetSettings();

        GUILayout.Space(8);
        EditorGUILayout.LabelField("Prefabs to paint", EditorStyles.boldLabel);

        Rect drop = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(drop, "Drag prefabs here");
        HandleDrop(drop);

        for (int i = prefabs.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();
            prefabs[i] = (GameObject)EditorGUILayout.ObjectField(
                prefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("x", GUILayout.Width(22)))
                prefabs.RemoveAt(i);
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(8);
        if (prefabs.Count == 0)
            EditorGUILayout.HelpBox("Add at least one prefab above.", MessageType.Warning);
        else
            EditorGUILayout.HelpBox(
                "Ctrl + Left click/drag : paint\n" +
                "Ctrl + Shift + drag    : erase", MessageType.Info);
    }

    void ResetSettings()
    {
        brushRadius     = 3f;
        minSpacing      = 2f;
        minScale        = 0.8f;
        maxScale        = 1.2f;
        randomYRotation = true;
        ringMode        = false;
        ringLayers      = 1;
        paintParent     = null;
    }

    void HandleDrop(Rect area)
    {
        var e = Event.current;
        if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform) return;
        if (!area.Contains(e.mousePosition)) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (e.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
                if (obj is GameObject go && !prefabs.Contains(go))
                    prefabs.Add(go);
        }
        e.Use();
    }

    void OnSceneGUI(SceneView sv)
    {
        if (prefabs.Count == 0) return;

        var e = Event.current;
        if (!e.control) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) { sv.Repaint(); return; }

        // Draw brush disc
        Handles.color = new Color(0.2f, 1f, 0.2f, 0.15f);
        Handles.DrawSolidDisc(hit.point, hit.normal, brushRadius);
        Handles.color = new Color(0.2f, 1f, 0.2f, 0.9f);
        Handles.DrawWireDisc(hit.point, hit.normal, brushRadius);
        sv.Repaint();

        if (e.type == EventType.MouseDown && e.button == 0 && !e.shift)
            strokePositions.Clear();

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
        {
            if (e.shift)
                Erase(hit.point);
            else if (ringMode && e.type == EventType.MouseDown)
                PaintRing(hit.point, hit.normal);
            else if (!ringMode)
                Paint(hit.point);
            e.Use();
        }
    }

    void Paint(Vector3 center)
    {
        int attempts = Mathf.Max(1, Mathf.RoundToInt(brushRadius));
        for (int i = 0; i < attempts; i++)
        {
            Vector2 r      = Random.insideUnitCircle * brushRadius;
            Vector3 origin = center + new Vector3(r.x, 20f, r.y);

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 40f)) continue;
            if (TooClose(hit.point)) continue;

            var prefab = prefabs[Random.Range(0, prefabs.Count)];
            var rot    = randomYRotation
                ? Quaternion.Euler(0, Random.Range(0f, 360f), 0)
                : Quaternion.identity;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetPositionAndRotation(hit.point, rot);
            go.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);

            if (paintParent != null)
                go.transform.SetParent(paintParent);

            Undo.RegisterCreatedObjectUndo(go, "Brush: Paint");
            strokePositions.Add(hit.point);
        }
    }

    void PaintRing(Vector3 center, Vector3 surfaceNormal)
    {
        for (int layer = 0; layer < ringLayers; layer++)
        {
            float radius = brushRadius - layer * minSpacing;
            if (radius <= 0f) break;

            float circumference = 2f * Mathf.PI * radius;
            int   count         = Mathf.Max(2, Mathf.FloorToInt(circumference / minSpacing));
            float angleStep     = 360f / count;
            // Offset each layer by half a step so trees don't line up radially
            float angleOffset   = (layer % 2 == 0) ? 0f : angleStep * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float   angle  = (i * angleStep + angleOffset) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                Vector3 origin = center + offset + Vector3.up * 20f;

                if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 40f)) continue;

                var prefab = prefabs[Random.Range(0, prefabs.Count)];
                var rot    = randomYRotation
                    ? Quaternion.Euler(0, Random.Range(0f, 360f), 0)
                    : Quaternion.identity;

                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.SetPositionAndRotation(hit.point, rot);
                go.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);

                if (paintParent != null)
                    go.transform.SetParent(paintParent);

                Undo.RegisterCreatedObjectUndo(go, "Brush: Ring");
            }
        }
    }

    // Returns true if another painted prefab is already within minSpacing of point.
    // Checks strokePositions first (Physics hasn't synced yet for brand-new objects),
    // then falls back to Physics for trees placed in previous strokes.
    bool TooClose(Vector3 point)
    {
        foreach (var p in strokePositions)
            if (Vector3.Distance(p, point) < minSpacing) return true;

        foreach (var c in Physics.OverlapSphere(point, minSpacing))
            if (IsPaintedPrefab(c.transform.root.gameObject)) return true;

        return false;
    }

    void Erase(Vector3 center)
    {
        var toDelete = new List<GameObject>();
        foreach (var c in Physics.OverlapSphere(center, brushRadius))
        {
            var root = c.transform.root.gameObject;
            if (!toDelete.Contains(root) && IsPaintedPrefab(root))
                toDelete.Add(root);
        }
        foreach (var go in toDelete)
            Undo.DestroyObjectImmediate(go);
    }

    bool IsPaintedPrefab(GameObject go)
    {
        if (!PrefabUtility.IsPartOfPrefabInstance(go)) return false;
        var src = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
        return prefabs.Contains(src);
    }
}
