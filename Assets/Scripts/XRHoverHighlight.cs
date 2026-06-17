using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class XRHoverHighlight : MonoBehaviour
{
    [Header("Surbrillance")]
    [Tooltip("Paramètres partagés — modifier cet asset change la surbrillance sur tous les objets")]
    public HighlightSettings parametres;

    private Material[] materiaux;

    void Awake()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var listeMat = new List<Material>();
        foreach (var r in renderers)
        {
            var mats = r.materials;
            foreach (var m in mats)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Color.black);
            }
            r.materials = mats;
            listeMat.AddRange(mats);
        }
        materiaux = listeMat.ToArray();

        var interactable = GetComponent<XRSimpleInteractable>();
        interactable.hoverEntered.AddListener(_ => Appliquer(true));
        interactable.hoverExited.AddListener(_ => Appliquer(false));
    }

    void Appliquer(bool activer)
    {
        Color emission = (activer && parametres != null)
            ? parametres.couleur * parametres.intensite
            : Color.black;
        foreach (var m in materiaux)
            m.SetColor("_EmissionColor", emission);
    }
}
