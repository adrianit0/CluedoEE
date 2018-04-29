using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tarjeta {

    public string nombreTarjeta;
    public int ID;
    public TIPO tipo;
    public GENERO genero;

    public string descripcion;
    public Sprite spriteTarjeta;
    
    public Tarjeta (Tarjeta target) {
        nombreTarjeta = target.nombreTarjeta;
        ID = target.ID;
        tipo = target.tipo;
        genero = target.genero;

        descripcion = target.descripcion;
        spriteTarjeta = target.spriteTarjeta;
    }

    public Tarjeta () {
        nombreTarjeta = "";
        ID = 0;
        tipo = TIPO.Sospechoso;
        genero = GENERO.Femenino;

        descripcion = "";
        spriteTarjeta = null;
    }
}

public class ValorTarjeta {
    public string nombre = "tarjeta";

    public bool tachado = false;
    public bool visto = false;

    public TarjetaObject objetoCarpeta;
    public TarjetaObject objetoAcusacion;

    public void Tachar (bool value) {
        tachado = value;

        if (objetoCarpeta!=null && objetoCarpeta.equis!=null) {
            objetoCarpeta.equis.SetActive(value);
        }

        if(objetoAcusacion != null && objetoAcusacion.equis != null) {
            objetoAcusacion.equis.SetActive(value);
        }
    }

    public void Ver (bool value) {
        tachado = value;

        if(objetoCarpeta != null && objetoCarpeta.ojo != null) {
            objetoCarpeta.ojo.SetActive(value);
        }

        if(objetoAcusacion != null && objetoAcusacion.ojo != null) {
            objetoAcusacion.ojo.SetActive(value);
        }
    }
}