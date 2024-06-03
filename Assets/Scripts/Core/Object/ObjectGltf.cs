using GLTFast;
using Teo.AutoReference;
using UnityEngine;

public class ObjectGltf : ObjectBase
{
    [Get, SerializeField] private GltfAsset gltfAsset;

    public void SetData(string filePath)
    {
        gltfAsset.Load($"file://{filePath}");
    }
}
