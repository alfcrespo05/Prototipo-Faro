using UnityEngine;
using UnityEngine.AI;
public class MounstroMarino : MonoBehaviour
{
    [Header("Vida")]
    public float vida = 100f;

    [Header("Movimiento")]
    private Vector3 posicion;
    private NavMeshAgent agente;

    public float velocidad = 5f;
    public float velocidadTierra = 5f;
    public float velocidadAtaqueJugador = 5f;
    public float velocidadAtaqueNaufrago = 5f;
    public float velocidadAtaqueBarco = 5f;
    public float velocidadAtaqueFaro = 5f;
    public float tiempoSumergir = 6f;

    public GameObject barco;

    [Header("Comportamiento")]
    bool sumergido = false;
    bool atacando = false;

    public GameObject monstruoMuerto;

    private void Start()
    {
        agente = GetComponent<NavMeshAgent>();

        if (barco != null)
        {
            AtaqueBarco(barco);
        }
    }

    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;

        Debug.Log(
            gameObject.name +
            " recibe " +
            cantidad +
            " de daño. Vida: " +
            vida
        );

        if (vida <= 0)
        {
            Morir();
        }
    }
    public void AtaqueBarco(GameObject Barco)
    {
        if (Barco == null)
            return;

        agente.speed = velocidadAtaqueBarco;
        agente.SetDestination(Barco.transform.position);
    }

    public void AtaqueFaro()
    {

    }
    public void AtaqueNaufragos()
    {

    }
   public void Sumergirse()
    {
        if (vida <= 50 && atacando == false) sumergido = true;
        if(sumergido == true)
        {
            posicion -= transform.up;
            Destroy(gameObject);
        }
    }
    public void Tierra()
    {

    }
    private void Morir()
    {
        Debug.Log(gameObject.name + " ha muerto.");

        if (monstruoMuerto != null)
        {
            Instantiate(
                monstruoMuerto,
                transform.position,
                transform.rotation
            );
        }

        Destroy(gameObject);
    }
}
