using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(LocalDiscovery))]
public class MyNetworkManager : MonoSingleton <MyNetworkManager> {

	private NetworkClient myClient;

	public Transform[] playerSpawnTransfrom;
	public Transform[] buttonsSpawnTransform;

	[Space(10)]
	public GameObject playerPrefab;
	public LampButton lampButtonPrefab;

	private Dictionary <int, Entity> entities;

	[Space(10)]
	public Transform localPlayerTransform;
	public LampButton localLampButton;
	private int localPlayerId;

	[Space(10)]          
	public bool lampIsOn;

	private bool connected;
	private bool gameFound;

	public int tickRate;
	private float timeBetweenTicks;

	private string host;
	private int port;

	public GameObject connectButton;

	private IEnumerator transfromSendingIEnumerator;

	private const short PlayerSpawnMessageId = 101;
	private const short PLayerConnectMessageId = 102;
	private const short PLayerDisonnectMessageId = 103;
	private const short PLayerTransformMessageId = 104;
	private const short LampStateMessageId = 105;

	void Awake () {
		
		Lamp.Instance.Set (lampIsOn);
		localLampButton.SetSilently (lampIsOn);

		connected = false;
		gameFound = false;
		timeBetweenTicks = 1f / ((float)tickRate);

		connectButton.SetActive (true);
		connectButton.GetComponentInChildren <Text> ().text = "Find Game";
	}


	public void OnButtonClicked () {
		
		// Finding game on LAN
		if (!gameFound) {
			connectButton.SetActive (false);
			GetComponent <LocalDiscovery> ().FindGame ();
			return;
		}

		// Connecting to found game
		if (!connected) {
			ConnectToServer ();
			connectButton.SetActive (false);

		// Disconnecting from game
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
		
		entities = new Dictionary <int, Entity> ();

		myClient = new NetworkClient();
		RegisterHandlers ();
		myClient.Connect(host, port);

		connectButton.SetActive (false);
	}

	public void RegisterHandlers () {

		myClient.RegisterHandler (MsgType.Disconnect, OnDisconnected);
		myClient.RegisterHandler (MsgType.Connect, OnConnected);  

		myClient.RegisterHandler (PlayerSpawnMessageId, OnSpawnPlayer);
		myClient.RegisterHandler (PLayerConnectMessageId, OnConnectPlayer);
		myClient.RegisterHandler (PLayerDisonnectMessageId, OnDisconnectPlayer);

		myClient.RegisterHandler (PLayerTransformMessageId, OnPlayerTransform);
		myClient.RegisterHandler (LampStateMessageId, OnChangeLampState);

	}
		
	// Gives local player his Id
	void OnSpawnPlayer (NetworkMessage msg) {
		localPlayerId = msg.ReadMessage <PlayerSpawnMessage> ().playerId;
		Logger.Instance.Log ("Spawned with playerId = " + localPlayerId.ToString ());

		transfromSendingIEnumerator = SendPlayerTransformCyclically ();
		StartCoroutine (transfromSendingIEnumerator);
	}

	// Gives local plater Id of other player
	void OnConnectPlayer (NetworkMessage msg) {

		int playerId = msg.ReadMessage <PLayerConnectMessage> ().playerId;

		Logger.Instance.Log ("Connected player with id = " + playerId.ToString ());

		entities.Add (playerId, InstantiateEntity (playerId));
	}

	// Insantiates non local player
	Entity InstantiateEntity (int playerId) {

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

		return new Entity (headMovementController, newLampButton);
	}

	// Deletes non local player if he disconnects
	void OnDisconnectPlayer (NetworkMessage msg) {

		int playerId = msg.ReadMessage <PLayerDisonnectMessage> ().playerId;

		Logger.Instance.Log ("Disonnected player with id = " + playerId.ToString ());

		foreach (int id in entities.Keys) {
			if (id == playerId) {
				entities [id].Destroy ();
				entities.Remove (id);
				break;
			}
		}
	}
		
	void OnChangeLampState (NetworkMessage msg) {

		bool _on = msg.ReadMessage <LampStateMessage> ()._on;
		ChangeLampState (_on);
	}

	// Public function to call if you wnat to set lamp state
	public void RequestChangeLampState (bool _on) {

		if (myClient != null && myClient.isConnected) {
			
			LampStateMessage lampStateMessage = new LampStateMessage ();
			lampStateMessage._on = _on;

			myClient.Send (LampStateMessageId, lampStateMessage);
		}

		// Server is authoritarian. In order to hide delay we change lamp state immidiatly, but if server sends other signal we change it again
		ChangeLampState (_on);
	}

	void ChangeLampState (bool _on) {
		
		if (lampIsOn != _on) {

			if (entities != null) {
				foreach (Entity e in entities.Values) {
					e.lampButton.Set (_on);
				}
			}

			localLampButton.Set (_on);
			Lamp.Instance.Set (_on);

			lampIsOn = _on;
		}
	}

	// Changes transform of non local players
	void OnPlayerTransform (NetworkMessage msg) {
		
		PLayerTransformMessage playerTransformMessage = msg.ReadMessage <PLayerTransformMessage> ();
		if (entities.ContainsKey (playerTransformMessage.playerId)) {
			
			((InterpolatingMovementController)entities [playerTransformMessage.playerId].headMovementController)
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
	}

	void OnDisconnected (NetworkMessage msg) {
		OnDisconnected ();
	}

	void OnDisconnected () {
		
		Logger.Instance.Log("Disconnected from server");
		connected = false;
		gameFound = false;

		foreach (Entity e in entities.Values) { e.Destroy (); }
		entities.Clear ();

		StopCoroutine (transfromSendingIEnumerator);

		connectButton.SetActive (true);
		connectButton.GetComponentInChildren <Text> ().text = "Find Game";
	}

	// Sends to server local player's transform several times every second
	IEnumerator SendPlayerTransformCyclically () {

		PLayerTransformMessage playerTransfromMessage = new PLayerTransformMessage ();
		playerTransfromMessage.playerId = localPlayerId;

		while (connected) {
			
			yield return new WaitForSeconds (timeBetweenTicks);
			playerTransfromMessage.eulerAngles = localPlayerTransform.rotation.eulerAngles;
			myClient.Send (PLayerTransformMessageId, playerTransfromMessage);
		}
	}


}
