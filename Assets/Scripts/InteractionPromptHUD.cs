using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Positionne et affiche un panneau ("Presser pour interagir") devant l'objet survolé, parmi tous les interactables de la scène
public class InteractionPromptHUD : MonoBehaviour
{
    [Tooltip("Panneau (Image touche + Texte) à positionner et afficher au survol — doit avoir CanvasFaceCamera pour rester face au joueur")]
    public GameObject panneau;
    [Tooltip("Décalage du panneau par rapport à la position de l'objet survolé")]
    public Vector3 decalage = new Vector3(0f, 0.3f, 0f);

    private int survolsActifs;

    void Start()
    {
        if (panneau != null)
            panneau.SetActive(false);

        foreach (var interactable in FindObjectsByType<XRSimpleInteractable>(FindObjectsSortMode.None))
        {
            interactable.hoverEntered.AddListener(_ => OnSurvolDebut(interactable.transform));
            interactable.hoverExited.AddListener(_ => OnSurvolFin());
        }
    }

    void OnSurvolDebut(Transform cible)
    {
        survolsActifs++;
        if (panneau == null) return;
        panneau.transform.position = cible.position + decalage;
        panneau.SetActive(true);
    }

    void OnSurvolFin()
    {
        survolsActifs = Mathf.Max(0, survolsActifs - 1);
        if (survolsActifs == 0 && panneau != null)
            panneau.SetActive(false);
    }
}
