using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trigger : MonoBehaviour {

    public GameObject loptica;
    public GameObject kockica;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "loptica")
        {
            Destroy(loptica);
        }
    }




}
