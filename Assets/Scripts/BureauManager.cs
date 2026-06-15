using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BureauManager : MonoBehaviour
{
    [Header("Avatar")]
    [Tooltip("L'avatar (ou cube) sur lequel appuyer B pour démarrer la salle")]
    public XRSimpleInteractable avatar;

    [Header("Imprimante")]
    [Tooltip("Optionnel — se lance automatiquement en fin de dialogue (Bureau 02 uniquement)")]
    public PrinterAnimation imprimante;

    [Header("Porte")]
    [Tooltip("La porte de cette salle")]
    public DoorInteractable porte;
    [Tooltip("La porte de la salle suivante à déverrouiller une fois cette salle validée")]
    public DoorInteractable porteSuivante;

    [Header("Dialogues")]
    [Tooltip("Canvas WorldSpace positionné statiquement devant le joueur")]
    public GameObject panneauDialogue;
    [Tooltip("Durée du fade in/out en secondes")]
    public float dureeTransition = 0.4f;
    [Tooltip("Texte du panneau de dialogue")]
    public TextMeshProUGUI texteDialogue;
    [Tooltip("Son global joué au début de la séquence de dialogues")]
    public AudioClip sonDialogue;
    [Tooltip("AudioSource positionnelle sur l'avatar — si vide, utilise celle du GameObject")]
    public AudioSource sourceAudioDialogue;

    [System.Serializable]
    public struct LigneDialogue
    {
        [TextArea] public string texte;
        [Tooltip("Durée d'affichage en secondes")]
        public float duree;
    }

    [Tooltip("Séquence de textes à afficher pendant le son")]
    public LigneDialogue[] dialogues;

    private CanvasGroup groupeDialogue;
    private InputDevice manetteDroite;
    private bool boutonBPrecedent;
    private bool survolAvatarActif;
    private bool salleDemarree;
    private bool salleValidee;

    void Awake()
    {
        if (sourceAudioDialogue == null)
        {
            if (!TryGetComponent(out sourceAudioDialogue))
                sourceAudioDialogue = gameObject.AddComponent<AudioSource>();
        }

        if (panneauDialogue != null)
        {
            groupeDialogue = panneauDialogue.GetComponent<CanvasGroup>();
            if (groupeDialogue == null)
                groupeDialogue = panneauDialogue.AddComponent<CanvasGroup>();
            groupeDialogue.alpha = 0f;
        }

        if (porteSuivante != null)
            porteSuivante.Verrouiller();

        if (avatar != null)
        {
            avatar.hoverEntered.AddListener(_ => survolAvatarActif = true);
            avatar.hoverExited.AddListener(_ => survolAvatarActif = false);
        }
    }

    void Update()
    {
        if (salleDemarree) return;

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

        if (survolAvatarActif && vientDEtreAppuye)
            DemarrerSalle();
    }

    void DemarrerSalle()
    {
        salleDemarree = true;

        if (avatar != null)
            avatar.enabled = false;

        if (porte != null)
        {
            porte.Fermer();
            porte.Verrouiller();
        }

        StartCoroutine(JouerDialogues());
    }

    public void SignalerMissionTerminee()
    {
        ValiderSalle();
    }

    [ContextMenu("TEST — Valider la salle")]
    void DebugValiderSalle()
    {
        ValiderSalle();
    }

    void ValiderSalle()
    {
        if (salleValidee) return;
        salleValidee = true;

        if (porte != null)
        {
            porte.InverserSens();
            porte.Deverrouiller();
        }

        if (porteSuivante != null)
            porteSuivante.Deverrouiller();
    }

    IEnumerator FadeDialogue(float cible)
    {
        if (groupeDialogue == null) yield break;
        float depart = groupeDialogue.alpha;
        float t = 0f;
        while (t < dureeTransition)
        {
            t += Time.deltaTime;
            groupeDialogue.alpha = Mathf.Lerp(depart, cible, t / dureeTransition);
            yield return null;
        }
        groupeDialogue.alpha = cible;
    }

    IEnumerator JouerDialogues()
    {
        yield return StartCoroutine(FadeDialogue(1f));

        if (sonDialogue != null)
        {
            sourceAudioDialogue.clip = sonDialogue;
            sourceAudioDialogue.Play();
        }

        if (dialogues != null && dialogues.Length > 0)
        {
            foreach (var ligne in dialogues)
            {
                if (texteDialogue != null)
                    texteDialogue.text = ligne.texte;

                yield return new WaitForSeconds(ligne.duree);
            }
        }
        else if (sourceAudioDialogue != null && sourceAudioDialogue.clip != null)
        {
            yield return new WaitForSeconds(sourceAudioDialogue.clip.length);
        }

        if (texteDialogue != null)
            texteDialogue.text = "";

        yield return StartCoroutine(FadeDialogue(0f));

        if (imprimante != null)
            imprimante.Imprimer();
    }
}
