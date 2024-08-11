using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderGrid : MonoBehaviour
{
    public GameObject cam;
    public GameObject dronePrefab;
    public int gridSize;
    public int droneCount;
    public int tolerance;
    public Dictionary<Vector2, Cell> cells;
    public bool visualize;
    public int movingDrones;

    public List<Vector2> remainingCells;
    public List<Vector2> searchedCells;
    public List<Vector2> finalPath;

    public List<Vector2> lidarSample;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        //do no drop frames lower than 50 fps
        Time.maximumDeltaTime = 0.025f;
        for(int i = 0; i < droneCount; i++){
            Instantiate(dronePrefab);
        }
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        cam.transform.position = new Vector3(gridSize/2,gridSize/2,-gridSize*0.9f);
        cells = new Dictionary<Vector2, Cell>();
        for(float x = 0; x < gridSize; x += 1){
            for(float y = 0; y < gridSize; y += 1){
                Vector2 pos = new Vector2(x, y);
                cells.Add(pos, new Cell(pos));
                if(lidarSample.Contains(pos)){
                    cells[pos].obstructed = true;
                }
                // if (Random.value >= 0.95f){
                //     cells[pos].obstructed = true;
                // }
            }
        }
    }
    //if things break change this to private
    public class Cell
    {
        public Vector2 position;
        //the algorithm picks the lowest value to travel along
        //so invalid points have to start at max and not zero
        public int fCost = int.MaxValue; //distance to end
        public int gCost = int.MaxValue; //distance to start
        public int hCost = int.MaxValue; //total cost
        public Vector2 connection;
        public bool obstructed;
        public int isUsed; //number of drones using the tile, currently only for debugging
        public List<int> tickObstruct = new List<int> {};

        public Cell(Vector2 pos)
        {
            position = pos;
        }
    }

    public void ResetGrid()
    {
        for(float x = 0; x < gridSize; x += 1){
            for(float y = 0; y < gridSize; y += 1){
                Vector2 pos = new Vector2(x, y);
                cells[pos].tickObstruct = new List<int> {};
            }
        }
        foreach(GameObject drone in GameObject.FindGameObjectsWithTag("Drone")){
            cells[new Vector2(Mathf.RoundToInt(drone.transform.position.x),Mathf.RoundToInt(drone.transform.position.y))].tickObstruct.Add(-1);
        }
    }

    public List<Vector2> FindPath(Vector2 startPos, Vector2 endPos, int offset)
    {
        searchedCells = new List<Vector2>();
        remainingCells = new List<Vector2> {startPos};
        finalPath = new List<Vector2>();

        for(float x = 0; x < gridSize; x += 1){
            for(float y = 0; y < gridSize; y += 1){
                Vector2 pos = new Vector2(x, y);
                cells[pos].fCost = int.MaxValue;
                cells[pos].gCost = int.MaxValue;
                cells[pos].hCost = int.MaxValue;
            }
        }

        Cell startCell = cells[startPos];
        startCell.gCost = 0;
        startCell.hCost = GetDistance(startPos, endPos);
        startCell.fCost = GetDistance(startPos, endPos);

        //Debug.Log("Moving from "+startPos+" to "+endPos);
        //check all available tiles
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
                pathCell.tickObstruct.Add(-1);
                while(pathCell.position != startPos){
                    finalPath.Add(pathCell.position);
                    // pathCell.obstructed = true;
                    pathCell.tickObstruct.Add(pathCell.gCost + offset);
                    pathCell.isUsed += 1;
                    pathCell = cells[pathCell.connection];
                }
                finalPath.Add(startPos);
                finalPath.Reverse();
                return finalPath;
            }
            SearchCellNeighbors(nextCell, endPos, startPos, offset);
        }
        //Debug.Log("Path is obstructed");
        return new List<Vector2> {};
    }


    private bool CheckValid(Vector2 cellPos, Vector2 startPos, int offset)
    {
        if(cellPos == startPos){
            return true;
        }
        if(cells[cellPos].obstructed || cells[cellPos].tickObstruct.Contains(-1)){
            return false;
        }
        for(int i = 0; i < cells[cellPos].tickObstruct.Count; i += 1){
            if(Mathf.Abs((cells[cellPos].gCost + offset) - cells[cellPos].tickObstruct[i]) <= tolerance * 15 || cells[cellPos].tickObstruct[i] == -1){
                return false;
            }
        }
        return true;
    }

    public Vector2 FindNearest(Vector2 cellPos, Vector2 startPos, int offset)
    {
        if(CheckValid(cellPos, startPos, offset)){
            return cellPos;
        }
        float y;
        float x;
        Vector2 checkPos;
        //I was so scared to use four for loops because of what my comp sci teachers would say but it's much more efficient than the last method
        for(int i = 0; i < gridSize; i++){
            y = cellPos.y -i;
            for (x = cellPos.x - i; x <= cellPos.x + i; x += 1)
            {
                checkPos = new Vector2(x, y);
                if(FindFuturePath(cellPos, checkPos, startPos, offset)){
                    return checkPos;
                }
            }
            for (y = cellPos.y - i; y <= cellPos.y + i; y += 1)
            {
                checkPos = new Vector2(x, y);
                if(FindFuturePath(cellPos, checkPos, startPos, offset)){
                    return checkPos;
                }
            }
            for (x = cellPos.x + i; x >= cellPos.x - i; x -= 1)
            {
                checkPos = new Vector2(x, y);
                if(FindFuturePath(cellPos, checkPos, startPos, offset)){
                    return checkPos;
                }
            }
            for (y = cellPos.y + i; y >= cellPos.y - i; y -= 1)
            {
                checkPos = new Vector2(x, y);
                if(FindFuturePath(cellPos, checkPos, startPos, offset)){
                    return checkPos;
                }
            }
        }
        return new Vector2(-1,-1);
    }
    public bool FindFuturePath(Vector2 cellPos, Vector2 checkPos, Vector2 startPos, int offset)
    {
        if(!(cells.TryGetValue(checkPos, out Cell c) && (CheckValid(checkPos, startPos, offset) || checkPos == startPos))){
            return false;
        }
        if(Mathf.Max(cells[checkPos].tickObstruct.ToArray()) > GetDistance(startPos, checkPos)  || cells[checkPos].tickObstruct.Contains(-1)){
            return false;
        }
        // if(Vector2.Distance(checkPos, cellPos) > gridSize/10){
        //     return false;
        // }
        return true;
    }

    private void SearchCellNeighbors(Vector2 cellPos, Vector2 endPos, Vector2 startPos, int offset)
    {
        //first check if the tile is touching a wall
        //might only need to check cardinal directions, but this version is expandable to demand a wider berth
        if(!CheckValid(cellPos, startPos, offset)){
            return;
        }
        //check neighbors and add them to the queue if eligible
        for (float x = cellPos.x - 1; x <= 1 + cellPos.x; x += 1)
        {
            for (float y = cellPos.y - 1; y <= 1 + cellPos.y; y += 1)
            {
                Vector2 neighborPos = new Vector2(x, y);
                if(cells.TryGetValue(neighborPos, out Cell c)){
                    bool valid = true;
                    if((x == cellPos.x || y == cellPos.y) && !(CheckValid(neighborPos, startPos, offset) && !searchedCells.Contains(neighborPos))){
                        valid = false;
                    } else if((x != cellPos.x && y != cellPos.y) && (!CheckValid(new Vector2(x, cellPos.y), startPos, offset) || !CheckValid(new Vector2(cellPos.x, y), startPos, offset))){
                        valid = false;
                    }
                    if(valid){
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
            if(kvp.Value.isUsed > 0){
                Gizmos.color = new Color(0f,255f,0f,0.3f);
            }
            Gizmos.DrawCube(kvp.Key + (Vector2)transform.position, new Vector2(1, 1));
        }
    }
}
