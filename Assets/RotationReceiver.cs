using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;

public class RotationReceiver : MonoBehaviour {
	public int port = 55555;
	public Text text;

	readonly IPEndPoint localEP;
	readonly Socket localSocket;
	readonly Queue<Package> packages;

	long newestPackageTimestamp;
	float[] newestPackageValues;

	byte[] headerBuffer;
	EndPoint headerRemoteEP;
	AsyncCallback headerCallback;

	public RotationReceiver() {
		localEP = new IPEndPoint (IPAddress.Any, port);
		localSocket = new Socket (localEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		packages = new Queue<Package> ();
	}

	void Start () {
		localSocket.Bind (localEP);
		BeginReceiveHeader ();
	}

	void Update () {
		Package package = null;
		lock (packages) {
			while (packages.Count > 0) {
				package = packages.Dequeue ();
			}
		}
		if (package != null) {
			string s = "";
			s += package.Timestamp ();
			s += "\n";
			s += "\n";
			float[] values = package.Values ();
			for (int i = 0; i < values.Length; ++i) {
				s += "[";
				s += i;
				s += "] ";
				s += values[i];
				s += "\n";
			}
			text.text = s;
		}
	}
	
	void BeginReceiveHeader() {
		headerBuffer = new byte[8 + 4 * 4];
		headerRemoteEP = new IPEndPoint(0, 0);
		headerCallback = new AsyncCallback (EndReceiveHeader);

		localSocket.BeginReceiveFrom (headerBuffer, 0, headerBuffer.Length, SocketFlags.None, ref headerRemoteEP, headerCallback, (object)this);
	}

	void EndReceiveHeader(IAsyncResult result) {
		localSocket.EndReceiveFrom (result, ref headerRemoteEP);

		byte[] timestamp = new byte[8];
		Array.Copy(headerBuffer, 0, timestamp, 0, 8);
		if (BitConverter.IsLittleEndian) {
			Array.Reverse (timestamp);
		}
		newestPackageTimestamp = BitConverter.ToInt64 (timestamp, 0);

		newestPackageValues = new float[4];
		for (int i = 0; i < 4; ++i) {
			byte[] value = new byte[4];
			Array.Copy(headerBuffer, 8 + i * 4, value, 0, 4);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (value);
			}
			newestPackageValues[i] = BitConverter.ToSingle (value, 0);
		}

		enqueuePackage ();

		BeginReceiveHeader ();
	}

	void enqueuePackage() {
		Package package = new Package (newestPackageTimestamp, newestPackageValues);
		lock (packages) {
			packages.Enqueue (package);
			if (packages.Count > 0) {
				Monitor.PulseAll(packages);
			}
		}
	}

	public class Package {
		private readonly long timestamp;
		private readonly float[] values;

		public Package(long timestamp, float[] values) {
			this.timestamp = timestamp;
			this.values = values;
		}

		public long Timestamp() {
			return timestamp;
		}
		
		public float[] Values() {
			return values;
		}
	}
}
