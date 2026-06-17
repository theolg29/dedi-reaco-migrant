using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ButtonBIndicator : MonoBehaviour
{
    [Header("Surbrillance")]
    [Tooltip("Paramètres partagés avec XRHoverHighlight — même couleur jaune")]
    public HighlightSettings parametres;

    [Header("Vibration")]
    public float intensite = 0.3f;
    [Tooltip("Durée d'une impulsion en secondes")]
    public float dureeImpulsion = 0.08f;
    [Tooltip("Intervalle entre deux impulsions en secondes")]
    public float intervalle = 0.5f;

    private Material materiau;
    private InputDevice manetteDroite;
    private Coroutine feedback;
    private int survolsActifs;

    void Start()
    {
        var rendu = GetComponentInChildren<Renderer>();
        if (rendu != null)
        {
            materiau = rendu.material;
            materiau.EnableKeyword("_EMISSION");
            materiau.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            materiau.SetColor("_EmissionColor", Color.black);
        }

        foreach (var interactable in FindObjectsByType<XRSimpleInteractable>(FindObjectsSortMode.None))
        {
            interactable.hoverEntered.AddListener(_ => OnSurvolDebut());
            interactable.hoverExited.AddListener(_ => OnSurvolFin());
        }
    }

    void OnSurvolDebut()
    {
        survolsActifs++;
        if (survolsActifs == 1)
        {
            ObtenirManette();
            if (feedback == null)
                feedback = StartCoroutine(BoucleFeedback());
        }
    }

    void OnSurvolFin()
    {
        survolsActifs = Mathf.Max(0, survolsActifs - 1);
        if (survolsActifs == 0)
        {
            if (feedback != null)
            {
                StopCoroutine(feedback);
                feedback = null;
            }
            AppliquerEmission(false);
        }
    }

    void ObtenirManette()
    {
        if (manetteDroite.isValid) return;
        var dispositifs = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, dispositifs);
        if (dispositifs.Count > 0)
            manetteDroite = dispositifs[0];
    }

    void AppliquerEmission(bool activer)
    {
        if (materiau == null) return;
        Color emission = (activer && parametres != null)
            ? parametres.couleur * parametres.intensite
            : Color.black;
        materiau.SetColor("_EmissionColor", emission);
    }

    IEnumerator BoucleFeedback()
    {
        while (true)
        {
            if (manetteDroite.isValid)
                manetteDroite.SendHapticImpulse(0, intensite, dureeImpulsion);

            AppliquerEmission(false);
            yield return new WaitForSeconds(dureeImpulsion);
            AppliquerEmission(true);

            yield return new WaitForSeconds(intervalle);
        }
    }
}
