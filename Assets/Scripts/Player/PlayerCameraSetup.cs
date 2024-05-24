using MistNet;
using Teo.AutoReference;
using UnityEngine;

public class PlayerCameraSetup : MonoBehaviour
{
    [GetInParent,SerializeField] private MistSyncObject syncObject;
    private void Start()
    {
        if (!syncObject.IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.SetParent(null);
    }
}
