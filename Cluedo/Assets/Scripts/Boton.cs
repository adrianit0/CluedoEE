using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Un script que intenta emular el botón de la UI pero para botones 
/// </summary>
public class Boton : MonoBehaviour {

    bool _interactable = true;

    public bool interactable  {
        set {
            if(render == null && cambiarMaterial) {
                GetComponent<MeshRenderer>();
                if (render== null) {
                    Debug.Log("No tiene render");
                    return;
                }
            }
                
            _interactable = value;
            if (cambiarMaterial)
                render.sprite = (value) ? imagenNormal : imagenDesactivada;
            if(cambiarColor)
                render.color = (value) ? colorNormal : colorDesactivado;
        }
        get { return _interactable; }
    }

    public SpriteRenderer render;

    [Space (10)]
    public bool cambiarMaterial = true;
    public Sprite imagenNormal;
    public Sprite imagenEncendido;
    public Sprite imagenPresionado;
    public Sprite imagenDesactivada;

    [Space(10)]
    public bool cambiarColor = true;
    public Color colorNormal = new Color(1f, 1f, 1f, 1f);
    public Color colorEncendido = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color colorPresionado = new Color(0.75f, 0.75f, 0.75f, 1f);
    public Color colorDesactivado = new Color (0.75f, 0.75f, 0.75f, 0.5f);

    [Space(10)]
    public bool cambiarTamaño = true;
    [Range (1, 2)]
    public float porcAumento = 1.25f;
    Vector3 posInicial, posPulsado;
    int iteraccion;

    //Textos interiores
    public bool cambiarTextosInteriores;
    public TextMesh[] textos;
    
    [Space(10)]
    public UnityEvent pulsarBoton;
    public UnityEvent onPointerEnter;
    public UnityEvent onPointerExit;

    bool presionando = false;

    void Awake () {
        if(onPointerEnter == null) onPointerEnter = new UnityEvent();
        if(onPointerExit == null) onPointerExit = new UnityEvent();

        if(render == null)
            render = GetComponent<SpriteRenderer>();

        if (cambiarTextosInteriores && (textos == null || textos.Length==0)) {
            GetText();
        }
    }
    
    void Start () {
        if(render != null) {
            if (cambiarMaterial)
                render.sprite = (interactable) ? imagenNormal : imagenDesactivada;

            if(cambiarColor) {
                CambiarColoresText((interactable) ? colorNormal : colorDesactivado);
            }
                
        }
            

        posInicial = render.transform.localScale;
        posPulsado = posInicial * porcAumento;
    }

    void OnMouseDown() {
        if(interactable) {
            if(render != null) {
                if (cambiarMaterial)
                    render.sprite = imagenPresionado;

                if(cambiarColor)
                    CambiarColoresText(colorPresionado);
            }
                
            presionando = true;
        }
    }

    void OnMouseEnter() {
        if(interactable) {
            if(render != null) {
                if (cambiarMaterial)
                    render.sprite = (presionando) ? imagenPresionado : imagenEncendido;

                if(cambiarColor)
                    CambiarColoresText((presionando) ? colorPresionado : colorEncendido);
            }
                
            onPointerEnter.Invoke();

            if (cambiarTamaño)
                StartCoroutine(CambiarTamaño(false));
        }
    }

    void OnMouseExit() {
        if(interactable) {
            if(render != null) {
                if (cambiarMaterial)
                    render.sprite = imagenNormal;

                if (cambiarColor)
                    CambiarColoresText(colorNormal);
            }
                
            onPointerExit.Invoke();

            if (cambiarTamaño)
                StartCoroutine(CambiarTamaño(true));
        }
    }

    void OnMouseUp() {
        if(interactable) {
            presionando = false;
        }
    }

    void OnMouseUpAsButton() {
        if(interactable) {
            if(render != null) {
                if (cambiarMaterial)
                    render.sprite = imagenEncendido;

                if(cambiarColor)
                    CambiarColoresText(colorEncendido);
            }

            pulsarBoton.Invoke();
        }
    }

    public void GetText() {
        textos = render.GetComponentsInChildren<TextMesh>();
    }

    public void CambiarColoresText(Color _color) {
        if(render == null || !cambiarColor)
            return;

        render.color = _color;

        if(!cambiarTextosInteriores)
            return;

        foreach(TextMesh text in textos) {
            if(text == null)
                continue;
            text.color = _color;
        }
    }

    IEnumerator CambiarTamaño (bool inicial) {
        int ite = ++iteraccion;

        Vector3 actual = render.transform.localScale;
        Vector3 final = inicial ? posInicial : posPulsado;
        float lerp = 0;
        while (lerp < 1) {
            if (ite != iteraccion) {
                break;
            }
            lerp += Time.deltaTime*8;

            render.transform.localScale = Vector3.Lerp(actual, final, lerp);

            yield return null;
        }
    }
}
