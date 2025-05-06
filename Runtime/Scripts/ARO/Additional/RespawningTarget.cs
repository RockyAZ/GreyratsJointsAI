using UnityEngine;
using System;
using EasyButtons;
using Greyrats.JointAI.JointAgent;

public class RespawningTarget : MonoBehaviour
{
    public Action OnTriggerEnterAction;
    public Action OnChangedPositionAction;
    public string TagToTrigger = "Agent";
    public AgentControllerBase AgentController;

    public Transform BoxCorner_1;
    public Transform BoxCorner_2;

    public float RewardOnAgentTouch = 0f;
    public bool endEpisodeOnTouch = false;

    void Awake()
    {
        AgentController.ON_FORCE_END += SetRandomPosition;
        AgentController.ON_EPISODE_BEGIN += SetRandomPosition;
    }

    void OnDestroy()
    {
        AgentController.ON_FORCE_END -= SetRandomPosition;
        AgentController.ON_EPISODE_BEGIN -= SetRandomPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TagToTrigger))
        {
            OnTriggerEnterAction?.Invoke();
            if (endEpisodeOnTouch)
            {
                AgentController.ForceEndEpisode(RewardOnAgentTouch);
            }
            else
            {
                AgentController.AddReward(RewardOnAgentTouch);
            }
            SetRandomPosition();
        }
    }

    private void SetRandomPosition()
    {
        Vector3 min = Vector3.Min(BoxCorner_1.position, BoxCorner_2.position);
        Vector3 max = Vector3.Max(BoxCorner_1.position, BoxCorner_2.position);

        float randomX = UnityEngine.Random.Range(min.x, max.x);
        float randomY = transform.position.y;//UnityEngine.Random.Range(min.y, max.y);
        float randomZ = UnityEngine.Random.Range(min.z, max.z);

        transform.position = new Vector3(randomX, randomY, randomZ);
        OnChangedPositionAction?.Invoke();
    }

    [Button]
    private void TestPosition()
    {
        SetRandomPosition();
    }
}
