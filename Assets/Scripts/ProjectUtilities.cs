#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProjectUtilities : MonoBehaviour
{
    [MenuItem("Project Tools/Create Materials based on current selected textures")]
    private static void CreateMaterials()
    {
        foreach (Object o in Selection.objects)
        {
            if (o.GetType() != typeof(Texture2D))
            {
                continue;
            }

            Debug.Log("Creating material from: " + o);

            Texture2D selected = o as Texture2D;

            Material material1 = new Material(Shader.Find("Standard (Specular setup)"));
            material1.mainTexture = (Texture)o;

            string savePath = AssetDatabase.GetAssetPath(selected);
            savePath = savePath.Substring(0, savePath.LastIndexOf('/') + 1);

            string newAssetName = savePath + selected.name + ".mat";

            AssetDatabase.CreateAsset(material1, newAssetName);

            AssetDatabase.SaveAssets();

        }
        Debug.Log("Done!");
    }
}
#endif