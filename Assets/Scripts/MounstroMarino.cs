using UnityEngine;

public class MounstroMarino : MonoBehaviour
{
    [Header("Vida")]
    public float vida = 100f;

    public GameObject monstruoMuerto;

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
