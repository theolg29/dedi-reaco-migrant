using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HallwayManager : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("XR Origin du joueur")]
    public Transform joueur;
    [Tooltip("Le couloir de sortie (vert) à déplacer")]
    public Transform couloirSortie;

    public enum Axe { X, Y, Z }

    [Header("Axe du couloir")]
    [Tooltip("Axe sur lequel le couloir se déplace (X, Y ou Z)")]
    public Axe axeCouloir = Axe.X;

    [Header("Effet de désespoir")]
    [Range(0f, 1f)] public float ratioInitial = 0.85f;
    [Range(0f, 1f)] public float ratioFinal = 0f;
    public float dureeDesespoir = 20f;

    private Vector3 dernierePosJoueur;
    private float tempsMarche = 0f;
    private bool actif = false;

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void Start()
    {
        dernierePosJoueur = joueur.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!ConditionsValidees()) return;

        actif = true;
    }

    bool ConditionsValidees()
    {
        // TODO : vérifier que l'étape 1 (bureau 1) et l'étape 2 (bureau 2) sont validées
        return true;
    }

    void Update()
    {
        if (!actif) return;

        float posActuelle = axeCouloir == Axe.X ? joueur.position.x
                          : axeCouloir == Axe.Y ? joueur.position.y
                          : joueur.position.z;

        float posPrec = axeCouloir == Axe.X ? dernierePosJoueur.x
                      : axeCouloir == Axe.Y ? dernierePosJoueur.y
                      : dernierePosJoueur.z;

        float deltaAvant = posActuelle - posPrec;

        if (deltaAvant > 0f)
        {
            tempsMarche += Time.deltaTime;
            float t = Mathf.Clamp01(tempsMarche / dureeDesespoir);
            float ratio = Mathf.Lerp(ratioInitial, ratioFinal, t);

            Vector3 deplacement = axeCouloir == Axe.X ? new Vector3(deltaAvant * ratio, 0f, 0f)
                                : axeCouloir == Axe.Y ? new Vector3(0f, deltaAvant * ratio, 0f)
                                : new Vector3(0f, 0f, deltaAvant * ratio);

            couloirSortie.position += deplacement;
        }

        dernierePosJoueur = joueur.position;
    }
}
