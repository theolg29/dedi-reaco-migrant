using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(BoxCollider), typeof(AudioSource))]
public class SeatsDiscussion : MonoBehaviour
{
    [System.Serializable]
    public struct Replique
    {
        [Tooltip("Optionnel — laisser vide si le son est déjà intégré dans une Timeline")]
        public AudioClip clip;
        [Tooltip("Timelines des avatars à lancer ensemble avec ce son (ex: les deux personnages qui discutent)")]
        public PlayableDirector[] timelines;
    }

    [Header("Sons du couloir")]
    [Tooltip("Répliques jouées dans l'ordre à chaque passage (son + animation de l'avatar)")]
    public Replique[] repliques;

    private AudioSource sourceAudio;
    private int indexCourant = 0;
    // Verrou posé immédiatement (synchrone), pour ne jamais dépendre d'un isPlaying/state
    // qui peut ne pas être encore à jour si le trigger est touché deux fois dans la même frame
    private bool discussionEnCours;

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        sourceAudio = GetComponent<AudioSource>();
        sourceAudio.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (indexCourant >= repliques.Length) return;
        if (discussionEnCours) return;

        discussionEnCours = true;
        Replique replique = repliques[indexCourant];
        indexCourant++;

        StartCoroutine(JouerReplique(replique));
    }

    IEnumerator JouerReplique(Replique replique)
    {
        if (replique.clip != null)
        {
            sourceAudio.clip = replique.clip;
            sourceAudio.Play();
        }

        if (replique.timelines != null)
        {
            foreach (var timeline in replique.timelines)
            {
                if (timeline != null)
                    timeline.Play();
            }
        }

        // Laisse les Play() s'appliquer avant de vérifier leur état
        yield return null;

        while (sourceAudio.isPlaying)
            yield return null;

        if (replique.timelines != null)
        {
            foreach (var timeline in replique.timelines)
            {
                while (timeline != null && timeline.state == PlayState.Playing)
                    yield return null;
            }
        }

        discussionEnCours = false;
    }
}
