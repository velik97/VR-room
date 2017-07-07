using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LocalDiscovery : NetworkDiscovery {

	public void FindGame () {
		Initialize ();
		StartAsClient ();
	}

	public override void OnReceivedBroadcast (string fromAddress, string data) {
		MyNetworkManager.Instance.OnFoundGame (fromAddress, int.Parse (data));
		StopBroadcast ();
	}

}
