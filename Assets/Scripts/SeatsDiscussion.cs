using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(AudioSource))]
public class SeatsDiscussion : MonoBehaviour
{
    [Header("Sons du couloir")]
    [Tooltip("Clips joués dans l'ordre à chaque passage")]
    public AudioClip[] clipsAudio;

    private AudioSource sourceAudio;
    private int indexCourant = 0;

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        sourceAudio = GetComponent<AudioSource>();
        sourceAudio.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (sourceAudio.isPlaying) return;
        if (indexCourant >= clipsAudio.Length) return;

        sourceAudio.clip = clipsAudio[indexCourant];
        sourceAudio.Play();
        indexCourant++;
    }
}
