using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class FeuilleRecuperable : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Bureau manager à notifier quand la feuille est récupérée")]
    public BureauManager bureauManager;

    [Header("Clignotement")]
    [Tooltip("Paramètres partagés de surbrillance (couleur + intensité)")]
    public HighlightSettings parametres;
    [Tooltip("Intervalle entre deux clignotements, en secondes")]
    public float intervalleClignotement = 0.6f;

    private XRSimpleInteractable interactable;
    private InputDevice manetteDroite;
    private bool gachettePrecedente;
    private bool survolActif;
    private bool recuperee;

    private Material materiau;
    private Coroutine clignotement;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.enabled = false;

        interactable.hoverEntered.AddListener(_ => survolActif = true);
        interactable.hoverExited.AddListener(_ => survolActif = false);

        var rendu = GetComponentInChildren<Renderer>();
        if (rendu != null)
        {
            materiau = rendu.material;
            materiau.EnableKeyword("_EMISSION");
            materiau.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            materiau.SetColor("_EmissionColor", Color.black);
        }
    }

    public void ActiverRecuperation()
    {
        interactable.enabled = true;
        clignotement = StartCoroutine(Clignoter());
    }

    IEnumerator Clignoter()
    {
        bool allume = false;
        while (true)
        {
            allume = !allume;
            AppliquerEmission(allume);
            yield return new WaitForSeconds(intervalleClignotement);
        }
    }

    void AppliquerEmission(bool activer)
    {
        if (materiau == null) return;
        Color emission = (activer && parametres != null)
            ? parametres.couleur * parametres.intensite
            : Color.black;
        materiau.SetColor("_EmissionColor", emission);
    }

    void Update()
    {
        if (!interactable.enabled || recuperee) return;

        if (!manetteDroite.isValid)
        {
            var dispositifs = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, dispositifs);
            if (dispositifs.Count > 0)
                manetteDroite = dispositifs[0];
        }

        float valeurGachette = 0f;
        if (manetteDroite.isValid)
            manetteDroite.TryGetFeatureValue(CommonUsages.trigger, out valeurGachette);
        bool gachetteActuelle = valeurGachette > 0.5f;

        bool vientDEtreAppuye = gachetteActuelle && !gachettePrecedente;
        gachettePrecedente = gachetteActuelle;

        if (survolActif && vientDEtreAppuye)
            Recuperer();
    }

    void Recuperer()
    {
        recuperee = true;
        interactable.enabled = false;

        if (clignotement != null)
            StopCoroutine(clignotement);

        gameObject.SetActive(false);

        if (bureauManager != null)
            bureauManager.SignalerMissionTerminee();
    }
}
