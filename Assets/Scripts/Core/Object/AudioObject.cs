using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class AudioObject : MonoBehaviour
{
    AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void LoadAudioFromLocal(string path)
    {
        if (!File.Exists(path)) return;
        StartCoroutine(Load($"file://{path}"));
    }

    IEnumerator Load(string path)
    {
        using (var www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error); yield break;
            }
            source.clip = DownloadHandlerAudioClip.GetContent(www);
            source.Play();
        }
    }
}
