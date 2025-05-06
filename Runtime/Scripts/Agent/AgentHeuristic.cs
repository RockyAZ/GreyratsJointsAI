using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Greyrats.JointAI.JointAgent
{
	public class AgentHeuristic : MonoBehaviour
	{
		[Range(-1, 1)]
		[SerializeField] List<float> actions;

		public void Initialize(int actionAmount)
		{
			if (actions == null)
				actions = new List<float>();
			else
				actions.Clear();

			float[] floats = new float[actionAmount];
			actions.AddRange(floats);
		}

		public void MakeHeuristic(in ActionBuffers actBuffer)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				actBuffer.ContinuousActions.Array[i] = actions[i];
			}
		}
	}
}