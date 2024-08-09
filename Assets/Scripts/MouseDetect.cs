using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDetect : MonoBehaviour
{
    public Vector3 mousePixel;
    public Vector3 mousePos;
    public Vector2Int mousePos2d;
    public Camera cam;
    public RenderGrid gridRenderer;
    public Vector2Int lastTarget;
    public Coroutine settle;
    // Start is called before the first frame update
    // IEnumerator Start()
    // {
    //     yield return 0;
    //     StartCoroutine(ChaseTick(0.01f));
    // }

    // Update is called once per frame
    void Update()
    {
        mousePixel = Input.mousePosition;
        mousePixel.z = -cam.transform.position.z;
        mousePos = cam.ScreenToWorldPoint(mousePixel);
        mousePos2d = new Vector2Int((int)mousePos.x,(int)mousePos.y);

        if(Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.X)){
            if((int)mousePos.x >= 0 && (int)mousePos.x < gridRenderer.gridSize && (int)mousePos.y >= 0 && (int)mousePos.y < gridRenderer.gridSize){
                StartCoroutine(MoveDrones(mousePos2d));
            } else {
                Debug.Log("Invalid Location");
            }
        }

        if(Input.GetMouseButton(0) && Input.GetKey(KeyCode.Z)){
            if(!gridRenderer.lidarSample.Contains(mousePos2d)){
                gridRenderer.lidarSample.Add(mousePos2d);
                gridRenderer.GenerateGrid();
            }
        }
        if(Input.GetMouseButton(0) && Input.GetKey(KeyCode.X)){
            if(gridRenderer.lidarSample.Contains(mousePos2d)){
                gridRenderer.lidarSample.Remove(mousePos2d);
                gridRenderer.GenerateGrid();
            }
        }
    }

    public IEnumerator MoveDrones(Vector2 cellPos)
    {
        if(settle != null){
            StopCoroutine(settle);
        }
        Debug.Log(mousePos2d);
        lastTarget = mousePos2d;
        StartCoroutine(Settle(mousePos2d, 3));
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
            drone.GetComponent<DroneMove>().MoveTo(mousePos2d);
            yield return 0;
        }
    }

    public IEnumerator Settle(Vector2 cellPos, int iterations)
    {
        iterations -= 1;
        yield return 0;
        while(gridRenderer.movingDrones > 0) yield return null;
        yield return 0;
        Debug.Log("Settling");
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
            drone.GetComponent<DroneMove>().MoveTo(cellPos);
            yield return 0;
        }
        if(iterations > 0){
            yield return new WaitForSeconds(0.1f);
            settle = StartCoroutine(Settle(cellPos, iterations));
        }
    }

    //chasetick sometimes causes drone crashes, not sure why
    public IEnumerator ChaseTick(float interval)
    {
        while(true){
            if((int)mousePos.x >= 0 && (int)mousePos.x < gridRenderer.gridSize && (int)mousePos.y >= 0 && (int)mousePos.y < gridRenderer.gridSize && (mousePos2d != lastTarget || gridRenderer.movingDrones == 0)){
                Debug.Log(mousePos2d);
                lastTarget = mousePos2d;
                foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
                    drone.GetComponent<DroneMove>().MoveTo(mousePos2d);
                }
            } else {
                Debug.Log("Invalid Location");
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
