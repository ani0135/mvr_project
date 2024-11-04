// using UnityEngine;
// using UnityEditor;
// using System.Collections.Generic;

// public class PrefabReferenceFinder : EditorWindow
// {
//     [MenuItem("Tools/Find Prefab References")]
//     public static void ShowWindow()
//     {
//         GetWindow<PrefabReferenceFinder>("Prefab References");
//     }

//     private void OnGUI()
//     {
//         if (GUILayout.Button("Find Prefabs"))
//         {
//             FindPrefabs();
//         }
//     }

//     private void FindPrefabs()
//     {
//         string[] guids = AssetDatabase.FindAssets("t:Prefab");
//         List<string> prefabList = new List<string>();

//         foreach (string guid in guids)
//         {
//             string path = AssetDatabase.GUIDToAssetPath(guid);
//             prefabList.Add(path);
//         }

//         foreach (string prefab in prefabList)
//         {
//             Debug.Log(prefab);
//         }
//     }
// }
