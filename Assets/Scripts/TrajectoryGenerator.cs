using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Trajectory;

public class TrajectoryGenerator : MonoBehaviour
{

    private SubscriberTraiettoria subscriberTraiettoria;

    private bool lettura = false;

    void Start()
    {
        subscriberTraiettoria = GameObject.Find("ROSConnector").GetComponent<SubscriberTraiettoria>();
    }

    void Update()
    {
        if (subscriberTraiettoria.getTrajectoryPoints().Count == 0)
        {
            
            if (!lettura)
            {
                print("Sono nel secondo if");
                List<Point> trajectory_points = subscriberTraiettoria.getTrajectoryPoints();
                foreach (Point p in trajectory_points)
                {
                    print("St per spawnare");
                    StartCoroutine(spawnSphere(p.positions, p.time));
                }
                //Genera sfere nella scena
                lettura = true;
            }
        }
    }
    IEnumerator spawnSphere(double[] array, float time){
        yield return new WaitForSeconds(time);
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3((float)array[0], (float)array[1], (float)array[2]);
    }
}