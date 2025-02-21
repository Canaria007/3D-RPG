using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    public GameObject playerPrefab;
    GameObject player;
    NavMeshAgent agent;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;
            
        }
    }

    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            Transform destiTransform = GetDestinationByDestinationTag(destinationTag).transform;
            yield return SceneManager.LoadSceneAsync(sceneName);

            //yield return Instantiate(playerPrefab, destiTransform.position, destiTransform.rotation);

            yield return Instantiate(playerPrefab, GetDestinationByDestinationTag(destinationTag).transform.position, GetDestinationByDestinationTag(destinationTag).transform.rotation);

            yield break;
        }
        else
        {
            player = GameManager.Instance.playerStats.gameObject;
            agent = player.GetComponent<NavMeshAgent>();
            agent.enabled = false;
            Transform destiTransform = GetDestinationByDestinationTag(destinationTag).transform;
            agent.enabled = true;
            player.transform.SetPositionAndRotation(destiTransform.position, destiTransform.rotation);
            yield return null;
        }
    }

    private TransitionDestination GetDestinationByDestinationTag(TransitionDestination.DestinationTag destinationTag)
    {
        var entrances = FindObjectsOfType<TransitionDestination>();

        for(int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].destinationTag == destinationTag)
                return entrances[i];
        }
        return null;
    }
}
