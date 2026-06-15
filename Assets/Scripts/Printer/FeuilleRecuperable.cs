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

    private XRSimpleInteractable interactable;
    private InputDevice manetteDroite;
    private bool boutonBPrecedent;
    private bool survolActif;
    private bool recuperee;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.enabled = false;

        interactable.hoverEntered.AddListener(_ => survolActif = true);
        interactable.hoverExited.AddListener(_ => survolActif = false);
    }

    public void ActiverRecuperation()
    {
        interactable.enabled = true;
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

        bool boutonBActuel = false;
        if (manetteDroite.isValid)
            manetteDroite.TryGetFeatureValue(CommonUsages.secondaryButton, out boutonBActuel);

        bool vientDEtreAppuye = boutonBActuel && !boutonBPrecedent;
        boutonBPrecedent = boutonBActuel;

        if (survolActif && vientDEtreAppuye)
            Recuperer();
    }

    void Recuperer()
    {
        recuperee = true;
        interactable.enabled = false;
        gameObject.SetActive(false);

        if (bureauManager != null)
            bureauManager.SignalerMissionTerminee();
    }
}
