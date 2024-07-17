using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Slicer
{
    /// <summary>
    /// Slice the object by the plane 
    /// </summary>
    /// <param name="plane"></param>
    /// <param name="objectToCut"></param>
    /// <returns></returns>
    public static GameObject[] Slice(Plane plane, GameObject objectToCut)
    {
        OpenPackManager.Singleton.TempOpenPack();
        //Get the current mesh and its verts and tris
        Mesh mesh = objectToCut.GetComponent<MeshFilter>().mesh;
        var a = mesh.GetSubMesh(0);
        Sliceable sliceable = objectToCut.GetComponent<Sliceable>();

        if (sliceable == null)
        {
            throw new NotSupportedException("Cannot slice non sliceable object, add the sliceable script to the object or inherit from sliceable to support slicing");
        }

        //Create left and right slice of hollow object
        SlicesMetadata slicesMeta = new SlicesMetadata(plane, mesh, sliceable.IsSolid, sliceable.ReverseWireTriangles, sliceable.ShareVertices, sliceable.SmoothVertices);

        GameObject positiveObject = CreateMeshGameObject(objectToCut);
        positiveObject.name = string.Format("{0}_positive", objectToCut.name);

        GameObject negativeObject = CreateMeshGameObject(objectToCut);
        negativeObject.name = string.Format("{0}_negative", objectToCut.name);

        var positiveSideMeshData = slicesMeta.PositiveSideMesh;
        var negativeSideMeshData = slicesMeta.NegativeSideMesh;

        positiveObject.GetComponent<MeshFilter>().mesh = positiveSideMeshData;
        negativeObject.GetComponent<MeshFilter>().mesh = negativeSideMeshData;

        SetupCollidersAndRigidBodys(ref positiveObject, positiveSideMeshData, sliceable.UseGravity);
        SetupCollidersAndRigidBodys(ref negativeObject, negativeSideMeshData, sliceable.UseGravity);

        return new GameObject[] { positiveObject, negativeObject };
    }

    /// <summary>
    /// Creates the default mesh game object.
    /// </summary>
    /// <param name="originalObject">The original object.</param>
    /// <returns></returns>
    private static GameObject CreateMeshGameObject(GameObject originalObject)
    {
        var originalMaterial = originalObject.GetComponent<MeshRenderer>().materials;

        GameObject meshGameObject = new GameObject();
        //Sliceable originalSliceable = originalObject.GetComponent<Sliceable>();

        meshGameObject.AddComponent<MeshFilter>();
        meshGameObject.AddComponent<MeshRenderer>();
        //Sliceable sliceable = meshGameObject.AddComponent<Sliceable>();

        meshGameObject.GetComponent<MeshRenderer>().materials = originalMaterial;

        meshGameObject.transform.localScale = originalObject.transform.localScale;
        meshGameObject.transform.rotation = originalObject.transform.rotation;
        meshGameObject.transform.position = originalObject.transform.position;

        meshGameObject.tag = originalObject.tag;

        return meshGameObject;
    }

    /// <summary>
    /// Add mesh collider and rigid body to game object
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="mesh"></param>
    private static void SetupCollidersAndRigidBodys(ref GameObject gameObject, Mesh mesh, bool useGravity)
    {
        var rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = useGravity; gameObject.AddComponent<FadeAndDestroy>().StartFading(1.0f);
    }
    private class FadeAndDestroy : MonoBehaviour
    {
        public void StartFading(float duration)
        {
            StartCoroutine(FadeOutAndDestroy(duration));
        }

        private IEnumerator FadeOutAndDestroy(float duration)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Material[] materials = renderer.materials;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                foreach (Material material in materials)
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
                yield return null;
            }

            // Ensure alpha is set to 0 at the end
            foreach (Material material in materials)
            {
                Color color = material.color;
                color.a = 0f;
                material.color = color;
            }
            Destroy(gameObject);
        }
    }
}
