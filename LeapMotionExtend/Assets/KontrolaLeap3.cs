﻿using UnityEngine;
using Leap;
using Leap.Unity;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine.UI;

// projekt je spremljen na github https://github.com/JosipSkrlec/LeapMotionExtend .
public class KontrolaLeap3 : MonoBehaviour
{
    // za punjenje memorije, pogledati unity profiler za gfx...
    // Gfx.waitForPresent is like the CPU is waiting till the rendering process is finished.

    float vrijeme = 0.0f;
    Leap.Vector PocetniVektor = new Leap.Vector();
    Leap.Vector NoviVektor1 = new Leap.Vector();
    Vector3 NoviVektor = new Vector3();
    bool jednom = false;
    bool sredinajednom = false;

    public GameObject objekt1;

    float deltaTime = 0.0f;
    GameObject GO;

    // ŠALJE IP = "10.0.1.27"; port = 60000;
    /*************************************************************************/
    private LeapServiceProvider leapProvider;
    UDPSend udpSend = new UDPSend();
    UDPReceive udpReceive = new UDPReceive();

    // sljedece public varijable sluze za prikaz teksta tj. konekcije na sucelje tj canvas
    public Text tekst;
    public bool DesniLeap = true;
    public bool slanjePodataka = false;
    public bool prvaCrta = true;
    public GameObject LeapHandController;

    // sljedece public varijable sluze za kontrolu ispisivanja u debug-u unutar editora
    public bool IspisivanjeKoordinataLijeveRuke;
    public bool IspisivanjeKoordinataDesneRuke;
    public bool IspisivanjeKoordinatadesetsec;

    public float zglobVelicina = 0.015f;
    public bool prikaziPolozajDlana = true;
    public bool prikaziKostiDlana = false;
    public float debljinaPrstaNaspramZgloba = 0.8f;
    public bool zgloboviObojani = true;
    private List<GameObject> zgloboviIKostiLeap = new List<GameObject>();
    private List<GameObject> w = new List<GameObject>();
    private List<GameObject> zgloboviIKostiPrimljeniPodaci = new List<GameObject>();

    GameObject prosliPolozajDlana = null;

    // Varijable za mjerenje udaljenosti Palm-a, te se varijable postavljaju na 5.0f
    float kordinateIzPoslanihPodataka = 0.0f;
    float KordinateIzTrenutnogRacunala = 0.0f;
    float kordinateIzPoslanihPodataka1 = 0.0f;
    float KordinateIzTrenutnogRacunala1 = 0.0f;

    // za racunanje koordinata
    static List<float> ListaKoordinata = new List<float>();
    float vrijemeodbrojavanja = 0.0f;
    float rezultat = 0;
    int brojackoordinata = 0;

    private void Start()
    {
        leapProvider = FindObjectOfType<LeapServiceProvider>();

        GO = new GameObject();
        Konekcija();

        // stvaranje 4 game objekta u kojem se spremaju objekti (pogledati metodu)
        GORukeSetup();

        //PostaviUdaljenost();

        udpSend.Init();
        udpReceive.Init();

        PocetniVektor = new Vector(0.0f, 0.0f, 0.0f);
    }

    // Metoda stvara game objekte LRT = lijevarukatrenutni i LRP = lijevarukaposlani, sto znaci da se ucitana ruka iz trenutnog leap-a stvara unutar
    // game objekta LRT da bi se lakše pratilo stanje u hierarchy
    private void GORukeSetup()
    {
        GameObject LRT = new GameObject();
        GameObject LRP = new GameObject();
        GameObject DRT = new GameObject();
        GameObject DRP = new GameObject();

        LRT.name = "LRT";
        //LRT.tag = "LRT";

        LRP.name = "LRP";
        //LRP.tag = "LRP";

        DRT.name = "DRT";
        //DRT.tag = "DRT";

        DRP.name = "DRP";
        //DRP.tag = "DRP";

    }

    // Udaljenost u virtualnom "svijetu" nije ista kao udaljenost u stvarnom, jer ukoliko se leap-ovi pomaknu u stvarnom
    // tada je potrebno namijestiti LeapHandController po X osi jer u suprotnom ruke ce se stvarati na krivim mjestima
    void PostaviUdaljenost()
    {
        if (DesniLeap == false)
        {
            prvaCrta = false;
        }

        if (DesniLeap == true)
        {
            if (prvaCrta == true)
            {
                //pozicija
                LeapHandController.transform.position = new Vector3(0.098f, -0.011f, 0.021f); // (0.098f, -0.011f, 0.021f);
                //rotacija
                LeapHandController.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f); // (-3.107f, 0.0f, 0.0f);
            }
            else
            {
                //pozicija
                LeapHandController.transform.position = new Vector3(0.19f, -0.011f, 0.021f); // (0.19f,-0.011f, 0.021f);
                //rotacija
                LeapHandController.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f); // (-3.107f, 0.0f, 0.0f);
            }

        }
        // lijevi leap
        else
        {
            LeapHandController.transform.position = new Vector3(-0.21f, 0.0f, 0.0f); // (-0.21f,0.0f,0.0f);
        }
    }


    private void Konekcija()
    {
        string gatewayadresa = " ";

        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                IPInterfaceProperties adapterProperties = ni.GetIPProperties();
                GatewayIPAddressInformationCollection adresses = adapterProperties.GatewayAddresses;

                if (adresses != null)
                {
                    foreach (GatewayIPAddressInformation adress in adresses)
                    {

                        //IP = adress.Address.ToString();
                        if (adress.Address.ToString() == "0.0.0.0")
                        {
                            Debug.Log("konekcija na gateway: " + "-----");
                        }
                        else
                        {
                            gatewayadresa = adress.Address.ToString();
                            Debug.Log("konekcija na gateway: " + gatewayadresa);

                        }
                        tekst.text = gatewayadresa;
                    }
                }
                else
                {
                    tekst.text = "Not Connected";
                }
            }//if
        }// foreach
    }//metoda


    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);

    }

    private UnityEngine.Vector3 Leap2UnityVector(Leap.Vector v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    private Leap.Vector Leap2LeapVector(Vector3 v)
    {
        return new Leap.Vector(v.x, v.y, v.z);
    }

    List<float> HandsToFloatArray(List<Leap.Hand> hands)
    {
        List<float> FloatArray = new List<float>();

        FloatArray.Add(hands.Count);

        for (int z = 0; z < hands.Count; z++)
        {
            Hand h = hands[z];

            if (h.IsLeft)
            {
                FloatArray.Add(0f);
            }
            else
            {
                FloatArray.Add(1f);
            }

            for (int i = 0; i < 5; i++)
            {
                Finger f = h.Fingers[i];
                // Debug.Log("PRST : " + f.ToString() + "indeks: " + i);

                FloatArray.Add(f.TipPosition.x);
                FloatArray.Add(f.TipPosition.y);
                FloatArray.Add(f.TipPosition.z);

                for (int j = 0; j < 4; j++)
                {
                    // Debug.Log("Kost broj :" + j);
                    Bone b = f.Bone((Bone.BoneType)j);
                    Vector pJoint = b.PrevJoint;
                    Vector nJoint = b.NextJoint;

                    FloatArray.Add(pJoint.x);
                    FloatArray.Add(pJoint.y);
                    FloatArray.Add(pJoint.z);

                    FloatArray.Add(nJoint.x);
                    FloatArray.Add(nJoint.y);
                    FloatArray.Add(nJoint.z);
                }
            }

            FloatArray.Add(h.PalmPosition.x);
            FloatArray.Add(h.PalmPosition.y);
            FloatArray.Add(h.PalmPosition.z);
            //Debug.Log("ASDASSDASFASASFASFASFASF" + h.PalmPosition.x);

        }



        return FloatArray;
    }

    List<Leap.Hand> FloatArrayToHands(List<float> FloatArray, List<GameObject> zgloboviIKostii)
    {
        List<Leap.Hand> hands = new List<Hand>();
        int z = 0;

        int handsCount = (int)FloatArray[z];
        z++;

        for (int k = 0; k < handsCount; k++)
        {
            Leap.Hand h = new Leap.Hand();

            float leftRight = FloatArray[z];
            z++;

            if (leftRight == 0)
            {
                h.IsLeft = true;
            }
            else
            {
                h.IsLeft = false;
            }

            for (int i = 0; i < 5; i++)
            {
                Finger f = h.Fingers[i];

                f.TipPosition.x = FloatArray[z];
                f.TipPosition.y = FloatArray[z + 1];
                f.TipPosition.z = FloatArray[z + 2];
                z += 3;


                for (int j = 0; j < 4; j++)
                {
                    Bone b = f.Bone((Bone.BoneType)j);

                    b.PrevJoint = new Vector(FloatArray[z], FloatArray[z + 1], FloatArray[z + 2]);
                    z += 3;

                    b.NextJoint = new Vector(FloatArray[z], FloatArray[z + 1], FloatArray[z + 2]);
                    z += 3;

                }
            }

            h.PalmPosition.x = FloatArray[z];
            h.PalmPosition.y = FloatArray[z + 1];
            h.PalmPosition.z = FloatArray[z + 2];
            z += 3;


            hands.Add(h);
        }

        return hands;
    }

    /* 
    Ova metoda je slicna kao i pocetna metoda od koje je ova modificirana IscrtajRuke(bez parametra Check)
    Metoda provjerava od kud su ucitani podaci, ukoliko su od poslanih tada stvara ruku u LRP game objektu koji je prijasnje objasnjen kod start() metode

    */
    void IscrtajRukee(List<Hand> hands, List<GameObject> zgloboviIKosti, bool localData, string Check)
    {
        // ovaj GO je game object pomocu kojeg se trazi check string u game(playmode) sceni kao GameObject , 4 vrste( LRT LRP DRT DRP)
        GO = GameObject.Find(Check);

        if (zgloboviIKosti != null)
        {

            foreach (GameObject go in zgloboviIKosti)
            {
                //System.GC.Collect(); // moze se ukljuciti dinamicki garbage collector koji oslobodi malo memorije ukoliko se gtx.waitforpresent povecava
                Destroy(go);
            }
            zgloboviIKosti.Clear();

        }

        foreach (Hand hand in hands)
        {
            if (hand == hands[0])
            {
                //Debug.Log("check" + hand + hands[0]);
                Color bojaZgloba;

                if (!localData)
                {
                    // Podaci preko mreže s drugog LEAP uređaja --> PUNA BOJA

                    bojaZgloba = hand.IsLeft ? Color.red : Color.blue;
                }
                else
                {
                    // Podaci s lokalnog uređaja priključenog na računalo

                    // bojaZgloba = hand.IsLeft ? new Color(250, 128, 114) : new Color(65, 105, 225);
                    bojaZgloba = hand.IsLeft ? new Color(0.98F, 0.5F, 0.44F) : new Color(0.25F, 0.41F, 0.88F);
                    //bojaZgloba = hand.IsLeft ?  Color.red :  Color.blue;
                }

                // Iscrtavanje sfere (palm-a)
                if (prikaziPolozajDlana)
                {
                    /* Iscrtavanje položaja dlana */
                    GameObject polozajDlana = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    // polozajDlana.transform.position = start;
                    polozajDlana.transform.position = Leap2UnityVector(hand.PalmPosition);
                    polozajDlana.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    zgloboviIKosti.Add(polozajDlana);
                    //dodano za odvojene dlanove
                    polozajDlana.transform.parent = GO.transform;
                    //
                }

                int i = 0;
                // foreach (Finger f in hand.Fingers)
                for (int e = 0; e < 5; e++)
                {
                    Finger f = hand.Fingers[e];

                    // TYPE_THUMB = = 0 - TYPE_INDEX = = 1 - TYPE_MIDDLE = = 2 - TYPE_RING = = 3 - TYPE_PINKY = = 4 -
                    GameObject k = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    k.transform.position = Leap2UnityVector(f.TipPosition);
                    k.transform.localScale = new Vector3(zglobVelicina, zglobVelicina, zglobVelicina);
                    //dodano za odvojene dlanove
                    k.transform.parent = GO.transform;
                    //
                    if (zgloboviObojani) k.GetComponent<Renderer>().material.color = bojaZgloba;

                    zgloboviIKosti.Add(k);
                    k.name = "Vrh prsta " + i++;

                    for (int j = 0; j < 4; j++)
                    {
                        k = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        k.transform.position = Leap2UnityVector(f.Bone((Bone.BoneType)j).PrevJoint);
                        k.transform.localScale = new Vector3(zglobVelicina, zglobVelicina, zglobVelicina);
                        if (zgloboviObojani) k.GetComponent<Renderer>().material.color = bojaZgloba;
                        zgloboviIKosti.Add(k);
                        k.name = "Zglob prethodni" + j;
                        //dodano za odvojene dlanove
                        k.transform.parent = GO.transform;
                        //

                        if ((!prikaziKostiDlana) && (j == 0)) continue;

                        // https://forum.unity.com/threads/draw-cylinder-between-2-points.23510/
                        Vector3 prevJoint = Leap2UnityVector(f.Bone((Bone.BoneType)j).PrevJoint);
                        Vector3 nextJoint = Leap2UnityVector(f.Bone((Bone.BoneType)j).NextJoint);

                        k = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                        Vector3 pos = Vector3.Lerp(prevJoint, nextJoint, (float)0.5);
                        k.transform.position = pos;
                        k.transform.up = nextJoint - prevJoint;

                        float razdaljina = Vector3.Distance(prevJoint, nextJoint);
                        k.transform.localScale = new Vector3(zglobVelicina * debljinaPrstaNaspramZgloba, razdaljina / 2, zglobVelicina * debljinaPrstaNaspramZgloba);

                        k.name = "Kost " + j;
                        zgloboviIKosti.Add(k);
                        //dodano za odvojene dlanove
                        k.transform.parent = GO.transform;
                        //
                    } // for
                } // foreach finger
            }

        }
    } // metoda

    // UPDATE
    // Update is called once per frame
    //private void Update()
    //{
    //    deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    //    Frame currentFrame = leapProvider.CurrentFrame;

    //    // refresh varijable na 5 pomocu kojih mjerimo udaljenost Palm-a
    //    // varijable su postavljene na 5 jer lijeva strana leap-a je u - koordinati pa se tako od 5 oduzme -0.2578458 koordinata te se kasnije provjeri koja je blize
    //    kordinateIzPoslanihPodataka = 5.0f;
    //    KordinateIzTrenutnogRacunala = 5.0f;
    //    kordinateIzPoslanihPodataka1 = 5.0f;
    //    KordinateIzTrenutnogRacunala1 = 5.0f;

    //    if (slanjePodataka != false)
    //    {

    //        //Dio kôda za slanje podataka
    //        IFormatter formatterSend = new BinaryFormatter();
    //        MemoryStream msSend = new MemoryStream();
    //        formatterSend.Serialize(msSend, HandsToFloatArray(currentFrame.Hands));
    //        byte[] bytePodaciRL = msSend.ToArray();
    //        // salje na drugo racunalo
    //        udpSend.sendBytes(bytePodaciRL);
    //        // -- završetak dijela kôda za slanje podataka.

    //    }

    //    // Dio kôda za primanje podataka
    //    float time1 = Time.realtimeSinceStartup;
    //    byte[] data = udpReceive.ReceiveDataOnce();
    //    float time2 = Time.realtimeSinceStartup;

    //    if (data != null)
    //    {

    //        IFormatter formatter = new BinaryFormatter();
    //        MemoryStream ms = new MemoryStream(data);

    //        List<float> dataReceived = (List<float>)formatter.Deserialize(ms);

    //        List<Hand> hands = FloatArrayToHands(dataReceived, zgloboviIKostiPrimljeniPodaci); // zgloboviIKostiPrimljeniPodaci

    //        // u listu se zapisuje samo jedna ruka od svakog leap-a
    //        // Pokusaj sa Hand objektom a ne Listom, u tom slucaju fps drop-a
    //        // lista Hand od trenutnih podatak sa leap-a
    //        List<Hand> lijevarukatrenutni = new List<Hand>();
    //        List<Hand> desnarukatrenutni = new List<Hand>();

    //        // lista Hand od prenesenih podatak sa leap-a
    //        List<Hand> lijevarukaposlani = new List<Hand>();
    //        List<Hand> desnarukaposlani = new List<Hand>();

    //        vrijemeodbrojavanja += Time.deltaTime;

    //        // desni leap == true oznacava public varijablu koja se postavi u editoru kao checkbox, ona oznacava da li je leap senzor sa desne strane 
    //        // il isa lijeve, mora se postaviti pravilno!
    //        // unutar if(desniLeap == true) i else su koordinate sa - i +, - je jer ukoliko je desni leap tada je palmposition negativan te se tako oduzme od 
    //        // 5.0f vrijednost koordinate koja kasnije sluzi da bi se odredila udaljenost i iscrtala pravilna ruka
    //        // hands podaci iz trenutnog racunala
    //        foreach (Hand hh in currentFrame.Hands)
    //        {
    //            if (hh.IsLeft)
    //            {
    //                if (DesniLeap == true)
    //                {
    //                    KordinateIzTrenutnogRacunala = KordinateIzTrenutnogRacunala - hh.PalmPosition.x;
    //                    //Debug.Log("LIJEVA ruka trenutni podaci: " + KordinateIzTrenutnogRacunala);
    //                    ListaKoordinata.Add(KordinateIzTrenutnogRacunala);
    //                    lijevarukatrenutni.Add(hh);
    //                }
    //                else
    //                {
    //                    KordinateIzTrenutnogRacunala = KordinateIzTrenutnogRacunala + hh.PalmPosition.x;
    //                    //Debug.Log("LIJEVA ruka trenutni podaci: " + KordinateIzTrenutnogRacunala);
    //                    lijevarukatrenutni.Add(hh);
    //                }

    //            }
    //            else if (!hh.IsLeft)
    //            {
    //                if (DesniLeap == true)
    //                {
    //                    KordinateIzTrenutnogRacunala1 = KordinateIzTrenutnogRacunala1 - hh.PalmPosition.x;
    //                    //Debug.Log("DESNA ruka trenutni podaci: " + KordinateIzTrenutnogRacunala1);
    //                    //ListaKoordinata.Add(KordinateIzTrenutnogRacunala1);
    //                    desnarukatrenutni.Add(hh);
    //                }
    //                else
    //                {
    //                    KordinateIzTrenutnogRacunala1 = KordinateIzTrenutnogRacunala1 + hh.PalmPosition.x;
    //                    //Debug.Log("DESNA ruka trenutni podaci: " + KordinateIzTrenutnogRacunala1);
    //                    desnarukatrenutni.Add(hh);
    //                }

    //            }

    //        }

    //        // hands podaci  iz poslanih podataka
    //        foreach (Hand h in hands)
    //        {
    //            if (h.IsLeft)
    //            {
    //                if (DesniLeap == true)
    //                {
    //                    kordinateIzPoslanihPodataka = kordinateIzPoslanihPodataka + h.PalmPosition.x;
    //                    //Debug.Log("LIJEVA ruka poslani podaci: " + kordinateIzPoslanihPodataka);
    //                    ListaKoordinata.Add(kordinateIzPoslanihPodataka);
    //                    lijevarukaposlani.Add(h);
    //                }
    //                else
    //                {
    //                    kordinateIzPoslanihPodataka = kordinateIzPoslanihPodataka - h.PalmPosition.x;
    //                    //Debug.Log("LIJEVA ruka poslani podaci: " + kordinateIzPoslanihPodataka);
    //                    lijevarukaposlani.Add(h);
    //                }

    //            }
    //            else if (!h.IsLeft)
    //            {
    //                if (DesniLeap == true)
    //                {
    //                    kordinateIzPoslanihPodataka1 = kordinateIzPoslanihPodataka1 + h.PalmPosition.x;
    //                    //Debug.Log("DESNA ruka poslani podaci: " + kordinateIzPoslanihPodataka1);
    //                    //ListaKoordinata.Add(kordinateIzPoslanihPodataka1);
    //                    desnarukaposlani.Add(h);

    //                }
    //                else
    //                {
    //                    kordinateIzPoslanihPodataka1 = kordinateIzPoslanihPodataka1 - h.PalmPosition.x;
    //                    //Debug.Log("DESNA ruka poslani podaci: " + kordinateIzPoslanihPodataka1);
    //                    desnarukaposlani.Add(h);
    //                }

    //            }

    //        }
    //        // ovo je za koordinate tj. pronalazenje srednje koordinate od svih(i lijeve i desne) ucitanih proteklih 10 sec

    //        if (IspisivanjeKoordinataLijeveRuke == true)
    //        {
    //            ProvjeraSrednjeKoordinateLijeveRuke();
    //        }
    //        if (IspisivanjeKoordinataDesneRuke == true)
    //        {
    //            ProvjeraSrednjeKoordinateDesneRuke();
    //        }
    //        if (IspisivanjeKoordinatadesetsec == true)
    //        {
    //            if (vrijemeodbrojavanja > 10)
    //            {
    //                ProvjeraSrednjeKoordinatePrvaMetoda();
    //            }

    //        }

    //        // Popravak ukoliko leap ucita pogresnu ruku, izbrise ju iz liste u kojoj je bila spremljena te se tako ruka ne zadrzava (freez-a) na sceni
    //        if (lijevarukatrenutni.Count == 0 || lijevarukaposlani.Count == 0)
    //        {
    //            Izbrisi(zgloboviIKostiLeap);
    //        }
    //        if (desnarukatrenutni.Count == 0 || desnarukaposlani.Count == 0)
    //        {
    //            Izbrisi(zgloboviIKostiPrimljeniPodaci);
    //        }

    //        // PRVI UVJET ZA LIJEVE RUKE
    //        if (kordinateIzPoslanihPodataka == 5.0f || kordinateIzPoslanihPodataka > KordinateIzTrenutnogRacunala)
    //        {
    //            //IscrtajRukee(lijevarukatrenutni, zgloboviIKostiLeap, true, "LRT");

    //            IscrtajRuke2(currentFrame.Hands, zgloboviIKostiLeap, true);
    //        }
    //        else if (KordinateIzTrenutnogRacunala == 5.0f || KordinateIzTrenutnogRacunala > kordinateIzPoslanihPodataka)
    //        {
    //            //IscrtajRukee(lijevarukaposlani, zgloboviIKostiLeap, true, "LRP");

    //            IscrtajRuke2(currentFrame.Hands, zgloboviIKostiLeap, true);
    //        }

    //        // DRUGI UVJET ZA DESNE RUKE
    //        if (kordinateIzPoslanihPodataka1 == 5.0f || kordinateIzPoslanihPodataka1 > KordinateIzTrenutnogRacunala1)
    //        {
    //            //IscrtajRukee(desnarukatrenutni, zgloboviIKostiPrimljeniPodaci, true, "DRT");

    //            IscrtajRuke2(currentFrame.Hands, zgloboviIKostiLeap, true);
    //        }
    //        else if (KordinateIzTrenutnogRacunala1 == 5.0f || KordinateIzTrenutnogRacunala1 > kordinateIzPoslanihPodataka1)
    //        {
    //            //IscrtajRukee(desnarukaposlani, zgloboviIKostiPrimljeniPodaci, true, "DRP");

    //            IscrtajRuke2(currentFrame.Hands, zgloboviIKostiLeap, true);
    //        }
    //    }
    //    //else
    //    //{
    //    //    // izvodi ukoliko je data == null tj. ukoliko konekcija nije uspostavljena
    //    //    IscrtajRuke(currentFrame.Hands, zgloboviIKostiLeap1, true);
    //    //}

    //} // Update


    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        Frame currentFrame = leapProvider.CurrentFrame;

        // ovo je u slucaju da ne postoji drugo racunalo...
        //IscrtajRuke2(currentFrame.Hands, zgloboviIKostiLeap, true);

        // refresh varijable na 5 pomocu kojih mjerimo udaljenost Palm-a
        // varijable su postavljene na 5 jer lijeva strana leap-a je u - koordinati pa se tako od 5 oduzme -0.2578458 koordinata te se kasnije provjeri koja je blize
        kordinateIzPoslanihPodataka = 5.0f;
        KordinateIzTrenutnogRacunala = 5.0f;

        if (slanjePodataka != false)
        {

            //Dio kôda za slanje podataka
            IFormatter formatterSend = new BinaryFormatter();
            MemoryStream msSend = new MemoryStream();
            formatterSend.Serialize(msSend, HandsToFloatArray(currentFrame.Hands));
            byte[] bytePodaciRL = msSend.ToArray();
            // salje na drugo racunalo
            udpSend.sendBytes(bytePodaciRL);
            // -- završetak dijela kôda za slanje podataka.

        }

        // Dio kôda za primanje podataka
        float time1 = Time.realtimeSinceStartup;
        byte[] data = udpReceive.ReceiveDataOnce();
        float time2 = Time.realtimeSinceStartup;

        if (data != null)
        {

            IFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(data);

            List<float> dataReceived = (List<float>)formatter.Deserialize(ms);

            List<Hand> hands = FloatArrayToHands(dataReceived, zgloboviIKostiPrimljeniPodaci);


            vrijemeodbrojavanja += Time.deltaTime;

            foreach (Hand hh in currentFrame.Hands)
            {
                if (hh.IsLeft)
                {
                    if (DesniLeap == true)
                    {
                        KordinateIzTrenutnogRacunala = KordinateIzTrenutnogRacunala - hh.PalmPosition.x;
                        Debug.Log("LIJEVA ruka trenutni podaci: " + hh.PalmPosition.x + " " + hh.PalmPosition.y + " " + hh.PalmPosition.z);
                        ListaKoordinata.Add(KordinateIzTrenutnogRacunala);
                    }
                }
            }

            // hands podaci  iz poslanih podataka
            foreach (Hand h in hands)
            {
                if (h.IsLeft)
                {
                    if (DesniLeap == true)
                    {
                        kordinateIzPoslanihPodataka = kordinateIzPoslanihPodataka + h.PalmPosition.x;
                        Debug.Log("LIJEVA ruka poslani podaci: " + h.PalmPosition.x + " " + h.PalmPosition.y + " " + h.PalmPosition.z);
                        ListaKoordinata.Add(kordinateIzPoslanihPodataka);
                    }
                }
            }


            // PRVI UVJET ZA LIJEVE RUKE
            if (kordinateIzPoslanihPodataka == 5.00000f || kordinateIzPoslanihPodataka > KordinateIzTrenutnogRacunala)
            {
                pomakniRuku(currentFrame.Hands, zgloboviIKostiLeap, true);
            }
            else if (KordinateIzTrenutnogRacunala == 5.00000f || KordinateIzTrenutnogRacunala > kordinateIzPoslanihPodataka)
            {
                //IscrtajRukee(lijevarukaposlani, zgloboviIKostiLeap, true, "LRP");
                pomakniRuku(hands, zgloboviIKostiLeap, false);
            }

        }

    } // Update

    // TODO - varijable staviti gore nakon sto se napravi potrebno
    Leap.Vector ReferentnaTocka = new Leap.Vector();

    void pomakniRuku(List<Hand> hands, List<GameObject> zgloboviIKosti, bool localData)
    {
        foreach (Hand hand in hands)
        {
            if (sredinajednom == false /*&& hand.PalmPosition.x < 0.03f && hand.PalmPosition.x > 0.0f*/)
            {
                sredinajednom = true;

                objekt1.transform.position = new Vector3(hand.PalmPosition.x, hand.PalmPosition.y, hand.PalmPosition.z);

                ReferentnaTocka = Leap2LeapVector(objekt1.transform.localPosition);

                Debug.Log("REFERENTNA TOCKA: " + ReferentnaTocka);
            }
            if (sredinajednom == true)
            {
                KOORDINATA2(Leap2UnityVector(hand.PalmPosition), Leap2UnityVector(ReferentnaTocka), localData);
                ReferentnaTocka = hand.PalmPosition;
            }

        }


    }

    float PomakXPrijasnji = 0.0f;
    float PomakYPrijasnji = 0.0f;
    float PomakZPrijasnji = 0.0f;

    void KOORDINATA2(Vector3 TrenutniFrameKoordinate, Vector3 ProsliFrameKoordinate, bool localData)
    {
        float PomakX = 0.0f;
        float PomakY = 0.0f;
        float PomakZ = 0.0f;

        // radi ukoliko je localdata true tj poslani podaci su samo iz trenutnog leap-a
        if (localData == true)
        {
            PomakX = TrenutniFrameKoordinate.x - ProsliFrameKoordinate.x;
            PomakY = TrenutniFrameKoordinate.y - ProsliFrameKoordinate.y;
            PomakZ = TrenutniFrameKoordinate.z - ProsliFrameKoordinate.z;

            PomakXPrijasnji = TrenutniFrameKoordinate.x - ProsliFrameKoordinate.x;
            PomakYPrijasnji = TrenutniFrameKoordinate.y - ProsliFrameKoordinate.y;
            PomakZPrijasnji = TrenutniFrameKoordinate.z - ProsliFrameKoordinate.z;

            Debug.Log("(TRENUTNI)pomicem objekt za = " + PomakX + " " + PomakXPrijasnji + " " + PomakY + " " + PomakZ);

            //TrenutniLeapZadnjaKoordinataPrijeSwitcha = TrenutniFrameKoordinate;

            objekt1.transform.position += new Vector3(PomakX, PomakY, PomakZ);



        }
        // radi ukoliko je localdata false tj poslani podaci su samo iz drugog leap-a
        else
        {
            PomakX = TrenutniFrameKoordinate.x + ProsliFrameKoordinate.x;
            PomakY = TrenutniFrameKoordinate.y + ProsliFrameKoordinate.y;
            PomakZ = TrenutniFrameKoordinate.z + ProsliFrameKoordinate.z;

            Debug.Log("(POSLANI)pomicem objekt za = " + PomakX + " " + PomakY + " " + PomakZ);

            //TrenutniLeapZadnjaKoordinataPrijeSwitcha = TrenutniFrameKoordinate;

            objekt1.transform.position = new Vector3(PomakXPrijasnji, PomakYPrijasnji,PomakZPrijasnji);
            objekt1.transform.position += new Vector3(PomakX, PomakY, PomakZ);

            //Vector3 privremenivektor = TrenutniFrameKoordinate - ProsliFrameKoordinate;

            //Vector3 privremeniVektor1 = new Vector3(TrenutniFrameKoordinate.x + ProsliFrameKoordinate.x,
            //    TrenutniFrameKoordinate.y + ProsliFrameKoordinate.y,
            //    TrenutniFrameKoordinate.z + ProsliFrameKoordinate.z);

            //Debug.Log("POSLANI " + TrenutniFrameKoordinate + " " + ProsliFrameKoordinate + " " + privremeniVektor1);

            ////Vector3 promjenasmjera = new Vector3(privremeniVektor1.x * -1, privremeniVektor1.y * -1, privremeniVektor1.z * -1);

            //objekt1.transform.position = privremeniVektor1 + TrenutniLeapZadnjaKoordinataPrijeSwitcha;


        }


    }

    void KOORDINATA(Vector3 TrenutneKoordinate, Vector3 ProsleKoordinate, bool localData)
    {
        float NovaX = 0.0f;
        float NovaY = 0.0f;
        float NovaZ = 0.0f;

        if (localData == true)
        {
            Vector3 TrenutneKoordinate2 = new Vector3(TrenutneKoordinate.x, TrenutneKoordinate.y, TrenutneKoordinate.z);
            Vector3 trenutna = new Vector3(ProsleKoordinate.x, ProsleKoordinate.y, ProsleKoordinate.z);

            #region Pretvorba - u +
            // trenutne koordinate iz hand-a
            if (TrenutneKoordinate.x < 0.0f)
            {
                TrenutneKoordinate.x = (TrenutneKoordinate.x * -1);
            }
            if (TrenutneKoordinate.y < 0.0f)
            {
                TrenutneKoordinate.y = (TrenutneKoordinate.y * -1);
            }
            if (TrenutneKoordinate.z < 0.0f)
            {
                TrenutneKoordinate.z = (TrenutneKoordinate.z * -1);
            }
            // prosle koordinate
            if (ProsleKoordinate.x < 0.0f)
            {
                ProsleKoordinate.x = (ProsleKoordinate.x * -1);
            }
            if (ProsleKoordinate.y < 0.0f)
            {
                ProsleKoordinate.y = (ProsleKoordinate.y * -1);
            }
            if (ProsleKoordinate.z < 0.0f)
            {
                ProsleKoordinate.z = (ProsleKoordinate.z * -1);
            }
            #endregion

            #region Racunanje razlike koordinata
            if (TrenutneKoordinate.x > ProsleKoordinate.x)
            {
                NovaX = TrenutneKoordinate.x - ProsleKoordinate.x;
            }
            else
            {
                NovaX = ProsleKoordinate.x - TrenutneKoordinate.x;
            }

            if (TrenutneKoordinate.y > ProsleKoordinate.y)
            {
                NovaY = TrenutneKoordinate.y - ProsleKoordinate.y;
            }
            else
            {
                NovaY = ProsleKoordinate.y - TrenutneKoordinate.y;
            }

            if (TrenutneKoordinate.z > ProsleKoordinate.z)
            {
                NovaZ = TrenutneKoordinate.z - ProsleKoordinate.z;
            }
            else
            {
                NovaZ = ProsleKoordinate.z - TrenutneKoordinate.z;
            }

            #endregion


            //Debug.Log(trenutna + " " + ProsleKoordinate);

            #region Check
            if (TrenutneKoordinate2.x < 0.0000f)
            {
                trenutna.x = trenutna.x - NovaX;
            }
            else
            {
                trenutna.x = trenutna.x - NovaX;
            }

            if (TrenutneKoordinate2.y < 0.0000f)
            {
                trenutna.y = trenutna.y - NovaY;
            }
            else
            {
                trenutna.y = trenutna.y - NovaY;
            }

            if (TrenutneKoordinate2.x < 0.0000f)
            {
                trenutna.z = trenutna.z - NovaZ;
            }
            else
            {
                trenutna.z = trenutna.z - NovaZ;
            }
            #endregion

            //Debug.Log("TRENUTNI = " + trenutna);

            objekt1.transform.position = trenutna;
        }
        else
        {
            Vector3 TrenutneKoordinate2 = new Vector3(TrenutneKoordinate.x, TrenutneKoordinate.y, TrenutneKoordinate.z);
            Vector3 trenutna = new Vector3(ProsleKoordinate.x, ProsleKoordinate.y, ProsleKoordinate.z);

            #region Pretvorba - u +
            // trenutne koordinate iz hand-a
            if (TrenutneKoordinate.x < 0.0f)
            {
                TrenutneKoordinate.x = (TrenutneKoordinate.x * -1);
            }
            if (TrenutneKoordinate.y < 0.0f)
            {
                TrenutneKoordinate.y = (TrenutneKoordinate.y * -1);
            }
            if (TrenutneKoordinate.z < 0.0f)
            {
                TrenutneKoordinate.z = (TrenutneKoordinate.z * -1);
            }
            // prosle koordinate
            if (ProsleKoordinate.x < 0.0f)
            {
                ProsleKoordinate.x = (ProsleKoordinate.x * -1);
            }
            if (ProsleKoordinate.y < 0.0f)
            {
                ProsleKoordinate.y = (ProsleKoordinate.y * -1);
            }
            if (ProsleKoordinate.z < 0.0f)
            {
                ProsleKoordinate.z = (ProsleKoordinate.z * -1);
            }
            #endregion

            #region Racunanje razlike koordinata
            if (TrenutneKoordinate.x > ProsleKoordinate.x)
            {
                NovaX = TrenutneKoordinate.x - ProsleKoordinate.x;
            }
            else
            {
                NovaX = ProsleKoordinate.x - TrenutneKoordinate.x;
            }

            if (TrenutneKoordinate.y > ProsleKoordinate.y)
            {
                NovaY = TrenutneKoordinate.y - ProsleKoordinate.y;
            }
            else
            {
                NovaY = ProsleKoordinate.y - TrenutneKoordinate.y;
            }

            if (TrenutneKoordinate.z > ProsleKoordinate.z)
            {
                NovaZ = TrenutneKoordinate.z - ProsleKoordinate.z;
            }
            else
            {
                NovaZ = ProsleKoordinate.z - TrenutneKoordinate.z;
            }

            #endregion


            //Debug.Log(trenutna + " " + ProsleKoordinate);

            #region Check
            if (TrenutneKoordinate2.x < 0.0000f)
            {
                trenutna.x = trenutna.x + NovaX;
            }
            else
            {
                trenutna.x = trenutna.x + NovaX;
            }

            if (TrenutneKoordinate2.y < 0.0000f)
            {
                trenutna.y = trenutna.y + NovaY;
            }
            else
            {
                trenutna.y = trenutna.y + NovaY;
            }

            if (TrenutneKoordinate2.x < 0.0000f)
            {
                trenutna.z = trenutna.z + NovaZ;
            }
            else
            {
                trenutna.z = trenutna.z + NovaZ;
            }
            #endregion

            if (trenutna.x > 0.0000f)
            {
                trenutna = new Vector3(trenutna.x * -1, trenutna.y, trenutna.z * -1);
            }
            //Debug.Log("POSLANI = " + trenutna);


            objekt1.transform.position = trenutna;
        }


    }


    // metoda koja popravlja ukoliko Leap ucita pogresnu ruku tj kada se iscrtana ruka treba pobrisati da se iscrta ruka koja je blize tj. suprotna
    void Izbrisi(List<GameObject> zgloboviIKosti)
    {
        if (zgloboviIKosti != null)
        {

            foreach (GameObject go in zgloboviIKosti)
            {
                Destroy(go);
            }
            zgloboviIKosti.Clear();

        }

    }

    /*************************************************************************/

    // https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/
    public class UDPReceive
    {

        // receiving Thread
        Thread receiveThread;


        // udpclient object
        UdpClient client = null;

        // public
        public string IP = "127.0.0.1"; // default local
        public int port; // define > init

        // infos
        public string lastReceivedUDPPacket = "";
        public string allReceivedUDPPackets = ""; // clean up this from time to time!

        // OnGUI
        void OnGUI()
        {
            Rect rectObj = new Rect(40, 10, 200, 400);
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            GUI.Box(rectObj, "# UDPReceive\n127.0.0.1 " + port + " #\n"
                        + "shell> nc -u 127.0.0.1 : " + port + " \n"
                        + "\nLast Packet: \n" + lastReceivedUDPPacket
                        + "\n\nAll Messages: \n" + allReceivedUDPPackets
                    , style);
        }

        // init
        public void Init()
        {
            // Endpunkt definieren, von dem die Nachrichten gesendet werden.
            print("UDPReceive.init()");

            // define port
            port = 60001;

            // status
            print("Sending to 127.0.0.1 : " + port);
            print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");


            // ----------------------------
            // Abhören
            // ----------------------------
            // Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
            // Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.

            // receiveThread = new Thread(new ThreadStart(ReceiveData));
            // receiveThread.IsBackground = true;
            // receiveThread.Start();
        }

        // receive thread
        public void ReceiveData()
        {
            client = new UdpClient(port);
            while (true)
            {
                try
                {
                    // Bytes empfangen.
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = client.Receive(ref anyIP);

                    // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                    string text = Encoding.UTF8.GetString(data);

                    // Den abgerufenen Text anzeigen.
                    print(">> " + text);

                    // latest UDPpacket
                    lastReceivedUDPPacket = text;

                    // ....
                    allReceivedUDPPackets = allReceivedUDPPackets + text;

                }
                catch (Exception err)
                {
                    print(err.ToString());
                }
            }
        }

        public byte[] ReceiveDataOnce()
        {

            byte[] data = null;

            // client = new UdpClient(port);
            client = client ?? new UdpClient(port); // if (client == null) client = new UdpClinet(port)
            if (client != null)
            {
                client.Client.Blocking = false;
            }

            try
            {
                // Bytes empfangen.
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                data = client.Receive(ref anyIP);

                /*
                // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                string text = Encoding.UTF8.GetString(data);

                // Den abgerufenen Text anzeigen.
                print(">> " + text);

                // latest UDPpacket
                lastReceivedUDPPacket = text;

                // ....
                allReceivedUDPPackets = allReceivedUDPPackets + text;
                */

            }
            catch (Exception err)
            {
                // print(err.ToString());
                //Debug.Log("Error kod ReceiveDataOnce : " + err.Message);
            }

            return data;

        }

        // getLatestUDPPacket
        // cleans up the rest
        public string getLatestUDPPacket()
        {
            allReceivedUDPPackets = "";
            return lastReceivedUDPPacket;
        }
    }

    /*************************************************************************/

    public class UDPSend
    {
        private static int localPort;

        // prefs
        private string IP;  // define in init
        public int port;  // define in init

        // "connection" things
        IPEndPoint remoteEndPoint;
        UdpClient client;

        // gui
        string strMessage = "";

        // OnGUI
        void OnGUI()
        {
            Rect rectObj = new Rect(40, 380, 200, 400);
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            GUI.Box(rectObj, "# UDPSend-Data\n127.0.0.1 " + port + " #\n"
                        + "shell> nc -lu 127.0.0.1  " + port + " \n"
                    , style);

            // ------------------------
            // send it
            // ------------------------
            strMessage = GUI.TextField(new Rect(40, 420, 140, 20), strMessage);
            if (GUI.Button(new Rect(190, 420, 40, 20), "send"))
            {
                sendString(strMessage + "\n");
            }
        }

        // init
        public void Init()
        {
            // Endpunkt definieren, von dem die Nachrichten gesendet werden.
            print("UDPSend.init()");

            // define
            // IP = "127.0.0.1";

            //string sHostName = Dns.GetHostName();
            //IPHostEntry ipE = Dns.GetHostByName(sHostName);
            //IPAddress[] IpA = ipE.AddressList;
            //for (int i = 0; i < IpA.Length; i++)
            //{
            //    //Console.WriteLine("IP Address {0}: {1} ", i, IpA[i].ToString());
            //    //IP = IpA.ToString();
            //    Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA:"+ IpA[i].ToString());
            //}

            //IPHostEntry host;
            //string localip = "?";
            //host = Dns.GetHostEntry(Dns.GetHostName());

            //foreach (IPAddress ip in host.AddressList)
            //{
            //    if (ip.AddressFamily.ToString() == "InterNetwork")
            //    {
            //        localip = ip.ToString();                    
            //        Debug.Log("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB:" + localip.ToString());
            //    }


            //}

            //NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            //foreach (NetworkInterface adapter in adapters)
            //{
            //    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
            //    GatewayIPAddressInformationCollection adresses = adapterProperties.GatewayAddresses;

            //    Debug.Log("UUUUUUUUUUUUUUUUUUUUUU" + adresses.Count);


            //    if (adresses.Count > 0)
            //    {
            //        foreach (GatewayIPAddressInformation adress in adresses)
            //        {
            //            Debug.Log("GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG" + adress.Address.ToString());

            //            IP = adress.Address.ToString();


            //        }
            //    }

            //}

            string gatewayadresa = " ";

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    IPInterfaceProperties adapterProperties = ni.GetIPProperties();
                    GatewayIPAddressInformationCollection adresses = adapterProperties.GatewayAddresses;

                    foreach (GatewayIPAddressInformation adress in adresses)
                    {

                        if (adress.Address.ToString() == "0.0.0.0")
                        {
                            Debug.Log("konekcija na gateway: " + "-----");
                        }
                        else
                        {
                            gatewayadresa = adress.Address.ToString();
                        }

                    }
                }//if
            }// foreach

            // IP = "10.0.1.27";

            IP = gatewayadresa;
            port = 60001;

            // ----------------------------
            // Senden
            // ----------------------------
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            client = new UdpClient();

            // status
            print("Sending to " + IP + " : " + port);
        }


        // inputFromConsole
        private void inputFromConsole()
        {
            try
            {
                string text;
                do
                {
                    text = Console.ReadLine();

                    // Den Text zum Remote-Client senden.
                    if (text != "")
                    {

                        // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
                        byte[] data = Encoding.UTF8.GetBytes(text);

                        // Den Text zum Remote-Client senden.
                        client.Send(data, data.Length, remoteEndPoint);
                    }
                } while (text != "");
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }

        // sendData
        public void sendString(string message)
        {
            try
            {
                //if (message != "")
                //{

                // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
                byte[] data = Encoding.UTF8.GetBytes(message);

                // Den message zum Remote-Client senden.
                client.Send(data, data.Length, remoteEndPoint);
                //}
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }

        public void sendBytes(byte[] data)
        {
            try
            {
                client.Send(data, data.Length, remoteEndPoint);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }



}
