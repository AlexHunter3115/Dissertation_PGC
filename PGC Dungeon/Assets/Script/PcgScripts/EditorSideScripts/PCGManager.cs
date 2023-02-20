using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Unity.VisualScripting.Metadata;
using Random = UnityEngine.Random;

public class PCGManager : MonoBehaviour
{
    public Dictionary<Tile.TileType, float> tileTypeToCostDict = new Dictionary<Tile.TileType, float>();

    [Tooltip("Test material given to draw the mesh generated variant of the outcome")]
    public Material mat;

    public Tile[][] gridArray2D = new Tile[1][];

    //public BasicTile[][] prevGridArray2D = new BasicTile[1][];

    public List<Tile[][]> prevGridArray2D = new List<Tile[][]>();
    private int maxBackUps = 3;


    private GameObject plane;
    public GameObject Plane
    {
        get { return plane; }
    }

    [Tooltip("How wide the drawing canvas where the algorithms will take place will be")]
    [Range(100, 1000)]
    public int width = 125;

    [Tooltip("How tall the drawing canvas where the algorithms will take place will be")]
    [Range(100, 1000)]
    public int height = 125;

    [Tooltip("How tall the dungeon will be.")]
    [Range(3f, 8f)]
    public int RoomHeight = 4;


    public enum MainAlgo
    {
        VORONI = 0,
        RANDOM_WALK = 1,
        ROOM_GEN = 2,
        CELLULAR_AUTOMATA = 3,
        L_SYSTEM = 4,
        DELUNARY = 5,
        WFC = 6,
        PERLIN_NOISE = 7,
        PERLIN_WORM = 8,
        DIAMOND_SQUARE = 9,
        MANUAL_EDITOR = 10
    }


    [Space(30)]
    [Tooltip("The main algorithm to start with, this depends on the type of dungeons prefered")]
    public MainAlgo mainAlgo;

    [Space(30)]
    [Tooltip("Name of file of the Rule that contains the tiles")]
    public string TileSetRuleFileName = "";


    public List<TileRuleSetPCG> FloorTiles = new List<TileRuleSetPCG>();
    public List<TileRuleSetPCG> CeilingTiles = new List<TileRuleSetPCG>();
    public List<TileRuleSetPCG> WallsTiles = new List<TileRuleSetPCG>();
 

    [Space(30)]
    [Tooltip("Name of file of the Rule that contains the tiles")]
    public string WeightRuleFileName = "";
    public float[] tileCosts = new float[0];


    [Header("THIS IS WHERE THE PLAYER GOES IN CASE OF TILESET GEN")]
    public List<GameObject> player = new List<GameObject>();

    [HideInInspector]
    public List<Chunk> chunks;


    private int chunkWidth = 10;
    public int ChunkWidth
    {
        get { return chunkWidth; }
        set { chunkWidth = value; }
    }


    private int chunkHeight = 10;
    public int ChunkHeight
    {
        get { return chunkHeight; }
        set { chunkHeight = value; }
    }

    [HideInInspector]
    public int CLength;
    [HideInInspector]
    public int CHeight;



    private int currMainAlgoIDX = 10;
    public int CurrMainAlgoIDX
    {
        get { return currMainAlgoIDX; }
    }

  

    private void Update()
    {
        if (chunks.Count > 0) 
        {
            CheckChunkRender();
        }
    }


    #region undo Button
    public void CreateBackUpGrid() 
    {
        var prevGridArray2Dstack = new Tile[gridArray2D.Length][];
        for (int y = 0; y < gridArray2D.Length; y++)
        {
            prevGridArray2Dstack[y] = new Tile[gridArray2D[0].Length];
            for (int x = 0; x < gridArray2D[0].Length; x++)
            {
                prevGridArray2Dstack[y][x] = new Tile(gridArray2D[y][x]);
            }
        }
        prevGridArray2D.Add(prevGridArray2Dstack);

        if (prevGridArray2D.Count >= maxBackUps+1)
            prevGridArray2D.RemoveAt(0);
    }

    public bool LoadBackUpGrid() 
    {
        if (prevGridArray2D.Count == 0)
            return false;

        var prevGridArray2DList = prevGridArray2D[prevGridArray2D.Count -1];

        gridArray2D = new Tile[prevGridArray2DList.Length][];
        for (int y = 0; y < prevGridArray2DList.Length; y++)
        {
            gridArray2D[y] = new Tile[prevGridArray2DList[0].Length];
            for (int x = 0; x < prevGridArray2DList[0].Length; x++)
            {
                gridArray2D[y][x] = new Tile(prevGridArray2DList[y][x]);
            }
        }

        prevGridArray2D.RemoveAt(prevGridArray2D.Count - 1);

        Plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextBiColShade(gridArray2D, 0, 1, true);

        return true;
    }


    public void ClearUndos() => prevGridArray2D.Clear();


    #endregion


    #region algo Managment
    public void CreatePlane()
    {
        RefreshPlane();

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

        plane.transform.position = Vector3.zero;
        plane.transform.parent = this.transform;

        plane.transform.localScale = new Vector3(width / 4, 1, height / 4);

        gridArray2D = new Tile[height][];

        for (int y = 0; y < height; y++)
        {
            gridArray2D[y] = new Tile[width];

            for (int x = 0; x < width; x++)
            {
                gridArray2D[y][x] = new Tile();
                gridArray2D[y][x].position = new Vector2Int(x, y);
                gridArray2D[y][x].tileType = Tile.TileType.VOID;
            }
        }

    }

    public void RefreshPlane()
    {
        if (plane != null)
            DestroyImmediate(plane);
    }

    public void LoadMainAlgo()
    {
        DelPrevAlgo();

        currMainAlgoIDX = (int)mainAlgo;





        if ((int)mainAlgo == 0)
        {
            var comp = this.transform.AddComponent<VoronoiMA>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 1)
        {
            var comp = this.transform.AddComponent<RandomWalkMA>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 2)
        {
            var comp = this.transform.AddComponent<RanRoomGenMA>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 3)
        {
            var comp = this.transform.AddComponent<CellularAutomataMA>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 4)
        {
            var comp = this.transform.AddComponent<NewLSystem>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 5)
        {
            var comp = this.transform.AddComponent<DelunaryMA>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 6)
        {
            this.transform.AddComponent<WFCRuleDecipher>();


            var comp = this.transform.AddComponent<NewWFCAlog>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 7)
        {
            var comp = this.transform.AddComponent<PerlinNoiseMA>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 8)
        {
            var comp = this.transform.AddComponent<PerlinWormsMA>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 9)
        {
            var comp = this.transform.AddComponent<DiamondSquareMA>();
            comp.InspectorAwake();
        }
        else if ((int)mainAlgo == 10)
        {
            var comp = this.transform.AddComponent<LoadMapMA>();
            comp.InspectorAwake();
        }
        else
        {
            Debug.Log($"There was an issue with this setting");
        }


    }

    public void DelPrevAlgo()
    {
        switch (currMainAlgoIDX)
        {
            case 0:
                DestroyImmediate(this.transform.GetComponent<VoronoiMA>());
                break;
            case 1:

                DestroyImmediate(this.transform.GetComponent<RandomWalkMA>());

                break;
            case 2:
                DestroyImmediate(this.transform.GetComponent<RanRoomGenMA>());
                break;
            case 3:
                DestroyImmediate(this.transform.GetComponent<CellularAutomataMA>());
                break;
            case 4:
                DestroyImmediate(this.transform.GetComponent<NewLSystem>());
                break;
            case 5:
                DestroyImmediate(this.transform.GetComponent<DelunaryMA>());
                break;
            case 6:
                DestroyImmediate(this.transform.GetComponent<NewWFCAlog>());
                DestroyImmediate(this.transform.GetComponent<WFCRuleDecipher>());

                foreach (Transform child in transform)
                    DestroyImmediate(child.gameObject);

                break;
            case 7:
                DestroyImmediate(this.transform.GetComponent<PerlinNoiseMA>());
                break;
            case 8:
                DestroyImmediate(this.transform.GetComponent<PerlinWormsMA>());
                break;
            case 9:
                DestroyImmediate(this.transform.GetComponent<DiamondSquareMA>());
                break;
            case 10:
                DestroyImmediate(this.transform.GetComponent<LoadMapMA>());
                break;

            default:
                break;
        }


        currMainAlgoIDX = 11;

    }

    public void Restart()
    {
        AlgosUtils.RestartArr(gridArray2D);
        CreateBackUpGrid();
        Plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextBiColAnchor(gridArray2D);

        prevGridArray2D.Clear();
    }



    #endregion

    public void TestFunc()
    {
        GeneralUtil.SetUpColorBasedOnType(gridArray2D);


        plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextSelfCol(gridArray2D);
    }



    #region Generation area
    private void CheckChunkRender()
    {
        var indexesToDraw = new HashSet<int>();


        foreach (var player in player)
        {
            int collidedIndex = -1;
            for (int i = 0; i < chunks.Count; i++)
            {
                if (AABBCol(player.transform.position, chunks[i]))
                {
                    collidedIndex = i;
                    break;
                }
            }


            if (collidedIndex == -1)
            {
                Debug.Log($"The player is out of bounds");
            }
            else
            {
                indexesToDraw.Add(collidedIndex);


                if (collidedIndex - 1 > 0)
                    indexesToDraw.Add(collidedIndex - 1);  //left

                if (collidedIndex + 1 < chunks.Count)
                    indexesToDraw.Add(collidedIndex + 1);  //right


                if (collidedIndex + CLength < chunks.Count)
                    indexesToDraw.Add(collidedIndex + CLength);  //up 

                if (collidedIndex - CLength > 0)
                    indexesToDraw.Add(collidedIndex - CLength);  //down

                if (collidedIndex + CLength < chunks.Count)
                    indexesToDraw.Add(collidedIndex + CLength);
                if (collidedIndex - CLength < chunks.Count)
                    indexesToDraw.Add(collidedIndex - CLength);


                if (collidedIndex - CLength + 1 > 0)
                    indexesToDraw.Add(collidedIndex - CLength + 1);
                if (collidedIndex + CLength - 1 > 0)
                    indexesToDraw.Add(collidedIndex + CLength - 1);
                if (collidedIndex + CLength + 1 < chunks.Count)
                    indexesToDraw.Add(collidedIndex + CLength + 1);
                if (collidedIndex - CLength - 1 > 0)
                    indexesToDraw.Add(collidedIndex - CLength - 1);

            }
        }

        foreach (var chunk in chunks)
        {
            if (indexesToDraw.Contains(chunk.index))
            {
                chunk.mainParent.SetActive(true);
            }
            else
            {
                chunk.mainParent.SetActive(false);
            }
        }
    }

    /// <summary>
    /// returns true if it collides
    /// </summary>
    /// <param name="player"></param>
    /// <param name="chunk"></param>
    /// <returns></returns>
    private bool AABBCol(Vector3 player, Chunk chunk)
    {
        if (player.x >= chunk.bottomLeft.x && player.x < chunk.topRight.x)
        {
            if (player.z >= chunk.bottomLeft.y && player.z < chunk.topRight.y)
            {
                return true;
            }
        }

        return false;
    }

    public void FormObject(Mesh mesh)
    {
        GameObject newPart = new GameObject();
        newPart.transform.position = this.transform.position;
        newPart.transform.rotation = this.transform.rotation;
        newPart.transform.localScale = this.transform.localScale;

        var renderer = newPart.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;

        var filter = newPart.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        var collider = newPart.AddComponent<MeshCollider>();
        collider.convex = false;
    }

    private int RatioBasedChoice(List<TileRuleSetPCG> objects) 
    {
        int totRatio = 0;

        foreach (var obj in objects) 
        {
            totRatio += obj.occurance;
        }

        int ranNum = Random.Range(1, totRatio);

        int countRatio = 0;
        int savedIdx = 0;
        for (int i = 1; i < objects.Count; i++)
        {
            if (ranNum > countRatio && ranNum <= countRatio + objects[i].occurance) 
            {
                savedIdx = i;
                break;
            }

            countRatio += objects[i].occurance;
        }

        return savedIdx;
    }

    public void ChunkCreate(int height, int width)
    {
        int maxWidth = gridArray2D[0].Length;
        int maxHeight = gridArray2D.Length;

        Vector2Int BLhead = Vector2Int.zero;
        Vector2Int TRhead = Vector2Int.zero;

        int correctHeight = (maxHeight - 1) - TRhead.y >= height ? height : (maxHeight - 1) - TRhead.y;
        TRhead = new Vector2Int(0, TRhead.y + correctHeight);

        int timerCheck = 0;

        chunks = new List<Chunk>();
        while (true)
        {

            if (TRhead.x + 1 >= maxWidth)  // needs to go in the new line
            {
                if (TRhead.y + 1 >= maxHeight)  // this checks if we are dont with the algo
                {
                    break;
                }

                BLhead = new Vector2Int(0, TRhead.y);

                correctHeight = (maxHeight - 1) - TRhead.y >= height ? height : (maxHeight - 1) - TRhead.y;

                TRhead = new Vector2Int(0, TRhead.y + correctHeight + 1);
                timerCheck = 0;
            }
            else
            {
                timerCheck++;
                int correctWidth = (maxWidth - 1) - TRhead.x >= width ? width : (maxWidth - 1) - TRhead.x;

                TRhead = new Vector2Int(TRhead.x + correctWidth + 1, TRhead.y);

                chunks.Add(new Chunk() { width = correctWidth, height = correctHeight });

                var currChunk = chunks[chunks.Count - 1];
                currChunk.topRight = TRhead;
                currChunk.bottomLeft = BLhead;
                currChunk.index = chunks.Count - 1;

                BLhead = new Vector2Int(TRhead.x, BLhead.y);
            }
        }

        for (int i = 0; i < chunks.Count; i++)
        {
            var objRef = new GameObject();
            objRef.transform.parent = this.transform;
            objRef.transform.name = i.ToString();
            objRef.isStatic = true;
            chunks[i].mainParent = objRef;

            int widthChunk = chunks[i].topRight.x - chunks[i].bottomLeft.x;
            int heightChunk = chunks[i].topRight.y - chunks[i].bottomLeft.y;

            for (int y = 0; y < heightChunk; y++)
            {
                for (int x = 0; x < widthChunk; x++)
                {
                    gridArray2D[y + chunks[i].bottomLeft.y][x + chunks[i].bottomLeft.x].idx = chunks[i].index;
                }
            }
        }

        CLength = timerCheck;

    }

    public void DrawTileMapDirectionalWalls()
    {
        int iter = 0;
        ChunkCreate(chunkWidth, chunkHeight);

        if (WallsTiles.Count == 0 || CeilingTiles.Count == 0 || FloorTiles.Count == 0)
        {
            EditorUtility.DisplayDialog("Invalid tile Rules given", "Please make sure you have loaded all of the tile object correctly and all the 3 lists have at least one object in them to use this Generation method", "OK!");
            return;
        }

        for (int z = 0; z < RoomHeight; z++)  // this is the heihgt of the room
        {
            for (int y = 0; y < gridArray2D.Length; y++)
            {
                for (int x = 0; x < gridArray2D[0].Length; x++)
                {  
                    if (z == 0 || z == RoomHeight - 1) //we draw everything as this is the ceiling and the floor       THIS IS WHERE THE CEILING SHOULD BE
                    {
                        if (gridArray2D[y][x].tileType != Tile.TileType.VOID)
                        {
                            var objRef = Instantiate(FloorTiles.Count > 1 ? FloorTiles[RatioBasedChoice(FloorTiles)].Tile : FloorTiles[0].Tile, this.transform);
                            this.transform.GetChild(0);

                            objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                            objRef.isStatic = true;
                            objRef.transform.position = new Vector3(x, z, y);
                            iter++;
                        }
                    }

                    if (gridArray2D[y][x].tileType == Tile.TileType.WALL)
                    {
                        var checkVector = new Vector2Int(x, y);

                        checkVector = new Vector2Int(x + 1, y);// right check

                        if (checkVector.x < 0 || checkVector.y < 0 || checkVector.x >= gridArray2D[0].Length || checkVector.y >= gridArray2D.Length)
                        {
                            var objRef = Instantiate(WallsTiles.Count > 1 ? WallsTiles[RatioBasedChoice(WallsTiles)].Tile : WallsTiles[0].Tile, this.transform);



                            objRef.transform.position = new Vector3(x, z, y);
                            objRef.transform.Rotate(0, 90, 0);
                            objRef.isStatic = true;
                            objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                            iter++;
                        }
                        else
                        {
                            if (gridArray2D[checkVector.y][checkVector.x].tileType == Tile.TileType.VOID)
                            {
                                var objRef = Instantiate(WallsTiles.Count > 1 ? WallsTiles[RatioBasedChoice(WallsTiles)].Tile : WallsTiles[0].Tile, this.transform);

                                objRef.transform.position = new Vector3(x, z, y);
                                objRef.transform.Rotate(0, 90, 0);
                                objRef.isStatic = true;
                                objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                                iter++;
                            }
                        }



                        checkVector = new Vector2Int(x - 1, y);// left check

                        if (checkVector.x < 0 || checkVector.y < 0 || checkVector.x >= gridArray2D[0].Length || checkVector.y >= gridArray2D.Length)
                        {
                            var objRef = Instantiate(WallsTiles.Count > 1 ? WallsTiles[RatioBasedChoice(WallsTiles)].Tile : WallsTiles[0].Tile, this.transform);

                            objRef.transform.position = new Vector3(x, z, y);
                            objRef.transform.Rotate(0, 270, 0);
                            objRef.isStatic = true;
                            objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                            iter++;
                        }
                        else
                        {
                            if (gridArray2D[checkVector.y][checkVector.x].tileType == Tile.TileType.VOID)
                            {
                                var objRef = Instantiate(WallsTiles.Count > 1 ? WallsTiles[RatioBasedChoice(WallsTiles)].Tile : WallsTiles[0].Tile, this.transform);

                                objRef.transform.position = new Vector3(x, z, y);
                                objRef.transform.Rotate(0, 270, 0);
                                objRef.isStatic = true;
                                objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                                iter++;

                            }
                        }




                        checkVector = new Vector2Int(x, y + 1);// above check

                        if (checkVector.x < 0 || checkVector.y < 0 || checkVector.x >= gridArray2D[0].Length || checkVector.y >= gridArray2D.Length)
                        {
                            var objRef = Instantiate(WallsTiles.Count > 1 ? WallsTiles[RatioBasedChoice(WallsTiles)].Tile : WallsTiles[0].Tile, this.transform);

                            objRef.transform.position = new Vector3(x, z, y);
                            objRef.isStatic = true;
                            objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                            objRef.transform.Rotate(0, 0, 0);
                            iter++;
                        }
                        else
                        {
                            if (gridArray2D[checkVector.y][checkVector.x].tileType == Tile.TileType.VOID)
                            {
                                var objRef = Instantiate(WallsTiles.Count > 1 ?      WallsTiles[RatioBasedChoice(WallsTiles)].Tile        : WallsTiles[0].Tile, this.transform);

                                objRef.transform.position = new Vector3(x, z, y);
                                objRef.transform.Rotate(0, 0, 0);
                                objRef.isStatic = true;
                                objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                                iter++;
                            }
                        }





                        checkVector = new Vector2Int(x, y - 1);// down check

                        if (checkVector.x < 0 || checkVector.y < 0 || checkVector.x >= gridArray2D[0].Length || checkVector.y >= gridArray2D.Length)
                        {
                            var objRef = Instantiate(WallsTiles.Count > 1 ? WallsTiles[RatioBasedChoice(WallsTiles)].Tile : WallsTiles[0].Tile, this.transform);

                            objRef.transform.position = new Vector3(x, z, y);
                            objRef.transform.Rotate(0, 180, 0);
                            objRef.isStatic = true;
                            objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                            iter++;
                        }
                        else
                        {
                            if (gridArray2D[checkVector.y][checkVector.x].tileType == Tile.TileType.VOID)
                            {
                                var objRef = Instantiate(WallsTiles.Count > 1 ? WallsTiles[RatioBasedChoice(WallsTiles)].Tile : WallsTiles[0].Tile, this.transform);

                                objRef.transform.position = new Vector3(x, z, y);
                                objRef.transform.Rotate(0, 180, 0);
                                objRef.isStatic = true;
                                objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);

                                iter++;
                            }
                        }
                    }
                }
            }
        }
    }



    public void DrawTileMapBlockType()
    {
        int iter = 0;
        ChunkCreate(chunkWidth, chunkHeight);

        if (WallsTiles.Count == 0 || CeilingTiles.Count == 0 || FloorTiles.Count == 0)
        {
            EditorUtility.DisplayDialog("Invalid tile Rules given", "Please make sure you have loaded all of the tile object correctly and all the 3 lists have at least one object in them to use this Generation method", "OK!");
            return;
        }

        for (int z = 0; z < RoomHeight; z++)  // this is the heihgt of the room
        {
            for (int y = 0; y < gridArray2D.Length; y++)
            {
                for (int x = 0; x < gridArray2D[0].Length; x++)
                {
                    if (z == 0) //we draw everything as this is the ceiling and the floor       THIS IS WHERE THE CEILING SHOULD BE
                    {
                        if (gridArray2D[y][x].tileType != Tile.TileType.VOID)
                        {
                            var objRef = Instantiate(FloorTiles.Count > 1 ? FloorTiles[RatioBasedChoice(FloorTiles)].Tile : FloorTiles[0].Tile, this.transform);
                            this.transform.GetChild(0);

                            objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                            objRef.isStatic = true;
                            objRef.transform.position = new Vector3(x, z, y);
                            iter++;
                        }
                    }
                    else if (z == RoomHeight - 1) 
                    {
                        if (gridArray2D[y][x].tileType != Tile.TileType.VOID)
                        {
                            var objRef = Instantiate(CeilingTiles.Count > 1 ? FloorTiles[RatioBasedChoice(CeilingTiles)].Tile : CeilingTiles[0].Tile, this.transform);
                            this.transform.GetChild(0);

                            objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                            objRef.isStatic = true;
                            objRef.transform.position = new Vector3(x, z, y);
                            iter++;
                        }
                    }
                    else 
                    {
                        if (gridArray2D[y][x].tileType == Tile.TileType.WALL)
                        {
                            var objRef = Instantiate(WallsTiles.Count > 1 ? WallsTiles[RatioBasedChoice(WallsTiles)].Tile : WallsTiles[0].Tile, this.transform);

                            objRef.transform.position = new Vector3(x, z, y);
                            objRef.isStatic = true;
                            objRef.transform.parent = this.transform.GetChild(gridArray2D[y][x].idx + 1);
                            iter++;
                        }
                    }
                }
            }
        }
    }




    #endregion
}


[Serializable]
public class Chunk
{
    public Vector2Int topRight = Vector2Int.zero;
    public Vector2Int bottomLeft = Vector2Int.zero;

    public int width;
    public int height;

    public int index = 0;
    public GameObject mainParent = null;
    public List<GameObject> listOfObjInChunk = new List<GameObject>();

}

[Serializable]
public class TilesRuleSetPCG
{
    public List<TileRuleSetPCG> FloorTiles = new List<TileRuleSetPCG>();
    public List<TileRuleSetPCG> CeilingTiles = new List<TileRuleSetPCG>();
    public List<TileRuleSetPCG> WallsTiles = new List<TileRuleSetPCG>();
}

[Serializable]
public class TileRuleSetPCG
{
    public GameObject Tile;
    public int occurance = 1;
}
