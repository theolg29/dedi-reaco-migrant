using UnityEngine;

// À poser sur le GameObject racine du XR Origin (dont le Scale doit rester à (1,1,1))
// Amplifie uniquement la distance parcourue physiquement (à l'horizontale), sans changer la taille perçue du joueur ni de la scène
public class RealWalkSpeedMultiplier : MonoBehaviour
{
    [Tooltip("Facteur d'amplification : 2 = un déplacement réel de 1m devient 2m dans le jeu")]
    public float multiplicateur = 2f;

    private Transform camera;
    private Vector3 dernierePositionLocale;

    void Start()
    {
        camera = Camera.main.transform;
        dernierePositionLocale = camera.localPosition;
    }

    void Update()
    {
        Vector3 positionActuelle = camera.localPosition;
        Vector3 delta = positionActuelle - dernierePositionLocale;
        delta.y = 0f; // n'amplifie que le déplacement horizontal, pas la hauteur de tête

        if (delta.sqrMagnitude > 0.0000001f)
        {
            Vector3 deltaMonde = transform.rotation * delta;
            transform.position += deltaMonde * (multiplicateur - 1f);
        }

        dernierePositionLocale = positionActuelle;
    }
}
