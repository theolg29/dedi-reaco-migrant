using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FixRigAmel : MonoBehaviour
{
    public Rig FixRig;

    private float targetWeight;
    
    void start()
    {
        Debug.Log("SIGNAL RECU !");
    }
    void Update()
    {
        FixRig.weight = Mathf.Lerp(
            FixRig.weight,
            targetWeight,
            Time.deltaTime * 10f
        );
    }

    public void IK_On()
    {
        targetWeight = 1f;
    }

    public void IK_Off()
    {
        targetWeight = 0f;
    }
}