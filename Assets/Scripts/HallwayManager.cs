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
    [Tooltip("Distance totale que le couloir va parcourir (en unités Unity)")]
    public float deplacementTotal = 10f;
    [Tooltip("Durée (en secondes de marche) pour parcourir cette distance")]
    public float dureeDesespoir = 20f;

    private Vector3 posDepart;
    private Vector3 dernierePosJoueur;
    private float tempsMarche = 0f;
    private bool actif = false;

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void Start()
    {
        posDepart = couloirSortie.position;
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
            float offset = Mathf.Lerp(0f, deplacementTotal, t);

            couloirSortie.position = posDepart + axeCouloir switch
            {
                Axe.X => new Vector3(offset, 0f, 0f),
                Axe.Y => new Vector3(0f, offset, 0f),
                _     => new Vector3(0f, 0f, offset),
            };
        }

        dernierePosJoueur = joueur.position;
    }
}
