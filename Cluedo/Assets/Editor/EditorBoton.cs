using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Boton))]
[CanEditMultipleObjects()]
public class EditorBoton : Editor {

    MeshRenderer mesh;

    Boton boton;
    SerializedObject property;
    SerializedProperty myEvent;

    void CogerComponentes () {
        boton = (Boton)target;
        property = new SerializedObject((Boton)target);

        if(mesh != null)
            return;
        mesh = boton.GetComponent<MeshRenderer>();
    }

    void Awake () {
        CogerComponentes();
    }

    void OnEnable () {
        CogerComponentes();
    }

    public override void OnInspectorGUI() {
        property.Update();

        bool _int = EditorGUILayout.Toggle("Interactable: ", boton.interactable);
        if (_int != boton.interactable) {
            boton.interactable = _int;
            _int = boton.interactable;
        }
        if(!boton.interactable)
            EditorGUILayout.HelpBox("Si está desactivado no funcionará el botón", MessageType.Info);


        GUILayout.Space(5);
        
        Serializar("render");
        if(boton.render == null) {
            EditorGUILayout.HelpBox("Sin un SpriteRenderer no se podrá cambiar los sprites", MessageType.Warning);
            boton.cambiarMaterial = false;
        }            

        GUILayout.Space(10);

        if (boton.render != null) {
            Serializar("cambiarMaterial");
            if (boton.cambiarMaterial) {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Sprites", EditorStyles.boldLabel);
                GUILayout.Space(5);
                Serializar("imagenNormal", "imagenEncendido", "imagenPresionado", "imagenDesactivada");
                EditorGUILayout.EndVertical();
            }

            Serializar("cambiarColor");
            if(boton.cambiarColor) {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Colores", EditorStyles.boldLabel);
                GUILayout.Space(5);
                Serializar("colorNormal", "colorEncendido", "colorPresionado", "colorDesactivado");
                EditorGUILayout.EndVertical();
            }
        } else {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Error", EditorStyles.boldLabel);
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Sin mesh no se puede cambiar el sprite y color según su estado", MessageType.Info);
            EditorGUILayout.EndVertical();
        }
        

        GUILayout.Space(10);

        Serializar("cambiarTamaño");
        if (boton.cambiarColor)
            Serializar("porcAumento");

        GUILayout.Space(10);

        Serializar("cambiarTextosInteriores");
        if (boton.cambiarTextosInteriores) {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Textos", EditorStyles.boldLabel);
            if (GUILayout.Button ("Actualizar")) {
                boton.GetText();
            }

            GUILayout.Space(10);

            for (int i = 0; i < boton.textos.Length; i++) {
                Serializar("textos.Array.data["+i+"]");
            }
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(10);

        Serializar("pulsarBoton", "onPointerEnter", "onPointerExit");

        property.ApplyModifiedProperties();
    }

    void Serializar (string nombreVariable) {
        myEvent = property.FindProperty(nombreVariable);
        if (myEvent == null) {
            EditorGUILayout.HelpBox(string.Format("La variable {0} no existe", nombreVariable), MessageType.Warning);
            return;
        }
        EditorGUILayout.PropertyField(myEvent);
    }

    void SerializarArray (string array) {
        myEvent = property.FindProperty(array);
        if(myEvent == null) {
            EditorGUILayout.HelpBox(string.Format("El array {0} no existe", array), MessageType.Warning);
            return;
        }

        int size = property.FindProperty(array + ".Array.size").intValue;

        int newSize = EditorGUILayout.IntField(array + " Size", size);

        if(newSize != size)
            property.FindProperty(array + ".Array.size").intValue = newSize;

        EditorGUI.indentLevel = 3;

        for(int i = 0; i < newSize; i++) {
            SerializedProperty prop = property.FindProperty(string.Format("{0}.Array.data[{1}]", array, i));
            EditorGUILayout.PropertyField(prop);
        }

        EditorGUILayout.PropertyField(myEvent);
    }

    void Serializar (params string[] nombreVariables) {
        for (int i = 0; i < nombreVariables.Length; i++) {
            Serializar(nombreVariables[i]);
        }
    }
}
