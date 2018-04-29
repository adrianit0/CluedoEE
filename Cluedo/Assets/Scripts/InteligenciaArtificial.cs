using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteligenciaArtificial : MonoBehaviour {

    int[] jugadores = new int[3] { 1, 2, 3 };    //Jugadores que controla la IA

    GameManager inGame;

    int turnoActual = -1;

    void Awake() {
        inGame = GetComponent<GameManager>();
    }

    /// <summary>
    /// En lugar de ponerlo en un Start, se inicializará en el Start del GameManager, para esperar a que cargue las ID de los otros jugadores.
    /// </summary>
    public void Inicializar() {
        int count = 0;
        for (int i = 0; i < inGame.jugadores.Length; i++) {
            if(inGame.jugadores[i].esBot)
                count++;
        }

        jugadores = new int[count];
        int x = 0;
        for(int i = 0; i < inGame.jugadores.Length; i++) {
            if(inGame.jugadores[i].esBot) {
                jugadores[x] = i;
                x++;
            }
        }

        StartCoroutine(ControlIA());
    } 

    public void IncluirBot (int personaje) {
        if (Array.Exists<int> (jugadores, p => personaje == p)) {
            Debug.Log("Este personaje actualmente ya es un bot");
            return;
        }

        Array.Resize<int>(ref jugadores, jugadores.Length + 1);
        jugadores[jugadores.Length - 1] = personaje;
        inGame.jugadores[personaje].esBot = true;
        inGame.jugadores[personaje].botText.gameObject.SetActive(true);

        Debug.Log("Personaje " + personaje + " convertido en un bot correctamente");
    }

    IEnumerator ControlIA () {
        yield return new WaitForSeconds(1f);
        bool turnoEncontrado = false;
        while (!turnoEncontrado) {
            for (int i = 0; i < jugadores.Length; i++) {
                if(inGame.turnoDe == jugadores[i]) {
                    turnoEncontrado = true;
                    turnoActual = jugadores[i];
                    break;
                }
            }

            yield return new WaitForSeconds(1f);
        }

        while (inGame.puedeMoverse) {
            inGame.SeleccionarLugar(UnityEngine.Random.Range(0, inGame.lugares.Length));

            yield return new WaitForSeconds(1f);
        }

        while (turnoActual==inGame.turnoDe) {
            if (UnityEngine.Random.Range (0, 2)==0) {
                inGame.Buscar();
            } else {
                inGame.IniciarDeduccion(UnityEngine.Random.Range(0, 8), inGame.jugadores[turnoActual].lugarActual + 8, UnityEngine.Random.Range(0, 8) + 16);
            }

            yield return new WaitForSeconds(1f);
        }

        turnoActual = -1;
        StartCoroutine(ControlIA());
    }
}
