using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

/// <summary>
/// QRコードをスキャンするプログラム
/// </summary>
public class QRCodeReader : MonoBehaviour
{
    [SerializeField] UI_ShowCloseFade ui;
    [SerializeField] RawImage rawImage;
    WebCamTexture camTexture;
    private bool selectCameraOpen;

    void Start()
    {
        foreach (var d in WebCamTexture.devices)
        {
            print(d.name);
        }
        GM.Add<UniTask<string>>("ReadQRCode", ReadQRCode);
        GM.Add("ShowQRCodeReader", ShowQRCodeReader);
    }

    void ShowQRCodeReader()
    {
        if (selectCameraOpen) return;
        if (!ui.active)
        {
            ReadQRCode().Forget();
        }
        else
        {
            camTexture.Stop();
            ui.active = false;
        }
    }

    async UniTask<string> ReadQRCode()
    {
        // Cameraを選択する
        selectCameraOpen = true;
        string[] cameraNames = new string[WebCamTexture.devices.Length+1];
        var i = 0;
        cameraNames[i++] = "Cancel";
        foreach (var d in WebCamTexture.devices)
        {
            cameraNames[i++] = d.name;
        }

        var result = await GM.Msg<UniTask<int>>("Question", (object)cameraNames);
        selectCameraOpen = false; 

        if (result == 0) { return ""; }

        ui.active = true;

        // Cameraを開く
        camTexture = new WebCamTexture(cameraNames[result]);
        
        rawImage.texture = camTexture;
        camTexture.Play();
        return await GetCodeContent();
    }

    async UniTask<string> GetCodeContent()
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

    string ReadCode(WebCamTexture texture)
    {
        var reader = new BarcodeReader();
        var rawRGB = texture.GetPixels32();
        var width = texture.width;
        var height = texture.height;
        var result = reader.Decode(rawRGB, width, height);
        return result != null ? result.Text : string.Empty;
    }
}
