using DC;
using System.IO;
using Newtonsoft.Json.Linq;
using Teo.AutoReference;
using UniGLTF;
using UniJSON;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AvatarView : UIComponent
{
    [GetInChildren, Name("SubmitButton"), SerializeField]
    private Button submitButton;

    [Get, SerializeField] private UIEasingAnimationPosition animation;

    [SerializeField] private AvatarButtonView avatarButtonPrefab;

    [GetInChildren, Name("Content"), SerializeField]
    private Transform content;

    private string avatarPath;

    private void Start()
    {
        avatarPath = Constants.AvatarPath;
        submitButton.onClick.AddListener(OnOpenFolder);
        avatarButtonPrefab.gameObject.SetActive(false);
    }

    public override void Show()
    {
        base.Show();
        animation.Show();

        content.DestroyChildren();

        var files = Directory.GetFiles(avatarPath, "*.vrm");
        foreach (var filePath in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var avatarButton = Instantiate(avatarButtonPrefab, content);
            avatarButton.gameObject.SetActive(true);
            avatarButton.avatarName.text = fileName;
            avatarButton.button.onClick.AddListener(() => OnClick(filePath));

            // VRMファイルからサムネイルを取得して表示
            var thumbnail = LoadVRMThumbnail(filePath);
            if (thumbnail != null)
            {
                avatarButton.avatarImage.sprite = thumbnail;
            }
        }
    }

    public override void Close()
    {
        base.Close();
        animation.Close();
    }

    private void OnClick(string filePath)
    {
        var id = GM.db.rtc.id;
        var objId = GM.db.rtc.syncObjectsByID[id][0];
        var obj = GM.db.rtc.syncObjects[objId];
        obj.SetObject(filePath);
    }

    private void OnOpenFolder()
    {
        System.Diagnostics.Process.Start(avatarPath);
    }

    /// <summary>
    /// VRMファイルからサムネイルを読み込み、Spriteとして返す
    /// </summary>
    /// <param name="filePath">VRMファイルのパス</param>
    /// <returns>サムネイルのSprite、存在しない場合はnull</returns>
    private Sprite LoadVRMThumbnail(string filePath)
    {
        var vrmData = File.ReadAllBytes(filePath);
        var data = new GlbLowLevelParser(filePath, vrmData).Parse();

        return IsVRM1(data) ? LoadVRM1Thumbnail(data) : LoadVRM0Thumbnail(data);
    }

    /// <summary>
    /// VRMファイルがバージョン1.xかどうかを判定する
    /// </summary>
    /// <param="data">GltfData</param>
    /// <returns>バージョン1.xの場合はtrue、それ以外はfalse</returns>
    private bool IsVRM1(GltfData data)
    {
        return data.Json.Contains("\"VRMC_vrm\"");
    }

    /// <summary>
    /// VRMバージョン0.xのファイルからサムネイルを取得し、Spriteとして返す
    /// </summary>
    /// <param name="data">GltfData</param>
    /// <returns>サムネイルのSprite、存在しない場合はnull</returns>
    public Sprite LoadVRM0Thumbnail(GltfData data)
    {
        // Jsonデータを解析してJsonNodeオブジェクトを取得
        var vrm0Node = JsonParser.Parse(data.Json);
        var vrm0 = UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.Deserialize(vrm0Node);

        // Extensionsフィールドを適切にキャスト
        Debug.Log("[Debug][-1]");
        if (vrm0.Extensions is not JObject extensions) return null;
        Debug.Log("[Debug][0]");
        if (!extensions.TryGetValue("VRM", out JToken vrmExtension)) return null;
        Debug.Log("[Debug][1]");
        var vrmMeta = vrmExtension["meta"];
        var thumbnailImageIndex = vrmMeta?["thumbnail"]?.Value<int>();

        if (!thumbnailImageIndex.HasValue) return null;
        Debug.Log("[Debug][2]");
        var textureIndex = thumbnailImageIndex.Value;
        var texture = data.GLTF.textures[textureIndex];
        var imageIndex = texture.source;
        if (!imageIndex.HasValue) return null;
        Debug.Log("[Debug][3]");
        var image = data.GLTF.images[imageIndex.Value];
        var bufferViewIndex = image.bufferView;
        var bufferView = data.GLTF.bufferViews[bufferViewIndex];

        // バイナリデータを取得してTexture2Dにロード
        var binary = data.GetBytesFromBufferView(bufferViewIndex);
        var imageData = new byte[binary.Length];
        var nativeArray = new NativeArray<byte>(binary.Length, Allocator.Temp);
        nativeArray.CopyFrom(binary);
        nativeArray.CopyTo(imageData);
        nativeArray.Dispose();

        var texture2D = new Texture2D(2, 2);
        texture2D.LoadImage(imageData);
        return TextureToSprite(texture2D);
    }

    /// <summary>
    /// VRMバージョン1.xのファイルからサムネイルを取得し、Spriteとして返す
    /// </summary>
    /// <param name="data">GltfData</param>
    /// <returns>サムネイルのSprite、存在しない場合はnull</returns>
    public Sprite LoadVRM1Thumbnail(GltfData data)
    {
        // Jsonデータを解析してJsonNodeオブジェクトを取得
        var vrm1Node = JsonParser.Parse(data.Json);
        var vrm1 = UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.Deserialize(vrm1Node);

        if (vrm1.Meta.ThumbnailImage.HasValue)
        {
            var textureIndex = vrm1.Meta.ThumbnailImage.Value;
            var texture = data.GLTF.textures[textureIndex];
            var imageIndex = texture.source;
            if (imageIndex.HasValue)
            {
                var image = data.GLTF.images[imageIndex.Value];
                var bufferView = data.GLTF.bufferViews[image.bufferView];
                var bufferViewIndex = image.bufferView;
                var buffer = data.GLTF.buffers[bufferView.buffer];

                // バイナリデータを取得してTexture2Dにロード
                var binary = data.GetBytesFromBufferView(bufferViewIndex);
                byte[] imageData = new byte[binary.Length];
                NativeArray<byte> nativeArray = new NativeArray<byte>(binary.Length, Allocator.Temp);
                nativeArray.CopyFrom(binary);
                nativeArray.CopyTo(imageData);
                nativeArray.Dispose();

                Texture2D texture2D = new Texture2D(2, 2);
                texture2D.LoadImage(imageData);
                return TextureToSprite(texture2D);
            }
        }

        Debug.LogError("Thumbnail not found in VRM 1.x file.");
        return null;
    }

    /// <summary>
    /// Texture2DをSpriteに変換するヘルパーメソッド
    /// </summary>
    /// <param name="texture">Texture2D</param>
    /// <returns>Sprite</returns>
    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
