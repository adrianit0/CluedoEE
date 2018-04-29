using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorTarjeta : EditorWindow {

    ListaTarjeta lista;

    Vector2 scrollPositionBarra;
    Vector2 scrollPositionContenido;
    
    int id = 0;

    //Valores
    float buttonWidth = 20;
    
    [MenuItem("Cluedo/Editor de tarjetas.", false, 140)]
    static void Init() {
        EditorTarjeta window = (EditorTarjeta) EditorWindow.GetWindow(typeof(EditorTarjeta));

        window.Show();
    }

    [MenuItem("Cluedo/Crear nueva lista de tarjetas.", false, 150)]
    public static void CreateAsset() {
        ListaTarjeta asset = ScriptableObject.CreateInstance<ListaTarjeta>();
        AssetDatabase.CreateAsset(asset, "Assets/NuevaListaCluedo.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    void OnInspectorUpdate() {
        Repaint();
    }
    
    void OnGUI() {

        PedirLista();
        if(lista == null)
            return;

        MostrarGUI();
    }

    void PedirLista() {
        if(lista != null) {
            return;
        } else {
            GUILayout.Label("Hace falta una lista con las tarjetas para hacerlo funcionar el editor.", EditorStyles.boldLabel/*, EditorStyles.largeLabel*/);
            GUILayout.Label("No se ha encontrado una lista, insertalo aquí:", EditorStyles.boldLabel);
            lista = (ListaTarjeta) EditorGUILayout.ObjectField("Lista de tarjetas: ", lista, typeof(ListaTarjeta), true, GUILayout.MinWidth(50), GUILayout.MaxWidth(300));
            GUILayout.Label("Si no tienes ninguno puedes crear uno aqui:", EditorStyles.boldLabel);
            if(GUILayout.Button("Crear nuevo", GUILayout.MinWidth(20), GUILayout.MaxWidth(100))) {
                CreateAsset();
            }
        }
    }

    void MostrarGUI() {
        if(lista.tarjetas == null)
            lista.tarjetas = new Tarjeta[0];

        if(id >= lista.tarjetas.Length)
            id = lista.tarjetas.Length;

        GUILayout.BeginHorizontal();

        SelectorUnidad();

        GUILayout.BeginVertical();

        EdicionUnidad();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void SelectorUnidad() {
        GUILayout.BeginVertical();
        GUILayout.BeginVertical("box", GUILayout.Width(300));
        GUILayout.Label("Tarjetas", EditorStyles.centeredGreyMiniLabel);
        scrollPositionBarra = GUILayout.BeginScrollView(scrollPositionBarra, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        int i = 0;
        foreach (Tarjeta tarjeta in lista.tarjetas) {
            

            string _nombre = "[" + tarjeta.tipo + "] "+ ((tarjeta.nombreTarjeta == "") ? "Tarjeta #" + (i+1) : tarjeta.nombreTarjeta + " (#" + (i+1) + ")");

            EditorGUILayout.BeginHorizontal();
            if(id == i) {
                GUILayout.Label(_nombre, GUILayout.Width(270 - (buttonWidth * 3)));
            } else {
                if(GUILayout.Button(_nombre, GUILayout.Width(270 - (buttonWidth * 3)))) {
                    GUIUtility.keyboardControl = 0;
                    id = i;
                }
            }
            if(lista.tarjetas.Length > 1) {
                if(GUILayout.Button("-", GUILayout.Width(buttonWidth))) {
                    if(EditorUtility.DisplayDialog("Confirmar", "¿Deseas eliminar la tarjeta " + tarjeta.nombreTarjeta + "?", "Sí", "No")) {
                        GUIUtility.keyboardControl = 0;
                        lista.tarjetas = BorrarValor<Tarjeta>(lista.tarjetas, i);
                        id = Mathf.Clamp(id - 1, 0, lista.tarjetas.Length - 1);
                    }
                }
            }

            if(i == lista.tarjetas.Length - 1) {
                GUILayout.Space(buttonWidth + 4);
            } else {
                if(GUILayout.Button("↓", GUILayout.Width(buttonWidth))) {
                    lista.tarjetas = CambiarValor<Tarjeta>(lista.tarjetas, i, i + 1);
                }
            }
            if(i == 0) {
                GUILayout.Space(buttonWidth);
            } else {
                if(GUILayout.Button("↑", GUILayout.Width(buttonWidth))) {
                    lista.tarjetas = CambiarValor<Tarjeta>(lista.tarjetas, i, i - 1);
                }
            }
            EditorGUILayout.EndHorizontal();

            i++;
        }
        GUILayout.Space(5);
        if(GUILayout.Button("Crear nueva tarjeta", GUILayout.Width(275)) || lista.tarjetas.Length == 0) {
            GUIUtility.keyboardControl = 0;
            lista.tarjetas = NuevoValor<Tarjeta>(lista.tarjetas);
            id = lista.tarjetas.Length - 1;
        }

        GUILayout.Space(10);

        if(GUILayout.Button("Guardar tarjetas", GUILayout.Width(275))) {
            GUIUtility.keyboardControl = 0;
            Guardar();
            Debug.Log("Datos guardados");
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        
        GUILayout.EndVertical();
    }

    void EdicionUnidad() {
        GUILayout.BeginVertical("box");
        scrollPositionContenido = GUILayout.BeginScrollView(scrollPositionContenido);
        Tarjeta tarjeta = lista.tarjetas[id];

        tarjeta.nombreTarjeta = EditorGUILayout.TextField("Nombre personaje", tarjeta.nombreTarjeta);
        tarjeta.tipo = (TIPO) EditorGUILayout.EnumPopup("Tipo: ", tarjeta.tipo);
        tarjeta.genero = (GENERO) EditorGUILayout.EnumPopup("Genero: ", tarjeta.genero);
        tarjeta.spriteTarjeta = (Sprite) EditorGUILayout.ObjectField("Sprite", tarjeta.spriteTarjeta, typeof (Sprite), false);

        tarjeta.descripcion = EditorGUILayout.TextArea(tarjeta.descripcion);
        

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    string Popup(string texto, string[] lista, params GUILayoutOption[] options) {
        for(int i = 0; i < lista.Length; i++) {
            if(lista[i] == texto) {
                int dev = EditorGUILayout.Popup(i, lista);
                return lista[dev];
            }
        }
        EditorGUILayout.Popup(0, lista);
        return lista[0];
    }

    /// <summary>
    /// Guarda todas las animaciones que hayan dentro de la carpeta "Animaciones".
    /// </summary>
    void Guardar() {
        if(lista == null)
            return;

        EditorUtility.SetDirty(lista);
    }

    T[] NuevoValor<T>(T[] array) where T : new() {
        T[] nuevoArray = new T[array.Length + 1];
        for(int i = 0; i < array.Length; i++) {
            nuevoArray[i] = array[i];
        }
        nuevoArray[array.Length] = new T();
        return nuevoArray;
    }

    T[] BorrarValor<T>(T[] array, int value) {
        if(value < 0 || value >= array.Length)
            return array;
        T[] nuevoArray = new T[array.Length - 1];

        int x = 0;
        for(int i = 0; i < nuevoArray.Length; i++) {
            if(x == value)
                x++;
            nuevoArray[i] = array[x];
            x++;
        }

        return nuevoArray;
    }

    T[] CambiarValor<T>(T[] array, int value1, int value2) {
        if(value1 < 0 || value1 >= array.Length || value2 < 0 || value2 >= array.Length)
            return array;

        T value = array[value1];
        array[value1] = array[value2];
        array[value2] = value;
        return array;
    }
}
