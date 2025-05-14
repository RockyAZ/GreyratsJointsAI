using System;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Greyrats.JointAI.ARO;

namespace Greyrats.JointAI.JointAgent
{
	public class AgentControllerBase : Agent
	{
		public Action<float, string> ON_REWARD;
		public Action ON_FORCE_END;
		public Action ON_EPISODE_BEGIN;

		public Transform ParentARO;
		public Transform AgentMainBone;
		public AgentHeuristic heuristic;

		private ActionRewardObservation[] ARO;

		public void ForceEndEpisode(float reward)
		{
			ON_REWARD?.Invoke(reward, "ForceEndEpisode");
			SetReward(reward);
			ON_FORCE_END?.Invoke();
			EndEpisode();
		}
		#region Agent

		public override void Initialize()
		{
			base.Initialize();

			ARO = ParentARO.GetComponentsInChildren<ActionRewardObservation>().Where(t => t.gameObject.activeSelf).ToArray();
			foreach (var tmp in ARO)
			{
				tmp.Initialize(this);
			}

			GetActionsObservations(out int a, out int o);
			heuristic.Initialize(a);
		}
		public override void OnEpisodeBegin()
		{
			base.OnEpisodeBegin();
			foreach (var tmp in ARO)
			{
				tmp.EpisodeBegin();
			}
			ON_EPISODE_BEGIN?.Invoke();
		}

		public override void CollectObservations(VectorSensor sensor)
		{
			base.CollectObservations(sensor);
			foreach (var tmp in ARO)
			{
				tmp.EpisodeObservation(sensor);
			}

		}

		public override void OnActionReceived(ActionBuffers actions)
		{
			base.OnActionReceived(actions);
			int i = 0;
			foreach (var tmp in ARO)
			{
				tmp.EpisodeAction(in actions, ref i);
			}
		}

		public override void Heuristic(in ActionBuffers actionsOut)
		{
			heuristic.MakeHeuristic(actionsOut);
		}
		#endregion

		public void ChangeAgentBody(Transform newMainBone)
		{
			foreach (var tmp in ARO)
			{
				tmp.Dispose();
			}

			newMainBone.position = AgentMainBone.position;
			newMainBone.rotation = AgentMainBone.rotation;
			AgentMainBone = newMainBone;
			Initialize();
		}

		public void PrintAmounts(out int a, out int o)
		{
			Initialize();
			GetActionsObservations(out a, out o);
			print($"Actions:{a}|Observations:{o}");
		}

		public void GetActionsObservations(out int actionAmount, out int observationAmount)
		{
			actionAmount = 0;
			observationAmount = 0;
			foreach (var tmp in ARO)
			{
				actionAmount += tmp.ActionsCount;
				observationAmount += tmp.ObservationsCount;
			}
		}

		void FixedUpdate()
		{
			foreach (var tmp in ARO)
			{
				tmp.EpisodeReward();
			}
		}

		public void AddTitledReward(float reward, string rewardName)
		{
			AddReward(reward);
			ON_REWARD?.Invoke(reward, rewardName);
		}
	}
}
