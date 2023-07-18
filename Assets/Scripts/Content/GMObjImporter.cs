using DC;
using Dummiesman;
using UnityEngine;

public class GMObjImporter : MonoBehaviour
{
    private OBJLoader oBJLoader;

    // Start is called before the first frame update
    void Start()
    {
        oBJLoader = new OBJLoader();
        GM.Add<string, GameObject>("LoadObjFile", (path) =>
        {
            return oBJLoader.Load(path);
        });
    }
}
