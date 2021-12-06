using RosSharp.RosBridgeClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrajectoryManager : MonoBehaviour {

    private StringPublisher stringPublisher;
    private SubscriberTraiettoria subscriberTraiettoria;
    private GameObject end_effector;
    private TextMeshPro t0;
    private TextMeshPro t1;
    private List<GameObject> listaSfere;
    private CalibrazioneQRCode calibrazioneQRCode;

    private bool coroutineRunning = false;

    public void Start(){
        listaSfere = new List<GameObject>();
        end_effector = GameObject.Find("End_Effector");
        end_effector.SetActive(false);
        stringPublisher = GameObject.Find("ROSConnector").GetComponent<StringPublisher>();
        subscriberTraiettoria = GameObject.Find("ROSConnector").GetComponent<SubscriberTraiettoria>();
        t0 = GameObject.Find("calibrazione").GetComponent<TextMeshPro>();
        calibrazioneQRCode = GameObject.Find("ButtonCalibraOnPressed").GetComponent<CalibrazioneQRCode>();
        t1 = GameObject.Find("ToggleHideTrajectory").transform.GetChild(4).transform.GetChild(0).GetComponent<TextMeshPro>();
    }

    public void gestisciSfereTraiettoria(){
        if (!coroutineRunning) {
            if (listaSfere.Count == 0){
                t0.SetText("You need to generate first the trajectory");
            } else {
                if (listaSfere[1].activeSelf) {
                    t1.SetText("Show Last Trajectory");
                    foreach (GameObject go in listaSfere) {
                        go.SetActive(false);
                    }
                    end_effector.SetActive(false);
                } else {
                    t1.SetText("Hide Last Trajectory");
                    foreach (GameObject go in listaSfere){
                        go.SetActive(true);
                    }
                    end_effector.SetActive(true);
                }
            }
        } else {
            t0.SetText("Wait, a Trajectory is generating");
        }
    }

    public void pubblicaMessaggio(){
        subscriberTraiettoria.clearTrajectoryPoints();
        stringPublisher.inviaMessaggio();
        t0.SetText("Loading Trajectory");
        if (!coroutineRunning){
            if(calibrazioneQRCode.getPosizione() != new Vector3(0, 0, 0)){
                StartCoroutine(aggiornaTraiettoria());
            } else {
                t0.SetText("You need to perform a calibration first");
                return;
            }
        } else {
            t0.SetText("Wait, a Trajectory is generating");
            return;
        }
    }

    IEnumerator aggiornaTraiettoria(){
        coroutineRunning = true;
        yield return new WaitForSeconds(1f);
        List<Point> trajectory_points = new List<Point>(subscriberTraiettoria.getTrajectoryPoints());
        print(trajectory_points.Count);
        if (trajectory_points.Count == 0){
            t0.SetText("Network Error. Try Again");
            coroutineRunning = false;
            yield break;
        }
        yield return StartCoroutine(spawnSphere(trajectory_points));
        t0.SetText("Trajectory Generated");
        coroutineRunning = false;
    }

    public void cancellaSfere(List<GameObject> list) {
        foreach(GameObject go in list){
            Destroy(go);
        }
        for(int i = 0; i<list.Count; i++){
            list.RemoveAt(i);
        }
    }

    IEnumerator spawnSphere(List<Point> points){
        if (listaSfere.Count != 0){
            cancellaSfere(listaSfere);
            end_effector.SetActive(false);
        }
        for(int i=0; i < points.Count; i++){
            if (i == 0){
                yield return new WaitForSeconds(points[i].time);
            } else {
                yield return new WaitForSeconds(points[i].time - points[i-1].time);
            }
            
            //Creazione delle sfere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            listaSfere.Add(sphere);

            //Setto grandezza e colore delle sfere
            sphere.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            Color32 azzurro = new Color32(0, 102, 204, 255);
            sphere.GetComponent<Renderer>().material.SetColor("_Color", azzurro);

            //Posizione e rotazione della terna base del robot rispetto al QRCode
            Vector3 posizionePanda = calibrazioneQRCode.getPosizione();
            Vector3 rotazionePanda = calibrazioneQRCode.getRotazione();
            sphere.transform.position = posizionePanda;
            sphere.transform.eulerAngles = rotazionePanda;

            //Terna delle posizioni della traiettoria
            float pos_x = (float)points[i].positions[0];
            float pos_y = (float)points[i].positions[1];
            float pos_z = (float)points[i].positions[2];
            sphere.transform.Translate(new Vector3((-pos_y), pos_z, pos_x), Space.Self);
            
            //Terna della rotazione del punto della traiettoria
            float rot_x = (float)((points[i].positions[3] * 180) / Math.PI);
            float rot_y = (float)((points[i].positions[4] * 180) / Math.PI);
            float rot_z = (float)((points[i].positions[5] * 180) / Math.PI);
            sphere.transform.Rotate(rot_y, rot_z, rot_x, Space.Self);
            
            //Operazioni sull'ologramma dell'end_effector
            if (i == points.Count-1){
                end_effector.SetActive(true);
                end_effector.transform.position = sphere.transform.position;     
                end_effector.transform.eulerAngles = rotazionePanda;
                end_effector.transform.Rotate(new Vector3(rot_y, -rot_x, -rot_z), Space.Self);
            }

            //Disattiva la prima e l'ultima sfera
            if(i == 0 || i == points.Count-1){
                sphere.SetActive(false);
            }
        }
    }
}
