using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TCP : MonoBehaviour
{
    public IPEndPoint serverIPEndPoint { get; set; }
    private Socket socket { get; set; }
    public const int bufferSize = 1024;
    public byte[] buffer { get; } = new byte[bufferSize];

    private void Start()
    {
        serverIPEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
    }

    private void Update()
    {
        
    }

    public void Connect(string address, string port)
    {

    }

    public void Send(string msg)
    {
        var sendBytes = new UTF8Encoding().GetBytes(msg);
        socket.Send(sendBytes);
    }

    public void Send(byte[] msg)
    {
        socket.Send(msg);
    }

    public void DisConnect()
    {
        socket?.Disconnect(false);
        socket?.Dispose();
    }
}
