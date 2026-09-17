using UnityEngine;

public class Jugador : MonoBehaviour
{
    [SerializeField] private float velocidadMovimiento;
    [SerializeField] private float velocidadRotacion;
    [SerializeField] public float limiteRotacionAr = 90f;
    [SerializeField] public float limiteRotacionAb = -90f;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform transformPersonaje;
    [SerializeField] public Camera camaraPersonaje;

    private Vector3 movimiento;
    public float rotacionX;

    public bool puedeMoverse = true; // Controla si el jugador puede desplazarse

    private void Update()
    {
        if (puedeMoverse)
        {
            MovimientoPersonaje();
        }

        MovimientoCamara(); // Siempre activo, para poder seguir mirando/apuntando
    }

    void MovimientoPersonaje()
    {
        float movX = Input.GetAxis("Horizontal");
        float movZ = Input.GetAxis("Vertical");

        movimiento = transform.right * movX + transform.forward * movZ;
        characterController.SimpleMove(movimiento * velocidadMovimiento);
    }

    void MovimientoCamara()
    {
        float ratonX = Input.GetAxis("Mouse X") * velocidadRotacion;
        float ratonY = Input.GetAxis("Mouse Y") * velocidadRotacion;

        rotacionX -= ratonY;
        rotacionX = Mathf.Clamp(rotacionX, limiteRotacionAb, limiteRotacionAr);

        camaraPersonaje.transform.localRotation = Quaternion.Euler(rotacionX, 0, 0);
        transformPersonaje.Rotate(Vector3.up * ratonX);
    }
}