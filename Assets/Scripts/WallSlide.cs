using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WallSlide : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("XR Origin du joueur")]
    public Transform joueur;
    [Tooltip("Le couloir de sortie (vert) à déplacer")]
    public Transform couloirSortie;
    [Tooltip("Optionnel — effet de panique à passer en intensité max quand le couloir s'allonge")]
    public HallwayPanicEffect effetPanique;

    public enum Axe { X, Y, Z }

    [Header("Axe du couloir")]
    [Tooltip("Axe sur lequel le couloir se déplace (X, Y ou Z)")]
    public Axe axeCouloir = Axe.X;

    [Header("Effet de désespoir")]
    [Tooltip("Distance totale que le couloir peut parcourir (en unités Unity)")]
    public float deplacementTotal = 10f;
    [Tooltip("Multiplicateur de vitesse : le couloir avance plus vite que le joueur si > 1")]
    [SerializeField] private float vitesseDeplacement = 1f;

    private Vector3 posDepart;
    private float posJoueurDepart;
    private float offsetMax = 0f;
    private bool actif = false;

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void Start()
    {
        posDepart = couloirSortie.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!ConditionsValidees()) return;
        if (actif) return;

        actif = true;
        posJoueurDepart = PositionSurAxe(joueur.position);
        offsetMax = 0f;

        if (effetPanique != null)
            effetPanique.DemarrerIntensiteMax();
    }

    bool ConditionsValidees()
    {
        // TODO : vérifier que l'étape 1 (bureau 1) et l'étape 2 (bureau 2) sont validées
        return true;
    }

    float PositionSurAxe(Vector3 position)
    {
        return axeCouloir == Axe.X ? position.x
             : axeCouloir == Axe.Y ? position.y
             : position.z;
    }

    void Update()
    {
        if (!actif) return;

        float deltaDepuisDebut = (PositionSurAxe(joueur.position) - posJoueurDepart) * vitesseDeplacement;
        float offsetSouhaite = Mathf.Clamp(deltaDepuisDebut, 0f, deplacementTotal);

        // Le couloir ne recule jamais, même si le joueur recule.
        offsetMax = Mathf.Max(offsetMax, offsetSouhaite);

        couloirSortie.position = posDepart + axeCouloir switch
        {
            Axe.X => new Vector3(offsetMax, 0f, 0f),
            Axe.Y => new Vector3(0f, offsetMax, 0f),
            _     => new Vector3(0f, 0f, offsetMax),
        };
    }
}
