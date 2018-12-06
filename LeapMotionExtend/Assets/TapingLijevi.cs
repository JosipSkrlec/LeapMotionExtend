using UnityEngine;

public class TapingLijevi : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        Taping.check1 = true;
        
    }
}
