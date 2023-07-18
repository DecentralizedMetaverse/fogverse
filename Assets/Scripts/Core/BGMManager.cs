using System.Collections;
using System.Collections.Generic;
using DC;
using UnityEngine;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    [SerializeField] DB_Settings db;

    AudioSource source;
    bool change;
    AudioClip nextAudio;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        GM.Add<string>("PlayBGM", ChangeBGM);
    }    

    public void ChangeBGM(string bgmName)
    {
        if (GM.mng.bgmOff) return;
        if (bgmName == "stop")
        {
            nextAudio = null;
            change = true;
            return;
        }
        Addressables.LoadAssetAsync<AudioClip>(bgmName).Completed += op =>
        {
            nextAudio = op.Result;
            change = true;
            Debug.Log(op.Result.name); //ロード完了時のメッセージ
        };
    }

    void Update()
    {
        if (!change)
        {
            source.volume = db.bgmVolume;
            return;
        }

        if (source.clip != null)
        {
            source.volume -= Time.deltaTime;

            if (source.volume > 0) return;
        }

        change = false;
        if (nextAudio == null)
        {
            source.Stop();
            return;
        }
        source.clip = nextAudio;
        source.volume = db.bgmVolume;
        source.Play();
    }
}
