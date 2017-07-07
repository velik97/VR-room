using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MyNetworkManager : MonoSingleton <MyNetworkManager> {

	public string host = "127.0.0.1";
	public int port = 6321;

	public Text logText;

	public NetworkClient myClient;

	public Transform[] playerSpawnTransfrom;
	public Transform[] buttonsSpawnTransform;

	public GameObject playerPrefab;
	public LampButton lampButtonPrefab;

	public Dictionary <int, NetworkEntity> networkEntities;
	public int localPlayerId;
	public NetworkEntity playerNetworkEntity;

	private bool connected;

	public bool lampIsOn;

	public int tickRate;
	private float timeBetweenTicks;

	private IEnumerator transfromSendingIEnumerator;

	public const short PlayerSpawnMessageId = 101;
	public const short PLayerConnectMessageId = 102;
	public const short PLayerDisonnectMessageId = 103;
	public const short PLayerTransformMessageId = 104;
	public const short LampStateMessageId = 105;

	void Awake () {
		Lamp.Instance.Set (lampIsOn);
		connected = false;
		timeBetweenTicks = 1f / ((float)tickRate);
	}

	public void ConnectToServer () {
		myClient = new NetworkClient();
		RegisterHandlers ();
		networkEntities = new Dictionary <int, NetworkEntity> ();
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
		playerNetworkEntity = InstantiateNetworkEntity <GyroHeadMovementController> (localPlayerId);

		Transform mainCameraTransform = Camera.main.transform;

		mainCameraTransform.position = playerNetworkEntity.headMovementController.transform.position;
		mainCameraTransform.rotation = playerNetworkEntity.headMovementController.transform.rotation;
		mainCameraTransform.SetParent (playerNetworkEntity.headMovementController.transform);

		playerNetworkEntity.headMovementController.gameObject.AddComponent <TouchManager> ();

		Log ("Spawned with playerId = " + localPlayerId.ToString ());
	}

	void OnConnectPlayer (NetworkMessage msg) {

		int playerId = msg.ReadMessage <PLayerConnectMessage> ().playerId;

		Log ("Connected player with id = " + playerId.ToString ());

		networkEntities.Add (playerId, InstantiateNetworkEntity <LinearInterpolatingMovementController> (playerId));
	}

	NetworkEntity InstantiateNetworkEntity <T> (int playerId) where T : HeadMovementController {
		
		GameObject newPLayer = (GameObject)Instantiate (playerPrefab,
			playerSpawnTransfrom [playerId - 1].position,
			playerSpawnTransfrom [playerId - 1].rotation);
		newPLayer.transform.SetParent (playerSpawnTransfrom [playerId - 1]);

		T headMovementController = newPLayer.AddComponent <T> ();

		LampButton newLampButton = (LampButton)Instantiate (lampButtonPrefab,
			buttonsSpawnTransform [playerId - 1].position,
			buttonsSpawnTransform [playerId - 1].rotation);
		newLampButton.transform.SetParent (buttonsSpawnTransform [playerId - 1]);

		newLampButton.Set (lampIsOn);

		return new NetworkEntity (headMovementController, newLampButton);
	}

	void OnDisconnectPlayer (NetworkMessage msg) {

		int playerId = msg.ReadMessage <PLayerDisonnectMessage> ().playerId;

		Log ("Disonnected player with id = " + playerId.ToString ());

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

		Log ("Server: lamp state is " + _on); 
		ChangeLampState (_on);

	}

	public void RequestChangeLampState (bool _on) {
		
		LampStateMessage lampStateMessage = new LampStateMessage ();
		lampStateMessage._on = _on;

		myClient.Send (LampStateMessageId, lampStateMessage);

		ChangeLampState (_on);
	}

	void ChangeLampState (bool _on) {
		
		if (lampIsOn != _on) {
			
			foreach (NetworkEntity ne in networkEntities.Values) {
				ne.lampButton.Set (_on);
			}

			playerNetworkEntity.lampButton.Set (_on);
			Lamp.Instance.Set (_on);

			lampIsOn = _on;
		}
	}

	void OnPlayerTransform (NetworkMessage msg) {
		PLayerTransformMessage playerTransformMessage = msg.ReadMessage <PLayerTransformMessage> ();

		if (networkEntities.ContainsKey (playerTransformMessage.playerId)) {
			((LinearInterpolatingMovementController)networkEntities [playerTransformMessage.playerId].headMovementController)
				.TakeNewValue (playerTransformMessage.eulerAngles);
		} else {
			Log ("[Error] trying to change transfrom of client " + playerTransformMessage.playerId + ", but id doesn't exist");
		}
	}

	public void OnConnected (NetworkMessage nsg) {
		Log("Connected to server");
		connected = true;
		transfromSendingIEnumerator = SendPlayerTransformCyclically ();
		StartCoroutine (transfromSendingIEnumerator);
	}

	public void OnDisconnected (NetworkMessage msg) {
		Log("Disconnected from server");
		connected = false;
		StopCoroutine (transfromSendingIEnumerator);
	}

	IEnumerator SendPlayerTransformCyclically () {
		
		while (connected) {
			yield return new WaitForSeconds (timeBetweenTicks);

			PLayerTransformMessage playerTransfromMessage = new PLayerTransformMessage ();
			playerTransfromMessage.playerId = localPlayerId;
			playerTransfromMessage.eulerAngles = playerNetworkEntity.headMovementController.transform.rotation.eulerAngles;

			myClient.Send (PLayerTransformMessageId, playerTransfromMessage);
		}
	}

	void Log (string log) {
		Debug.Log(log);
		logText.text += "\n// " + log;
	}

}
