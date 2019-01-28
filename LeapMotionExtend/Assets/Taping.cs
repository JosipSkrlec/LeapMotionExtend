using UnityEngine;

public class Taping : MonoBehaviour {

    //public varijable
    public GameObject GOVrijeme;
    public GameObject SCORE;
    public static bool check1 = false;
    public static bool check2 = false;

    // privatne varijable za vrijeme
    string TextVrijeme;
    float Vrijeme = 0.0f;
    int pocetnovrijeme = 60;
    const int tempvrijeme = 1;

    // privatne varijable za SCORE
    float pocetniScore = 0;
    int scorezaprikaz = 0;


    private void Start()
    {
        //GOVrijeme = GetComponent<TextMesh>().gameObject;
        //SCORE = GetComponent<TextMesh>().gameObject;
        SCORE.GetComponent<TextMesh>().text = "0";

    }

    // u update metodi se broji od 60 prema 0 te se provjerava svaki drugi doticaj "daske za taping". Klase TapingDesni i TapingLijevi su 
    // na objektima "daske za taping" te postavljaju public varijable na true ukoliko se dotakne...
    // nakon isteka vremena od 60 sec se ispise score na ekran korisnika.
    private void Update()
    {
        
        #region ZA VRIJEME
        Vrijeme += Time.deltaTime;

        if (Vrijeme > 1.0f)
        {
            pocetnovrijeme = pocetnovrijeme - tempvrijeme;
            TextVrijeme = pocetnovrijeme.ToString();

            if (pocetnovrijeme > 0)
            {

                if (pocetnovrijeme < 10)
                {
                    GOVrijeme.GetComponent<TextMesh>().color = new Color(255.0f,0.0f,0.0f); // unity color picker!
                }

                GOVrijeme.GetComponent<TextMesh>().text = TextVrijeme;

                Vrijeme = 0;
            }
            else
            {
                

                GOVrijeme.GetComponent<TextMesh>().color = new Color(0.0f, 0.0f, 0.0f);


                GOVrijeme.GetComponent<TextMesh>().text = "Vaš score je " + scorezaprikaz.ToString() + "!";
                SCORE.SetActive(false);
            }

        }
        #endregion

        #region ZA SCORE
        if (check1 == true && check2 == true && pocetnovrijeme > 1)
        {
            pocetniScore += 0.5f;
            scorezaprikaz = (int)pocetniScore;
            SCORE.GetComponent<TextMesh>().text = scorezaprikaz.ToString();
            Taping.check1 = false;
            Taping.check2 = false;

        }

        #endregion



    }


}
