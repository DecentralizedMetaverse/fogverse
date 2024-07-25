using Cysharp.Threading.Tasks;
using TC;
using UnityEngine;
using UniVRM10;

public class VRMModel : MonoBehaviour
{
    void Start()
    {
        Message.Subscribe<string, UniTask<GameObject>>("VRMModelLoad", Load);
        // GM.Add<byte[], GameObject>("VRMModelLoadFromData", (data) => { return Load(null, data); });
    }

    private async UniTask<GameObject> Load(string path)
    {
        var vrm10Instance = await Vrm10.LoadPathAsync(path);
        if (vrm10Instance != null) return vrm10Instance.gameObject;

        Debug.LogError($"[Error] VRMModel Load: {path}");
        return null;
    }
}
