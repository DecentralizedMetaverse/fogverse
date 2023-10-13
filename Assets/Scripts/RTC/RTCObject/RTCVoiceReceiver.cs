using DC;
using Unity.WebRTC;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RTCVoiceReceiver : MonoBehaviour
{
    const float spatialBlendStrength = 1.0f;
    AudioSource audioSource;
    RTCObjectSync sync;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = spatialBlendStrength;    // 3Dâπê∫Ç…Ç∑ÇÈ

        if (!transform.parent.TryGetComponent(out sync))
        {
            Debug.LogError("RTCObjectSync is not found at parent transform");
            return;
        }

        // DBÇ÷ÇÃìoò^ÇçsÇ§
        if (sync.isLocal) return;
        SetAudioSource();
        // sync.InitMethod += SetAudioSource;
    }

    private void Update()
    {
        if (sync.isLocal) return;
        // Debug.Log($"volume: {AudioListener.GetSpectrumData()}")
    }

    void SetAudioSource()
    {
        Debug.Log("SetAudioSource");
        var userData = GM.db.user.GetData(sync.ownerId);
        userData.audioSource = audioSource;
        var peer = GM.db.rtc.peers[sync.ownerId];

        foreach (var track in peer.remoteStream.GetTracks())
        {
            if (track.Kind == TrackKind.Audio)
            {
                Debug.Log("SetAudioSource2");
                var audioTrack = track as AudioStreamTrack;
                audioSource.SetTrack(audioTrack);
                audioSource.loop = true;
                audioSource.Play();
            }
        };
    }
}
