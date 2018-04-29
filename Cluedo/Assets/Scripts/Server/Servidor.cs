using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Servidor : MonoBehaviour {

    public int puerto = 6321;
    public DatosJuegos datos;

    int internalSeed;

    List<ServidorCliente> clientes;
    List<ServidorCliente> desconectados;

    TcpListener server;
    bool serverEmpezado;

    public void Init() {
        DontDestroyOnLoad(gameObject);
        clientes = new List<ServidorCliente>();
        desconectados = new List<ServidorCliente>();

        internalSeed = (int) DateTime.Now.Ticks;

        try {
            server = new TcpListener(IPAddress.Any, puerto);
            server.Start();

            StartListening();
            serverEmpezado = true;
        } catch (Exception error) {
            Debug.Log("Error del socket: " + error.Message);
        }
    }

    void Update() {
        if(!serverEmpezado)
            return;

        foreach (ServidorCliente c in clientes) {
            //¿Está el cliente todavía conectado?
            if (!EstaConectado(c.tcp)) {
                c.tcp.Close();
                desconectados.Add(c);
                continue;
            } else {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable) {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null) {
                        OnIncomingData(c, data);
                    }
                }
            }
        }

        for (int i = 0; i < desconectados.Count; i++) {
            //Le dice al jugador que alguien se ha desconectado de la partida.
            //En caso del Cluedo, si un jugador se sale, para no joder la partida, su rol lo tomaría la IA o quedaría eliminado pasando su turno automaticamente.

            Debug.Log(SceneManager.GetActiveScene().name);
            switch (SceneManager.GetActiveScene ().name) {
                case "Lobby":
                    break;

                case "Scene":
                    break;
            }

            clientes.Remove(desconectados[i]);
            desconectados.RemoveAt(i);
        }
    } 

    void StartListening () {
        server.BeginAcceptTcpClient(AceptarClienteTcp, server);
    }

    void AceptarClienteTcp (IAsyncResult ar) {
        TcpListener listener = (TcpListener) ar.AsyncState;

        string allUser = "";
        foreach(ServidorCliente sCl in clientes) {
            allUser += sCl.nombreCliente + '|' + sCl.ID + '|';
        }

        ServidorCliente sc = new ServidorCliente(listener.EndAcceptTcpClient(ar));
        clientes.Add(sc);

        StartListening();

        //Debug.Log("¡Alguien se ha conectado!");
        Broadcast("SWHO|"+allUser, clientes[clientes.Count - 1]);
    }

    bool EstaConectado(TcpClient c) {
        try {
            if (c != null && c.Client != null && c.Client.Connected) {
                if(c.Client.Poll(0, SelectMode.SelectError))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                return true;
            } else {
                return false;
            }
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Envía desde el servidor a todos los clientes conectados.
    /// </summary>
    void Broadcast (string data, List<ServidorCliente> cl) {
        foreach (ServidorCliente sl in cl) {
            try {
                StreamWriter writer = new StreamWriter(sl.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            } catch (Exception error) {
                Debug.Log("Error de escritura: " + error.Message);
            }
        }
    }

    /// <summary>
    /// Envía los datos a un único cliente.
    /// Al ser una única difusión se llamaría unicast.
    /// </summary>
    void Broadcast(string data, ServidorCliente cl) {
        List<ServidorCliente> sc = new List<ServidorCliente>() { cl };
        Broadcast(data, sc);
    }

    // Lee desde el servidor.
    void OnIncomingData (ServidorCliente c, string data) {
        Debug.Log(c.nombreCliente + " Server: " + data);

        string[] datos = data.Split('|');

        switch(datos[0]) {
            //FUERA DEL JUEGO (Conectar cuentas).
            case "CWHO":
                c.nombreCliente = datos[1];
                c.esHost = datos[2] == "1" ? true : false;
                Broadcast("SCNN|" + c.nombreCliente + "|" + (clientes.Count-1).ToString() + "|" + internalSeed, clientes);
                break;

            case "CDIS":
                Broadcast("SDIS|" + datos[1] + "|" + datos[2] +  "|" + datos[3], clientes);
                break;

            case "CMSG":
                Broadcast("SMSG|" + datos[1] + "|" + datos[2], clientes);
                break;

            //DENTRO DEL JUEGO (Acciones del juego).
            case "CMOV":    //Cliente Movimiento
                Broadcast("SMOV|" + datos[1] + "|" + datos[2], clientes);
                break;

            case "CBUS":    //Cliente Búsqueda
                Broadcast("SBUS|" + datos[1], clientes);
                break;

            case "CDED":    //Cliente Deducción
                Broadcast("SDED|" + datos[1] + "|" + datos[2] + "|" + datos[3] + "|" + datos[4], clientes);
                break;

            case "CACU":    //Cliente Acusación
                Broadcast("SACU|" + datos[1] + "|" + datos[2] + "|" + datos[3] + "|" + datos[4], clientes);
                break;

            default:
                Debug.LogWarning("ORDEN " + datos[0] + " no reconocida");
                break;
        }
    }

    public int RandomValue (int min, int max) {
        return UnityEngine.Random.Range(min, max);
    }
}

public class ServidorCliente {
    public string nombreCliente;
    public int ID;
    public bool esHost;

    public TcpClient tcp;

    public ServidorCliente (TcpClient tcp) {
        this.tcp = tcp;
    }
}
