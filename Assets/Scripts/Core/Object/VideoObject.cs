using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

[RequireComponent(typeof(VideoPlayer))] 
public class VideoObject : ObjectBase
{
    VideoPlayer player;
    private void Awake()
    {
        player = GetComponent<VideoPlayer>();
    }

    public void LoadVideo(string path)
    {
        if (!File.Exists(path)) return;
        fileName = path;
        player.url = path;
        player.Play();        
    }
}
