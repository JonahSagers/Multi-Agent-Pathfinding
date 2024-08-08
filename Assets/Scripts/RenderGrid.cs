using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderGrid : MonoBehaviour
{
    public GameObject cam;
    public GameObject dronePrefab;
    public int gridSize;
    public int cellSize;
    public int droneCount;
    public Dictionary<Vector2, Cell> cells;
    public bool visualize;
    public bool regenerate;

    public List<Vector2> remainingCells;
    public List<Vector2> searchedCells;
    public List<Vector2> finalPath;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < droneCount; i++){
            Instantiate(dronePrefab);
        }
        GenerateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if(regenerate){
            GenerateGrid();
            regenerate = false;
        }
    }

    public void GenerateGrid()
    {
        cam.transform.position = new Vector3(gridSize/2,gridSize/2,-gridSize*0.9f);
        cells = new Dictionary<Vector2, Cell>();
        for(float x = 0; x < gridSize; x += cellSize){
            for(float y = 0; y < gridSize; y += cellSize){
                Vector2 pos = new Vector2(x, y);
                cells.Add(pos, new Cell(pos));
                if (Random.value >= 0.7f)
                {
                    cells[pos].obstructed = true;
                }
            }
        }
        FindPath(new Vector2(Random.Range(0,gridSize-1),Random.Range(0,gridSize-1)), new Vector2(Random.Range(0,gridSize-1),Random.Range(0,gridSize-1)));
    }
    //if things break change this to private
    public class Cell
    {
        public Vector2 position;
        //the algorithm picks the lowest value to travel along
        //so invalid points have to start at max and not zero
        public int fCost = int.MaxValue; //distance to start
        public int gCost = int.MaxValue; //distance to end
        public int hCost = int.MaxValue; //total cost
        public Vector2 connection;
        public bool obstructed;

        public Cell(Vector2 pos)
        {
            position = pos;
        }
    }

    public List<Vector2> FindPath(Vector2 startPos, Vector2 endPos)
    {
        searchedCells = new List<Vector2>();
        remainingCells = new List<Vector2> {startPos};
        finalPath = new List<Vector2>();

        Cell startCell = cells[startPos];
        startCell.gCost = 0;
        startCell.hCost = GetDistance(startPos, endPos);
        startCell.fCost = GetDistance(startPos, endPos);

        for(float x = 0; x < gridSize; x += cellSize){
            for(float y = 0; y < gridSize; y += cellSize){
                Vector2 pos = new Vector2(x, y);
                cells[pos].fCost = int.MaxValue;
                cells[pos].gCost = int.MaxValue;
                cells[pos].hCost = int.MaxValue;
            }
        }

        if(cells[startPos].obstructed || cells[endPos].obstructed){
            Debug.Log("End point obstructed");
            return remainingCells;
        }
        Debug.Log("Moving from "+startPos+" to "+endPos);
        while(remainingCells.Count > 0){
            Vector2 nextCell = remainingCells[0];
            //pos is the cell current being scanned
            foreach(Vector2 pos in remainingCells){
                Cell c = cells[pos];
                if(c.fCost < cells[nextCell].fCost || c.fCost == cells[nextCell].fCost && c.hCost == cells[nextCell].hCost){
                    nextCell = pos;
                }
            }
            remainingCells.Remove(nextCell);
            searchedCells.Add(nextCell);

            if(nextCell == endPos){
                Cell pathCell = cells[endPos];

                while(pathCell.position != startPos){
                    finalPath.Add(pathCell.position);
                    pathCell = cells[pathCell.connection];
                }
                finalPath.Add(startPos);
                finalPath.Reverse();
                return finalPath;
            }
            SearchCellNeighbors(nextCell, endPos);
        }
        Debug.Log("Path is obstructed");
        return finalPath;
    }

    private void SearchCellNeighbors(Vector2 cellPos, Vector2 endPos)
    {
        for (float x = cellPos.x - cellSize; x <= cellSize + cellPos.x; x += cellSize)
        {
            for (float y = cellPos.y - cellSize; y <= cellSize + cellPos.y; y += cellSize)
            {
                Vector2 neighborPos = new Vector2(x, y);
                if(cells.TryGetValue(neighborPos, out Cell c) && !searchedCells.Contains(neighborPos) && !cells[neighborPos].obstructed){
                    int GcostToNeighbor = cells[cellPos].gCost + GetDistance(cellPos, neighborPos);
                    if(GcostToNeighbor < cells[neighborPos].gCost){
                        Cell neighborNode = cells[neighborPos];

                        neighborNode.connection = cellPos;
                        neighborNode.gCost = GcostToNeighbor;
                        neighborNode.hCost = GetDistance(neighborPos, endPos);
                        neighborNode.fCost = neighborNode.gCost + neighborNode.hCost;

                        if(!remainingCells.Contains(neighborPos)){
                            remainingCells.Add(neighborPos);
                        }
                    }
                }
            }
        }
    }


    public int GetDistance(Vector2 pos1, Vector2 pos2)
    {
        //this is placeholder code from a tutorial, I'll replace it with my own once I have a better idea of what's needed
        Vector2Int dist = new Vector2Int(Mathf.Abs((int)pos1.x - (int)pos2.x), Mathf.Abs((int)pos1.y - (int)pos2.y));

        int lowest = Mathf.Min(dist.x, dist.y);
        int highest = Mathf.Max(dist.x, dist.y);

        int horizontalMovesRequired = highest - lowest;
        //14 is our integer stand-in for sqrt(2)
        return lowest * 14 + horizontalMovesRequired * 10;
    }

    public void OnDrawGizmos()
    {
        if(!Application.isPlaying || !visualize){
            return;
        }
        foreach(KeyValuePair<Vector2, Cell> kvp in cells){
            if(!kvp.Value.obstructed){
                Gizmos.color = new Color(255f,255f,255f,0.3f);
            } else {
                Gizmos.color = new Color(0f,0f,0f,0.3f);
            }
            if(finalPath.Contains(kvp.Key)){
                Gizmos.color = new Color(0f,255f,0f,0.3f);
            }
            Gizmos.DrawCube(kvp.Key + (Vector2)transform.position, new Vector2(cellSize, cellSize));
        }
    }
}
