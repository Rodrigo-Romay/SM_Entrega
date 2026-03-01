using UnityEngine;

public class SensorVision : MonoBehaviour
{
    public float radioVision = 10f;    
    [Range(0, 180)]
    public float anguloVision = 45f;   
    public LayerMask capaObstaculos;  

    public bool DetectarObjetivo(Transform objetivo, Transform ejecutor)
    {
        if (objetivo == null) return false;

        float distancia = Vector3.Distance(ejecutor.position, objetivo.position);
        
        if (distancia < radioVision)
        {
            Vector3 direccion = (objetivo.position - ejecutor.position).normalized;
            float angulo = Vector3.Angle(ejecutor.forward, direccion);

            if (angulo < anguloVision)
            {
                RaycastHit hit;
                Debug.DrawRay(ejecutor.position + Vector3.up, direccion * distancia, Color.red);

                if (Physics.Raycast(ejecutor.position + Vector3.up, direccion, out hit, radioVision, capaObstaculos))
                {
                    // Si choca con algo y ese algo es el humano, lo vemos
                    if (hit.transform == objetivo)
                    {
                        Debug.DrawRay(ejecutor.position + Vector3.up, direccion * distancia, Color.green);
                        return true;
                    }
                }
                else
                {
                    // Si el rayo no choca con NINGÚN obstáculo, también lo vemos
                    return true;
                }
            }
        }
        return false;
    }
}