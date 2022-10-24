using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTransparency : MonoBehaviour
{
    Renderer thisRenderer;
    float originalTransparency;
    Material thisMaterial;
    private void Awake()
    {
        thisRenderer = this.gameObject.GetComponent<Renderer>();
    }
    private void Start()
    {
    }
    public void ChangeTransparent(int v)
    {
        thisRenderer.material = GameManager.singleton.TransparentSharedMat;
        Color32 col = thisRenderer.material.GetColor("_Color");
        col.a = 50;
        this.gameObject.GetComponent<Renderer>().material.SetColor("_Color", col);
    }

    public void SetOpaque()
    {
        thisRenderer.material = GameManager.singleton.OpaqueSharedMat;
        Color32 col = thisRenderer.material.GetColor("_Color");
        col.a = 255;
        this.gameObject.GetComponent<Renderer>().material.SetColor("_Color", col);
    }
}
