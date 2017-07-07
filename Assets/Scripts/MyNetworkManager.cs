using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MyNetworkManager : MonoSingleton <MyNetworkManager> {

	public NetworkClient myClient;

	public Transform[] playerSpawnTransfrom;
	public Transform[] buttonsSpawnTransform;

	public GameObject playerPrefab;
	public LampButton lampButtonPrefab;

	public GameObject connectButton;

	public Dictionary <int, NetworkEntity> networkEntities;
	public int localPlayerId;
	public Transform playerTransfrom;
	public LampButton lampButton;

	private bool connected;
	private bool gameFound;

	public bool lampIsOn;

	public int tickRate;
	private float timeBetweenTicks;

	private string host;
	private int port;

	private IEnumerator transfromSendingIEnumerator;

	public const short PlayerSpawnMessageId = 101;
	public const short PLayerConnectMessageId = 102;
	public const short PLayerDisonnectMessageId = 103;
	public const short PLayerTransformMessageId = 104;
	public const short LampStateMessageId = 105;

	void Awake () {
		Lamp.Instance.Set (lampIsOn);
		lampButton.SetSilently (lampIsOn);
		connected = false;
		gameFound = false;
		timeBetweenTicks = 1f / ((float)tickRate);

		connectButton.SetActive (true);
		connectButton.GetComponentInChildren <Text> ().text = "Find Game";
	}

	public void OnButtonClicked () {
		if (!gameFound) {
			connectButton.SetActive (false);
			GetComponent <LocalDiscovery> ().FindGame ();
			return;
		}

		if (!connected) {
			ConnectToServer ();
			connectButton.SetActive (false);
		} else {
			myClient.Disconnect ();
			OnDisconnected ();
		}
	}

	public void OnFoundGame (string _host, int _port) {
		host = _host;
		port = _port;
		gameFound = true;
		Logger.Instance.Log ("Game found");
		connectButton.SetActive (true);
		connectButton.GetComponentInChildren <Text> ().text = "Connect";
	}

	public void ConnectToServer () {
		myClient = new NetworkClient();
		RegisterHandlers ();
		networkEntities = new Dictionary <int, NetworkEntity> ();
		connectButton.SetActive (false);
		myClient.Connect(host, port);
	}

	public void RegisterHandlers () {

		myClient.RegisterHandler (MsgType.Disconnect, OnDisconnected);
		myClient.RegisterHandler (MsgType.Connect, OnConnected);  

		myClient.RegisterHandler (PlayerSpawnMessageId, OnSpawnPlayer);
		myClient.RegisterHandler (PLayerConnectMessageId, OnConnectPlayer);
		myClient.RegisterHandler (PLayerDisonnectMessageId, OnDisconnectPlayer);

		MyNetworkManager.Instance.myClient.RegisterHandler (PLayerTransformMessageId, OnPlayerTransform);
		myClient.RegisterHandler (LampStateMessageId, OnChangeLampState);

	}

	void OnSpawnPlayer (NetworkMessage msg) {
		localPlayerId = msg.ReadMessage <PlayerSpawnMessage> ().playerId;
		Logger.Instance.Log ("Spawned with playerId = " + localPlayerId.ToString ());
	}

	void OnConnectPlayer (NetworkMessage msg) {

		int playerId = msg.ReadMessage <PLayerConnectMessage> ().playerId;

		Logger.Instance.Log ("Connected player with id = " + playerId.ToString ());

		networkEntities.Add (playerId, InstantiateNetworkEntity (playerId));
	}

	NetworkEntity InstantiateNetworkEntity (int playerId) {

		int spawnPoint = (playerId - localPlayerId + 4) % 4;

		GameObject newPLayer = (GameObject)Instantiate (playerPrefab,
			playerSpawnTransfrom [spawnPoint].position,
			playerSpawnTransfrom [spawnPoint].rotation);
		newPLayer.transform.SetParent (playerSpawnTransfrom [spawnPoint]);

		InterpolatingMovementController headMovementController = newPLayer.AddComponent <InterpolatingMovementController> ();

		LampButton newLampButton = (LampButton)Instantiate (lampButtonPrefab,
			buttonsSpawnTransform [spawnPoint].position,
			buttonsSpawnTransform [spawnPoint].rotation);
		newLampButton.transform.SetParent (buttonsSpawnTransform [spawnPoint]);

		newLampButton.Set (lampIsOn);

		return new NetworkEntity (headMovementController, newLampButton);
	}

	void OnDisconnectPlayer (NetworkMessage msg) {

		int playerId = msg.ReadMessage <PLayerDisonnectMessage> ().playerId;

		Logger.Instance.Log ("Disonnected player with id = " + playerId.ToString ());

		foreach (int id in networkEntities.Keys) {
			if (id == playerId) {
				networkEntities [id].Destroy ();
				networkEntities.Remove (id);
				break;
			}
		}
	}

	void OnChangeLampState (NetworkMessage msg) {

		bool _on = msg.ReadMessage <LampStateMessage> ()._on;

		ChangeLampState (_on);

	}

	public void RequestChangeLampState (bool _on) {

		if (myClient != null && myClient.isConnected) {
			LampStateMessage lampStateMessage = new LampStateMessage ();
			lampStateMessage._on = _on;

			myClient.Send (LampStateMessageId, lampStateMessage);
		}
		ChangeLampState (_on);
	}

	void ChangeLampState (bool _on) {
		
		if (lampIsOn != _on) {

			if (networkEntities != null) {
				foreach (NetworkEntity ne in networkEntities.Values) {
					ne.lampButton.Set (_on);
				}
			}

			lampButton.Set (_on);
			Lamp.Instance.Set (_on);

			lampIsOn = _on;
		}
	}

	void OnPlayerTransform (NetworkMessage msg) {
		PLayerTransformMessage playerTransformMessage = msg.ReadMessage <PLayerTransformMessage> ();

		if (networkEntities.ContainsKey (playerTransformMessage.playerId)) {
			((InterpolatingMovementController)networkEntities [playerTransformMessage.playerId].headMovementController)
				.TakeNewValue (playerTransformMessage.eulerAngles);
		} else {
			Logger.Instance.Log ("[Error] trying to change transfrom of client " + playerTransformMessage.playerId + ", but id doesn't exist");
		}
	}

	void OnConnected (NetworkMessage nsg) {
		Logger.Instance.Log("Connected to server");
		connected = true;
		connectButton.SetActive (true);
		connectButton.GetComponentInChildren <Text> ().text = "Disconnect";
		transfromSendingIEnumerator = SendPlayerTransformCyclically ();
		StartCoroutine (transfromSendingIEnumerator);
	}

	void OnDisconnected (NetworkMessage msg) {
		OnDisconnected ();
	}

	void OnDisconnected () {
		Logger.Instance.Log("Disconnected from server");
		connected = false;
		gameFound = false;
		connectButton.SetActive (true);
		connectButton.GetComponentInChildren <Text> ().text = "Connect";
		foreach (NetworkEntity ne in networkEntities.Values) {
			ne.Destroy ();
		}
		networkEntities.Clear ();
		StopCoroutine (transfromSendingIEnumerator);
	}

	IEnumerator SendPlayerTransformCyclically () {
		
		while (connected) {
			yield return new WaitForSeconds (timeBetweenTicks);

			PLayerTransformMessage playerTransfromMessage = new PLayerTransformMessage ();
			playerTransfromMessage.playerId = localPlayerId;
			playerTransfromMessage.eulerAngles = playerTransfrom.rotation.eulerAngles;

			myClient.Send (PLayerTransformMessageId, playerTransfromMessage);
		}
	}


}
