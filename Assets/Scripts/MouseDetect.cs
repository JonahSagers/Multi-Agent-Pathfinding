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
    public Coroutine action;
    public int segments = 50;
    public float radius;
    public LineRenderer line;
    public float cooldown;
    public List<Vector2> payload;
    public string payloadString;
    public string payloadTemp;
    public UdpSocket socket;
    public int droneCount;
    public List<Vector2> targetOffsets;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return 0;
        line.positionCount =  (segments + 1);
        droneCount = 0;
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
            droneCount += 1;
        }
        radius = Mathf.Sqrt((droneCount * 5)/Mathf.PI) + 1;
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
            targetOffsets.Add(FindOffset());
            drone.GetComponent<DroneMove>().targetOffset = targetOffsets[^1];
        }
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
        //StartCoroutine(ChaseTick());
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
                if(action != null){
                    StopCoroutine(action);
                }
                action = StartCoroutine(MoveDrones(mousePos2d, 2));
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

    public IEnumerator MoveDrones(Vector2 cellPos, int iterations)
    {
        yield return 0;
        while(gridRenderer.movingDrones > 0) yield return null;
        //Debug.Log(cellPos);
        gridRenderer.ResetGrid();
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
            payload = drone.GetComponent<DroneMove>().MoveTo(cellPos, 0);
            // payloadString = "";
            // //TODO: only send the vertices required to draw the line, not every point it intersects
            // for(int i = 0; i < payload.Count; i++){
            //     payloadTemp = ((int)payload[i].x).ToString();
            //     if(payloadTemp.Length < 2){
            //         payloadTemp = '0' + payloadTemp;
            //     }
            //     payloadString += payloadTemp;
            //     payloadTemp = ((int)payload[i].y).ToString();
            //     if(payloadTemp.Length < 2){
            //         payloadTemp = '0' + payloadTemp;
            //     }
            //     payloadString += payloadTemp;
            // }
            // socket.SendData(payloadString);
        }
        if(iterations > 0){
            action = StartCoroutine(MoveDrones(cellPos, iterations - 1));
        }
        // socket.SendData("complete");
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
                //Debug.Log(mousePos2d);
                //cooldown += 20;
                if(action != null){
                    StopCoroutine(action);
                }
                action = StartCoroutine(MoveDrones(mousePos2d, 2));
            }
            yield return 0;
        }
    }

    public Vector2 FindOffset(){
        float y;
        float x;
        Vector2 checkPos;
        for(int i = 0; i < radius; i += 2){
            y = -i;
            for (x =  -i; x <= i; x += 2)
            {
                checkPos = new Vector2(x, y);
                if(!targetOffsets.Contains(checkPos)){
                    return checkPos;
                }
            }
            for (y = -i; y <= i; y += 2)
            {
                checkPos = new Vector2(x, y);
                if(!targetOffsets.Contains(checkPos)){
                    return checkPos;
                }
            }
            for (x = i; x >= -i; x -= 2)
            {
                checkPos = new Vector2(x, y);
                if(!targetOffsets.Contains(checkPos)){
                    return checkPos;
                }
            }
            for (y = i; y >= -i; y -= 2)
            {
                checkPos = new Vector2(x, y);
                if(!targetOffsets.Contains(checkPos)){
                    return checkPos;
                }
            }
        }
        Debug.Log("!!!Offset Location Failed!!!");
        return new Vector2(-1,-1);
    }
}
