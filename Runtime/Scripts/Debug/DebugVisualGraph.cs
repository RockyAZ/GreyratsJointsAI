using Greyrats.JointAI.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace Greyrats.JointAI.Analysis
{
	public class DebugVisualGraph : MonoBehaviour
	{
		public MainAgentHolder mainAgentHolder;
		public GameObject debugInstance;
		public Transform parent;
		public int linePointsAmount;

		private Dictionary<string, float> nameValue;
		private Dictionary<string, DebugVisualInstance> nameToInstance;

		void Awake()
		{
			nameValue = new Dictionary<string, float>();
			nameToInstance = new Dictionary<string, DebugVisualInstance>();
			debugInstance.SetActive(false);
		}

		void OnEnable()
		{
			mainAgentHolder.Agent.ON_REWARD += ApplyNewValue;
		}

		void OnDisable()
		{
			mainAgentHolder.Agent.ON_REWARD -= ApplyNewValue;
		}

		void Update()
		{
			foreach (var key in nameValue.Keys)
			{
				nameToInstance[key].AddQueueValue(nameValue[key]);
			}
		}

		public void ApplyNewValue(float value, string valueName)
		{
			if (!nameValue.ContainsKey(valueName))
			{
				nameValue.Add(valueName, value);
				CreateInstance(valueName);
			}
			else
			{
				nameValue[valueName] = value;
			}
		}

		void CreateInstance(string valueName)
		{
			var instance = Instantiate(debugInstance, parent).GetComponent<DebugVisualInstance>();
			instance.gameObject.SetActive(true);
			instance.Initialize(linePointsAmount, valueName);
			nameToInstance.Add(valueName, instance);
		}
	}
}