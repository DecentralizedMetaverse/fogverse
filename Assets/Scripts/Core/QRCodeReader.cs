using Cysharp.Threading.Tasks;
using DC;
using Teo.AutoReference;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

/// <summary>
/// QRコードをスキャンするプログラム
/// </summary>
public class QRCodeReader : UIComponent
{
    [Get, SerializeField] private UIEasingAnimationPosition animation;
    [SerializeField] private RawImage rawImage;
    private WebCamTexture camTexture;
    private bool selectCameraOpen;

    private void Start()
    {
        foreach (var d in WebCamTexture.devices)
        {
            print(d.name);
        }
        GM.Add<UniTask<string>>("ReadQRCode", ReadQRCode);
        GM.Add("ShowQRCodeReader", Show);
    }

    public override void Show()
    {
        base.Show();
        animation.Show();
        ReadQRCode().Forget();
    }

    public override void Close()
    {
        base.Close();
        animation.Close();
        camTexture.Stop();
    }

    private async UniTask<string> ReadQRCode()
    {
        // Cameraを選択する
        selectCameraOpen = true;
        var cameraNames = new string[WebCamTexture.devices.Length+1];
        var i = 0;
        cameraNames[i++] = "Cancel";
        foreach (var d in WebCamTexture.devices)
        {
            cameraNames[i++] = d.name;
        }

        var result = await GM.Msg<UniTask<int>>("Question", (object)cameraNames);
        selectCameraOpen = false; 

        if (result == 0) { return ""; }

        // Cameraを開く
        camTexture = new WebCamTexture(cameraNames[result]);
        
        rawImage.texture = camTexture;
        camTexture.Play();
        return await GetCodeContent();
    }

    private async UniTask<string> GetCodeContent()
    {
        var result = "";

        while (true)
        {
            if (!camTexture.isPlaying) return "";
            result = ReadCode(camTexture);

            if (result != "")
            {
                print(result);
                camTexture.Stop();

                // ContentのDownloadを行う
                await GM.Msg<UniTask<string>>("DownloadContent", result);
                return result;
            }

            await UniTask.Yield();
        }
    }

    private string ReadCode(WebCamTexture texture)
    {
        var reader = new BarcodeReader();
        var rawRGB = texture.GetPixels32();
        var width = texture.width;
        var height = texture.height;
        var result = reader.Decode(rawRGB, width, height);
        return result != null ? result.Text : string.Empty;
    }
}
