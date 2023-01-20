using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MazeGenerator : MonoBehaviour
{

    // maze dimension variables - must be odd 
    public static int xTiles = 65; // number of tiles in each row, x axis
    public static int yTiles = 35; // number of tiles in each column, y axis

    public static float tileScale = 1.5f; // scale transform of tile block prefab

    // 2D array for each tile in the maze, element at index is true if the tile is a solid wall
    // and false if the player can walk on the tile
    private bool[,] isWallAtTile = new bool[xTiles, yTiles];

    // tile array used during pathing. if the tile is a path, the function
    // will not consider that tile as a possible direction to path out to.
    // path tile can also be regarded as visited tile
    private bool[,] isPathAtTile = new bool[xTiles, yTiles];

    public bool[,] isEnemySpawnTile = new bool[xTiles, yTiles];

    public bool mazeGenerationComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsMasterClient){ // only host should generate the maze
            setCompulsoryWalls();
            setRooms(60, 4, 7);
            setPathing();
            generateMaze();
        }
        mazeGenerationComplete = true;
    }

    private void setCompulsoryWalls(){ // function sets the walls which are always solid in the array
        for(int i = 0; i < xTiles; i++){ // loop through row tiles (columns)
            for(int j = 0; j < yTiles; j++){ // loop through column tiles (rows)

                if(i == 0 || j== 0 || i == xTiles - 1 || j == yTiles - 1 ||  // checks if tile is an outer wall of maze
                    (i % 2 == 0 && j % 2 == 0)) { // checks if tile is a connector wall
                        isWallAtTile[i, j] = true;  // set wall at coordinate to solid
                }

            }
        }
    }

    // function to add rooms to the maze
    // attempts - how many tries at creating rooms, the more attempts the more rooms will show up
    // min and max room size scale over connector tiles. a room size of 3 connects 3 connector tiles, etc.
    private void setRooms(int attempts, int minRoomSize, int maxRoomSize){
        int cTilesX = (xTiles - 2) / 2; // number of corner/connector tiles on the x axis
        int cTilesY = (yTiles - 2) / 2; // number of corner/connector tiles on the y axis
        int totalRoomsCreated = 0; // debug var - a counter for number of rooms created by the end

        // connector tile list - if true, then tile is already used in a room
        // used to prevent overlapping rooms
        bool[,] iscTileTaken = new bool[cTilesX, cTilesY];

        for(int i = 0; i < attempts; i++){ // try to create a room 'attempts' amount of times

            // randomly generate room dimensions and positions
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize);
            int roomXPos = Random.Range(0, cTilesX - roomWidth);
            int roomYPos = Random.Range(0, cTilesY - roomHeight);

            // following code decides if a room with the dimensions and positions 
            // given doesnt overlap with an already created room
            bool canCreateRoom = true;
            
            // loop through connector tiles within the room dimensions
            for(int j = roomXPos; j < roomXPos + roomWidth; j++){
                for(int k = roomYPos; k < roomYPos + roomHeight; k++){
                    // check if connector tile is not already taken by another room
                    if(iscTileTaken[j, k]){
                        canCreateRoom = false; // set flag to false if cant create room
                        break;
                    }
                }
                if(!canCreateRoom) break; // break out of loop if cant create room
            }

            // if cant create room, retry original loop - try creating a new room
            if(!canCreateRoom) continue;

            // if can create room, increment room number counter and log room details to console
            totalRoomsCreated++;
            Debug.Log("Room " + totalRoomsCreated + "|cornerTileCoords: " + roomXPos + "; " + roomYPos + "|size: " + roomWidth + "; " + roomHeight);

            // loop through connector tiles and join the connector tiles of the room with solid walls
            // remove gaps between room walls, enclosing the room
            
            // store the tiles connecting the connector tiles to form rooms as possible door tiles
            List<int> possibleDoorTilesX = new List<int>();
            List<int> possibleDoorTilesY = new List<int>();

            for(int j=roomXPos; j<roomXPos+roomWidth; j++){
                for(int k=roomYPos; k<roomYPos+roomHeight; k++){

                    iscTileTaken[j, k] = true; // set taken to true to prevent overlapping

                    // sets tiles between connector tiles to solid walls, creating an enclosed room as intended
                    // left and right walls
                    if((j==roomXPos || j==roomXPos+roomWidth-1) && k != roomYPos+roomHeight-1){
                        possibleDoorTilesX.Add(j*2+2); // add to possible tile list
                        possibleDoorTilesY.Add(k*2+3);
                        isWallAtTile[j*2+2, k*2+3] = true; 
                    }

                    // repeat for top and bottom walls
                    if((k==roomYPos || k==roomYPos+roomHeight-1) && j != roomXPos+roomWidth-1){
                        possibleDoorTilesX.Add(j*2+3);
                        possibleDoorTilesY.Add(k*2+2);
                        isWallAtTile[j*2+3, k*2+2] = true;
                    }

                    // remove the connector walls inside the room itself
                    if(j!=roomXPos && j!=roomXPos+roomWidth-1 && k!=roomYPos && k!=roomYPos+roomHeight-1){
                        isWallAtTile[j*2+2, k*2+2] = false;
                        isEnemySpawnTile[j*2+2, k*2+2] = true;
                    }
                }
            }

            // choose a random door tile from possible door tile list
            int randomDoorTile = Random.Range(0, possibleDoorTilesX.Count);
            // remove wall tile at tile
            isWallAtTile[possibleDoorTilesX[randomDoorTile], possibleDoorTilesY[randomDoorTile]] = false;

            // set door tile to path (visited) to avoid pathing algorithm from entering the room
            isPathAtTile[possibleDoorTilesX[randomDoorTile], possibleDoorTilesY[randomDoorTile]] = true;

        }
    }

    // Tile class used during pathing, refers to initialised 2d array of tiles
    private class Tile {
        public int x, y; // 2D tile coordinates on the level

        // List used during pathing, determines possible directions to continue the path
        // 0 = up, 1 = right, 2 = down, 3 = left
        public List<int> possiblePaths = new List<int>();

        // function to check if there are any possible directions to path out to from this tile
        public bool canPathOut(bool[,] isWallAtTile, bool[,] isPathAtTile){
            possiblePaths.Clear(); // reset possible path list as it could have been changed

            // check if tiles adjacent to this tile is not a wall and not an existing path
            if(!isWallAtTile[x, y-1] && !isPathAtTile[x, y-1]){ // up check
                if(!isPathAtTile[x, y-2]){
                    // if the 2nd tile up from this tile is not a path tile, enable pathing up
                    possiblePaths.Add(0); // add to possible path tiles list
                } else {
                    // if not, then create a wall in between the two path tiles (this tile and the 2nd tile up)
                    isWallAtTile[x, y-1] = true;
                }
            }

            // repeat with the other directions
            if(!isWallAtTile[x+1, y] && !isPathAtTile[x+1, y]){ // right check
                if(!isPathAtTile[x+2, y]){
                    possiblePaths.Add(1);
                } else {
                    isWallAtTile[x+1, y] = true;
                }
            }
            
            if(!isWallAtTile[x, y+1] && !isPathAtTile[x, y+1]){ // down check
                if(!isPathAtTile[x, y+2]){
                    possiblePaths.Add(2);
                } else {
                    isWallAtTile[x, y+1] = true;
                }
            }
            
            if(!isWallAtTile[x-1, y] && !isPathAtTile[x-1, y]){ // left check
                if(!isPathAtTile[x-2, y]){
                    possiblePaths.Add(3);
                } else {
                    isWallAtTile[x-1, y] = true;
                }
            }

            // if there is a possible path, return true, else return false
            return possiblePaths.Count > 0;
        }

    }

    // function used to generate the maze itself
    private void setPathing(){

        // tile list used to hold tiles which still have explorable paths
        // once the list is empty, there are no more branchable pathways
        // meaning the maze is finished
        List<Tile> tileList = new List<Tile>();

        // create the starting tile, which will be the top left tile at (1,1) excluding the wall
        Tile t = new Tile();
        t.x = 1;
        t.y = 1;

        do{ // pathing loop - repeat until tile list is empty [while(tileList.Count > 0)]
            if(t.canPathOut(isWallAtTile, isPathAtTile)){ // if current tile can path out (not a dead end)

                if (t.possiblePaths.Count > 1){ // if the current tile has more than 1 possible path,
                    // add current tile to list to recursively backtrack through once a dead end is reached (to explore other paths)
                    tileList.Add(t); 
                }

                // temp variables used to start tile x and y positions
                int cx = t.x;
                int cy = t.y;
                isPathAtTile[cx, cy] = true; // mark current tile as visited in the path array

                // select random path direction to continue the maze
                int randomIndex = Random.Range(0, t.possiblePaths.Count);
                int selectedDirection = t.possiblePaths[randomIndex];
                
                // switch on direction, choose which tile to repeat this check with based on direction
                switch(selectedDirection){
                    case 0: // up direction
                        isPathAtTile[t.x, t.y-1] = true; // set tile directly above to a path tile
                        t = new Tile();  // repeat loop with 2nd tile up
                        t.x = cx;
                        t.y = cy - 2;
                        break;
                        
                    // repeat with other directions
                    case 1: // right direction
                        isPathAtTile[t.x+1, t.y] = true;
                        t = new Tile();
                        t.x = cx + 2;
                        t.y = cy;
                        break;
                    case 2: // down direction
                        isPathAtTile[t.x, t.y+1] = true;
                        t = new Tile();
                        t.x = cx;
                        t.y = cy + 2;
                        break;
                    case 3: // left direction
                        isPathAtTile[t.x-1, t.y] = true;
                        t = new Tile();
                        t.x = cx - 2;
                        t.y = cy;
                        break;
                    default: // error
                        Debug.Log("Error while pathing!");
                        break;
                }
            } else { // if cant path out from current tile, remove it from tile list
                        // then, repeat with next tile from tile list
                        // continue until tile list is empty
                isPathAtTile[t.x, t.y] = true;
                tileList.RemoveAt(tileList.Count - 1);
                if(tileList.Count != 0){
                    t = tileList[tileList.Count - 1];
                }
            }
        } while(tileList.Count > 0); // tile list is empty conditional loop check

    }

    private void generateMaze(){ // function which generates the walls by instantiating prefabs
        for(int i = 0; i < xTiles; i++){ // loop through row tiles (columns)
            for(int j = 0; j < yTiles; j++){ // loop through column tiles (rows)
                if(isWallAtTile[i, j]){ // create wall prefab if tile is a solid wall
                    GameObject wall = PhotonNetwork.InstantiateRoomObject("CollisionBlock", new Vector2(i * tileScale, -j * tileScale), Quaternion.identity);
                } else if(isPathAtTile[i, j]){
                    // spawn points at path tiles
                    GameObject point = PhotonNetwork.InstantiateRoomObject("Point", new Vector2(i * tileScale, -j * tileScale), Quaternion.identity);
                }
            }
        }
        // spawn goal at bottom right of maze
        GameObject goal = PhotonNetwork.InstantiateRoomObject("GoalTrigger", new Vector2((xTiles - 2) * tileScale, -(yTiles - 2) * tileScale), Quaternion.identity);
    }

}
