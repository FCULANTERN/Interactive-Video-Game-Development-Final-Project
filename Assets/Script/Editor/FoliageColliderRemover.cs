using UnityEngine;
using UnityEditor;

public static class FoliageColliderRemover
{
    static readonly string[] FoliagePrefabNames =
    {
        "Bush_01", "Bush_02", "Bush_03",
        "Flowers_01", "Flowers_02",
        "Grass_01", "Grass_02",
        "Mushroom_01", "Mushroom_02",
    };

    [MenuItem("Tools/Remove Foliage Colliders in Scene")]
    static void RemoveCollidersInScene()
    {
        int removed = 0;

        foreach (var root in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!IsFoliage(root)) continue;

            foreach (var col in root.GetComponentsInChildren<Collider>())
            {
                Undo.DestroyObjectImmediate(col);
                removed++;
            }
        }

        Debug.Log($"[FoliageColliderRemover] Removed {removed} collider(s) from foliage objects.");
    }

    static bool IsFoliage(GameObject go)
    {
        if (!PrefabUtility.IsPartOfPrefabInstance(go)) return false;
        var src = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
        if (src == null) return false;

        foreach (var name in FoliagePrefabNames)
            if (src.name == name) return true;

        return false;
    }
}
