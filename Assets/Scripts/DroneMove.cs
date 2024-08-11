using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMove : MonoBehaviour
{
    public float speed;
    public List<Vector2> targets;
    public RenderGrid gridRenderer;
    public Vector2 currentPos;
    public bool locked;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return 0;
        gridRenderer = GameObject.Find("Grid").GetComponent<RenderGrid>();
        currentPos = new Vector2(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.y));
        gridRenderer.cells[currentPos].tickObstruct.Add(-1);
    }

    // Update is called once per frame
    void Update()
    {
        if(targets.Count > 0 && !locked){
            int nextIndex = Mathf.Min(1,targets.Count-1);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targets[nextIndex].x,targets[nextIndex].y,transform.position.z), Time.deltaTime * speed);
            if(Vector2.Distance(transform.position, targets[nextIndex]) == 0){
                if(targets.Count > 1){
                    if(gridRenderer.cells[targets[0]].isUsed > 0){
                        gridRenderer.cells[targets[0]].isUsed -= 1;
                    }
                } else {
                    gridRenderer.movingDrones -= 1;
                    transform.position = targets[0];
                }
                targets.RemoveAt(0);
            }
        }
    }
    public void MoveTo(Vector2 cellPos, int offset)
    {
        currentPos = new Vector2(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.y));
        //remove all previous targets
        if(targets.Count > 0){
            gridRenderer.movingDrones -= 1;
            foreach(Vector2 c in targets){
                if(c != currentPos){
                    RenderGrid.Cell cell = gridRenderer.cells[c];
                    if(cell.isUsed > 0){
                        cell.isUsed -= 1;
                    }
                }
            }
        }
        Vector2 targetPos = gridRenderer.FindNearest(cellPos, currentPos, offset);
        if(targetPos != currentPos){
            if(targetPos != new Vector2(-1,-1)){
                targets = gridRenderer.FindPath(currentPos, targetPos, offset);
            }
            if(targets.Count == 0 || targetPos == new Vector2(-1,-1)){
                //drones without a valid path sometimes get stuck at a weird decimal position, and don't count their tile as occupied
                //this code makes it move to the nearest int while waiting for a path
                if(offset < gridRenderer.tolerance * 500){
                    MoveTo(cellPos, offset + gridRenderer.tolerance * 10);
                }
            } else {
                StartCoroutine(lockMotion(offset));
            }
        } else {
            gridRenderer.cells[currentPos].tickObstruct.Add(-1);
        }
    }

    public IEnumerator lockMotion(float duration)
    {
        gridRenderer.movingDrones += 1;
        locked = true;
        yield return new WaitForSeconds(duration/(speed*10));
        locked = false;
    }
}
