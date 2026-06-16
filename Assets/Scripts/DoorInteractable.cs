using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class DoorInteractable : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Angle d'ouverture en degrés sur l'axe Y local")]
    public float angleOuverture = 90f;
    [Tooltip("Durée de l'animation d'ouverture/fermeture en secondes")]
    public float dureeAnimation = 0.8f;

    // Invoqué à chaque animation de porte : (vaOuvrir, dureeAnimation)
    public event System.Action<bool, float> Animee;

    public bool EstOuverte => porteOuverte;

    private XRSimpleInteractable interactable;
    private InputDevice manetteDroite;
    private bool boutonBPrecedent;
    private bool survolActif;
    private bool porteOuverte;
    private bool enAnimation;
    private int facteurSens = 1;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.hoverEntered.AddListener(_ => survolActif = true);
        interactable.hoverExited.AddListener(_ => survolActif = false);
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

        bool boutonBActuel = false;
        if (manetteDroite.isValid)
            manetteDroite.TryGetFeatureValue(CommonUsages.secondaryButton, out boutonBActuel);

        bool vientDEtreAppuye = boutonBActuel && !boutonBPrecedent;
        boutonBPrecedent = boutonBActuel;

        if (survolActif && !enAnimation && vientDEtreAppuye)
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

        if (AmbianceCouloir.Instance != null)
        {
            if (vaOuvrir) AmbianceCouloir.Instance.Retablir(dureeAnimation);
            else AmbianceCouloir.Instance.Etouffer(dureeAnimation);
        }

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
