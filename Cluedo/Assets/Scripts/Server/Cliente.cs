using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cliente : MonoBehaviour {

    public string nombre;
    public int ID;

    public int seed;

    public bool esHost = false;

    public DatosJuegos datosScript;

    bool socketPreparado;

    public List<GameCliente> jugadores = new List<GameCliente>();

    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;

    float tiempoBusqueda = 0, busquedaMaxima = 2f;

    //Valores para recuperar conexión (Si la primera vez falla).
    string lastHost;
    int lastPort;

    void Start() {
        DontDestroyOnLoad(gameObject);
    }

    public bool ConectarServidor (string host, int puerto) {
        if(socketPreparado)
            return false;

        lastHost = host;
        lastPort = puerto;

        try {
            socket = new TcpClient(host, puerto);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketPreparado = true;
        } catch (Exception error) {
            Debug.Log("Error del socket: " + error.Message);
        }

        return socketPreparado;
    }
    void Update() {
        if (socketPreparado) {
            if (stream.DataAvailable) {
                string data = reader.ReadLine();
                if (data != null) {
                    OnIncomingData(data);   //Aquí le llega los datos que lee el cliente.
                }
            }
        } else {

            //Si a la primera no encuentra un servidor estará intentando conectarse a un servidor cada 2s.
            tiempoBusqueda += Time.deltaTime;

            if (tiempoBusqueda>busquedaMaxima) {
                tiempoBusqueda = 0;

                ConectarServidor(lastHost, lastPort);
            }
        }
    }

    //Enviando mensaje al servidor
    public void EnviarDatos (string data) {
        if(!socketPreparado)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    //Leer mensaje desde el servidor
    void OnIncomingData (string data) {
        Debug.Log("Cliente: "+data);

        string[] datos = data.Split('|');

        //En el juego, datos[1] siempre es la ID del jugador.
        int _idJugador = 0;
        if (datos.Length>1 && int.TryParse (datos[1], out _idJugador)) {
            //El TryParse ya está en el out el valor
        }

        switch(datos[0]) {
            case "SWHO":
                //Te acabas de conectar, pues recibes toda la información necesaria
                for(int i = 1; i < datos.Length - 1; i+=2) {
                    UsuarioConectado(datos[i], int.Parse(datos[(i+1)]), false);
                }

                EnviarDatos("CWHO|" + nombre + "|" + ((esHost) ? "1" : "0"));
                break;

            case "SCNN":
                int _ID = int.Parse(datos[2]);
                if(nombre == datos[1])
                    ID = _ID;

                UsuarioConectado(datos[1], _ID, false);
                seed = int.Parse(datos[3]);
                break;

            case "SDIS":
                Debug.Log(datos[1] + " se ha desconectado en la escena " + datos[3]);
                if (datosScript!=null && datosScript.manager!= null) {
                    datosScript.manager.GetComponent<InteligenciaArtificial>().IncluirBot(int.Parse(datos[2]));
                    datosScript.manager.GetComponent<ChatManager>().EnviarMensaje(AVISO.Desconectado, ID == int.Parse (datos[2]), datos[1]);
                }
                break;

            case "SMSG":
                this.datosScript.manager.GetComponent<ChatManager>().EnviarMensaje(datos[1], datos[2]);
                break;

            case "SMOV":
                int _idLugar = int.Parse(datos[2]);

                if (datosScript.manager.turnoDe == _idJugador)
                    datosScript.manager.SeleccionarLugar(_idLugar);

                break;

            case "SBUS":
                if(datosScript.manager.turnoDe == _idJugador)
                    datosScript.manager.Buscar();

                break;

            case "SDED":
                if(datosScript.manager.turnoDe == _idJugador)
                    datosScript.manager.IniciarDeduccion (int.Parse (datos[2]), int.Parse(datos[3]), int.Parse(datos[4]));

                break;

            case "SACU":
                if(datosScript.manager.turnoDe == _idJugador)
                    datosScript.manager.IniciarAcusacion (int.Parse(datos[2]), int.Parse(datos[3]), int.Parse(datos[4]));

                break;

            default:
                Debug.LogWarning("ORDEN " + datos[0] + " no reconocida");
                break;
        }
    }

    void UsuarioConectado(string nombre, int ID, bool host) {
        GameCliente c = new GameCliente();
        c.nombre = nombre;
        c.ID = ID;
        c.isHost = host;

        jugadores.Add(c);

        datosScript.MAXIMO_JUGADORES = jugadores.Count;

        if(jugadores.Count == 2)
            datosScript.EmpezarPartida();
    }

    void OnApplicationQuit() {
        CerrarSocket();
    }
    void OnDisable() {
        CerrarSocket();
    }
    void CerrarSocket () {
        if(!socketPreparado)
            return;

        EnviarDatos("CDIS|" + nombre + "|" + ID.ToString() + "|" + SceneManager.GetActiveScene().name);

        writer.Close();
        reader.Close();
        socket.Close();

        socketPreparado = false;
    }
}

[System.Serializable]
public class GameCliente {
    public string nombre;
    public int ID;
    public bool isHost;
}