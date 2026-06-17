using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FixRigAmel : MonoBehaviour
{
public Rig FixRig;

public void IK_On()
{
    FixRig.weight = 1f;
    Debug.Log("IK ON");
    }

public void IK_Off()
{
    FixRig.weight = 0f;
        Debug.Log("IK OFF");
    }
    
}