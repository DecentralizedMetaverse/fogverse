using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour
{
    [SerializeField] Sprite notFoundIcon;
    void Start()
    {
        GM.Add<string, UniTask<Sprite>>("LoadImage", Load);
        GM.Add<Texture2D, Sprite>("ConvertToSprite", CreateSprite);
    }

    async UniTask<Sprite> Load(string url)
    {
        print($"LoadImage: {url}");
        var request = UnityWebRequestTexture.GetTexture(url);
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var texture = DownloadHandlerTexture.GetContent(request);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        else
        {
            Debug.LogWarning($"Failed to load image from {url}");
            return notFoundIcon;
        }
    }

    Sprite CreateSprite(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError($"texture2d is null");
            return null;
        }
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
