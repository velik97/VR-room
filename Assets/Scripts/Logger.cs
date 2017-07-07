using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public class Logger : MonoSingleton <Logger> {

	public Text logText;

	public string header;
	public int stringsCount;

	private Queue <string> logHistory;

	void Awake () {
		logHistory = new Queue <string> ();
		logText.text = header;
	}

	public void Log (string log) {
		
		if (logHistory.Count == stringsCount) {
			logHistory.Dequeue ();
		}
		logHistory.Enqueue (log);

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine (header);

		foreach (string l in logHistory) {
			stringBuilder.AppendLine (l);
		}

		logText.text = stringBuilder.ToString ();
		print (log); 

	}
}
