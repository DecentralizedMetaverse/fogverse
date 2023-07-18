using AnKuchen.Map;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Renci.SshNet;
using System;
using DC;
using UnityEngine.UI;

public class ObjectSSH : ObjectBase
{
    [SerializeField] UICache root;

    string host;
    int port;
    string user;
    string pass;

    private TMP_InputField input;
    private TMP_InputField output;
    private SshClient client;

    ObjectPipe pipe;

    private void Start()
    {
        // TryGetComponent(out IPipe obj);
        pipe = gameObject.AddComponent<ObjectPipe>();
    }

    public void SetData(string path, Dictionary<string, object> data)
    {
        fileName = path;

        input = root.Get<TMP_InputField>("I_InputField");
        output = root.Get<TMP_InputField>("O_InputField");
        input.onSubmit.AddListener((command) => OnSubmit(command));


        host = data["ip"].ToString();
        port = int.Parse(data["port"].ToString());
        user = data["user"].ToString();
        pass = data["password"].ToString();
        
        
        // SSH�N���C�A���g�̍쐬
        client = new SshClient(host, port, user, pass);
        // SSH�T�[�o�[�ɐڑ�
        try
        {
            client.Connect();
        }
        catch(Exception ex)
        {
            GM.Msg("ShortMessage", ex.Message);
        }

        if (client.IsConnected)
        {
            output.text += "Connection success\n";
        }
        else
        {
            output.text += "Connection failed\n";
        }
    }

    private void OnDestroy()
    {
        // SSH�T�[�o�[����ؒf
        client.Disconnect();
    }

    void OnSubmit(string command)
    {
        // �R�}���h�̎��s
        using (var cmd = client.CreateCommand(input.text))
        {
            // �R�}���h�̏o�͂��擾
            var result = cmd.Execute();
            output.text = $"{output.text}{input.text}\n{result}";
            input.text = "";

            pipe.Send(result);

            // GM.Msg("Output1", result);
        }
    }    
}
