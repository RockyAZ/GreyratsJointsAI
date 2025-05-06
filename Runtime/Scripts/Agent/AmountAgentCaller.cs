using EasyButtons;
using Greyrats.JointAI.ARO;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace Greyrats.JointAI.JointAgent
{
	public class AmountAgentCaller : MonoBehaviour
	{
		public AgentControllerBase agent;
		public BehaviorParameters behaviorParameters;
		public JointsControllerARO jointsControllerARO;

		[Button]
		public void Call()
		{
			agent.PrintAmounts(out int a, out int o);
			if (jointsControllerARO.Discrete)
			{
				int[] array = new int[a];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = jointsControllerARO.DiscreteSize;
				}

				behaviorParameters.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(array);
			}
			else
			{
				behaviorParameters.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(a);
			}

			behaviorParameters.BrainParameters.VectorObservationSize = o;
		}
	}
}