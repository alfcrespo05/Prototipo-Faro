using UnityEngine;

public class DañoLuz : MonoBehaviour
{
    [Header("Configuración del cono")]
    public float distancia = 20f;

    [Range(1f, 90f)]
    public float angulo = 30f;

    [Header("Daño")]
    public float dañoPorSegundo = 20f;

    [Header("Detección")]
    public LayerMask capaEnemigos;

    [Header("Obstáculos")]
    public LayerMask capaObstaculos;

    [Header("Control de luz")]
    public ControlLuz controlLuz;

    private void Start()
    {
        // Si no lo asignamos manualmente,
        // buscamos ControlLuz en el mismo objeto.
        if (controlLuz == null)
        {
            controlLuz =
                GetComponent<ControlLuz>();
        }
    }

    private void Update()
    {
        // Si existe un ControlLuz y la luz está apagada,
        // no hacemos nada.
        if (controlLuz != null &&
            !controlLuz.LuzEncendida())
        {
            return;
        }

        DetectarEnemigos();
    }

    private void DetectarEnemigos()
    {
        Collider[] objetos =
            Physics.OverlapSphere(
                transform.position,
                distancia,
                capaEnemigos
            );

        foreach (Collider objeto in objetos)
        {
            MounstroMarino enemigo =
                objeto.GetComponentInParent<MounstroMarino>();

            if (enemigo == null)
                continue;

            Vector3 direccion =
                objeto.bounds.center -
                transform.position;

            float distanciaEnemigo =
                direccion.magnitude;

            if (distanciaEnemigo <= 0.01f)
                continue;

            direccion.Normalize();

            // -------------------------------------------------
            // COMPROBAR ÁNGULO
            // -------------------------------------------------

            float anguloEntre =
                Vector3.Angle(
                    transform.forward,
                    direccion
                );

            if (anguloEntre > angulo)
                continue;

            // -------------------------------------------------
            // COMPROBAR OBSTÁCULO
            // -------------------------------------------------

            if (capaObstaculos != 0)
            {
                RaycastHit golpe;

                bool bloqueado =
                    Physics.Raycast(
                        transform.position,
                        direccion,
                        out golpe,
                        distanciaEnemigo,
                        capaObstaculos
                    );

                if (bloqueado)
                    continue;
            }

            // -------------------------------------------------
            // CALCULAR DAÑO
            // -------------------------------------------------

            float dañoActual =
                dañoPorSegundo;

            if (controlLuz != null)
            {
                dañoActual *=
                    controlLuz.MultiplicadorDaño();
            }

            enemigo.RecibirDaño(
                dañoActual *
                Time.deltaTime
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 direccion =
            transform.forward *
            distancia;

        Vector3 centro =
            transform.position +
            direccion;

        Gizmos.DrawWireSphere(
            centro,
            Mathf.Tan(
                angulo *
                Mathf.Deg2Rad
            ) *
            distancia
        );

        Gizmos.DrawLine(
            transform.position,
            centro
        );
    }
}