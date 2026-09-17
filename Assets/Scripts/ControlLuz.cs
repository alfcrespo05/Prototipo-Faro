
using UnityEngine;
using UnityEngine.UI;

public class ControlLuz : MonoBehaviour
{
    // =========================================================
    // LUZ
    // =========================================================

    [Header("Luz")]
    public Light luz;

    public Color colorNormal = Color.white;
    public Color colorFiltro = new Color(0.7f, 0f, 1f);

    // =========================================================
    // BATERIA
    // =========================================================

    [Header("Bateria")]
    public float bateriaMaxima = 100f;
    public float bateriaInicial = 100f;

    [Tooltip("Bateria consumida por segundo mientras la luz esta encendida.")]
    public float consumoBateriaPorSegundo = 5f;

    [Tooltip("Bateria recuperada al gastar una unidad de sangre.")]
    public float bateriaPorSangre = 25f;

    public Slider barraBateria;

    private float bateriaActual;

    // =========================================================
    // FILTRO
    // =========================================================

    [Header("Filtro")]
    public float duracionFiltro = 15f;
    public float filtroMaximo = 100f;
    public float filtroInicial = 100f;
    public float consumoFiltroPorSegundo = 5f;

    [Tooltip("Multiplicador del dano mientras el filtro esta activo.")]
    public float multiplicadorDañoFiltro = 2f;

    public float filtroPorEscama = 25f;

    private float filtroActual;

    public Slider barraFiltro;

    private float tiempoFiltroRestante;

    private bool filtroActivo = false;
    private bool filtroRoto = false;

    // =========================================================
    // FASES VISUALES
    // =========================================================

    [Header("Fases visuales del filtro")]
    public GameObject faseFiltro1;
    public GameObject faseFiltro2;
    public GameObject faseFiltro3;

    // =========================================================
    // INVENTARIO
    // =========================================================

    [Header("Inventario")]
    public Inventario inventario;

    // =========================================================
    // ESTADO
    // =========================================================

    private bool luzEncendida = true;
    private bool puedeControlarse = false;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (luz == null)
        {
            luz = GetComponent<Light>();
        }

        if (luz != null)
        {
            colorNormal = luz.color;
            luz.enabled = false;
            luzEncendida = false;
        }

        bateriaActual =
            Mathf.Clamp(
                bateriaInicial,
                0f,
                bateriaMaxima
            );

        filtroActual =
           Mathf.Clamp(
               filtroInicial,
               0f,
               filtroMaximo
           );

        // UI oculta al comenzar.

        OcultarUI();

        // Filtro completamente oculto al comenzar.

        OcultarFasesFiltro();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!puedeControlarse)
            return;

        // F = ENCENDER / APAGAR

        if (Input.GetKeyDown(KeyCode.F))
        {
            CambiarEstadoLuz();
        }

        // Q = FILTRO

        if (Input.GetKeyDown(KeyCode.Q))
        {
            CambiarEstadoFiltro();
        }

        // R = RECARGAR

        if (Input.GetKeyDown(KeyCode.R))
        {
            RecargarConSangre();
            RecargarConEscama();
        }

        // BATERIA

        if (luzEncendida)
        {
            bateriaActual -=
                consumoBateriaPorSegundo *
                Time.deltaTime;

            bateriaActual =
                Mathf.Clamp(
                    bateriaActual,
                    0f,
                    bateriaMaxima
                );

            if (bateriaActual <= 0f)
            {
                bateriaActual = 0f;

                luzEncendida = false;

                ActualizarLuz();

                Debug.Log(
                    "<color=red>BATERIA AGOTADA. Luz apagada.</color>"
                );
            }

            ActualizarSliderBateria();
        }

        // FILTRO

        if (filtroActivo &&
            luzEncendida &&
            !filtroRoto)
        {
            //tiempoFiltroRestante -=
            //    Time.deltaTime;

            //tiempoFiltroRestante =
            //    Mathf.Clamp(
            //        tiempoFiltroRestante,
            //        0f,
            //        duracionFiltro
            //    );

            filtroActual -=
                consumoFiltroPorSegundo *
                Time.deltaTime;

            ActualizarSliderFiltro();
            ActualizarVisualFiltro();

            if (filtroActual <= 0f)
            {
                RomperFiltro();
            }
        }
    }

    // =========================================================
    // ACTIVAR / DESACTIVAR CONTROL
    // =========================================================

    public void ActivarControl()
    {
        puedeControlarse = true;

        MostrarUI();

        ActualizarSliderBateria();
        ActualizarSliderFiltro();

        Debug.Log(
            "Control de luz activado. " +
            "Bateria: " +
            bateriaActual.ToString("F0") +
            "/100"
        );
    }

    public void DesactivarControl()
    {
        puedeControlarse = false;

        OcultarUI();
    }

    // =========================================================
    // LUZ
    // =========================================================

    private void CambiarEstadoLuz()
    {
        if (!luzEncendida &&
            bateriaActual <= 0f)
        {
            Debug.Log(
                "<color=red>No puedes encender la luz: bateria agotada.</color>"
            );

            return;
        }

        luzEncendida = !luzEncendida;

        ActualizarLuz();

        if (luzEncendida)
        {
            Debug.Log(
                "<color=green>LUZ ENCENDIDA</color> Bateria: " +
                bateriaActual.ToString("F0") +
                "/100"
            );
        }
        else
        {
            Debug.Log(
                "<color=yellow>LUZ APAGADA</color> Bateria conservada en: " +
                bateriaActual.ToString("F0") +
                "/100"
            );
        }
    }

    private void ActualizarLuz()
    {
        if (luz == null)
            return;

        luz.enabled = luzEncendida;

        if (!luzEncendida)
            return;

        if (filtroActivo && !filtroRoto)
        {
            luz.color = colorFiltro;
        }
        else
        {
            luz.color = colorNormal;
        }
    }

    // =========================================================
    // FILTRO
    // =========================================================

    private void CambiarEstadoFiltro()
    {
        if (filtroRoto)
        {
            Debug.Log(
                "<color=red>El filtro esta roto.</color>"
            );

            return;
        }

        filtroActivo = !filtroActivo;

        if (filtroActivo)
        {
            Debug.Log(
                "<color=magenta>FILTRO ACTIVADO</color> " +
                "Tiempo restante: " +
                tiempoFiltroRestante.ToString("F1") +
                " s"
            );

            ActualizarVisualFiltro();
        }
        else
        {
            Debug.Log(
                "<color=yellow>FILTRO DESACTIVADO</color> " +
                "Tiempo conservado: " +
                tiempoFiltroRestante.ToString("F1") +
                " s"
            );

            // Al apagarlo desaparece visualmente.

            OcultarFasesFiltro();
        }

        ActualizarLuz();
        ActualizarSliderFiltro();
    }

    private void RomperFiltro()
    {
        filtroActivo = false;
        filtroRoto = true;

        tiempoFiltroRestante = 0f;

        ActualizarSliderFiltro();

        OcultarFasesFiltro();
        ActualizarLuz();

        Debug.Log(
            "<color=red>FILTRO ROTO! Ya no puede utilizarse.</color>"
        );
    }

    // =========================================================
    // RECARGAR CON SANGRE
    // =========================================================

    private void RecargarConSangre()
    {
        if (inventario == null)
        {
            Debug.LogError(
                "ControlLuz no tiene asignado un Inventario."
            );

            return;
        }

        if (bateriaActual >= bateriaMaxima)
        {
            Debug.Log(
                "<color=yellow>Bateria ya esta al maximo: " +
                bateriaActual.ToString("F0") +
                "/100</color>"
            );

            return;
        }

        if (!inventario.TieneObjeto(TipoObjeto.Sangre))
        {
            Debug.Log(
                "<color=red>No tienes sangre para recargar.</color>"
            );

            return;
        }

        bool sangreEliminada =
            inventario.QuitarObjeto(
                TipoObjeto.Sangre
            );

        if (!sangreEliminada)
        {
            Debug.LogWarning(
                "No se pudo consumir la sangre."
            );

            return;
        }

        float bateriaAntes =
            bateriaActual;

        bateriaActual +=
            bateriaPorSangre;

        bateriaActual =
            Mathf.Clamp(
                bateriaActual,
                0f,
                bateriaMaxima
            );

        float bateriaAnadida =
            bateriaActual - bateriaAntes;

        ActualizarSliderBateria();

        Debug.Log(
            "<color=cyan>RECARGA REALIZADA</color> " +
            "Sangre utilizada: 1 " +
            "Bateria: " +
            bateriaAntes.ToString("F0") +
            " -> " +
            bateriaActual.ToString("F0") +
            "/100 +" +
            bateriaAnadida.ToString("F0")
        );
    }
    private void RecargarConEscama()
    {
        if (inventario == null)
        {
            Debug.LogError(
                "ControlLuz no tiene asignado un Inventario."
            );

            return;
        }

        if (filtroActual >= filtroMaximo)
        {
            Debug.Log(
                "<color=purple>Filtro ya esta al maximo: " +
                filtroActual.ToString("F0") +
                "/50</color>"
            );

            return;
        }

        if (!inventario.TieneObjeto(TipoObjeto.Escamas))
        {
            Debug.Log(
                "<color=red>No tienes escamas para recargar.</color>"
            );

            return;
        }

        bool escamaEliminada =
            inventario.QuitarObjeto(
                TipoObjeto.Escamas
            );

        if (!escamaEliminada)
        {
            Debug.LogWarning(
                "No se pudo consumir la escama."
            );

            return;
        }

        float filtroAntes =
            filtroActual;

        filtroActual +=
            filtroPorEscama;

        filtroActual =
            Mathf.Clamp(
                filtroActual,
                0f,
                filtroMaximo
            );

        float filtroAnadida =
            filtroActual - filtroAntes;

        ActualizarSliderFiltro();

        Debug.Log(
            "<color=cyan>RECARGA REALIZADA</color> " +
            "Escama utilizada: 1 " +
            "Filtro: " +
            filtroAntes.ToString("F0") +
            " -> " +
            filtroActual.ToString("F0") +
            "/50 +" +
            filtroAnadida.ToString("F0")
        );
    }

    // =========================================================
    // SLIDER BATERIA
    // =========================================================

    private void ActualizarSliderBateria()
    {
        if (barraBateria == null)
            return;

        barraBateria.minValue = 0f;
        barraBateria.maxValue = bateriaMaxima;
        barraBateria.value = bateriaActual;
    }

    // =========================================================
    // SLIDER FILTRO
    // =========================================================

    private void ActualizarSliderFiltro()
    {
        if (barraFiltro == null)
            return;

        barraFiltro.minValue = 0f;
        barraFiltro.maxValue = filtroMaximo;
        barraFiltro.value = filtroActual;
    }

    // =========================================================
    // FASES DEL FILTRO
    // =========================================================

    private void ActualizarVisualFiltro()
    {
        // Si el filtro NO esta activo,
        // no mostramos absolutamente nada.

        if (!filtroActivo ||
            filtroRoto)
        {
            OcultarFasesFiltro();
            return;
        }

        float porcentaje =
            filtroActual /
            filtroInicial;

        if (porcentaje > 0.66f)
        {
            MostrarSolo(
                faseFiltro1
            );
        }
        else if (porcentaje > 0.33f)
        {
            MostrarSolo(
                faseFiltro2
            );
        }
        else
        {
            MostrarSolo(
                faseFiltro3
            );
        }
    }

    private void MostrarSolo(GameObject fase)
    {
        if (faseFiltro1 != null)
            faseFiltro1.SetActive(
                fase == faseFiltro1
            );

        if (faseFiltro2 != null)
            faseFiltro2.SetActive(
                fase == faseFiltro2
            );

        if (faseFiltro3 != null)
            faseFiltro3.SetActive(
                fase == faseFiltro3
            );
    }

    private void OcultarFasesFiltro()
    {
        if (faseFiltro1 != null)
            faseFiltro1.SetActive(false);

        if (faseFiltro2 != null)
            faseFiltro2.SetActive(false);

        if (faseFiltro3 != null)
            faseFiltro3.SetActive(false);
    }

    // =========================================================
    // UI
    // =========================================================

    private void MostrarUI()
    {
        if (barraBateria != null)
            barraBateria.gameObject.SetActive(true);

        if (barraFiltro != null)
            barraFiltro.gameObject.SetActive(true);
    }

    private void OcultarUI()
    {
        if (barraBateria != null)
            barraBateria.gameObject.SetActive(false);

        if (barraFiltro != null)
            barraFiltro.gameObject.SetActive(false);
    }

    // =========================================================
    // INFORMACION PARA DANOLUZ
    // =========================================================

    public bool FiltroActivo()
    {
        return filtroActivo &&
               !filtroRoto &&
               luzEncendida;
    }

    public bool LuzEncendida()
    {
        return luzEncendida;
    }

    public float MultiplicadorDaño()
    {
        if (FiltroActivo())
            return multiplicadorDañoFiltro;

        return 1f;
    }

    public float BateriaActual()
    {
        return bateriaActual;
    }

    public float FiltroActual()
    {
        return filtroActual;
    }
}

