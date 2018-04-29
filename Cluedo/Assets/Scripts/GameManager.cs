using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TIPO { Sospechoso = 0, Lugar = 1, Arma = 2 }
public enum GENERO { Masculino, Femenino}

public class GameManager : MonoBehaviour {

    //Inicio
    public Jugadores[] jugadores = new Jugadores[4];    //Jugadores en partida, de manera predeterminada hay 4, todavía no se puede jugar con menos.
    public Personaje[] personajes = new Personaje[8];   //Sospechosos del asesinato, ellos te ayudaran a conseguir información.
    public Lugar[] lugares = new Lugar[8];              //Habitaciones deç la mansión donde puedes moverte para encontrar información.

    public ListaTarjeta lista;                          //Lista con todo el contenido almacenado en el Scriptable Object.

    Tarjeta[] casoFinal = new Tarjeta[3];               //SOLUCION AL CASO, aquí dentro está el asesino, con que lo mató y donde lo mató.

    //Control de la partida
    [Range(0, 3)]
    public int tuJugador = 0;                           //Dice cual de los 4 jugadores eres tu, siendo 0 el jugador 1, 1 el 2, etc.
    public int maxJugadores = 4;                        //Cantidad de jugadores en la partida, el máximo es 4
    public int turnoDe = -1;                            //Turno de juego actual, si es 0 es turno de jugador 1, etc.
    public int ronda = 0;                               //Cantidad de rondas, cada vez que llega al jugador nº1 este se incrementa en 1.
    public bool puedeMoverse = false;                   //Si está true puede moverse el jugador que juega el turno.
    public bool puedeRealizarAccion = false;            //Si está en true puede gastar la acción tras moverse.

    [Space(10)]
    public TarjetaObject[] cuadrosCarpeta;              //Todos los cuadros de la carpeta, para poder tachar cada una.
    public TarjetaObject[] cuadrosAcusacion;            //Todos los cuadros del cuadro de acusación, para poder hacer una acusación.
    
    //Tarjeta
    Tarjeta[] tarjetas = new Tarjeta[24];               //Todas las tarjetas

    //Inspector
    Personaje inspector;                                //Tarjeta del inspector, necesaria para poder hacer una acusación.
    int rondaMinInspector = 2;                          //Ronda mínima para que llegue el inspector
    float probAparecerInspector = 0.33f;                //Probabilidad de que aparezca el inspector 
    bool haLlegadoInspector = false;                    //Si ha llegado o no, para que no aparezcan más de un inspector cuando se cumpla las anteriores condiciones.
    
    //Carpeta menu
    [Space(10)]
    public GameObject panelMenu;
    public SpriteRenderer hojaSuperior;
    public GameObject canvasSuperior;
    public Sprite[] spriteHojas = new Sprite[2];

    public GameObject[] folios = new GameObject[3];
    int hojaActual = 0;

    public GameObject[] flechas = new GameObject[2];

    Vector3 vectorCerrado = new Vector3(-10, 0.25f, 0), vectorAbierto = new Vector3(0.5f, 0, 0);
    Vector3 rotCerrado = new Vector3(0, 0, -20), rotAbierto = Vector3.zero;

    public TextMesh textoTurno;

    //Boton menu
    [Space(10)]
    public GameObject panelBotonMenu;
    public Button botonEntrarMenu;
    public Button botonSalirMenu;

    //Panel de acciones
    [Space(10)]
    public GameObject panelAcciones;
    public Button accionAcusacion;
    public Button accionBuscar;
    public Button accionResolver;

    //Panel Jugadores
    [Space(10)]
    public GameObject panelJugadores;
    
    //Panel deduccion
    [Space(10)]
    public GameObject panelAcusacion;
    bool esAcusar = false;
    public Text textoCartelDeduccion;
    public RectTransform[] rectAcusacion = new RectTransform[3];    //Los 3 selectores.
    public Text textoDeduccion;
    public Button botonCancelarAcusacion;
    public Button botonDeducir;
    public Button botonAcusar;


    int[] valoresAcusacion = new int[3];                     //Los 3 valores usados para seleccionar la deducción.

    //Variables que se cargan al inicio de la partida
    ChatManager chat;
    InteligenciaArtificial IA;

    DatosJuegos datos;  //Datos con la información de la partida.

    void Awake() {
        chat = GetComponent<ChatManager>();
        IA = GetComponent<InteligenciaArtificial>();
        datos = FindObjectOfType<DatosJuegos>();

        inspector = new Personaje("Inspector Cenizo", -1, GENERO.Masculino, true);
    } 

    void Start() {

        if (datos != null) {
            maxJugadores = datos.MAXIMO_JUGADORES;
            datos.manager = this;

            //CONFIGURA EL JUEGO ONLINE
            if (datos.client != null) {
                Random.InitState(datos.client.seed);    //inicializa la seed para tener ambos la misma partida
                tuJugador = datos.client.ID;
                
                foreach (GameCliente cliente in datos.client.jugadores) {
                    jugadores[cliente.ID].esBot = false;
                    jugadores[cliente.ID].nombre = cliente.nombre;
                }
            } else {
                tuJugador = 0;
                jugadores[0].nombre = datos.nombre;
                jugadores[0].esBot = false;

                for (int i = 1; i < jugadores.Length; i++) {
                    jugadores[i].esBot = true;
                }
            }
        }

        panelMenu.transform.position = vectorCerrado;
        panelMenu.transform.localRotation = Quaternion.Euler(rotCerrado);

        canvasSuperior.SetActive(true);
        panelAcusacion.SetActive(false);
        hojaSuperior.sprite = spriteHojas[0];
        hojaSuperior.transform.localScale = new Vector3(1, 1, 1);

        flechas[0].GetComponent<Boton>().pulsarBoton.AddListener(() => CambiarPagina(-1));
        flechas[1].GetComponent<Boton>().pulsarBoton.AddListener(() => CambiarPagina(1));

        botonEntrarMenu.onClick.AddListener(() => StartCoroutine(AbrirMenu()));
        botonSalirMenu.onClick.AddListener(() => StartCoroutine(CerrarMenu()));

        accionBuscar.onClick.AddListener(() => BuscarBoton());
        accionAcusacion.onClick.AddListener(() => AbrirAcusacion(false));
        accionResolver.onClick.AddListener(() => AbrirAcusacion(true));

        botonCancelarAcusacion.onClick.AddListener(() => CerrarAcusacion());
        botonDeducir.onClick.AddListener(() => IniciarDeduccionBoton(valoresAcusacion[0], valoresAcusacion[1], valoresAcusacion[2]));
        botonAcusar.onClick.AddListener(() => IniciarAcusacionBoton(valoresAcusacion[0], valoresAcusacion[1], valoresAcusacion[2]));

        for (int i = 0; i < lugares.Length; i++) {
            int value = i;
            lugares[i].boton.pulsarBoton.AddListener(() => SeleccionarLugarBoton(value));
        }
        
        //Bloqueas las acciones
        panelAcciones.SetActive(false);
        accionAcusacion.interactable = false;
        accionBuscar.interactable = false;
        accionResolver.interactable = false;

        Configurar();
        IA.Inicializar();

        StartCoroutine(ComenzarPartida());
    }

    //Configura toda la partida
    void Configurar () {
        for (int i = 0; i < jugadores.Length; i++) {
            jugadores[i].texto.text = jugadores[i].nombre;
            jugadores[i].botText.gameObject.SetActive(jugadores[i].esBot);
            jugadores[i].info = new ValorTarjeta[cuadrosCarpeta.Length];
            jugadores[i].tarjetas = new List<Tarjeta>();

            for (int x = 0; x < jugadores[i].info.Length; x++) {
                jugadores[i].info[x] = new ValorTarjeta();
            }

            if (jugadores[i].grafico != null) {
                jugadores[i].personaje.sprite = jugadores[i].grafico;
            }

            jugadores[i].personaje.gameObject.SetActive(i < maxJugadores);
            jugadores[i].ficha.gameObject.SetActive(i < maxJugadores);
        }

        System.Array.Resize<Jugadores>(ref jugadores, maxJugadores);

        for (int i = 0; i < personajes.Length; i++) {
            personajes[i] = new Personaje();
            personajes[i].nombre = lista.tarjetas[i].nombreTarjeta;
            personajes[i].genero = lista.tarjetas[i].genero;
            personajes[i].ID = i;
        }
        
        for (int i = 0; i < cuadrosCarpeta.Length; i++) {

            TarjetaObject carpeta = cuadrosCarpeta[i];
            carpeta.ID = i;
            carpeta.tipo = lista.tarjetas[i].tipo;
            carpeta.equis.SetActive(false);
            carpeta.ojo.SetActive(false);
            carpeta.texto.text = lista.tarjetas[i].nombreTarjeta;
            carpeta.sprite.sprite = lista.tarjetas[i].spriteTarjeta;
            if(carpeta.sprite.sprite != null)
                carpeta.sprite.color = Color.white;
            
            jugadores[tuJugador].info[i].objetoCarpeta = carpeta;

            TarjetaObject acusacion = cuadrosAcusacion[i];
            acusacion.ID = i;
            acusacion.tipo = lista.tarjetas[i].tipo;
            acusacion.equis.SetActive(false);
            acusacion.ojo.SetActive(false);
            acusacion.texto.text = lista.tarjetas[i].nombreTarjeta;
            acusacion.sprite.sprite = lista.tarjetas[i].spriteTarjeta;
            if(acusacion.sprite.sprite != null)
                acusacion.sprite.color = Color.white;

            int pos = i;
            acusacion.boton.onClick.AddListener(() => SeleccionarTarjetaAcusacion(acusacion.tipo, pos));
            jugadores[tuJugador].info[i].objetoAcusacion = acusacion;

            //______________________________________

            Tarjeta tarjeta = new Tarjeta(lista.tarjetas[i]);
            tarjeta.ID = i;
            tarjetas[i] = tarjeta;

        }
    }

    /// <summary>
    /// Comienza la partida con una corrutina
    /// </summary>
    IEnumerator ComenzarPartida() {
        chat.EnviarMensaje(AVISO.Presentacion, true, jugadores[tuJugador].nombre, "Tudor.");

        yield return new WaitForSeconds (0.5f);

        List<Tarjeta> tarjetasTotales = new List<Tarjeta>();
        List<Tarjeta> t_sospechosos = new List<Tarjeta>();
        List<Tarjeta> t_lugares = new List<Tarjeta>();
        List<Tarjeta> t_armas = new List<Tarjeta>();

        foreach (Tarjeta tarjet in tarjetas) {
            tarjetasTotales.Add(tarjet);

            switch (tarjet.tipo) {
                case TIPO.Sospechoso:
                    t_sospechosos.Add(tarjet);
                    break;
                case TIPO.Lugar:
                    t_lugares.Add(tarjet);
                    break;
                case TIPO.Arma:
                    t_armas.Add(tarjet);
                    break;
                default:
                    Debug.LogWarning("Tipo de arma no reconocida.");
                    break;
            }
        }

        for (int i = 0; i < lugares.Length; i++) {
            lugares[i].nombre = t_lugares[i].nombreTarjeta;
            lugares[i].genero = t_lugares[i].genero;
            lugares[i].ID = i;
        }

        //Crea la tarjeta del caso final
        casoFinal = new Tarjeta[3];
        //Selecciona el asesino
        Tarjeta cartaSeleccionada_1 = t_sospechosos[Random.Range(0, t_sospechosos.Count)];
        casoFinal[0] = cartaSeleccionada_1;
        t_sospechosos.Remove(cartaSeleccionada_1);
        tarjetasTotales.Remove(cartaSeleccionada_1);

        //Selecciona el lugar del crimen
        Tarjeta cartaSeleccionada_2 = t_lugares[Random.Range(0, t_lugares.Count)];
        casoFinal[1] = cartaSeleccionada_2;
        t_lugares.Remove(cartaSeleccionada_2);
        tarjetasTotales.Remove(cartaSeleccionada_2);

        //Selecciona el arma del crimen
        Tarjeta cartaSeleccionada_3 = t_armas[Random.Range(0, t_armas.Count)];
        casoFinal[2] = cartaSeleccionada_3;
        t_armas.Remove(cartaSeleccionada_3);
        tarjetasTotales.Remove(cartaSeleccionada_3);

        //Ahora reparte una carta aleatoria a cada sospechoso en el mapa y lo pones en algún lugar aleatorio de la mansión de manera oculta o visible.
        for (int i = 0; i < personajes.Length; i++) {
            Tarjeta cartaSeleccionada = tarjetasTotales[Random.Range(0, tarjetasTotales.Count)];
            personajes[i].tarjeta = cartaSeleccionada;

            tarjetasTotales.Remove(cartaSeleccionada);

            personajes[i].oculto = Random.Range(0, 4)!=0 ? false : true;    //Probabilidad de que un personaje quiera esconderse es de un 25%
            lugares[Random.Range(0, lugares.Length)].personajes.Add(personajes[i]);
        }

        //Por último, reparte las cartas sobrantes a cada jugador.
        int sobrante = tarjetasTotales.Count % jugadores.Length;
        int cartasJugador = tarjetasTotales.Count / jugadores.Length;
        Debug.Log("Cada jugador tendría "+cartasJugador+" carta(s) y sobrarian tras repartir: " + sobrante + " carta(s).");

        for (int i = 0; i < jugadores.Length; i++) {
            for (int x = 0; x < cartasJugador; x++) {
                Tarjeta cartaSeleccionada = tarjetasTotales[Random.Range(0, tarjetasTotales.Count)];
                jugadores[i].tarjetas.Add(cartaSeleccionada);

                //Debug.Log("Jugador " + i + " tacha las tarjeta " + cartaSeleccionada.ID + " y su info es "+ jugadores[i].info[i]);
                jugadores[i].info[cartaSeleccionada.ID].Tachar(true);

                tarjetasTotales.Remove(cartaSeleccionada);

                if (i==tuJugador) {
                    chat.EnviarMensaje(AVISO.Tarjetas, true, chat.DevolverTipoDelito(cartaSeleccionada.tipo, chat.DevolverGenero(cartaSeleccionada.genero, cartaSeleccionada.nombreTarjeta)));
                }

                yield return null;
            }
        }

        for (int i = 0; i < sobrante; i++) {
            Tarjeta cartaSeleccionada = tarjetasTotales[Random.Range(0, tarjetasTotales.Count)];
            int valor = Random.Range(0, jugadores.Length);
            Jugadores _jugador = jugadores[valor];
            _jugador.tarjetas.Add(cartaSeleccionada);

            Debug.Log("La carta sobrante es para "+ _jugador.nombre);
            _jugador.info[cartaSeleccionada.ID].Tachar(true);

            tarjetasTotales.Remove(cartaSeleccionada);

            if(valor == tuJugador) {
                chat.EnviarMensaje(AVISO.Tarjetas, true, chat.DevolverTipoDelito(cartaSeleccionada.tipo, chat.DevolverGenero(cartaSeleccionada.genero, cartaSeleccionada.nombreTarjeta)));
            }
        }

        yield return new WaitForSeconds(0.5f);

        StartCoroutine (PasarTurno());
    }

    IEnumerator AbrirMenu () {
        float lerp = 0;

        panelBotonMenu.SetActive(false);
        botonEntrarMenu.gameObject.SetActive(false);
        botonSalirMenu.gameObject.SetActive(true);
        panelAcciones.SetActive(false);
        panelJugadores.SetActive(false);
        panelAcusacion.SetActive(false);
        chat.panelChat.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        while (lerp < 1) {
            lerp += Time.deltaTime*2;

            panelMenu.transform.position = Vector3.Lerp(vectorCerrado, vectorAbierto, lerp);
            panelMenu.transform.localRotation = Quaternion.Euler(Vector3.Lerp(rotCerrado, rotAbierto, lerp));

            yield return null;
        }

        lerp = 0;
        while (lerp < 1) {
            lerp += Time.deltaTime*4;

            hojaSuperior.transform.localScale = new Vector3 (Mathf.Lerp(1, -1, lerp), 1, 1);
            if(hojaSuperior.transform.localScale.x < 0 && canvasSuperior.gameObject.activeSelf) {
                hojaSuperior.sprite = spriteHojas[1];
                canvasSuperior.gameObject.SetActive(false);
            }
                

            yield return null;
        }

        for (int i = 0; i < flechas.Length; i++) {
            flechas[i].SetActive(true);
        }

        panelBotonMenu.SetActive(true);
    }

    IEnumerator CerrarMenu() {
        float lerp = 0;
        
        panelBotonMenu.SetActive(false);

        for(int i = 0; i < flechas.Length; i++) {
            flechas[i].SetActive(false);
        }

        yield return new WaitForSeconds(0.25f);

        while(lerp < 1) {
            lerp += Time.deltaTime*4;

            hojaSuperior.transform.localScale = new Vector3(Mathf.Lerp(-1, 1, lerp), 1, 1);
            if(hojaSuperior.transform.localScale.x > 0 && !canvasSuperior.gameObject.activeSelf) {
                hojaSuperior.sprite = spriteHojas[0];
                canvasSuperior.gameObject.SetActive(true);
            }


            yield return null;
        }

        lerp = 0;
        while(lerp < 1) {
            lerp += Time.deltaTime*2;

            panelMenu.transform.position = Vector3.Lerp(vectorAbierto, vectorCerrado, lerp);
            panelMenu.transform.localRotation = Quaternion.Euler(Vector3.Lerp(rotAbierto, rotCerrado, lerp));

            yield return null;
        }

        panelBotonMenu.SetActive(true);
        botonEntrarMenu.gameObject.SetActive(true);
        botonSalirMenu.gameObject.SetActive(false);
        panelAcciones.SetActive(turnoDe==tuJugador);
        panelJugadores.SetActive(true);
        chat.panelChat.SetActive(true);
    }

    void SeleccionarLugarBoton (int lugar) {
        if(turnoDe != tuJugador || !puedeMoverse) {
            Debug.Log("No es tu turno");
            return;
        }

        for (int i = 0; i < lugares.Length; i++) {
            lugares[i].coll.enabled = false;
            lugares[i].boton.interactable = true;
        }
        lugares[lugar].boton.interactable = false;
        
        //Función online
        if (datos != null && datos.client != null) {
            string msg = "CMOV|";               //[0] Orden a enviar
            msg += tuJugador.ToString() + "|";  //[1] ID del jugador
            msg += lugar.ToString();            //[2] Lugar a enviar

            datos.client.EnviarDatos(msg);
        } else {
            SeleccionarLugar(lugar);
        }
    }

    public void SeleccionarLugar (int lugar) {
        if(!puedeMoverse || jugadores[turnoDe].lugarActual==lugar) {
            Debug.Log(puedeMoverse + " y " + lugar.ToString() + " - " + jugadores[turnoDe].lugarActual.ToString());

            return;
        }

        StartCoroutine(MoverFicha(jugadores[turnoDe].ficha, lugares[lugar].transform.position));

        Lugar _lugar = lugares[lugar];

        jugadores[turnoDe].lugarActual = lugar;
        chat.EnviarMensaje(AVISO.Moverse, turnoDe == tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(_lugar.genero, _lugar.nombre));

        //______________

        int persCount = 0;
        for (int i = 0; i < _lugar.personajes.Count; i++) {
            if(!_lugar.personajes[i].oculto)
                persCount++;
        }

        if (persCount>0) {
            foreach (Personaje _personaje in _lugar.personajes) {
                if(!_personaje.oculto) {
                    if (!_personaje.esInspector) {
                        jugadores[turnoDe].info[_personaje.tarjeta.ID].Tachar(true);
                        jugadores[turnoDe].info[_personaje.ID].Ver(true);

                        chat.EnviarMensaje(AVISO.Encontrado, turnoDe == tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(_lugar.genero, _lugar.nombre), chat.DevolverGenero(_personaje.genero, _personaje.nombre), chat.DevolverTipoDelito(_personaje.tarjeta.tipo, chat.DevolverGenero(_personaje.tarjeta.genero, _personaje.tarjeta.nombreTarjeta)));
                    } else {
                        chat.EnviarMensaje(AVISO.EncontrarInspector, turnoDe == tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(_lugar.genero, _lugar.nombre), chat.DevolverGenero(_personaje.genero, _personaje.nombre));
                    }
                }
            }
        } else {
            chat.EnviarMensaje (AVISO.NoEncontrado, turnoDe==tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(_lugar.genero, _lugar.nombre));
        }

        chat.EnviarMensaje(AVISO.RealizarAccion, turnoDe == tuJugador);

        if (turnoDe==tuJugador) {
            accionBuscar.interactable = true;
            accionAcusacion.interactable = true;
            accionResolver.interactable = lugares[jugadores[tuJugador].lugarActual].personajes.Contains (inspector) && !inspector.oculto;
        }

        puedeMoverse = false;
    }

    IEnumerator MoverFicha (GameObject ficha, Vector3 destino) {
        float lerp = 0;
        Vector3 inicio = ficha.transform.position;
        while (lerp < 1) {
            lerp += Time.deltaTime*4;

            ficha.transform.position = Vector3.Lerp(inicio, destino, lerp);

            yield return null;
        }
    }

    IEnumerator PasarTurno () {
        puedeRealizarAccion = false;

        panelAcciones.SetActive(false);
        if (turnoDe==-1) {
            turnoDe = 3;
        } else {
            chat.EnviarMensaje(AVISO.FinTurno, turnoDe == tuJugador, jugadores[turnoDe].nombre);
            yield return null;

            //AQUI SE MOSTRARIA LOS PERSONAJES QUE SE SALDRÍA DE LA HABITACION
            Lugar lugarActual = lugares[jugadores[turnoDe].lugarActual];
            for (int i = 0; i < lugarActual.personajes.Count; i++) {
                //Tiene un 50% de que un personaje cambie de habitación.
                if (Random.Range (0, 2)==0) {
                    Personaje _personaje = lugarActual.personajes[i];
                    chat.EnviarMensaje(AVISO.MoverSala, false, chat.DevolverGenero (_personaje.genero, _personaje.nombre), chat.DevolverGenero (lugarActual.genero, lugarActual.nombre));

                    //Posibles lugares donde se va a mover el personaje.
                    Lugar lugarA = lugarActual.ID + 1 >= lugares.Length ? lugares[0] : lugares[lugarActual.ID + 1];
                    Lugar lugarB = lugarActual.ID - 1 < 0 ? lugares[lugares.Length-1] : lugares[lugarActual.ID - 1];

                    //Hay un 50% de que el personaje se vaya a una habitación u otra.
                    if (Random.Range (0, 2)==0) {
                        lugarA.personajes.Add(_personaje);
                    } else {
                        lugarB.personajes.Add(_personaje);
                    }
                    lugarActual.personajes.Remove(_personaje);

                    //Y la probabilidad para que esté oculto en su nuevo lugar será de un 50% tambien.
                    _personaje.oculto = Random.Range(0, 2) != 0 ? false : true;  

                    i--;
                }

                yield return null;

                if (!haLlegadoInspector && ronda >= rondaMinInspector && probAparecerInspector > Random.Range (0f, 1f)) {
                    chat.EnviarMensaje(AVISO.Inspector, false);
                    lugares[Random.Range(0, lugares.Length)].personajes.Add(inspector);
                    haLlegadoInspector = true;
                }
            }

            //MOVER PERSONAJES
            Vector2[] posicionesInicial = new Vector2[jugadores.Length];
            Vector2[] posicionesFinales = new Vector2[jugadores.Length];
            for (int i = 0; i < jugadores.Length; i++) {
                posicionesInicial[i] = jugadores[i].personaje.rectTransform.anchoredPosition;
                posicionesFinales[((i>=jugadores.Length-1)?0:i+1)] = jugadores[i].personaje.rectTransform.anchoredPosition;
            }

            float lerp = 0;
            int nextTurno = turnoDe >= jugadores.Length-1 ? 0 : turnoDe+1;
            while (lerp < 1) {
                lerp += Time.deltaTime*4;

                for (int i = 0; i < jugadores.Length; i++) {
                    jugadores[i].personaje.rectTransform.anchoredPosition = Vector2.Lerp(posicionesInicial[i], posicionesFinales[i], lerp);
                }

                jugadores[turnoDe].personaje.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(120, 96, lerp));
                jugadores[turnoDe].personaje.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(150, 120, lerp));

                jugadores[nextTurno].personaje.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(96, 120, lerp));
                jugadores[nextTurno].personaje.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(120, 150, lerp));

                yield return null;
            }
        }

        turnoDe++;
        if(turnoDe >= jugadores.Length) {
            turnoDe = 0;
            ronda++;
        }
            
        if (!jugadores[turnoDe].eliminado) {
            if(turnoDe == tuJugador) {
                panelAcciones.SetActive(true);
                accionAcusacion.interactable = false;
                accionBuscar.interactable = false;
                accionResolver.interactable = haLlegadoInspector && lugares[jugadores[tuJugador].lugarActual].personajes.Contains(inspector); //SI EL INSPECTOR ESTÁ EN LA HABITACIÓN PUEDES RESOLVER SIN TENER QUE MOVERTE.

                for(int i = 0; i < lugares.Length; i++) {
                    lugares[i].coll.enabled = true;
                }
            }

            chat.EnviarMensaje(AVISO.InicioTurno, turnoDe == tuJugador, jugadores[turnoDe].nombre);
            textoTurno.text = "Turno de <i>" + jugadores[turnoDe].nombre + "</i>.";

            puedeMoverse = true;
            puedeRealizarAccion = true;
        } else {
            chat.EnviarMensaje(AVISO.Eliminado, turnoDe == tuJugador, jugadores[turnoDe].nombre);
            StartCoroutine(PasarTurno());
        }
    }

    void AbrirAcusacion(bool esAcusacion) {
        esAcusar = esAcusacion;
        textoCartelDeduccion.text = (esAcusar) ? "Acusación:" : "Deducción:";
        panelAcusacion.SetActive(true);

        panelAcciones.SetActive(false);
        panelBotonMenu.SetActive(false);
        
        if (!esAcusacion) {
            //Para realizar una deducción solo puede seleccionar el lugar donde estes.
            //Para hacer una acusación puedes estar en cualquier habitación.
            for(int i = 0; i < cuadrosAcusacion.Length; i++) {
                if(cuadrosAcusacion[i].tipo == TIPO.Lugar && cuadrosAcusacion[i].boton != null) {
                    if(jugadores[turnoDe].lugarActual != i - 8) {
                        cuadrosAcusacion[i].boton.interactable = false;
                    } else {
                        cuadrosAcusacion[i].boton.interactable = true;
                        SeleccionarTarjetaAcusacion(TIPO.Lugar, i);
                    }
                }
            }

            botonAcusar.gameObject.SetActive(false);
            botonDeducir.gameObject.SetActive(true);
        } else {
            //Activas todas las tarjetas.
            for(int i = 0; i < cuadrosAcusacion.Length; i++) {
                if(cuadrosAcusacion[i].tipo == TIPO.Lugar && cuadrosAcusacion[i].boton != null) {
                    cuadrosAcusacion[i].boton.interactable = true;
                }
            }

            botonAcusar.gameObject.SetActive(true);
            botonDeducir.gameObject.SetActive(false);
        }
        
    }

    void CerrarAcusacion () {
        panelAcusacion.SetActive(false);

        panelAcciones.SetActive(true);
        panelBotonMenu.SetActive(true);
    }

    void SeleccionarTarjetaAcusacion (TIPO tipo, int valor) {

        valoresAcusacion[(int) tipo] = valor;

        StartCoroutine(MoverSelectorAcusacion(rectAcusacion[(int) tipo], valor));

        if (!esAcusar) {
            textoDeduccion.text = string.Format("<b>{0}:</b> <color=#0000a0ff><i><<Deduzco que el asesinato lo cometió <b>{1}</b> en <b>{2}</b> usando <b>{3}</b>.>></i></color>",
                jugadores[turnoDe].nombre, chat.DevolverGenero(tarjetas[valoresAcusacion[0]].genero, tarjetas[valoresAcusacion[0]].nombreTarjeta),
                chat.DevolverGenero(tarjetas[valoresAcusacion[1]].genero, tarjetas[valoresAcusacion[1]].nombreTarjeta),
                chat.DevolverGenero(tarjetas[valoresAcusacion[2]].genero, tarjetas[valoresAcusacion[2]].nombreTarjeta));
        } else {
            textoDeduccion.text = string.Format("<b>{0}:</b> <color=#0000a0ff><i><<Fué <b>{1}</b> quien cometió el asesinato en <b>{2}</b> usando <b>{3}</b>.>></i></color>",
                jugadores[turnoDe].nombre, chat.DevolverGenero(tarjetas[valoresAcusacion[0]].genero, tarjetas[valoresAcusacion[0]].nombreTarjeta),
                chat.DevolverGenero(tarjetas[valoresAcusacion[1]].genero, tarjetas[valoresAcusacion[1]].nombreTarjeta),
                chat.DevolverGenero(tarjetas[valoresAcusacion[2]].genero, tarjetas[valoresAcusacion[2]].nombreTarjeta));
        }
        
    }

    IEnumerator MoverSelectorAcusacion(RectTransform selector, int valor) {
        Vector2 posInicial = selector.anchoredPosition;
        Vector2 posFinal = cuadrosAcusacion[valor].sprite.rectTransform.anchoredPosition;

        float lerp = 0;
        while (lerp < 1) {
            lerp += Time.deltaTime*10;

            selector.anchoredPosition = Vector2.Lerp(posInicial, posFinal, lerp);

            yield return null;
        }

        selector.anchoredPosition = posFinal;
    }

    void IniciarDeduccionBoton(int sospechosoID, int lugarID, int armaID) {
        if(datos != null && datos.client != null) {
            //Función online.
            string msg = "CDED|";                   //[0] Orden a enviar
            msg += tuJugador.ToString() + "|";      //[1] ID del jugador
            msg += sospechosoID.ToString() + "|";   //[2] ID del sospechoso
            msg += lugarID.ToString() + "|";        //[3] ID del lugar
            msg += armaID.ToString();               //[4] ID de la arma

            datos.client.EnviarDatos(msg);
        } else {
            //Función offline.
            IniciarDeduccion(sospechosoID, lugarID, armaID);
        }
    }

    public void IniciarDeduccion (int sospechosoID, int lugarID, int armaID) {
        //Solo podrás iniciar una acusación si te has movido antes o 
        if(!puedeRealizarAccion)
            return;

        CerrarAcusacion();

        Tarjeta sospechoso = tarjetas[sospechosoID];
        Tarjeta lugar = tarjetas[lugarID];
        Tarjeta arma = tarjetas[armaID];

        chat.EnviarMensaje(AVISO.Deduccion, turnoDe == tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(sospechoso.genero, sospechoso.nombreTarjeta), 
            chat.DevolverGenero(arma.genero, arma.nombreTarjeta), chat.DevolverGenero(lugar.genero, lugar.nombreTarjeta));

        bool encontrado = false;
        int _jugador = turnoDe;

        while (true) {
            _jugador++;
            if(_jugador == jugadores.Length)
                _jugador = 0;

            if(_jugador == turnoDe)
                break;

            foreach (Tarjeta _tarjeta in jugadores[_jugador].tarjetas) {
                if (_tarjeta==sospechoso || _tarjeta==lugar || _tarjeta==arma) {
                    encontrado = true;

                    jugadores[turnoDe].info[_tarjeta.ID].Tachar(true);

                    chat.EnviarMensaje(AVISO.Erroneo, turnoDe == tuJugador, jugadores[_jugador].nombre, chat.DevolverTipoDelito(_tarjeta.tipo, chat.DevolverGenero(_tarjeta.genero, _tarjeta.nombreTarjeta)));
                    break;
                }
            }

            if(encontrado)
                break;
        }

        if (!encontrado)
            chat.EnviarMensaje(AVISO.NoErroneo, turnoDe == tuJugador);

        StartCoroutine(PasarTurno());
    }

    void IniciarAcusacionBoton(int sospechosoID, int lugarID, int armaID) {
        if(datos != null && datos.client != null) {
            //Función online.
            string msg = "CDED|";                   //[0] Orden a enviar
            msg += tuJugador.ToString() + "|";      //[1] ID del jugador
            msg += sospechosoID.ToString() + "|";   //[2] ID del sospechoso
            msg += lugarID.ToString() + "|";        //[3] ID del lugar
            msg += armaID.ToString();               //[4] ID de la arma

            datos.client.EnviarDatos(msg);
        } else {
            //Función offline.
            IniciarDeduccion(sospechosoID, lugarID, armaID);
        }
    }

    public void IniciarAcusacion(int sospechosoID, int lugarID, int armaID) {
        if(!haLlegadoInspector || !lugares[jugadores[turnoDe].lugarActual].personajes.Contains(inspector))
            return;

        CerrarAcusacion();

        Tarjeta sospechoso = tarjetas[sospechosoID];
        Tarjeta lugar = tarjetas[lugarID];
        Tarjeta arma = tarjetas[armaID];

        chat.EnviarMensaje(AVISO.Acusacion, turnoDe == tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(sospechoso.genero, sospechoso.nombreTarjeta),
            chat.DevolverGenero(arma.genero, arma.nombreTarjeta), chat.DevolverGenero(lugar.genero, lugar.nombreTarjeta));
        
        int _jugador = turnoDe;

        int count = 0;
        foreach(Tarjeta _tarjeta in casoFinal) {
            if(_tarjeta == sospechoso || _tarjeta == lugar || _tarjeta == arma) {
                count++;
            }
        }

        if(count==3) {
            chat.EnviarMensaje(AVISO.AcusCorrecta, turnoDe == tuJugador);

            //EL JUEGO TERMINA AQUÍ...

            accionBuscar.interactable = false;
            accionResolver.interactable = false;
            accionAcusacion.interactable = false;

            //Crear menu de victoria y listo
        } else {
            jugadores[turnoDe].eliminado = true;
            jugadores[turnoDe].equis.SetActive(true);

            chat.EnviarMensaje(AVISO.AcusErronea, turnoDe == tuJugador, jugadores[_jugador].nombre, chat.DevolverGenero(casoFinal[0]), chat.DevolverGenero(casoFinal[2]), chat.DevolverGenero(casoFinal[1]));

            StartCoroutine(PasarTurno());
        }
    }

    void BuscarBoton () {
        if(datos != null && datos.client != null) {
            //Función online.
            string msg = "CBUS|";                   //[0] Orden a enviar
            msg += tuJugador.ToString();            //[1] ID del jugador

            datos.client.EnviarDatos(msg);
        } else {
            //Función offline.
            Buscar ();
        }
    }

    public void Buscar () {
        if(!puedeRealizarAccion)
            return;

        chat.EnviarMensaje(AVISO.Buscar, turnoDe == tuJugador, jugadores[turnoDe].nombre);

        Lugar _lugar = lugares[jugadores[turnoDe].lugarActual];
        int persCount = 0;
        for(int i = 0; i < _lugar.personajes.Count; i++) {
            if(_lugar.personajes[i].oculto)
                persCount++;
        }

        if(persCount > 0) {
            foreach(Personaje _personaje in _lugar.personajes) {
                if(_personaje.oculto) {
                    if (!_personaje.esInspector) {
                        jugadores[turnoDe].info[_personaje.tarjeta.ID].Tachar(true);
                        jugadores[turnoDe].info[_personaje.ID].Ver(true);

                        chat.EnviarMensaje(AVISO.Encontrado, turnoDe == tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(_lugar.genero, _lugar.nombre), chat.DevolverGenero(_personaje.genero, _personaje.nombre), chat.DevolverTipoDelito(_personaje.tarjeta.tipo, chat.DevolverGenero(_personaje.tarjeta.genero, _personaje.tarjeta.nombreTarjeta)));
                    } else {
                        chat.EnviarMensaje(AVISO.EncontrarInspector, turnoDe == tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(_lugar.genero, _lugar.nombre), chat.DevolverGenero(_personaje.genero, _personaje.nombre));
                        _personaje.oculto = false;
                    }
                }
            }
        } else {
            chat.EnviarMensaje(AVISO.NoEncontrado, turnoDe == tuJugador, jugadores[turnoDe].nombre, chat.DevolverGenero(_lugar.genero, _lugar.nombre));
        }


        StartCoroutine (PasarTurno());
    }

    /// <summary>
    /// Función que hace funcionar lo de enviar mensajes entre personas en el Cluedo.
    /// </summary>
    public void EnviarMensajeBoton(string persona, string mensaje) {
        if(string.IsNullOrEmpty(mensaje))
            return;

        if(datos != null && datos.client != null) {
            //Función online.
            string msg = "CMSG|";                   //[0] Orden a enviar
            msg += persona + "|";                   //[1] Persona quien escribe
            msg += mensaje;                         //[2] Mensaje a enviar

            datos.client.EnviarDatos(msg);
        } else {
            //Función offline.
            chat.EnviarMensaje(persona, mensaje);
        }
    }

    public void TacharTarjeta (int jugador, Tarjeta tarjeta) {
        jugadores[jugador].info[tarjeta.ID].Tachar(true);
    }

    public void VerSospechoso(int jugador, Tarjeta tarjeta) {
        jugadores[jugador].info[tarjeta.ID].Ver(true);
    }

    void CambiarPagina (int paginas) {
        hojaActual += paginas;
        if (hojaActual >= folios.Length) {
            hojaActual = 0;
        } else if (hojaActual < 0) {
            hojaActual = folios.Length-1;
        }

        for (int i = 0; i < folios.Length; i++) {
            folios[i].SetActive(hojaActual<=i);
        }
    }
}

[System.Serializable]
public class Jugadores {
    public string nombre = "Jugador";
    public bool eliminado = false;
    public bool esBot = false;
    public int lugarActual = -1;
    public GameObject ficha;
    public Image personaje;
    public GameObject equis;
    public Text texto;
    public Sprite grafico;

    public Text botText;


    public List<Tarjeta> tarjetas = new List<Tarjeta>();
    public ValorTarjeta[] info;
}

/// <summary>
/// Personaje del juego
/// </summary>
public class Personaje {
    public string nombre = "Jugador";
    public int ID = 0;
    public GENERO genero = GENERO.Femenino;
    public bool oculto = false;         //Si el personaje está oculto solo puedes verle con la acción "Buscar".

    public bool esInspector = false;    //Si el personaje es el inspector.

    public Tarjeta tarjeta;

    public Personaje () {
        nombre = "Jugador";
        ID = 0;
        genero = GENERO.Femenino;
        oculto = false;

        esInspector = false;

        tarjeta = null;
    }

    public Personaje (string nombre, int ID, GENERO genero, bool esInspector) {
        this.nombre = nombre;
        this.ID = ID;
        this.genero = genero;
        oculto = false;

        this.esInspector = esInspector;

        tarjeta = null;
    }
}