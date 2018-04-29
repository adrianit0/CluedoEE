using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    //Prefabs
    public GameObject servidor;
    public GameObject cliente;

    //Panel de los botones
    [Space(10)]
    public GameObject panelBotones;
    public InputField inputNick;
    public Button botonOffline;
    public Button botonCrear;
    public Button botonUnirse;
    

    //Panel crear partida
    [Space(10)]
    public GameObject panelCrearPartida;
    public Button botonConectarCerrar;

    //Panel unirse partida
    [Space(10)]
    public GameObject panelUnirse;
    public InputField inputText;
    public Button botonUnirseConectar;
    public Button botonUnirseCerrar;

    DatosJuegos datos;

    void Awake() {
        datos = FindObjectOfType<DatosJuegos>();
        if (datos == null) {
            GameObject obj = new GameObject();
            datos = obj.AddComponent<DatosJuegos>();
            obj.name = "Datos del juego.";
        }
    }
    
	void Start () {
        //Panel botones
        botonCrear.onClick.AddListener(() => CrearPartida());
        botonUnirse.onClick.AddListener(() => BotonUnirse());
        botonOffline.onClick.AddListener(() => BotonOffline());

        //Panel host
        botonUnirseCerrar.onClick.AddListener(() => CerrarPanel(panelUnirse));
        botonUnirseConectar.onClick.AddListener(() => ConectarAlServidor());

        //Panel conectar
        botonConectarCerrar.onClick.AddListener(() => CerrarPanel(panelCrearPartida));

        //Paneles
        panelCrearPartida.SetActive(false);
        panelUnirse.SetActive(false);
        panelBotones.SetActive(true);
    }

    public void BotonOffline() {
        datos.nombre = inputNick.text;
        if(string.IsNullOrEmpty(datos.nombre))
            datos.nombre = "Citrino";

        datos.MAXIMO_JUGADORES = 4;

        datos.EmpezarPartida();
    }

    public void BotonUnirse() {
        panelUnirse.SetActive(true);
        panelBotones.SetActive(false);
    }

    public void CrearPartida() {
        if (string.IsNullOrEmpty (inputNick.text)) {
            Debug.Log("Necesitas un nombre!");
            return;
        }

        try {
            Servidor s = Instantiate(servidor).GetComponent<Servidor>();
            s.datos = datos;
            datos.server = s;
            s.Init();

            Cliente c = Instantiate(cliente).GetComponent<Cliente>();
            c.datosScript = datos;
            datos.client = c;
            c.esHost = true;
            
            c.nombre = inputNick.text;
            if(string.IsNullOrEmpty(c.nombre)) {
                c.nombre = "Cliente";
            }

            c.ConectarServidor("127.0.0.1", 6321);
        } catch (Exception error) {
            Debug.Log("Error servidor: " + error.Message);
        }

        panelCrearPartida.SetActive(true);
        panelBotones.SetActive(false);
    }

    void CerrarPanel (GameObject obj) {
        obj.SetActive(false);
        panelBotones.SetActive(true);

        Servidor s = FindObjectOfType<Servidor>();
        if(s != null)
            Destroy(s.gameObject);

        Cliente c = FindObjectOfType<Cliente>();
        if(c != null)
            Destroy(c.gameObject);
    }

    void ConectarAlServidor () {
        string direccionHost = inputText.text;

        if (string.IsNullOrEmpty (direccionHost)) {
            direccionHost = "127.0.0.1";
        }

        try {
            Cliente c = Instantiate(cliente).GetComponent<Cliente>();
            c.datosScript = datos;
            datos.client = c;
            c.ConectarServidor(direccionHost, 6321);

            c.nombre = inputNick.text;
            if (string.IsNullOrEmpty (c.nombre)) {
                c.nombre = "Cliente";
            }

            panelUnirse.SetActive(false);
        } catch (Exception error) {
            Debug.Log("Crear servidor: " + error.Message);
        }
    }
}
