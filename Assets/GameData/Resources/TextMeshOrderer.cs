using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMeshOrderer : MonoBehaviour
{
    public string sortingLayerName = "UI"; // Имя слоя
    public int orderInLayer = 1;          // Порядок в слое

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = orderInLayer;
        }
    }
}
