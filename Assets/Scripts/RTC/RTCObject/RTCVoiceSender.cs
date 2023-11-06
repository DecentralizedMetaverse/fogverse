using DC;
using Unity.WebRTC;
using UnityEngine;

// https://docs.unity3d.com/ja/Packages/com.unity.webrtc@3.0/manual/audiostreaming.html
[RequireComponent(typeof(AudioSource))]
public class RTCVoiceSender : MonoBehaviour
{
    const int sampleRate = 48000;
    const int deviceIndex = 3;
    string deviceName = "";
    const int processLength = 512;

    public bool isStreaming = true;
    private int readHead;
    private AudioStreamTrack track;

    AudioClip microphoneClip;
    RTCObjectSync sync;
    private RTCRtpSender sender;

    float[] microphoneBuffer = new float[sampleRate];
    float[] processBuffer = new float[processLength];
    private int count;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (!transform.parent.TryGetComponent(out sync))
        {
            Debug.LogError("RTCObjectSync is not found at parent transform");
            return;
        }

        // 3: C922 Pro Stream Webcam
        for (var i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log($"{i}:{Microphone.devices[i]}");
        }

        if (!sync.isLocal) return;

        SetMicrophone(deviceIndex);
        // audioSource.Play();

        GM.Add<RTCPeerConnection>("SetTrack", SetTrack);
        GM.Add<int>("SetMicrophone", (deviceIndex) =>
        {
            SetMicrophone(deviceIndex);
        });
    }

    private void SetMicrophone(int deviceIndex)
    {
        deviceName = Microphone.devices[deviceIndex];

        Microphone.GetDeviceCaps(deviceName, out int minFreq, out int maxFreq);
        Debug.Log($"Device: {deviceName}, minFreq:{minFreq}, maxFreq:{maxFreq}");
        microphoneClip ??= Microphone.Start(deviceName, true, 10, maxFreq);
    }

    /// <summary>
    /// Offerを送る前に設定する
    /// </summary>
    /// <param name="connectedId"></param>
    void SetTrack(RTCPeerConnection peer)
    {
        if (!sync.isLocal) return;
        if (track == null)
        {
            audioSource.clip = microphoneClip;
            audioSource.Play();
            track = new AudioStreamTrack(audioSource);
        }
        var stream = new MediaStream();
        sender = peer.AddTrack(track, stream);
    }

    private void OnDestroy()
    {
        if (sender == null) return;

        foreach (var peer in GM.db.rtc.peers.Values)
        {
            peer.pc.RemoveTrack(sender);
        }
    }

    //private void Update()
    //{
    //    if (!sync.isLocal) return;
    //    if (!isStreaming) return;
    //    if (track == null) return;

    //    Debug.Log("Voice Chat Update");

    //    var position = Microphone.GetPosition(deviceName);

    //    Debug.Log($"position: {position}");

    //    if (position < 0) return;
    //    if (position == readHead) return;

    //    Debug.Log("Recording");

    //    // マイクからデータを取得
    //    microphoneClip.GetData(microphoneBuffer, 0);
    //    SendAudioData(microphoneBuffer);
    //}

    //void SendAudioData(float[] data, int channels = 1)
    //{
    //    if (track == null) return;
    //    track.SetData(data, channels, sampleRate);
    //}
}
