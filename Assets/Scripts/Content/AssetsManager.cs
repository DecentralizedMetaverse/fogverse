using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
//using Dummiesman;
using YamlDotNet.RepresentationModel;
using System;
using UniGLTF;
using Cysharp.Threading.Tasks;
using DC;

[Obsolete("�폜�\��", error: true)]
public class AssetsManager : MonoBehaviour
{
    [SerializeField] DB_User dbUser;
    [SerializeField] GameObject prefabMonitor;
    [SerializeField] GameObject prefabVideo;
    [SerializeField] GameObject prefabAudio;
    [SerializeField] GameObject prefabScript;
    [SerializeField] GameObject prefabCube;
    [SerializeField] GameObject prefabSphere;
    [SerializeField] GameObject prefabPlane;

    string path = "";
    Dictionary<string, string> worldPath = new Dictionary<string, string>();
    //OBJLoader loader = new OBJLoader();
    const int moveLayer = 6;

    /// <summary>
    /// ���^�f�[�^
    /// </summary>
    Dictionary<string, MetaDataFile> metaData = new Dictionary<string, MetaDataFile>();

    /// <summary>
    /// World�Ɋւ���f�[�^
    /// </summary>
    Dictionary<string, MetaDataObject> worldData = new Dictionary<string, MetaDataObject>();
    Dictionary<string, MetaDataObject> nextLoadTargetWorldData = new Dictionary<string, MetaDataObject>();
    Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
    Dictionary<string, string> nextLoadMultiWorld = new Dictionary<string, string>();
    List<string> nextLoadMutiWorldName = new List<string>();

    /// <summary>
    /// Object�ǂݍ��݂̊֐��Q
    /// </summary>
    /// <param name="objectId"></param>
    /// <param name="metaData"></param>
    delegate void LoadMethod(string objectId, MetaDataObject metaData);
    Dictionary<string, LoadMethod> Load = new Dictionary<string, LoadMethod>();

    private void Awake()
    {
        //Object�ǂݍ��݊֐��̓o�^
        Load.Add("world", LoadWorld);
        Load.Add("empty", LoadEmpty);
        Load.Add("object", Load3DModel);
        Load.Add("monitor", LoadMonitor);
        Load.Add("image", Load2DImage);
        Load.Add("video", LoadVideo);
        Load.Add("audio", LoadAudio);
        Load.Add("script", LoadScript);

        path = Application.dataPath + "/../World";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        //GM.Add("world.load", this);
        //GM.Add("world.request", this);
        //GM.Add("world.check", this);
        //GM.Add("object.set", this);
        //GM.Add("object.active", this);
        //GM.Add("object.is_active", this);
    }

    void Receive(string data1, params object[] data2)
    {
        if (data1 == "object.set")
        {
            WriteToObjectMetaData((string)data2[0]);
        }
        else if (data1 == "world.load")
        {
            //2��ނ̃f�[�^��ǂݍ���
            var worldDir = (string)data2[0];
            var noClear = (bool)data2[1];
            var parent = data2.Length == 3 ? (string)data2[2] : "";
            ReadAllMetaData(worldDir, noClear);
            LoadObjectsInWorld(noClear, parent);
        }
        else if (data1 == "world.check")
        {
            //World��ێ����Ă��邩�m�F
            var worldPath = path + (string)data2[0];
            if (!Directory.Exists(worldPath))
            {
                GM.Msg("send", "world.request", dbUser.data[0].id, (string)data2[0]);
                return;
            }
        }
        else if (data1 == "world.request")
        {
            //World�f�[�^���M
            //0: ���M���ID
            //1: WorldId
            var worldPath = path + (string)data2[1];
            ZipFile.CreateFromDirectory(worldPath, $"{worldPath}.zip");
            byte[] byteData = fg.ConvertToByte(worldPath);
            //GM.Msg("send", "world.receive", (string)data2[0], (string)data2[1], byteData);
        }
        else if (data1 == "world.receive")
        {
            //World�f�[�^��M
            //0: ���g��ID
            //1: WorldId
            //2: World�f�[�^
            var worldPath = path + (string)data2[1];
            fg.ConvertToFile(worldPath, (byte[])data2[2]);
            ZipFile.ExtractToDirectory($"{worldPath}.zip", worldPath);
        }
        else if (data1 == "object.active")
        {
            var id = (string)data2[0];
            var b = bool.Parse(data2[1].ToString());
            if (!gameObjects.ContainsKey(id))
            {
                GM.Msg("message", $"{id}�����݂��܂���");
                return;
            }
            gameObjects[id].SetActive(b);
        }
        else if (data1 == "object.is_active")
        {
            var id = (string)data2[0];
            if (!gameObjects.ContainsKey(id))
            {
                GM.Msg("message", $"{id}�����݂��܂���");
                return;
            }

            var ret = gameObjects[id].activeSelf ? 1 : 0;
            GM.Msg("YsResult", ret.ToString());
        }
    }

    /// <summary>
    /// �S�Ă�File�̃��^�f�[�^��ǂݍ���
    /// </summary>
    /// <param name="path"></param>
    void ReadAllMetaData(string path, bool noClear)
    {
        if (!noClear) metaData.Clear();

        //�SFile�擾
        var files = Directory.GetFiles(path, "*.yaml");
        foreach (var file in files)
        {
            var yamlPath = @$"{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}.yaml";
            ReadMetaData(yamlPath, noClear);
        }
    }

    /// <summary>
    /// ���^�f�[�^��ǂݍ���
    /// </summary>
    /// <param name="path"></param>
    void ReadMetaData(string path, bool noClear)
    {
        using (var reader = new StreamReader(path))
        {
            MetaDataFile data = new MetaDataFile();
            var yaml = new YamlStream();
            yaml.Load(reader);
            var rootNode = yaml.Documents[0].RootNode;

            data.version = (string)rootNode["version"];
            data.updated = (string)rootNode["updated"];
            data.type = (string)rootNode["type"];
            if (data.type == "world")
            {
                var worldDir = Path.GetDirectoryName(path);
                ReadObjectMetaData(rootNode, worldDir, noClear);
            }
            metaData.Add(Path.GetFileNameWithoutExtension(path), data);
        }
    }

    /// <summary>
    /// ���E�ɕK�v��Object��S�ēǂݍ���
    /// </summary>
    void LoadObjectsInWorld(bool noClear, string parent)
    {
        //�폜
        if (!noClear)
        {
            worldData.Clear();
            gameObjects.Clear();
            foreach (Transform obj in transform)
            {
                Destroy(obj.gameObject);
            }
        }

        //Object�̏����擾
        foreach (var (objectId, metaData) in nextLoadTargetWorldData)
        {
            if (!Load.ContainsKey(metaData.type)) continue;
            if (!worldData.ContainsKey(objectId))
            {
                worldData.Add(objectId, metaData);
            }
            if (parent != "" && metaData.parent == "")
            {
                //�eObject�̎w��
                var metaDataTemp = metaData;
                metaDataTemp.parent = parent;
                worldData[objectId] = metaDataTemp;
            }
            //��ނɉ�����Object��ǂݍ���
            Load[metaData.type](objectId, metaData);
        }

        //�e�q�֌W�̐ݒ�
        SetParent();

        //����World��ǂݍ���
        for (int i = 0; i < nextLoadMutiWorldName.Count; i++)
        {
            var worldName = nextLoadMutiWorldName[i];
            var objectId = nextLoadMultiWorld[worldName];
            nextLoadMutiWorldName.RemoveAt(i);
            i--;
            GM.Msg("world.load", worldName, true, objectId);
        }
    }


    //=====================================================    
    /// <summary>
    /// �e��ݒ肷��
    /// </summary>
    void SetParent()
    {
        foreach (var (id, metaData) in worldData)
        {
            if (metaData.parent == null) continue;
            if (metaData.parent == "") continue;
            if (!gameObjects.ContainsKey(metaData.parent)) continue;

            var tra = gameObjects[id].transform;
            tra.SetParent(gameObjects[metaData.parent].transform);
            tra.localPosition = metaData.position;
            tra.localRotation = metaData.rotation;
            tra.localScale = metaData.scale;
        }
    }

    /// <summary>
    /// Object�̃��^�f�[�^���擾
    /// </summary>
    /// <param name="node"></param>
    void ReadObjectMetaData(YamlNode node, string worldDir, bool noClear = false)
    {
        nextLoadTargetWorldData.Clear();
        var obj = (YamlMappingNode)node["objects"];

        foreach (var (objectId, c) in obj.Children)
        {
            MetaDataObject data = new MetaDataObject();

            try
            {
                data.file = (string)c["file"];
                data.type = (string)c["type"];
                data.move = bool.Parse((string)c["move"]);
                data.gravity = bool.Parse((string)c["gravity"]);
                data.position = c["position"].ToString().ToVector3();
                data.rotation = Quaternion.Euler(c["rotation"].ToString().ToVector3());
                data.scale = c["scale"].ToString().ToVector3();
                data.parent = (string)c["parent"];
                data.custom = (string)c["custom"];
            }
            catch (Exception ex)
            {
                GM.Msg("message", ex.Message);
            }

            if (!worldPath.ContainsKey((string)objectId))
            {
                worldPath.Add((string)objectId, worldDir);
            }
            if (!nextLoadTargetWorldData.ContainsKey((string)objectId))
            {
                nextLoadTargetWorldData.Add((string)objectId, data);
            }
        }
    }

    /// <summary>
    /// Object��ۑ�����
    /// </summary>
    /// <param name="key"></param>
    void WriteToObjectMetaData(string key)
    {
        if (!gameObjects.ContainsKey(key)) return;

        var tra = gameObjects[key].transform;
        var path = @$"{worldPath[key]}\{Path.GetFileNameWithoutExtension(worldPath[key])}.yaml";
        print(path);
        var sr = new StreamReader(path);
        var yaml = new YamlStream();
        yaml.Load(sr);
        sr.Close();

        var root_node = (YamlMappingNode)yaml.Documents[0].RootNode;
        var node = (YamlMappingNode)root_node["objects"][key];
        ((YamlScalarNode)node.Children["position"]).Value = tra.position.ToSplitString();
        ((YamlScalarNode)node.Children["rotation"]).Value = tra.rotation.eulerAngles.ToSplitString();
        ((YamlScalarNode)node.Children["scale"]).Value = tra.localScale.ToSplitString();
        var sw = new StreamWriter(path);
        yaml.Save(sw, false);
        sw.Close();
    }

    //=====================================================

    void LoadWorld(string objectId, MetaDataObject metaData)
    {
        GameObject obj = new GameObject();
        obj.name = metaData.file;
        obj.transform.SetParent(transform);
        SetGameObject(objectId, metaData, obj);
        var worldName = $"{path}/{metaData.file}";
        if (!nextLoadMultiWorld.ContainsKey(worldName))
        {
            nextLoadMultiWorld.Add(worldName, objectId);
        }
        nextLoadMutiWorldName.Add(worldName);
    }

    void LoadEmpty(string objectId, MetaDataObject metaData)
    {
        GameObject obj = new GameObject();
        obj.transform.SetParent(transform);
        SetGameObject(objectId, metaData, obj);
    }

    void Load2DImage(string objectId, MetaDataObject metaData)
    {
        var pathImg = $"{worldPath[objectId]}/{metaData.file}";
        var data = fg.ConvertToByte(pathImg);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(data);
        GameObject obj = new GameObject();
        obj.transform.SetParent(transform);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        SetGameObject(objectId, metaData, obj);
    }

    void Load3DModel(string objectId, MetaDataObject metaData)
    {
        if (metaData.file.IndexOf('.') == -1)
        {
            LoadBasic3DModel(objectId, metaData);
            return;
        }

        var pathObj = $"{worldPath[objectId]}/{metaData.file}";
        if (!File.Exists(pathObj))
        {
            GM.Msg("message", $"{metaData.file}�����݂��܂���");
            return;
        }

        GameObject obj = null;
        var extension = Path.GetExtension(metaData.file);
        if (extension == ".obj")
        {
            //obj = loader.Load(pathObj);
        }
        else if (extension == ".glb")
        {
            //obj = Importer.LoadFromFile(pathObj);
            //obj = LoadGltf(pathObj);
        }

        //var parent = obj.transform.parent;
        obj.transform.SetParent(transform);
        //Destroy(parent.gameObject);
        foreach (Transform child in obj.transform)
        {
            if (metaData.move) child.gameObject.layer = moveLayer;
            try
            {
                if (child.gameObject.GetComponent<MeshFilter>())
                {
                    var collider = child.gameObject.AddComponent<MeshCollider>(); //�Փ˔���̐ݒ�
                    if (metaData.move) collider.convex = true;
                }
            }
            catch
            {
                GM.Msg("message", "Collider�̐ݒ�Ɏ��s���܂���");
            }
        }
        SetGameObject(objectId, metaData, obj);
    }

    void LoadBasic3DModel(string objectId, MetaDataObject metaData)
    {
        GameObject obj = null;
        if (metaData.file == "cube")
        {
            obj = Instantiate(prefabCube);
        }
        else if (metaData.file == "sphere")
        {
            obj = Instantiate(prefabSphere);
        }
        else return;
        obj.transform.SetParent(transform);
        SetGameObject(objectId, metaData, obj);
    }

    void LoadMonitor(string objectId, MetaDataObject metaData)
    {
        var obj = Instantiate(prefabMonitor);
        obj.AddComponent<BoxCollider>();
        obj.transform.SetParent(transform);
        SetGameObject(objectId, metaData, obj);
    }

    void LoadAudio(string objectId, MetaDataObject metaData)
    {
        var obj = Instantiate(prefabAudio);
        var audio = obj.GetComponent<AudioObject>();
        audio.LoadAudioFromLocal($"{worldPath[objectId]}/{metaData.file}");
        obj.transform.SetParent(transform);
        SetGameObject(objectId, metaData, obj);
    }

    void LoadVideo(string objectId, MetaDataObject metaData)
    {
        var obj = Instantiate(prefabVideo);
        var video = obj.GetComponent<VideoObject>();
        video.LoadVideo($"{worldPath[objectId]}/{metaData.file}");
        obj.transform.SetParent(transform);
        SetGameObject(objectId, metaData, obj);
    }

    class CustomDataScript
    {
        public string exe;
        public string hint;
    }

    void LoadScript(string objectId, MetaDataObject metaData)
    {
        var obj = Instantiate(prefabScript);

        var exe = obj.GetComponent<EventObject>();
        var data = JsonUtility.FromJson<CustomDataScript>(metaData.custom);

        //�X�N���v�g���e
        var txt = File.ReadAllText($"{worldPath[objectId]}/{metaData.file}");
        exe.luaScript = new TextAsset(txt);
        exe.hintText = data.hint;

        //���s���@
        if (data.exe == "none") exe.type = eYScript.exeType.none;
        else if (data.exe == "key") exe.type = eYScript.exeType.interact;
        else if (data.exe == "hit") exe.type = eYScript.exeType.hit;
        else if (data.exe == "start")
        {
            exe.type = eYScript.exeType.none;
            // StartCoroutine(ExeSciprt(exe));
            GM.MsgAsync("Run", exe.luaScript.text).Forget();
        }

        obj.transform.SetParent(transform);
        SetGameObject(objectId, metaData, obj);
    }    

    void SetGameObject(string objectId, MetaDataObject metaData, GameObject obj)
    {
        obj.transform.position = metaData.position;
        obj.transform.rotation = metaData.rotation;
        obj.transform.localScale = metaData.scale;
        obj.AddComponent<ObjectData>().id = objectId;
        if (metaData.move) obj.layer = moveLayer;
        if (metaData.gravity) obj.AddComponent<Rigidbody>().useGravity = true;
        gameObjects.Add(objectId, obj);
    }

    //=====================================================    

    GameObject LoadGltf(string path)
    {
        var data = new AutoGltfFileParser(path).Parse();
        using (data)
        using (var loader = new ImporterContext(data))
        {
            try
            {
                var loaded = loader.Load();
                if (loaded == null) return null;


                return loaded.gameObject;
            }
            catch (Exception ex)
            {
                GM.Msg("message", ex.Message);
                print(ex.Message);
                return null;
            }

        }
    }
}

[System.Serializable]
public struct MetaDataFile
{
    public string version;
    public string updated;
    public string type;
}

[System.Serializable]
public struct MetaDataObject
{
    public string file;
    public string type;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public bool gravity;
    public bool move;
    public string parent;
    public string child;
    public string custom;
}