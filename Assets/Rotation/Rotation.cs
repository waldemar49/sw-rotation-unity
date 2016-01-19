using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Rotation : MonoBehaviour {

    public int port = 55555;

    private readonly List<Listener> listeners;

    private readonly IPEndPoint localEP;
    private readonly Socket localSocket;
    private readonly Queue<Quaternion> qs;

    private byte[] buffer;
    private EndPoint remoteEP;
    private AsyncCallback callback;

    public Rotation() {
        localEP = new IPEndPoint(IPAddress.Any, port);
        localSocket = new Socket(localEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        qs = new Queue<Quaternion>();
        listeners = new List<Listener>();
    }

    void Start() {
        localSocket.Bind(localEP);
        BeginReceive();
    }

    void Update() {
        Quaternion q = new Quaternion();
        bool update = false;
        lock (qs) {
            while (qs.Count > 0) {
                q = qs.Dequeue();
                update = true;
            }
        }
        if (update) {
            foreach (Listener listener in listeners) {
                listener.On(q);
            }
        }
    }

    public void Add(Listener listener) {
        listeners.Add(listener);
    }

    public void Remove(Listener listener) {
        listeners.Remove(listener);
    }

    private void BeginReceive() {
        buffer = new byte[4 * 4];
        remoteEP = new IPEndPoint(0, 0);
        callback = new AsyncCallback(EndReceive);

        localSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEP, callback, (object)this);
    }

    private void EndReceive(IAsyncResult result) {
        localSocket.EndReceiveFrom(result, ref remoteEP);

        float[] values = new float[4];
        for (int i = 0; i < 4; ++i) {
            byte[] value = new byte[4];
            Array.Copy(buffer, i * 4, value, 0, 4);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(value);
            }
            values[i] = BitConverter.ToSingle(value, 0);
        }

        float f = (float) Math.Sqrt(2) / 2;
        Quaternion q = new Quaternion(
            f * (values[0] + values[3]),
            -f * (values[1] - values[2]),
            -f * (values[2] + values[1]),
            f * (values[3] - values[0])
        );

        lock (qs) {
            qs.Enqueue(q);
            if (qs.Count > 0) {
                Monitor.PulseAll(qs);
            }
        }

        BeginReceive();
    }

    public interface Listener {
        void On(Quaternion q);
    }
}
