using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//	Type						TypeId
//	PlayerSpawnMessage			101
//	PLayerConnectMessage		102
//	PLayerDisonnectMessage		103
//	PLayerTransformMessage		104
//	LampStateMessage			105

public class PlayerSpawnMessage : MessageBase {

	public int playerID;

}

public class PLayerConnectMessage : MessageBase {

	public int playerID;

}

public class PLayerDisonnectMessage : MessageBase {

	public int playerID;

}

public class PLayerTransformMessage : MessageBase {

	public int playerID;
//	public Vector3 position;
	public Quaternion rotation;

}

public class LampStateMessage : MessageBase {

	public bool _on;

}
