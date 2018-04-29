using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Distinto tipos de avisos personalizados para el chat para mostrar el trascuso de la partida.
/// </summary>
public enum AVISO {
    Presentacion, Tarjetas,             //Presentación del juego y entrega de tus tarjetas.
    InicioTurno, FinTurno, Eliminado,   //De turno, represente el principio y final de cada turno.
    Moverse, Encontrado, NoEncontrado,  //Al entrar a una habitación te dirá si ha encontrado o no a ha alguien.
    RealizarAccion,                     //Te dice que debes realizar una acción antes de pasar turno.
    Deduccion, Buscar, Acusacion,       //Acciones disponibles.
    Erroneo, NoErroneo,                 //Accion Deduccion: Te dice si tu acusación es contradecida por la información de los otros jugadores o no.
    MoverSala,                          //Al final de cada turno puede moverse las personas de su habitación
    Inspector, EncontrarInspector,      //Si ha llegado el inspector
    AcusErronea, AcusCorrecta,          //Si la acusación final o no correcta

    Mensaje, Susurro,                   //Para el modo online, permite enviar mensaje
    Desconectado                        //Un usuario se ha desconectado del juego.
}

public class ChatManager : MonoBehaviour {

    //Chat
    public GameObject panelChat;
    public RectTransform panelInferior;

    public GameObject chatMensaje;

    List<Mensaje> mensajesEnviados = new List<Mensaje>();

    public InputField texto;
    public Button botonEnviar;

    GameManager manager;

    void Awake() {
        manager = GetComponent<GameManager>();
    }

    void Start() {
        botonEnviar.onClick.AddListener(() => {
            manager.EnviarMensajeBoton(manager.jugadores[manager.tuJugador].nombre, texto.text);
            texto.text = "";
            });
    }

    void Update() {
        if (Input.GetKeyUp (KeyCode.Return) && texto.isFocused) {
            manager.EnviarMensajeBoton(manager.jugadores[manager.tuJugador].nombre, texto.text);
            texto.text = "";
        }
    }

    public void EnviarMensaje(AVISO aviso, bool eresTu, params string[] textos) {
        GameObject _obj = Instantiate(chatMensaje);
        Mensaje _mensaje = _obj.GetComponent<Mensaje>();

        if(_mensaje == null) {
            Debug.LogWarning("Existe un error con el mensaje");
            return;
        }

        _obj.transform.SetParent(panelInferior);
        _mensaje.texto.text = "";

        switch(aviso) {
            case AVISO.Presentacion:
                _mensaje.imagen.color = new Color(1f, 1f, 0.9f, 1f);
                //Ya pensaré en alguna presentación más decente para dar inicio a la partida
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                   "<b><i>{0}</i></b>, bienvenidos a <b>la mansión de los {1}</b>, intenta resolver el caso antes que tus contrincantes. Mucha suerte.",
                   textos);
                }
                break;

            case AVISO.Tarjetas:
                _mensaje.imagen.color = new Color(0.5f, 1f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                   "Sabes que el crimen no <b>{0}</b>",
                   textos);
                }
                break;

            case AVISO.InicioTurno:
                _mensaje.imagen.color = new Color(1f, 1f, 0.9f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "<b>Tu turno</b>.\n" +
                    "Muevete una de las habitaciones de la mansión para buscar pistas sobre la muerte del <b>Señor Blanco</b>.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<b>Turno de <i>{0}</i></b>\n" +
                    "Espera a que se mueva a alguna de las habitaciones.",
                    textos);
                }
                break;

            case AVISO.Eliminado:
                _mensaje.imagen.color = new Color(1f, 1f, 0.9f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "<b>Estas eliminado por acusar falsamente</b>.\n" +
                    "Pasa al siguiente jugador.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<i>{0}</i> está eliminado.\n" +
                    "Pasa al siguiente jugador.",
                    textos);
                }
                break;
                
            case AVISO.RealizarAccion:
                _mensaje.imagen.color = new Color(1f, 1f, 0.9f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "Ahora tienes que realizar una acción para pasar tu turno.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "Espera a que realice alguna acción para pasar el turno.",
                    textos);
                }
                break;

            case AVISO.FinTurno:
                _mensaje.imagen.color = new Color(1f, 1f, 0.9f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "<b>Fin de tu turno</b>",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<b>Fin del turno del <i>{0}</i></b>",
                    textos);
                }
                break;

            case AVISO.Deduccion:
                _mensaje.imagen.color = new Color(1f, 1f, 0f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                   "Has hecho una deducción:\n" +
                   "<b>{1}</b> mató al Señor Blanco usando <b>{2}</b> en <b>{3}</b>",
                   textos);
                } else {
                    _mensaje.texto.text = string.Format(
                   "<i>{0}</i> ha hecho una deducción:\n" +
                   "<b>{1}</b> mató al Señor Blanco usando <b>{2}</b> en <b>{3}</b>",
                   textos);
                }
                break;

            case AVISO.Buscar:
                _mensaje.imagen.color = new Color(1f, 1f, 0f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "Empiezas a buscar.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<i>{0}</i> empieza a buscar.",
                    textos);
                }
                break;

            case AVISO.Moverse:
                _mensaje.imagen.color = new Color(1f, 1f, 0.9f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "Te has movido a <b>{1}</b>.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<i>{0}</i> se ha movido a <b>{1}</b>.",
                    textos);
                }
                break;

            case AVISO.Encontrado:
                _mensaje.imagen.color = new Color(0.5f, 1f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "Has encontrado en <b>{1}</b> a <b>{2}</b>.\n" +
                    "{2} sabe que el crimen no <b>{3}</b>",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<i>{0}</i> ha encontrado en <b>{1}</b> a <b>{2}</b>.",
                    textos);
                }
                break;

            case AVISO.EncontrarInspector:
                _mensaje.imagen.color = new Color(0.5f, 1f, 1f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "Has encontrado en <b>{1}</b> a <b>{2}</b>.\n" +
                    "Ahora puedes hacer la acusación final.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<i>{0}</i> ha encontrado en <b>{1}</b> a <b>{2}</b>.",
                    textos);
                }
                break;

            case AVISO.NoEncontrado:
                _mensaje.imagen.color = new Color(1f, 1f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "No has encontrado a nadie en <b>{1}</b>.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<i>{0}</i> no ha encontrado a nadie en <b>{1}</b>.",
                    textos);
                }
                break;

            case AVISO.Erroneo:
                _mensaje.imagen.color = new Color(0.5f, 1f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "<b>Acusación erronea</b>:\n" +
                    "<b>{0}</b> te confirma que el crimen no {1}.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<b>Acusación erronea</b>:\n" +
                    "<b>{0}</b> le confirma que su acusación es erronea.",
                    textos);
                }

                break;

            case AVISO.NoErroneo:
                _mensaje.imagen.color = new Color(1f, 0.5f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "<b>Nadie te niega que la acusación sea falsa</b>.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<b>Nadie niega que la acusación sea falsa</b>.",
                    textos);
                }

                break;

            case AVISO.Inspector:
                _mensaje.imagen.color = new Color(0.5f, 1f, 1f, 1f);
                _mensaje.texto.text = string.Format(
                    "El <b>Inspector</b> llegó a la mansión.\n" +
                    "Encuentrale en unas de las habitaciones cuando quieras <b>resolver el caso</b>.",
                    textos);
                break;

            case AVISO.Acusacion:
                _mensaje.imagen.color = new Color(0f, 1f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "Vas a resolver el caso:\n" +
                    "<b>{1}</b> mató al Señor Blanco usando <b>{2}</b> en <b>{3}</b>",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<i>{0}</i> va a resolver el caso:\n" +
                    "<b>{1}</b> mató al Señor Blanco usando <b>{2}</b> en <b>{3}</b>",
                    textos);
                }
                break;

            case AVISO.AcusCorrecta:
                _mensaje.imagen.color = new Color(0f, 1f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "¡Has resuelto correctamente el caso!",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "¡<i>{0}</i> ha resuelto correctamente el caso!",
                    textos);
                }
                break;

            case AVISO.AcusErronea:
                _mensaje.imagen.color = new Color(0f, 1f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "Tu acusación es incorrecta.\n" +
                    "Fué <b>{1}</b> quien mató al Señor Blanco usando <b>{2}</b> en <b>{3}</b>.\n" + 
                    "Durante el resto de la partida estas fuera y no puedes seguir jugando.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "La acusación de <i>{0}</i> es incorrecta.\n" +
                    "Durante el resto de la partida está fuera y no puede seguir jugando.",
                    textos);
                }
                break;

            case AVISO.MoverSala:
                _mensaje.imagen.color = new Color(1f, 0.5f, 1f, 1f);
                _mensaje.texto.text = string.Format(
                    "<b>{0}</b> abandonó <b>{1}</b>",
                    textos);
                break;

            case AVISO.Mensaje:
                _mensaje.imagen.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                _mensaje.texto.text = string.Format(
                    "<b>{0}:</b> <i>{1}</i>",
                    textos);
                break;

            case AVISO.Susurro:
                _mensaje.imagen.color = new Color(1f, 1f, 0.5f, 1f);
                if(eresTu) {
                    _mensaje.texto.text = string.Format(
                    "<b>{0}</b> te susurra: <i>{2}</i>",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<b>{0}</b> ha susurrado algo a <b>{1}</b>.",
                    textos);
                }
                break;

            case AVISO.Desconectado:
                _mensaje.imagen.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                if (eresTu) {
                    _mensaje.texto.text = string.Format(
                    "Te has desconectado de la partida.",
                    textos);
                } else {
                    _mensaje.texto.text = string.Format(
                    "<b>{0}</b> se ha desconectado de la partida. Un bot tomará su puesto.",
                    textos);
                }
                break;

            default:
                Debug.Log("Texto sin color ni formato.");
                break;
        }

        mensajesEnviados.Add(_mensaje);
        _mensaje.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        _mensaje.imagen.GetComponent<LayoutElement>().minHeight = _mensaje.texto.preferredHeight + 12;
        panelInferior.anchoredPosition = new Vector2(panelInferior.anchoredPosition.x, 0);
    }

    public void EnviarMensaje (string persona, string mensaje) {
        if(string.IsNullOrEmpty(mensaje))
            return;
        
        EnviarMensaje(AVISO.Mensaje, true, persona, mensaje);
    }
    
    /// <summary>
    /// Debido que en el chat se tiene que hacer uso del determinante para que quede bien construida las frases, tiene que mirar si la palabra está en femenino o masculino.
    /// </summary>
    public string DevolverGenero(GENERO genero, string texto) {
        switch(genero) {
            case GENERO.Masculino:
                return "el " + texto;
            case GENERO.Femenino:
                return "la " + texto;
        }

        return texto;
    }

    public string DevolverGenero(Tarjeta tarjeta) {
        return DevolverGenero (tarjeta.genero, tarjeta.nombreTarjeta);
    }

    /// <summary>
    /// De la misma manera que el género, la frase varía según si se refiere a un arma, un sospechoso o un lugar.
    /// </summary>
    public string DevolverTipoDelito (TIPO tipo, string texto) {
        switch (tipo) {
            case TIPO.Sospechoso:
                return "lo cometió " + texto;
            case TIPO.Lugar:
                return "se cometió en " + texto;
            case TIPO.Arma:
                return "se cometió con " + texto;
        }

        return texto;
    }
}
