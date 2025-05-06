using Greyrats.JointAI.JointAgent;
using TMPro;
using UnityEngine;

namespace Greyrats.JointAI.Analysis
{
    public class DebugEpisodeCounter : MonoBehaviour
    {
        private const string EPISODE_COUNTER_TEXT = "Episode: {0}";
        public TextMeshProUGUI episodeCounter;
		public AgentControllerBase agent;
        private int episodeCount = 0;

        void Start()
        {
            episodeCounter.text = string.Format(EPISODE_COUNTER_TEXT, episodeCount);
            agent.ON_EPISODE_BEGIN += UpdateEpisodeCounter;
        }

        void OnDestroy()
        {
            agent.ON_EPISODE_BEGIN -= UpdateEpisodeCounter;
        }

        public void UpdateEpisodeCounter()
        {
            episodeCount++;
            episodeCounter.text = string.Format(EPISODE_COUNTER_TEXT, episodeCount);
        }

        public void AddToCounter(int value)
        {
            episodeCount += value;
            episodeCounter.text = string.Format(EPISODE_COUNTER_TEXT, episodeCount);
        }
        
    }
}