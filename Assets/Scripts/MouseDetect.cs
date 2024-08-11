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
    public Coroutine settle;
    public int segments = 50;
    float radius;
    public LineRenderer line;
    public float cooldown;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return 0;
        line.positionCount =  (segments + 1);

        radius = gridRenderer.gridSize/10+0.5f;
        float x;
        float y;
        float angle = 20f;
        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos (Mathf.Deg2Rad * angle) * radius;

            line.SetPosition (i,new Vector3(x,y,0) );

            angle += (360f / segments);
        }
        StartCoroutine(ChaseTick());
    }

    // Update is called once per frame
    void Update()
    {
        mousePixel = Input.mousePosition;
        mousePixel.z = -cam.transform.position.z;
        mousePos = cam.ScreenToWorldPoint(mousePixel);
        mousePos2d = new Vector2Int((int)mousePos.x,(int)mousePos.y);
        transform.position = mousePos;

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
        Debug.Log(cellPos);
        gridRenderer.ResetGrid();
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
            drone.GetComponent<DroneMove>().MoveTo(cellPos, 0);
        }
        yield return null;
    }

    public IEnumerator Settle(Vector2 cellPos, int iterations)
    {
        iterations -= 1;
        yield return 0;
        while(gridRenderer.movingDrones > 0) yield return null;
        yield return 0;
        //Debug.Log("Settling");
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
            drone.GetComponent<DroneMove>().MoveTo(cellPos, 0);
            yield return 0;
        }
        if(iterations > 0){
            yield return new WaitForSeconds(0.1f);
            settle = StartCoroutine(Settle(cellPos, iterations));
        }
    }

    void FixedUpdate()
    {
        if(cooldown > 0){
            cooldown -= 1;
        }
    }

    //recalculating chasetick while there are still moving drones can cause crashes, you have to check for movingDrones == 0
    public IEnumerator ChaseTick()
    {
        while(true){
            if((int)mousePos.x >= 0 && (int)mousePos.x < gridRenderer.gridSize && (int)mousePos.y >= 0 && (int)mousePos.y < gridRenderer.gridSize && cooldown < 1 && gridRenderer.movingDrones < 1){
                Debug.Log(mousePos2d);
                //cooldown += 20;
                StartCoroutine(MoveDrones(mousePos2d));
            }
            yield return 0;
        }
    }
}
