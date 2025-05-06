using EasyButtons;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Greyrats.JointAI.JointAgent;

namespace Greyrats.JointAI.ARO
{
	public abstract class ActionRewardObservation : MonoBehaviour
	{
		protected AgentControllerBase agent;
		public float RewardMultiplier = 1;
		public virtual void Initialize(AgentControllerBase a)
		{
			if (a == null)
				agent = transform.parent.parent.GetComponentInChildren<AgentControllerBase>();
			else
				this.agent = a;
		}

		public abstract void Dispose();

		public virtual void EpisodeBegin() { }
		public virtual void EpisodeAction(in ActionBuffers actions, ref int index) { }
		public virtual void EpisodeReward() { }
		public virtual void EpisodeObservation(VectorSensor sensor) { }
		public virtual void EpisodeHeuristic(in ActionBuffers actionsOut) { }
		public virtual int ObservationsCount => 0;
		public virtual int ActionsCount => 0;
		public virtual int DiscreteActionsCount => 0;

		[Button]
		public void ForceDebugInit()
		{
			Initialize(agent);
		}

	}
}