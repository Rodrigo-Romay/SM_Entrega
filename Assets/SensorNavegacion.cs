using UnityEngine;
using UnityEngine.AI;

public class SensorNavegacion : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        if (agent == null) Debug.LogError("SensorNavegacion: No encontré el NavMeshAgent en mi padre.");
    }

    public bool HaLlegadoAlDestino(float umbral)
    {
        if (agent == null) return false;
        return !agent.pathPending && agent.remainingDistance < umbral;
    }

}