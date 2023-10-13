using UnityEngine;

public class DontRotate : MonoBehaviour
{

    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
