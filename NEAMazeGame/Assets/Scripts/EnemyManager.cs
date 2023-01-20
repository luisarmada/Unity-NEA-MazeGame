using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyManager : MonoBehaviour
{

    // min and max delay between enemy spawning times
    public float minSpawnDelay = 12f;
    public float maxSpawnDelay = 20f;

    public float spawnDelay = 20f;

    public int maxEnemies = 25; // enemy limit in level
    private int enemyCountInRoom = 0; // used to count for max enemies

    // set to false when an enemy is spawned, set to true after a delay
    // spawn an enemy if true
    private bool canSpawnEnemy = true;

    // used to determine possible spawn tiles - cant spawn where enemies already exist
    private bool[,] doesEnemyExistAtTile;
    private List<int> possibleEnemySpawnTilesX = new List<int>();
    private List<int> possibleEnemySpawnTilesY = new List<int>();

    // reference to MazeGenerator script component on game manager game object in scene
    public MazeGenerator mazeGeneratorScript; 

    void Awake(){
        mazeGeneratorScript = GetComponent<MazeGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        // ensure maze generation is complete before attempting to spawn enemies
        if(mazeGeneratorScript.mazeGenerationComplete){ 

            // initialise enemy spawning arrays
            doesEnemyExistAtTile = new bool[MazeGenerator.xTiles, MazeGenerator.yTiles];
            setPossibleEnemySpawnTiles();

            if(PhotonNetwork.IsMasterClient){ // master client handles enemy spawning
                if(canSpawnEnemy && enemyCountInRoom < maxEnemies && enemyCountInRoom != possibleEnemySpawnTilesX.Count){ // check to see if enemy count is not at limit
                    SpawnEnemyAtRandomRoomTile(); // spawn enemy is canSpawnEnemy is true
                    canSpawnEnemy = false; // set to false so only one enemy is spawned
                    StartCoroutine("StartEnemySpawnDelay"); // start countdown for setting canSpawnEnemy to true again
                }
            }

        }
        
    }

    IEnumerator StartEnemySpawnDelay(){ // a countdown between enemy spawn times
        // generate random spawn cooldown based on limits set
        yield return new WaitForSeconds(spawnDelay);
        canSpawnEnemy = true; // set to true, enemy is spawned in update function
    }

    void setPossibleEnemySpawnTiles(){
        // loop through tiles, check if tile is an enemy spawn tile, add to array if so
        for(int i = 0; i < MazeGenerator.xTiles; i++){ 
            for(int j = 0; j < MazeGenerator.yTiles; j++){ 
                if(mazeGeneratorScript.isEnemySpawnTile[i, j]){ 
                    possibleEnemySpawnTilesX.Add(i);
                    possibleEnemySpawnTilesY.Add(j);
                }
            }
        } 
    }

    void SpawnEnemyAtRandomRoomTile(){

        // choose random spawn tile to use
        int randomEnemyTile = Random.Range(0, possibleEnemySpawnTilesX.Count);
        int tileX = possibleEnemySpawnTilesX[randomEnemyTile];
        int tileY = possibleEnemySpawnTilesY[randomEnemyTile];

        // check if enemy exists at that tile
        if(doesEnemyExistAtTile[tileX, tileY]){
            // if true, try the function again with different spawn tile
            SpawnEnemyAtRandomRoomTile();
            return;
        }

        // else, create enemy
        enemyCountInRoom++;
        doesEnemyExistAtTile[tileX, tileY] = true; // set enemy exists at tile to true

        // spawn enemy as network object so it is replicated across the network
        GameObject enemy = PhotonNetwork.InstantiateRoomObject("Enemy", new Vector2(tileX * MazeGenerator.tileScale, -tileY * MazeGenerator.tileScale), Quaternion.identity);
        enemy.GetComponent<Enemy>().tileX = tileX;
        enemy.GetComponent<Enemy>().tileY = tileY;
    }

}
