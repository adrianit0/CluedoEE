using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DatosJuegos : MonoBehaviour {

    public string nombre = "";
    public int MAXIMO_JUGADORES = 4;

    public GameManager manager; //Necesario para jugar

    public Servidor server;
    public Cliente client;

	void Start () {
        DontDestroyOnLoad(gameObject);
	}


    public void EmpezarPartida() {
        SceneManager.LoadScene("Scene");
    }
}
