using UnityEngine;
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

public class KontrolaLeap : MonoBehaviour {

    float deltaTime = 0.0f;

    private LeapServiceProvider leapProvider;
    UDPSend udpSend = new UDPSend();
    UDPReceive udpReceive = new UDPReceive();

    public GameObject objekt1;

    public GameObject LeapHandController;
    // sljedece public varijable sluze za prikaz teksta tj. konekcije na sucelje tj canvas
    public Text tekst;
    public bool DesniLeap = true;


    private void Start()
    {
        leapProvider = FindObjectOfType<LeapServiceProvider>();

        Konekcija();

        udpSend.Init();
        udpReceive.Init();
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
    }//metoda konekcija

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

    //public float zglobVelicina = 0.015f;
    //public bool prikaziPolozajDlana = true;
    //public bool prikaziKostiDlana = false;
    //public float debljinaPrstaNaspramZgloba = 0.8f;
    //public bool zgloboviObojani = true;
    //private List<GameObject> w = new List<GameObject>();
    //private List<GameObject> zgloboviIKostiPrimljeniPodaci = new List<GameObject>();

    private List<GameObject> zgloboviIKostiLeap = new List<GameObject>();

    List<float> HandToFloatArray(Leap.Hand h)
    {
        List<float> FloatArray = new List<float>();

        for (int i = 0; i < 5; i++)
        {
            Finger f = h.Fingers[i];
            // Debug.Log("PRST : " + f.ToString() + "indeks: " + i);

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
        return FloatArray;
    }

    Leap.Hand FloatArrayToHand(List<float> FloatArray)
    {

        Leap.Hand h = new Leap.Hand();
        int z = 0;

        for (int i = 0; i < 5; i++)
        {
            Finger f = h.Fingers[i];
            for (int j = 0; j < 4; j++)
            {
                Bone b = f.Bone((Bone.BoneType)j);

                b.PrevJoint = new Vector(FloatArray[z], FloatArray[z + 1], FloatArray[z + 2]);
                z += 3;

                b.NextJoint = new Vector(FloatArray[z], FloatArray[z + 1], FloatArray[z + 2]);
                z += 3;
            }
        }

        return h;
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

    // za racunanje koordinata
    static List<float> ListaKoordinata = new List<float>();
    float vrijemeodbrojavanja = 0.0f;
    float rezultat = 0;
    int brojackoordinata = 0;


    float vrijeme = 0.0f;
    Leap.Vector PocetniVektor = new Leap.Vector();
    Leap.Vector NoviVektor1 = new Leap.Vector();
    Vector3 NoviVektor = new Vector3();
    bool jednom = false;
    bool sredinajednom = false;

    // Update is called once per frame
    private void Update()
    {       
        // delta time je za ms i prikaz na ekran
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        Frame currentFrame = leapProvider.CurrentFrame;

        //IscrtajRuke(currentFrame.Hands, zgloboviIKostiLeap, true);

        //IscrtajRuke(currentFrame.Hands, zgloboviIKostiLeap, true);

        vrijeme += Time.deltaTime;
        if (vrijeme >= 3.0f)
        {
            vrijeme = 0.0f;
            IscrtajRuke(currentFrame.Hands, zgloboviIKostiLeap, true);
        }

    } // Update


    void IscrtajRuke(List<Hand> hands, List<GameObject> zgloboviIKosti, bool localData)
    {
        foreach (Hand hand in hands)
        {
            if (hand.PalmPosition.x <= 0.003f && hand.PalmPosition.x >= 0.0025 && hand.PalmPosition.z <= 0.003f && hand.PalmPosition.z >= 0.0025 && jednom == false)
            {
                jednom = true;
                PocetniVektor = hand.PalmPosition;
            }
            else
            {
                //GameObject polozajDlana = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //polozajDlana.transform.position = Leap2UnityVector(hand.PalmPosition);

                NoviVektor1 = hand.PalmPosition;

                Vector3 vec = new Vector3((NoviVektor1.x - PocetniVektor.x)/2, (NoviVektor1.y - PocetniVektor.y) / 2, (NoviVektor1.z - PocetniVektor.z) / 2);

                Debug.Log(PocetniVektor + " " + NoviVektor1 + " " + vec);

                objekt1.transform.position = Leap2UnityVector(hand.PalmPosition);
                objekt1.transform.SetParent(null);               
                NoviVektor = objekt1.transform.position;

                Debug.Log(Vector3.Distance(new Vector3(PocetniVektor.x, PocetniVektor.y, PocetniVektor.z), NoviVektor));

                PocetniVektor.x = NoviVektor.x;
                PocetniVektor.y = NoviVektor.y;
                PocetniVektor.z = NoviVektor.z;

                //polozajDlana.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                //zgloboviIKosti.Add(objekt1);
            }


            //Color bojaZgloba;

            //if (!localData)
            //{
            //    // Podaci preko mreže s drugog LEAP uređaja --> PUNA BOJA

            //    bojaZgloba = hand.IsLeft ? Color.red : Color.blue;
            //}
            //else
            //{
            //    // Podaci s lokalnog uređaja priključenog na računalo

            //    // bojaZgloba = hand.IsLeft ? new Color(250, 128, 114) : new Color(65, 105, 225);
            //    bojaZgloba = hand.IsLeft ? new Color(0.98F, 0.5F, 0.44F) : new Color(0.25F, 0.41F, 0.88F);
            //    //bojaZgloba = hand.IsLeft ?  Color.red :  Color.blue;
            //}

            //// Iscrtavanje sfere (palm-a)
            //if (prikaziPolozajDlana)
            //{
            //    /* Iscrtavanje položaja dlana */
            //    GameObject polozajDlana = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    // polozajDlana.transform.position = start;
            //    polozajDlana.transform.position = Leap2UnityVector(hand.PalmPosition);
            //    polozajDlana.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            //    zgloboviIKosti.Add(polozajDlana);

            //}

            //int i = 0;
            //// foreach (Finger f in hand.Fingers)
            //for (int e = 0; e < 5; e++)
            //{
            //    Finger f = hand.Fingers[e];

            //    // TYPE_THUMB = = 0 - TYPE_INDEX = = 1 - TYPE_MIDDLE = = 2 - TYPE_RING = = 3 - TYPE_PINKY = = 4 -
            //    GameObject k = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    k.transform.position = Leap2UnityVector(f.TipPosition);
            //    k.transform.localScale = new Vector3(zglobVelicina, zglobVelicina, zglobVelicina);
            //    if (zgloboviObojani) k.GetComponent<Renderer>().material.color = bojaZgloba;

            //    zgloboviIKosti.Add(k);
            //    k.name = "Vrh prsta " + i++;

            //    for (int j = 0; j < 4; j++)
            //    {
            //        k = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //        k.transform.position = Leap2UnityVector(f.Bone((Bone.BoneType)j).PrevJoint);
            //        k.transform.localScale = new Vector3(zglobVelicina, zglobVelicina, zglobVelicina);
            //        if (zgloboviObojani) k.GetComponent<Renderer>().material.color = bojaZgloba;
            //        zgloboviIKosti.Add(k);
            //        k.name = "Zglob prethodni" + j;

            //        if ((!prikaziKostiDlana) && (j == 0)) continue;

            //        // https://forum.unity.com/threads/draw-cylinder-between-2-points.23510/
            //        Vector3 prevJoint = Leap2UnityVector(f.Bone((Bone.BoneType)j).PrevJoint);
            //        Vector3 nextJoint = Leap2UnityVector(f.Bone((Bone.BoneType)j).NextJoint);

            //        k = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            //        Vector3 pos = Vector3.Lerp(prevJoint, nextJoint, (float)0.5);
            //        k.transform.position = pos;
            //        k.transform.up = nextJoint - prevJoint;

            //        float razdaljina = Vector3.Distance(prevJoint, nextJoint);
            //        k.transform.localScale = new Vector3(zglobVelicina * debljinaPrstaNaspramZgloba, razdaljina / 2, zglobVelicina * debljinaPrstaNaspramZgloba);

            //        k.name = "Kost " + j;
            //        zgloboviIKosti.Add(k);
            //    } // for
            //} // foreach finger
        } // foreach hand
    } // metoda

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

    // uzima koordinate te podijeli ukupan zbroj koordinata sa ukupnim brojem koordinata i ispisuje u debug line
    public void ProvjeraSrednjeKoordinatePrvaMetoda()
    {
        rezultat = 0;
        foreach (float broj in ListaKoordinata)
        {
            rezultat += broj;
            brojackoordinata++;
        }

        Debug.Log("Broj svih koordinata je " + (rezultat) + " te je uzeto u obzir " + (brojackoordinata) + " koordinata");
        Debug.Log("Srednja koordinata proteklih 10 sekundi: " + (rezultat / brojackoordinata));
        vrijemeodbrojavanja = 0.0f;
    }


    /*************************************************************************/

    // https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/
    public class UDPReceive{
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
            print("UDPSend.init()");

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
