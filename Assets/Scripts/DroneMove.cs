using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMove : MonoBehaviour
{
    public bool activate;
    public float speed;
    public List<Vector2> targets;
    public RenderGrid gridRenderer;
    public Vector2 currentPos;
    // Start is called before the first frame update
    void Start()
    {
        gridRenderer = GameObject.Find("Grid").GetComponent<RenderGrid>();
    }

    // Update is called once per frame
    void Update()
    {
        if(activate){
            MoveTo(new Vector2(Random.Range(0,gridRenderer.gridSize-1),Random.Range(0,gridRenderer.gridSize-1)));
        }
        if(targets.Count > 0){
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targets[0].x,targets[0].y,transform.position.z), Time.deltaTime * speed);
            if(Vector3.Distance(transform.position, new Vector3(targets[0].x,targets[0].y,transform.position.z)) == 0){
                gridRenderer.cells[targets[0]].obstructed = false;
                //there should never be a case where isUsed goes negative, but I think it happens when intersecting paths are invalid
                if(gridRenderer.cells[targets[0]].isUsed > 0){
                    gridRenderer.cells[targets[0]].isUsed -= 1;
                }
                if(targets.Count == 1){
                    gridRenderer.movingDrones -= 1;
                    transform.position = targets[0];
                }
                targets.RemoveAt(0);
            }
        } else {
            currentPos = new Vector2(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.y));
            gridRenderer.cells[currentPos].obstructed = true;
            if(gridRenderer.cells[currentPos].isUsed < 1){
                gridRenderer.cells[currentPos].isUsed += 1;
            }
        }
        
        // else {
        //     activate = true;
        // }
    }
    public void MoveTo(Vector2 cellPos)
    {
        currentPos = new Vector2(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.y));
        //remove all previous targets
        if(targets.Count > 0){
            gridRenderer.movingDrones -= 1;
        }
        while(targets.Count > 0){
            gridRenderer.cells[targets[0]].obstructed = false;
            if(gridRenderer.cells[targets[0]].isUsed > 0){
                gridRenderer.cells[targets[0]].isUsed -= 1;
            }
            targets.RemoveAt(0);
        }
        // gridRenderer.cells[new Vector2(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.y))].obstructed = false;
        // if(gridRenderer.cells[currentPos].isUsed > 0){
        //     gridRenderer.cells[new Vector2(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.y))].isUsed -= 1;
        // }
        cellPos = gridRenderer.FindNearest(cellPos, currentPos);
        if(cellPos != currentPos){
            targets = gridRenderer.FindPath(new Vector2(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.y)), cellPos);
            if(targets.Count == 0){
                StartCoroutine(RetryMove(cellPos));
            }
        }// else {
        //     gridRenderer.cells[currentPos].obstructed = true;
        //     if(gridRenderer.cells[currentPos].isUsed < 1){
        //         gridRenderer.cells[currentPos].isUsed += 1;
        //     }
        // }
        activate = false;
        return;
    }

    public IEnumerator RetryMove(Vector2 cellPos)
    {
        yield return new WaitForSeconds(0.5f);
        MoveTo(cellPos);
    }

    //purely for debug as real drones will not have collision detection
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision Detected");
    }
}
