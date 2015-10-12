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
	int newestPackageN;
	int[] newestPackageSigificants;
	int[] newestPackageExponents;
	string newestPackageInfo;

	byte[] headerBuffer;
	EndPoint headerRemoteEP;
	AsyncCallback headerCallback;

	byte[] bodyBuffer;
	EndPoint bodyRemoteEP;
	AsyncCallback bodyCallback;

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
			if (packages.Count > 0) {
				package = packages.Dequeue ();
			}
		}
		if (package != null) {
			string s = "";
			s += package.Timestamp ();
			s += "\n";
			s += "\n";
			int[] significants = package.Significants ();
			int[] exponents = package.Exponents ();
			for (int i = 0; i < significants.Length; ++i) {
				s += "[";
				s += i;
				s += "] ";
				s += significants [i];
				s += "E-";
				s += exponents [i];
				s += "\n";
			}
			s += "\n";
			s += package.Info ();
			text.text = s;
		}
	}
	
	void BeginReceiveHeader() {
		headerBuffer = new byte[8 + 4];
		headerRemoteEP = new IPEndPoint(0, 0);
		headerCallback = new AsyncCallback (EndReceiveHeader);

		localSocket.BeginReceiveFrom (headerBuffer, 0, headerBuffer.Length, SocketFlags.None, ref headerRemoteEP, headerCallback, (object)this);
	}

	void EndReceiveHeader(IAsyncResult result) {
		localSocket.EndReceiveFrom (result, ref headerRemoteEP);

		byte[] timestamp = new byte[8];
		byte[] n = new byte[4];
		
		Array.Copy (headerBuffer, 0, timestamp, 0, 8);
		Array.Copy (headerBuffer, 8, n, 0, 4);

		if (BitConverter.IsLittleEndian) {
			Array.Reverse (timestamp);
			Array.Reverse (n);
		}

		newestPackageTimestamp = BitConverter.ToInt64 (timestamp, 0);
		newestPackageN = BitConverter.ToInt32 (n, 0);

		BeginReceiveBody ();
	}

	void BeginReceiveBody() {
		bodyBuffer = new byte[(4 + 4) * newestPackageN];
		bodyRemoteEP = new IPEndPoint(0, 0);
		bodyCallback = new AsyncCallback (EndReceiveBody);

		localSocket.BeginReceiveFrom (bodyBuffer, 0, bodyBuffer.Length, SocketFlags.None, ref bodyRemoteEP, bodyCallback, (object)this);
	}

	void EndReceiveBody(IAsyncResult result) {
		localSocket.EndReceiveFrom (result, ref bodyRemoteEP);

		newestPackageSigificants = new int[newestPackageN];
		newestPackageExponents = new int[newestPackageN];
		newestPackageInfo = "TODO";

		for (int i = 0; i < newestPackageN; ++i) {
			byte[] significant = new byte[4];
			byte[] exponent = new byte[4];
			
			Array.Copy(bodyBuffer, i * (4 + 4), significant, 0, 4);
			Array.Copy(bodyBuffer, i * (4 + 4) + 4, exponent, 0, 4);

			if (BitConverter.IsLittleEndian) {
				Array.Reverse (significant);
				Array.Reverse (exponent);
			}
			
			newestPackageSigificants[i] = BitConverter.ToInt32 (significant, 0);
			newestPackageExponents[i] = BitConverter.ToInt32 (exponent, 0);
		}

		enqueuePackage ();

		BeginReceiveHeader ();
	}

	void enqueuePackage() {
		Package package = new Package (newestPackageTimestamp, newestPackageSigificants, newestPackageExponents, newestPackageInfo);
		lock (packages) {
			packages.Enqueue (package);
			if (packages.Count > 0) {
				Monitor.PulseAll(packages);
			}
		}
	}

	public class Package {
		private readonly long timestamp;
		private readonly int[] significants;
		private readonly int[] exponents;
		private readonly string info;

		public Package(long timestamp, int[] significants, int[] exponents, string info) {
			this.timestamp = timestamp;
			this.significants = significants;
			this.exponents = exponents;
			this.info = info;
		}

		public long Timestamp() {
			return timestamp;
		}
		
		public int[] Significants() {
			return significants;
		}
		
		public int[] Exponents() {
			return exponents;
		}

		public string Info() {
			return info;
		}
	}
}
