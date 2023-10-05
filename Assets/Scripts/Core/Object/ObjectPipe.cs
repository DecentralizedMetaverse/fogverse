using Cysharp.Threading.Tasks;
using DC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPipe : MonoBehaviour
{
    LineRenderer line;
    const float lineWidth = 0.005f;
    // public Queue<object[]> output;

    // TODO: objIdで管理し、同じobjが登録されないようにする必要がある
    public List<IPipe> connectedObjects { get; set; } = new List<IPipe>();
    object[] outputBuffer;

    public void Add(IPipe target)
    {
        connectedObjects.Add(target);

        if (line == null)
        {
            InitLineRenderer();
        }

        if(outputBuffer!= null)
        {
            target.Receive(outputBuffer);
        }

        DrawLine();
    }

    public void Send(params object[] args)
    {
        outputBuffer = args;
        foreach (var obj in connectedObjects)
        {
            obj.Receive(args);
        }
    }

    private void InitLineRenderer()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        
        // Set color
        line.startColor = Color.green;
        line.endColor = Color.red;

        // Set width
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
    }    

    /// <summary>
    /// 接続先と線で結ぶ
    /// </summary>
    void DrawLine()
    {
        if (connectedObjects.Count == 0) return;

        Vector3[] positions = new Vector3[connectedObjects.Count * 2];
        int j = -1;

        foreach (var obj in connectedObjects)
        {
            positions[++j] = (obj as MonoBehaviour).transform.position;
            positions[++j] = transform.position;
        }

        line.SetPositions(positions);
    }
}
