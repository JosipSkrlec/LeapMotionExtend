using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOPTICA : MonoBehaviour {

    public GameObject LOPTA;
    public GameObject DESNI;
    public GameObject LIJEVI;

    int PLAVI = 0;
    int CRVENI = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // za pomak prema nama i od nas po Z koordinati
        if (LOPTA.transform.position.z > 0.025f || LOPTA.transform.position.z < 0.025f)
        {
            LOPTA.transform.position = new Vector3(LOPTA.transform.position.x,LOPTA.transform.position.y,0.025f);
        }


        // ZA Y gore i Y dolje
        if (LOPTA.transform.position.y < 0.12f)
        {
            LOPTA.transform.position = new Vector3(LOPTA.transform.position.x, 0.12f, LOPTA.transform.position.z);
        }
        if (LOPTA.transform.position.y > 0.4f)
        {
            LOPTA.transform.position = new Vector3(LOPTA.transform.position.x, 0.4f, LOPTA.transform.position.z);
        }

        // ZA SCORE
        if (LOPTA.transform.position.x < -0.57f)
        {
            LOPTA.transform.position = new Vector3(0.15f, LOPTA.transform.position.y, LOPTA.transform.position.z);
            PLAVI += 1;
            DESNI.gameObject.GetComponent<TextMesh>().text = PLAVI.ToString();

        }
        if (LOPTA.transform.position.x > 0.36f)
        {
            LOPTA.transform.position = new Vector3(0.15f, LOPTA.transform.position.y, LOPTA.transform.position.z);
            CRVENI += 1;
            LIJEVI.gameObject.GetComponent<TextMesh>().text = CRVENI.ToString();

        }



    }
}
