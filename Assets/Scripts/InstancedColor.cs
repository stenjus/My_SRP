using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedColor : MonoBehaviour
{
    [SerializeField] Color color = Color.white;
    static MaterialPropertyBlock propertyBlock;
    static int colorId = Shader.PropertyToID("_Color");

    void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        propertyBlock.SetColor(colorId, color);
        GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }
}
