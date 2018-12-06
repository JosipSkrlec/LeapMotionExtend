using UnityEngine;

public class TapingDesni : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        Taping.check2 = true;
    }


}
