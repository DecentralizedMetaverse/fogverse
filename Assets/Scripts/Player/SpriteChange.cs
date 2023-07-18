using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteChange : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    static int idMainTex = Shader.PropertyToID("_MainTex");
    MaterialPropertyBlock block;

    [SerializeField] Texture texture = default;
    public Texture overrideTexture
    {
        get { return texture; }
        set
        {
            texture = value;
            if (block == null)
            {
                Init();
            }
            block.SetTexture(idMainTex, texture);
        }
    }

    void Awake()
    {
        Init();
        overrideTexture = texture;
    }

    void LateUpdate()
    {
        spriteRenderer.SetPropertyBlock(block);
    }

    void OnValidate()
    {
        overrideTexture = texture;
    }

    void Init()
    {
        block = new MaterialPropertyBlock();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.GetPropertyBlock(block);
    }
}
