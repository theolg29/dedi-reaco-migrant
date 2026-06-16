using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(AudioSource))]
public class DoorInteractable : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Angle d'ouverture en degrés sur l'axe Y local")]
    public float angleOuverture = 90f;
    [Tooltip("Durée de l'animation d'ouverture/fermeture en secondes")]
    public float dureeAnimation = 0.8f;

    [Header("Audio")]
    [Tooltip("Son joué à l'ouverture de la porte")]
    public AudioClip sonOuverture;
    [Tooltip("Son joué à la fermeture de la porte")]
    public AudioClip sonFermeture;

    // Invoqué à chaque animation de porte : (vaOuvrir, dureeAnimation)
    public event System.Action<bool, float> Animee;

    public bool EstOuverte => porteOuverte;

    private XRSimpleInteractable interactable;
    private AudioSource sourceAudio;
    private InputDevice manetteDroite;
    private bool gachettePrecedente;
    private bool survolActif;
    private bool porteOuverte;
    private bool enAnimation;
    private int facteurSens = 1;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.hoverEntered.AddListener(_ => survolActif = true);
        interactable.hoverExited.AddListener(_ => survolActif = false);
        sourceAudio = GetComponent<AudioSource>();
    }

    public void Verrouiller()
    {
        interactable.enabled = false;
        survolActif = false;
    }

    public void Deverrouiller()
    {
        interactable.enabled = true;
    }

    public void Fermer()
    {
        if (porteOuverte && !enAnimation)
            StartCoroutine(AnimerPorte());
    }

    public void InverserSens()
    {
        facteurSens = -1;
    }

    void Update()
    {
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

        if (interactable.enabled && survolActif && !enAnimation && vientDEtreAppuye)
            StartCoroutine(AnimerPorte());
    }

    IEnumerator AnimerPorte()
    {
        enAnimation = true;

        Quaternion rotDepart = transform.localRotation;
        float sens = (porteOuverte ? -angleOuverture : angleOuverture) * facteurSens;
        Quaternion rotCible = Quaternion.Euler(transform.localEulerAngles + new Vector3(0f, sens, 0f));
        bool vaOuvrir = !porteOuverte;
        porteOuverte = !porteOuverte;

        AudioClip sonAJouer = vaOuvrir ? sonOuverture : sonFermeture;
        if (sonAJouer != null)
            sourceAudio.PlayOneShot(sonAJouer);

        if (vaOuvrir) DoorMuffledVolume.RetablirToutes(dureeAnimation);
        else DoorMuffledVolume.EtoufferToutes(dureeAnimation);

        Animee?.Invoke(vaOuvrir, dureeAnimation);

        float t = 0f;
        while (t < dureeAnimation)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Lerp(rotDepart, rotCible, Mathf.SmoothStep(0f, 1f, t / dureeAnimation));
            yield return null;
        }
        transform.localRotation = rotCible;
        enAnimation = false;
    }
}
