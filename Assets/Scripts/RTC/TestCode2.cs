using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TestCode2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Dictionary<string, object> sendData = new()
        {
            { "type", "location" } ,
            { "position", transform.position.ToSplitString() },
            { "rotation", transform.rotation.eulerAngles.ToSplitString() },
            { "objId", "6e31327d9d01476c9f0882a52a141f88" },
            { "id", "6e31327d9d01476c9f0882a52a141f88" },
            { "targetId", "*" },
        };

        var text = sendData.GetString();

        Debug.Log(text);

        // 文字列をUTF-8エンコーディングでバイト配列に変換
        byte[] bytes = Encoding.UTF8.GetBytes(text);

        // バイト数を求める
        int byteCount = bytes.Length;

        Debug.Log("テキストのバイト数: " + byteCount);
        GetKBPS(byteCount, 1000);
        GetKBPS(byteCount, 320);
    }

    void GetKBPS(int byteCount, int packetsPerSecond)
    {
        // バイト数と1秒間の通信回数を設定
        //int byteCount = 1024; // 通信したバイト数
        //int packetsPerSecond = 100; // 1秒間の通信回数

        // バイトをビットに変換 (1バイト = 8ビット)
        int bitsPerByte = 8;
        int totalBits = byteCount * bitsPerByte;

        // 通信速度（kbps）を計算
        double kbps = (double)(totalBits * packetsPerSecond) / 1000f;

        Debug.Log("通信速度: " + kbps + " kbps");
    }
}
