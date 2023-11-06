using System.Collections.Generic;
using DC;
using UnityEngine;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(AudioSource))]
public class SEManager : MonoBehaviour
{
    AudioSource source;
    Queue<string> qlist = new Queue<string>();
    Dictionary<string, AudioClip> seList = new Dictionary<string, AudioClip>();

    void Start()
    {
        source = GetComponent<AudioSource>();
        GM.Add<string>("PlaySE", Play);
    }

    public void Play(string seName)
    {
        if (seList.ContainsKey(seName))
        {
            qlist.Enqueue(seName);
            return;
        }
        Addressables.LoadAssetAsync<AudioClip>(seName).Completed += op =>
        {
            qlist.Enqueue(seName);
            if (!seList.ContainsKey(seName))
            {
                seList.Add(seName, op.Result);
            }
        };
    }

    void Update()
    {
        if (qlist.Count == 0) return;
        source.volume = GM.db.settings.data.seVolume;
        source.PlayOneShot(seList[qlist.Dequeue()]);
    }
}
