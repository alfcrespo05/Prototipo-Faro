using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class Camilla : MonoBehaviour
{
    [Header("Cadáver")]
    public GameObject prefabCadaver;
    public Transform puntoCadaver;

    [Header("Extracción")]
    public float tiempoExtraccion = 3f;

    [Header("UI")]
    public Slider barraExtraccion;

    private bool extrayendo = false;

    public void Usar(Inventario inventario)
    {
        // No permitir otra extracción mientras está trabajando
        if (extrayendo)
        {
            Debug.Log("La camilla ya está extrayendo sangre.");
            return;
        }

        if (inventario == null)
        {
            Debug.LogError(
                "La camilla no ha recibido un Inventario."
            );

            return;
        }

        // Comprobar si tenemos cadáver
        if (!inventario.TieneObjeto(TipoObjeto.Cadaver))
        {
            Debug.Log(
                "No tienes ningún cadáver para colocar en la camilla."
            );

            return;
        }

        // Quitar el cadáver del inventario
        bool eliminado =
            inventario.QuitarObjeto(
                TipoObjeto.Cadaver
            );

        if (!eliminado)
        {
            Debug.LogWarning(
                "No se pudo quitar el cadáver del inventario."
            );

            return;
        }

        // Empezar extracción
        StartCoroutine(
            ExtraerSangre(inventario)
        );
    }

    private IEnumerator ExtraerSangre(Inventario inventario)
    {
        extrayendo = true;

        Debug.Log(
            "Extracción de sangre y escama comenzada."
        );

        // ---------------------------------------------------------
        // PREPARAR BARRA
        // ---------------------------------------------------------

        if (barraExtraccion != null)
        {
            barraExtraccion.gameObject.SetActive(true);
            barraExtraccion.minValue = 0f;
            barraExtraccion.maxValue = 1f;
            barraExtraccion.value = 0f;
        }

        // ---------------------------------------------------------
        // CREAR CADÁVER SOBRE LA CAMILLA
        // ---------------------------------------------------------

        GameObject cadaverInstanciado = null;

        if (prefabCadaver != null &&
            puntoCadaver != null)
        {
            cadaverInstanciado = Instantiate(
                prefabCadaver,
                puntoCadaver.position,
                puntoCadaver.rotation
            );

            cadaverInstanciado.transform.SetParent(
                puntoCadaver
            );
        }
        else
        {
            Debug.LogWarning(
                "Falta asignar Prefab Cadaver o Punto Cadaver."
            );
        }

        // ---------------------------------------------------------
        // EXTRAER SANGRE
        // ---------------------------------------------------------

        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoExtraccion)
        {
            tiempoTranscurrido += Time.deltaTime;

            float progreso =
                tiempoTranscurrido /
                tiempoExtraccion;

            if (barraExtraccion != null)
            {
                barraExtraccion.value = progreso;
            }

            yield return null;
        }

        // Asegurar que llegue exactamente al 100%
        if (barraExtraccion != null)
        {
            barraExtraccion.value = 1f;
        }

        // ---------------------------------------------------------
        // ELIMINAR CADÁVER
        // ---------------------------------------------------------

        if (cadaverInstanciado != null)
        {
            Destroy(cadaverInstanciado);
        }

        // ---------------------------------------------------------
        // DAR SANGRE Y ESCAMAS
        // ---------------------------------------------------------

        inventario.AñadirObjeto(
            TipoObjeto.Sangre
        );
        inventario.AñadirObjeto(
           TipoObjeto.Escamas
       );

        Debug.Log(
            "Extracción terminada. Sangre y escama obtenida."
        );

        // ---------------------------------------------------------
        // OCULTAR BARRA
        // ---------------------------------------------------------

        if (barraExtraccion != null)
        {
            barraExtraccion.gameObject.SetActive(false);
        }

        extrayendo = false;
    }
}

