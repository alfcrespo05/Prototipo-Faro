using UnityEngine;

public class Puntero : MonoBehaviour
{
    [Header("Detección")]
    public float distanciaInteraccion = 15f;
    public float radioDeteccion = 0.3f;
    public LayerMask capasInteraccion = ~0;

    [Header("Referencias")]
    public Jugador scriptJugador;
    public Transform jugadorTransform;

    [Header("Teletransporte")]
    public Vector3 offsetTeletransporte = new Vector3(1.756f, 21.495f, -5.102f);

    [Header("Control de luz por órbita")]
    public Camera camaraJugador;
    public Camera camaraEncimaFaro;
    public float velocidadOrbita = 60f;

    [Header("Área rectangular de la cámara del faro")]
    public Transform centroArea;
    public Vector2 tamanoArea = new Vector2(10f, 10f);
    public float velocidadRotacionCamara = 90f;
    public Vector3 offsetCamaraFaro = new Vector3(0f, 10f, 0f);

    [Header("Debug / Visualización")]
    public bool mostrarDebug = true;
    public LineRenderer lineaDebug;

    [Header("Luz del faro")]
    public Transform luzFaro;
    public ControlLuz controlLuz;

    [Header("Puntero de direccion")]
    public RectTransform imagenPuntero;
    public Canvas canvasPuntero;
    public float distanciaProyeccionLuz = 10f;
    public float desplazamientoPunteroX = -50f;

    private Transform esferaSeleccionada;
    private Vector3 posicionCentroLuz;
    private float radioOrbita;
    private float alturaJugador;
    private float anguloActual;
    private bool controlandoLuz = false;

    private Quaternion rotacionOriginalCamaraFaro;
    private Vector3 posicionOriginalCamaraFaro;

    public Inventario inventario;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (lineaDebug != null)
        {
            lineaDebug.positionCount = 2;
            lineaDebug.startWidth = radioDeteccion * 2f;
            lineaDebug.endWidth = radioDeteccion * 2f;
        }

        if (camaraEncimaFaro != null)
        {
            camaraEncimaFaro.enabled = false;

            posicionOriginalCamaraFaro =
                camaraEncimaFaro.transform.position;

            rotacionOriginalCamaraFaro =
                camaraEncimaFaro.transform.rotation;
        }

        if (camaraJugador != null)
        {
            camaraJugador.enabled = true;
        }
    }

    void Update()
    {
        // =========================================================
        // MODO CONTROL DE LUZ
        // =========================================================

        if (controlandoLuz)
        {
            
            float input = Input.GetAxis("Horizontal");

            anguloActual -= input *
                            velocidadOrbita *
                            Mathf.Deg2Rad *
                            Time.deltaTime;

            // -----------------------------------------------------
            // Movimiento orbital del jugador
            // -----------------------------------------------------

            Vector3 offsetOrbita =
                new Vector3(
                    Mathf.Cos(anguloActual),
                    0f,
                    Mathf.Sin(anguloActual)
                ) * radioOrbita;

            Vector3 nuevaPosicion =
                posicionCentroLuz + offsetOrbita;

            nuevaPosicion.y =
                alturaJugador;

            if (jugadorTransform != null)
            {
                jugadorTransform.position =
                    nuevaPosicion;
            }

            // -----------------------------------------------------
            // La esfera mira al jugador
            // -----------------------------------------------------

            if (esferaSeleccionada != null &&
                jugadorTransform != null)
            {
                Vector3 direccion =
                    jugadorTransform.position -
                    posicionCentroLuz;

                direccion.y = 0f;

                if (direccion.sqrMagnitude > 0.01f)
                {
                    esferaSeleccionada.rotation =
                        Quaternion.LookRotation(
                            direccion.normalized
                        );
                }
            }

            // -----------------------------------------------------
            // CÁMARA DEL FARO
            // -----------------------------------------------------

            if (camaraEncimaFaro != null &&
                jugadorTransform != null)
            {
                bool dentroDelArea =
                    DentroDelAreaRectangular(
                        jugadorTransform.position
                    );

                // =================================================
                // FUERA DEL ÁREA
                // =================================================

                if (!dentroDelArea &&
                    esferaSeleccionada != null &&
                    luzFaro != null)
                {
                    // La cámara sigue al Light
                    camaraEncimaFaro.transform.position =
                        luzFaro.position +
                        offsetCamaraFaro;

                    // Rotación horizontal de la cámara
                    // según la orientación del Light
                    float yawObjetivo =
                        luzFaro.eulerAngles.y;

                    float yawActual =
                        camaraEncimaFaro.transform.eulerAngles.y;

                    float nuevoYaw =
                        Mathf.MoveTowardsAngle(
                            yawActual,
                            yawObjetivo,
                            velocidadRotacionCamara *
                            Time.deltaTime
                        );

                    camaraEncimaFaro.transform.rotation =
                        Quaternion.Euler(
                            rotacionOriginalCamaraFaro.eulerAngles.x,
                            nuevoYaw,
                            rotacionOriginalCamaraFaro.eulerAngles.z
                        );
                }

                // =================================================
                // DENTRO DEL ÁREA
                // =================================================

                else
                {
                    // Vuelve a la posición original
                    camaraEncimaFaro.transform.position =
                        Vector3.MoveTowards(
                            camaraEncimaFaro.transform.position,
                            posicionOriginalCamaraFaro,
                            velocidadRotacionCamara *
                            Time.deltaTime
                        );

                    // Vuelve a la rotación original
                    float yawVolver =
                        Mathf.MoveTowardsAngle(
                            camaraEncimaFaro.transform.eulerAngles.y,
                            rotacionOriginalCamaraFaro.eulerAngles.y,
                            velocidadRotacionCamara *
                            Time.deltaTime
                        );

                    camaraEncimaFaro.transform.rotation =
                        Quaternion.Euler(
                            rotacionOriginalCamaraFaro.eulerAngles.x,
                            yawVolver,
                            rotacionOriginalCamaraFaro.eulerAngles.z
                        );
                }
            }
            ActualizarPunteroLuz();
            // -----------------------------------------------------
            // Salir del modo control
            // -----------------------------------------------------

            if (Input.GetKeyDown(KeyCode.E) ||
                Input.GetKeyDown(KeyCode.Escape))
            {
                SalirModoControlLuz();
            }

            return;
        }

        // =========================================================
        // DETECCIÓN NORMAL
        // =========================================================

        Ray rayo =
            new Ray(
                transform.position,
                transform.forward
            );

        RaycastHit golpe;

        bool impacto =
            Physics.SphereCast(
                rayo,
                radioDeteccion,
                out golpe,
                distanciaInteraccion,
                capasInteraccion
            );

        if (mostrarDebug)
        {
            float largoLinea =
                impacto
                    ? golpe.distance
                    : distanciaInteraccion;

            Vector3 puntoFinal =
                transform.position +
                transform.forward *
                largoLinea;

            Debug.DrawRay(
                transform.position,
                transform.forward *
                largoLinea,
                impacto
                    ? Color.green
                    : Color.red
            );

            if (lineaDebug != null)
            {
                lineaDebug.SetPosition(
                    0,
                    transform.position
                );

                lineaDebug.SetPosition(
                    1,
                    puntoFinal
                );

                lineaDebug.startColor =
                    impacto
                        ? Color.green
                        : Color.red;

                lineaDebug.endColor =
                    impacto
                        ? Color.green
                        : Color.red;
            }
        }

        if (impacto)
        {

            Transform actual =
                golpe.collider.transform;

            while (actual != null)
            {
                // ============================================
                // FARO
                // ============================================

                if (actual.CompareTag("Faro"))
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Teletransportar(
                            actual.gameObject
                        );
                    }

                    break;
                }

                // ============================================
                // LUZ
                // ============================================

                if (actual.CompareTag("Luz"))
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        EntrarModoControlLuz(
                            actual.gameObject
                        );
                    }

                    break;
                }

                // ============================================
                // CADÁVER
                // ============================================

                if (actual.CompareTag("MounstroMuerto"))
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        RecogerCadaver(
                            actual.gameObject
                        );
                    }

                    break;
                }

                // ============================================
                // CAMILLA
                // ============================================

                if (actual.CompareTag("camilla"))
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Camilla camilla =
                            actual.GetComponent<Camilla>();

                        if (camilla != null)
                        {
                            camilla.Usar(inventario);
                        }
                        else
                        {
                            Debug.LogError(
                                "El objeto tiene el tag Camilla pero no tiene el componente Camilla."
                            );
                        }
                    }

                    break;
                }

                actual = actual.parent;
            }
        }
    }

    // =============================================================
    // RECOGER CADÁVER
    // =============================================================

    void RecogerCadaver(GameObject cadaver)
    {
        if (inventario == null)
        {
            Debug.LogError(
                "El Puntero no tiene asignado el Inventario."
            );

            return;
        }

        inventario.AñadirObjeto(
            TipoObjeto.Cadaver
        );

        Destroy(cadaver);

        Debug.Log(
            "Has recogido un cadáver."
        );
    }

    // =============================================================
    // ÁREA RECTANGULAR
    // =============================================================

    bool DentroDelAreaRectangular(
        Vector3 posicion
    )
    {
        Vector3 centro =
            centroArea != null
                ? centroArea.position
                : posicionCentroLuz;

        float mitadAncho =
            tamanoArea.x * 0.5f;

        float mitadProfundidad =
            tamanoArea.y * 0.5f;

        float diffX =
            Mathf.Abs(
                posicion.x -
                centro.x
            );

        float diffZ =
            Mathf.Abs(
                posicion.z -
                centro.z
            );

        return diffX <= mitadAncho &&
               diffZ <= mitadProfundidad;
    }

    // =============================================================
    // TELETRANSPORTE
    // =============================================================

    private void Teletransportar(GameObject faro)
    {
        Debug.Log("TELETRANSPORTE DEL FARO");

        GameObject puntoDestino = GameObject.Find("PuntoTeletransporte");

        if (puntoDestino == null)
        {
            Debug.LogError("NO SE ENCONTRO PuntoTeletransporte");
            return;
        }

        CharacterController controlador =
            jugadorTransform.GetComponentInParent<CharacterController>();

        if (controlador != null)
            controlador.enabled = false;

        jugadorTransform.SetPositionAndRotation(
            puntoDestino.transform.position,
            puntoDestino.transform.rotation
        );

        Physics.SyncTransforms();

        if (controlador != null)
            controlador.enabled = true;

        Debug.Log("Jugador teletransportado a: " +
                  puntoDestino.transform.position);
    }

    // =============================================================
    // ENTRAR EN CONTROL DE LUZ
    // =============================================================

    void EntrarModoControlLuz(
        GameObject objetoGolpeado
    )
    {
        Transform esfera =
            objetoGolpeado.transform;

        if (esfera != null &&
            jugadorTransform != null)
        {
            esferaSeleccionada =
                esfera;

            posicionCentroLuz =
                esfera.position;

            // Buscar el Light dentro de la esfera
            Light componenteLuz =
                esfera.GetComponentInChildren<Light>();

            if (componenteLuz != null)
            {
                luzFaro = componenteLuz.transform;

                // Buscar el controlador de la luz
                controlLuz =
                    componenteLuz.GetComponent<ControlLuz>();

                if (controlLuz != null)
                {
                    controlLuz.ActivarControl();
                }
            }
            else
            {
                luzFaro = null;

                Debug.LogWarning(
                    "No se encontró ningún Light dentro de la esfera."
                );
            }

            if (luzFaro == null)
            {
                Debug.LogWarning(
                    "No se encontró ningún Light dentro de la esfera."
                );
            }

            Vector3 diferencia =
                jugadorTransform.position -
                posicionCentroLuz;

            alturaJugador =
                jugadorTransform.position.y;

            diferencia.y = 0f;

            radioOrbita =
                diferencia.magnitude;

            anguloActual =
                Mathf.Atan2(
                    diferencia.z,
                    diferencia.x
                );

            controlandoLuz =
                true;

            if (scriptJugador != null)
            {
                scriptJugador.puedeMoverse =
                    false;
            }

            if (camaraJugador != null)
            {
                camaraJugador.enabled =
                    false;
            }

            if (camaraEncimaFaro != null)
            {
                camaraEncimaFaro.transform.position =
                    posicionOriginalCamaraFaro;

                camaraEncimaFaro.transform.rotation =
                    rotacionOriginalCamaraFaro;

                camaraEncimaFaro.enabled =
                    true;
            }
        }
    }

    // =============================================================
    // SALIR DEL CONTROL DE LUZ
    // =============================================================

    void SalirModoControlLuz()
    {
        controlandoLuz =
            false;

        esferaSeleccionada =
            null;

        if (scriptJugador != null)
        {
            scriptJugador.puedeMoverse =
                true;

            scriptJugador.limiteRotacionAb =
                -90f;

            scriptJugador.limiteRotacionAr =
                90f;
        }

        if (camaraEncimaFaro != null)
        {
            camaraEncimaFaro.transform.position =
                posicionOriginalCamaraFaro;

            camaraEncimaFaro.transform.rotation =
                rotacionOriginalCamaraFaro;

            camaraEncimaFaro.enabled =
                false;
        }

        if (camaraJugador != null)
        {
            camaraJugador.enabled = true;
        }

        if (controlLuz != null)
        {
            controlLuz.DesactivarControl();
        }
    }

    private void ActualizarPunteroLuz()
    {
        if (imagenPuntero == null ||
            canvasPuntero == null ||
            camaraEncimaFaro == null ||
            luzFaro == null ||
            jugadorTransform == null)
        {
            return;
        }

        bool dentroDelArea =
            DentroDelAreaRectangular(
                jugadorTransform.position
            );

        RectTransform rectCanvas =
            canvasPuntero.transform as RectTransform;

        if (!dentroDelArea)
        {
            imagenPuntero.anchoredPosition =
                Vector2.zero;

            return;
        }

        Vector3 direccionSuelo =
            Vector3.ProjectOnPlane(
                luzFaro.forward,
                Vector3.up
            );

        if (direccionSuelo.sqrMagnitude < 0.001f)
        {
            return;
        }

        direccionSuelo.Normalize();

        Vector3 puntoProyeccion =
            luzFaro.position +
            direccionSuelo *
            distanciaProyeccionLuz;

        Vector3 posicionPantalla =
            camaraEncimaFaro.WorldToScreenPoint(
                puntoProyeccion
            );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectCanvas,
            posicionPantalla,
            null,
            out Vector2 posicionCanvas
        );

        posicionCanvas.x += desplazamientoPunteroX;

        imagenPuntero.anchoredPosition =
            posicionCanvas;
    }

    // =============================================================
    // GIZMOS
    // =============================================================

    void OnDrawGizmos()
    {
        if (mostrarDebug)
        {
            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position +
                transform.forward *
                distanciaInteraccion,
                radioDeteccion
            );
        }

        Vector3 centro =
            centroArea != null
                ? centroArea.position
                : (
                    Application.isPlaying
                        ? posicionCentroLuz
                        : transform.position
                );

        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireCube(
            centro,
            new Vector3(
                tamanoArea.x,
                0.5f,
                tamanoArea.y
            )
        );
    }
}
