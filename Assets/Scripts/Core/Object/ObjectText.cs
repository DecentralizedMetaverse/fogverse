using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObjectText : ObjectBase, IPipe
{
    [SerializeField] Text text;
    ObjectPipe pipe;

    void Start()
    {
        pipe = gameObject.AddComponent<ObjectPipe>();
    }

    public void Receive(params object[] args)
    {
        var text = string.Join(",", args);
        this.text.text = text;
        
        pipe.Send(text);
    }

    public void SetData(string fileName, string text)
    {
        this.fileName = fileName;
        this.text.text = text;

        if(pipe == null) pipe = gameObject.AddComponent<ObjectPipe>();
        pipe.Send(text);
    }
}
