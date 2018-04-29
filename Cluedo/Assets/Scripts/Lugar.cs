using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lugar : MonoBehaviour {

    public string nombre;
    public int ID;
    public GENERO genero;

    public List<Personaje> personajes = new List<Personaje>();  //Cantidad de sospechosos que hay dentro de la habitación.

    public Boton boton;
    public BoxCollider coll;

    void Awake() {
        boton = GetComponent<Boton>();
        coll = GetComponent<BoxCollider>();
    } 

}