using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMove : MonoBehaviour
{
    public bool activate;
    public float speed;
    public List<Vector2> targets;
    public RenderGrid gridRenderer;
    // Start is called before the first frame update
    void Start()
    {
        gridRenderer = GameObject.Find("Grid").GetComponent<RenderGrid>();
    }

    // Update is called once per frame
    void Update()
    {
        if(activate){
            targets = gridRenderer.FindPath(new Vector2(Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(transform.position.y)), new Vector2(Random.Range(0,gridRenderer.gridSize-1),Random.Range(0,gridRenderer.gridSize-1)));
            activate = false;
        }
        if(targets.Count > 0){
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targets[0].x,targets[0].y,transform.position.z), Time.deltaTime * speed);
            if(Vector3.Distance(transform.position, new Vector3(targets[0].x,targets[0].y,transform.position.z)) == 0){
                gridRenderer.cells[targets[0]].obstructed = false;
                gridRenderer.cells[targets[0]].isUsed = false;
                targets.RemoveAt(0);
            }
        } else {
            activate = true;
        }
    }
}
