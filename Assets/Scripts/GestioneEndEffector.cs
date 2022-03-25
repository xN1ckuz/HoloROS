using UnityEngine;
using TMPro;
using RosSharp.RosBridgeClient;
using UnityEngine.SceneManagement;
using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System;

/*
     Andare in AggiornamentoRobot e disattivare il robot
     Andare in TrajectoryManager e disattivare end Effector
     Togliere il visualizza se la calibrazione non e' avvenuta
     Vedere che topic mettere in ROSCOnnector-> PosePublisher
     */
//Metodo per trovare gameObject non attivi: https://www.unity3dtutorials.it/2018/07/13/trovare-gameobjects-non-attivi/

public class GestioneEndEffector : MonoBehaviour {

    private GameObject panda;
    private GameObject end_effector;
    private TextMeshPro calibrazione;
    public EndEffectorPosePublisher endEffectorPublisher;
    public ComandoMovimentoPublisher comandoPublisher;
    private GameObject pannello;
    private bool coroutineRunning = false;
    private bool coroutineAttendi= false;
    private AggiornamentoRobot aggiornaRobot;
    private TrajectoryManager trajectoryManager;
    private CalibrazioneQRCode calibrazioneQr;
    private InviaMessaggioPublisher inviaMessaggioPublisher;
    private SubscriberPosaIniziale subscriberPosa;
    public List<GameObject> listaSfere;
  
    private Vector3 posizioneFinale;
    private Quaternion rotazioneFinale;
    private Vector3 rotazioneFinaleAngoli;


    private GameObject panda_hand;

    public void Start(){
        calibrazione = GameObject.Find("calibrazione").GetComponent<TextMeshPro>();
        aggiornaRobot = GameObject.Find("Panda").GetComponent<AggiornamentoRobot>();
        trajectoryManager = GameObject.Find("ButtonTrajectoryOnPressed").GetComponent<TrajectoryManager>();
        comandoPublisher = GameObject.Find("ROSConnector").GetComponent<ComandoMovimentoPublisher>();
        panda = FindByName("panda_link0");
        end_effector = FindByName("End_Effector");
        panda_hand = FindByName("panda_hand");
        pannello = FindByName("PannelloScenaSimulazione");
        endEffectorPublisher = GameObject.Find("ROSConnector").GetComponent<EndEffectorPosePublisher>();
        inviaMessaggioPublisher = GameObject.Find("ROSConnector").GetComponent<InviaMessaggioPublisher>();
        subscriberPosa = GameObject.Find("ROSConnector").GetComponent<SubscriberPosaIniziale>();
    }
    
    //Permette di restituire sia GameObject attivi che non attivi
    static GameObject FindByName(string goName){
        GameObject go = null;
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]){
            if (obj.name.Equals(goName)){
                go = obj;
                return go;
            }
        }
        return null;
    }

    public void gestisciEndEffector(){
        if (panda.transform.position == new Vector3(0, 0, 0)){
            calibrazione.SetText("You need to perform a calibration first");
        }else{
            listaSfere = trajectoryManager.getListaSfere();
            if (listaSfere.Count != 0){
                calibrazione.SetText("A trajectory has already been generated, delete it");
            }else{
                if (!coroutineAttendi){
                    StartCoroutine(couroutineAttendiRisposta());
                } else{
                calibrazione.SetText("Wait..");
                }
            }        
        }
    }
    

    //chiedo a ros di darmi la posizione
    IEnumerator couroutineAttendiRisposta(){
        coroutineAttendi = true;
        end_effector.transform.localPosition = new Vector3(0, 0, 0);
        Quaternion quaternione = Quaternion.Euler(0, 0, 0);
        end_effector.transform.localRotation = quaternione;
        end_effector.GetComponent<ObjectManipulator>().enabled = true;
        inviaMessaggioPublisher.inviaMessaggio();
        calibrazione.SetText("Wait a few seconds..");
        yield return new WaitUntil(() => (subscriberPosa.isRicevuto()));
        inviaMessaggioPublisher.inviaMessaggio();
        yield return new WaitUntil(() => (subscriberPosa.isRicevuto()));
        visualizza();
        calibrazione.SetText("Now You can move the end effector");
        end_effector.GetComponent<ObjectManipulator>().enabled = true;
        Vector3 posizioneRicevuta = subscriberPosa.getPosition();
        end_effector.transform.Translate(posizioneRicevuta, Space.Self);
        Quaternion rotazioneRicevuta = subscriberPosa.getRotation();
        end_effector.transform.localRotation = rotazioneRicevuta;
        
        //calibrazione.SetText("posizione chiesta " + getPosizioneChiesta());
        
        //AGGIUNTO
        /*Quaternion q_1 = new Quaternion(0.7071f, 0f, 0.7071f, 0f);
        float w_finale = ((q_1.w * rotazioneRicevuta.w) - ((q_1.x * posizioneRicevuta.x) + (q_1.y * posizioneRicevuta.y) + (q_1.z * rotazioneRicevuta.z)));
        Vector3 n1_e2 = new Vector3(0f, 0f, 0f);
        n1_e2.x = q_1.w * rotazioneRicevuta.x;
        n1_e2.y = q_1.w * rotazioneRicevuta.y;
        n1_e2.z = q_1.w * rotazioneRicevuta.z;
        Vector3 n2_e1 = new Vector3(0f, 0f, 0f);
        n2_e1.x = rotazioneRicevuta.w * q_1.x;
        n2_e1.y = rotazioneRicevuta.w * q_1.y;
        n2_e1.z = rotazioneRicevuta.w * q_1.z;
        Vector3 prodottoVettoriale = new Vector3(0f, 0f, 0f);
        prodottoVettoriale.x = ((q_1.y * rotazioneRicevuta.z) - (q_1.z * rotazioneRicevuta.y));
        prodottoVettoriale.y = ((q_1.z * rotazioneRicevuta.x) - (q_1.x * rotazioneRicevuta.z));
        prodottoVettoriale.z = ((q_1.x * rotazioneRicevuta.y) - (q_1.y * rotazioneRicevuta.x));
        Vector3 vettore_x_y_z_finale = new Vector3(0f, 0f, 0f);
        vettore_x_y_z_finale.x = n1_e2.x + n2_e1.y + prodottoVettoriale.x;
        vettore_x_y_z_finale.y = n1_e2.y + n2_e1.y + prodottoVettoriale.y;
        vettore_x_y_z_finale.z = n1_e2.z + n2_e1.z + prodottoVettoriale.z;
        Quaternion q_risultato = new Quaternion(vettore_x_y_z_finale.x, vettore_x_y_z_finale.y, vettore_x_y_z_finale.z, w_finale);
        end_effector.transform.localRotation = q_risultato;*/
        //FINE AGGIUNTO
        endEffectorPublisher.inviaMessaggio();
        coroutineAttendi = false;
    }


    private void visualizza(){
        //aggiornaRobot.attivaRobot();
        end_effector.SetActive(true);
    }


    private void nascondi(){
        //aggiornaRobot.disattivaRobot();
        end_effector.SetActive(false);
    }

  
    


    public void gestione(){
        if (!coroutineRunning){
            end_effector.GetComponent<ObjectManipulator>().enabled = false;
            StartCoroutine(gestioneCoroutine());
        }else{
            calibrazione.SetText("Una coroutine e' gia' in esecuzione");
        }
    }

    IEnumerator gestioneCoroutine(){
        coroutineRunning = true;
        SceneManager.LoadScene("ScenaConfermaPosizione", LoadSceneMode.Additive);
        pannello.SetActive(false);
        SceltaSimulazione.Start();
        yield return new WaitUntil(() => !SceltaSimulazione.scelta.Equals(""));
        SceneManager.UnloadSceneAsync("ScenaConfermaPosizione");
        if (SceltaSimulazione.scelta.Equals("si")){
            calibrazione.SetText("Confirmed position, I'm generating the trajectory.");
            endEffectorPublisher.inviaMessaggio();
            yield return new WaitForSeconds(2f);
            trajectoryManager.pubblicaMessaggio();
        }else{
            calibrazione.SetText("Position not confirmed.");
            riposizionaEndEffector();
        }
        pannello.SetActive(true);
        coroutineRunning = false;
    }


    public void riposizionaEndEffector(){
        end_effector.transform.localPosition = new Vector3(0, 0, 0);
        Quaternion quaternione = Quaternion.Euler(0, 0, 0);
        end_effector.transform.localRotation = quaternione;
        end_effector.GetComponent<ObjectManipulator>().enabled = true;
        nascondi();
    }

    public void inivaComando()
    {
        listaSfere = trajectoryManager.getListaSfere();
        if (listaSfere.Count != 0){
            calibrazione.SetText("Wait a few second...");
            comandoPublisher.inviaMessaggio();
            StartCoroutine(gestioneSfere());
        } else{
            calibrazione.SetText("You need to generate first the trajectory." +
                        "Move the end effector to generate a trajectory.");
        }
    }

    IEnumerator gestioneSfere(){
        int tempoFinale = 20;
        calibrazione.SetText("Wait a few second...");
        List<GameObject> list = trajectoryManager.getListaSfere();
        foreach (GameObject go in list){
            if (tempoFinale != 0){
                Destroy(go);
                tempoFinale = tempoFinale - 1;
                yield return new WaitForSeconds(1f);
            }
        }
        calibrazione.SetText("Movement Completed");
        end_effector.SetActive(false);
        list.Clear();
    }

}

