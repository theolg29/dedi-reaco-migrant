using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(AudioSource))]
public class DialogueTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct LigneSousTitre
    {
        [Tooltip("Durée d'affichage en secondes")]
        public float duree;
        [TextArea] public string texte;
    }

    [Header("Audio")]
    public AudioClip clip;

    [Header("Sous-titres")]
    public LigneSousTitre[] lignes;

    private AudioSource sourceAudio;
    private TextMeshProUGUI texteSousTitre;
    private bool declenche = false;

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        sourceAudio = GetComponent<AudioSource>();
        sourceAudio.playOnAwake = false;

        var objSousTitres = GameObject.FindWithTag("SousTitres");
        if (objSousTitres != null)
            texteSousTitre = objSousTitres.GetComponent<TextMeshProUGUI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || declenche) return;
        declenche = true;
        sourceAudio.clip = clip;
        sourceAudio.Play();
        StartCoroutine(AfficherSousTitres());
    }

    IEnumerator AfficherSousTitres()
    {
        if (texteSousTitre == null) yield break;

        foreach (var ligne in lignes)
        {
            texteSousTitre.text = ligne.texte;
            yield return new WaitForSeconds(ligne.duree);
        }

        texteSousTitre.text = "";
    }
}
