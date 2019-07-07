using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedColor : MonoBehaviour
{
    [SerializeField] Color _Color = Color.white;
    static MaterialPropertyBlock _propertyBlock;
    static int _colorID = Shader.PropertyToID("_Color");

    void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
        }
        _propertyBlock.SetColor(_colorID, _Color);
        GetComponent<MeshRenderer>().SetPropertyBlock(_propertyBlock);
    }
}
