using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;




[CustomEditor(typeof(DiamondSquareMA))]
public class DiamondSquareEditor : Editor
{

    bool showRules = false;

    bool useWeights = false;
    bool DjAvoidWalls = false;

    int corridorThickness = 2;

    int selGridConnectionType = 0;
    int selGridPathGenType = 0;
    int selGridGenType = 0;

    int randomAddCorr = 0;

    int bezierOndulation = 20;
    int deadEndOndulation = 20;

    int deadEndAmount = 0;
    int deadEndCorridorThickness = 3;

    int radius = 10;

    int width = 10;
    int height = 10;

    bool blockGeneration = false;
    string saveMapFileName = "";

    int heightDSA = 4;
    int roughnessDSA = 4;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DiamondSquareMA mainScript = (DiamondSquareMA)target;


        #region explanation

        showRules = EditorGUILayout.BeginFoldoutHeaderGroup(showRules, "Instructions");

        if (showRules)
        {
            GUILayout.TextArea("Diamond Square");
        }

        if (!Selection.activeTransform)
        {
            showRules = false;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        #endregion

        GeneralUtil.SpacesUILayout(4);


        switch (mainScript.currUiState)
        {
            case GeneralUtil.UISTATE.MAIN_ALGO:
                {
                    mainScript.allowedBack = false;

                    heightDSA = (int)EditorGUILayout.Slider(new GUIContent() { text = "Height", tooltip = "Creates a circular room in a random position on the canvas. The code will try to fit it, if nothing spawns try again or lower the size" }, heightDSA, 4, 16);
                    roughnessDSA = (int)EditorGUILayout.Slider(new GUIContent() { text = "roughness", tooltip = "Creates a circular room in a random position on the canvas. The code will try to fit it, if nothing spawns try again or lower the size" }, roughnessDSA, 1, 8);

                    if (GUILayout.Button("Generate DiamondSqaure Randomisation"))// gen something
                    {
                        var centerPoint = new Vector2Int(mainScript.pcgManager.gridArray2D[0].Length / 2, mainScript.pcgManager.gridArray2D.Length / 2);
                        AlgosUtils.RestartArr(mainScript.pcgManager.gridArray2D);
                        //AlgosUtils.DiamondSquare( heightDSA, -heightDSA ,roughnessDSA , mainScript.pcgManager.gridArray2D);


                        var sphereRoom = AlgosUtils.DrawCircle(mainScript.pcgManager.gridArray2D, centerPoint, 6, draw: true);
                        int size = mainScript.pcgManager.gridArray2D.Length   *     mainScript.pcgManager.gridArray2D[0].Length;

                        AlgosUtils.DiffLimAggregation(mainScript.pcgManager.gridArray2D, (int)(size * 0.25f), 300);

                        mainScript.rooms.Add(sphereRoom);

                        mainScript.pcgManager.Plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextBiColShade(mainScript.pcgManager.gridArray2D,-heightDSA, heightDSA);

                        mainScript.allowedForward = true;


                        AlgosUtils.SetUpTileCorridorTypesUI(mainScript.pcgManager.gridArray2D, corridorThickness);

                        mainScript.pcgManager.Plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextBiColShade(mainScript.pcgManager.gridArray2D, 0, 1, true);

                    }
                }
                break;

            case GeneralUtil.UISTATE.CA:
                {
                    mainScript.allowedForward = true;
                    mainScript.allowedBack = true;

                    GeneralUtil.CellularAutomataEditorSection(mainScript.pcgManager, mainScript.neighboursNeeded, out mainScript.neighboursNeeded);
                }
                break;

            case GeneralUtil.UISTATE.ROOM_GEN:
                {
                    mainScript.allowedBack = true;

                    List<List<Tile>> rooms;
                    if (GeneralUtil.CalculateRoomsEditorSection(mainScript.pcgManager, mainScript.minSize, out rooms, out mainScript.minSize))
                    {
                        mainScript.allowedForward = true;
                    }

                    if (rooms != null)
                    {
                        mainScript.rooms = rooms;
                    }
                }
                break;

            case GeneralUtil.UISTATE.EXTRA_ROOM_GEN:
                {
                    mainScript.allowedForward = true;
                    mainScript.allowedBack = false;

                    radius = (int)EditorGUILayout.Slider(new GUIContent() { text = "Radius of the arena", tooltip = "Creates a circular room in a random position on the canvas. The code will try to fit it, if nothing spawns try again or lower the size" }, radius, 10, 40);

                    if (GUILayout.Button(new GUIContent() { text = "Spawn one Arena" }))
                    {
                        bool success = false;

                        for (int i = 0; i < 5; i++)
                        {
                            var randomPoint = new Vector2Int(Random.Range(0 + radius + 3, mainScript.pcgManager.gridArray2D[0].Length - radius - 3), Random.Range(0 + radius + 3, mainScript.pcgManager.gridArray2D.Length - radius - 3));

                            var room = AlgosUtils.DrawCircle(mainScript.pcgManager.gridArray2D, randomPoint, radius + 2);

                            if (room != null)
                            {
                                mainScript.pcgManager.CreateBackUpGrid();
                                room = AlgosUtils.DrawCircle(mainScript.pcgManager.gridArray2D, randomPoint, radius, draw: true);

                                mainScript.pcgManager.Plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextBiColShade(mainScript.pcgManager.gridArray2D, 0, 1, true);

                                mainScript.rooms.Add(room);

                                success = true;

                                break;
                            }
                        }

                        if (!success)
                            Debug.Log($"<color=red>I tried to spawn the Room as requested 5 times but couldnt find any free space either try again or lower the size</color>");
                    }


                    GeneralUtil.SpacesUILayout(2);

                    height = (int)EditorGUILayout.Slider(new GUIContent() { text = "Height", tooltip = "" }, height, 10, 40);
                    width = (int)EditorGUILayout.Slider(new GUIContent() { text = "Widht", tooltip = "" }, width, 10, 40);

                    if (GUILayout.Button(new GUIContent() { text = "gen Room" }))
                    {

                        bool success = false;
                        for (int i = 0; i < 5; i++)
                        {
                            var randomPoint = new Vector2Int(Random.Range(0 + radius + 3, mainScript.pcgManager.gridArray2D[0].Length - radius - 3), Random.Range(0 + radius + 3, mainScript.pcgManager.gridArray2D.Length - radius - 3));

                            var squareRoom = AlgosUtils.SpawnRoom(width, height, randomPoint, mainScript.pcgManager.gridArray2D, true);

                            if (squareRoom != null)
                            {
                                mainScript.pcgManager.CreateBackUpGrid();
                                squareRoom = AlgosUtils.SpawnRoom(width, height, randomPoint, mainScript.pcgManager.gridArray2D);

                                mainScript.pcgManager.Plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextBiColShade(mainScript.pcgManager.gridArray2D, 0, 1, true);

                                mainScript.rooms.Add(squareRoom);

                                success = true;
                                break;
                            }
                        }

                        if (!success)
                            Debug.Log($"<color=red>I tried to spawn the Room as requested 5 times but couldnt find any free space either try again or lower the size</color>");

                    }
                }
                break;

            case GeneralUtil.UISTATE.PATHING:

                #region corridor making region
                //if (mainScript.pcgManager.prevGridArray2D.Count == 0)
                //{
                mainScript.allowedBack = true;

                if (mainScript.rooms.Count == 1)
                {
                    mainScript.allowedForward = true;
                    GUILayout.Label("Only one room detected, Corridor making is not possible");
                }
                else if (mainScript.rooms.Count == 2)
                {
                    GUILayout.Label("Only two rooms detected, triangulation not possible");

                    GUILayout.Label("Choose the algorithm to create the corridor");

                    GeneralUtil.SpacesUILayout(2);

                    GUILayout.BeginVertical("Box");
                    selGridPathGenType = GUILayout.SelectionGrid(selGridPathGenType, GeneralUtil.selStringPathGenType, 1);
                    GUILayout.EndVertical();

                    GeneralUtil.SpacesUILayout(2);

                    switch (selGridPathGenType)
                    {
                        case 0:   // A* pathfindind
                            mainScript.pathType = EditorGUILayout.Toggle(new GUIContent() { text = "Use Straight corridors", tooltip = "PathFinding will prioritize the creation of straight corridors" }, mainScript.pathType);
                            useWeights = EditorGUILayout.Toggle(new GUIContent() { text = "Use weights", tooltip = "" }, useWeights);
                            break;

                        case 1:   // djistra 
                            DjAvoidWalls = EditorGUILayout.Toggle(new GUIContent() { text = "Avoid Walls", tooltip = "" }, DjAvoidWalls);
                            break;
                        case 4:   // beizier 

                            bezierOndulation = (int)EditorGUILayout.Slider(new GUIContent() { text = "Curve Multiplier", tooltip = "beizeir curve thing to change" }, bezierOndulation, 10, 40);


                            GeneralUtil.SpacesUILayout(1);

                            mainScript.pathType = EditorGUILayout.Toggle(new GUIContent() { text = "Use Straight corridors", tooltip = "PathFinding will prioritize the creation of straight corridors" }, mainScript.pathType);


                            break;

                        default:
                            break;
                    }

                    if (GUILayout.Button("Connect all the rooms"))// dfor the corridor making
                    {
                        mainScript.pcgManager.CreateBackUpGrid();

                        Vector2Int tileA = mainScript.rooms[0][Random.Range(0, mainScript.rooms[0].Count - 1)].position;
                        Vector2Int tileB = mainScript.rooms[1][Random.Range(0, mainScript.rooms[1].Count - 1)].position;


                        mainScript.allowedForward = true;

                        switch (selGridPathGenType)
                        {
                            case 0:   //A* pathfingin

                                var path = AlgosUtils.A_StarPathfinding2DNorm(mainScript.pcgManager.gridArray2D, new Vector2Int(tileA.x, tileA.y), new Vector2Int(tileB.x, tileB.y), !mainScript.pathType, useWeights: useWeights, arrWeights: mainScript.pcgManager.tileCosts);

                                AlgosUtils.SetUpCorridorWithPath(path.Item1);

                                break;
                            case 1:  //dijistra

                                var pathD = AlgosUtils.DijstraPathfinding(mainScript.pcgManager.gridArray2D, new Vector2Int(tileA.x, tileA.y), new Vector2Int(tileB.x, tileB.y), DjAvoidWalls);

                                AlgosUtils.SetUpCorridorWithPath(pathD);


                                break;
                            case 2://   bfs
                                break;
                            case 3://  dfs
                                break;
                            case 4://  beizier curve

                                AlgosUtils.BezierCurvePathing(new Vector2Int(tileA.x, tileA.y), new Vector2Int(tileB.x, tileB.y), bezierOndulation, mainScript.pcgManager.gridArray2D, !mainScript.pathType);

                                break;

                            default:
                                break;
                        }


                        AlgosUtils.SetUpTileCorridorTypesUI(mainScript.pcgManager.gridArray2D, corridorThickness);

                        mainScript.pcgManager.Plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextBiColShade(mainScript.pcgManager.gridArray2D, 0, 1, true);
                    }

                }
                else if (mainScript.rooms.Count > 2)
                {

                    GUILayout.Label("Choose how to order the connection of the rooms");

                    GeneralUtil.SpacesUILayout(2);

                    GUILayout.BeginVertical("Box");
                    selGridConnectionType = GUILayout.SelectionGrid(selGridConnectionType, GeneralUtil.selStringsConnectionType, 1);
                    GUILayout.EndVertical();

                    GeneralUtil.SpacesUILayout(2);

                    GUILayout.Label("Choose the Thickness of the corridor");

                    corridorThickness = (int)EditorGUILayout.Slider(new GUIContent() { text = "Thickness of the corridor", tooltip = "How wide should the corridor be" }, corridorThickness, 2, 5);

                    GeneralUtil.SpacesUILayout(3);


                    GUILayout.Label("Choose the algorithm to that creates the corridor");


                    GeneralUtil.SpacesUILayout(2);

                    GUILayout.BeginVertical("Box");
                    selGridPathGenType = GUILayout.SelectionGrid(selGridPathGenType, GeneralUtil.selStringPathGenType, 1);
                    GUILayout.EndVertical();

                    GeneralUtil.SpacesUILayout(2);


                    switch (selGridPathGenType)
                    {
                        case 0:   // A* pathfindind
                            mainScript.pathType = EditorGUILayout.Toggle(new GUIContent() { text = "Use Straight corridors", tooltip = "PathFinding will prioritize the creation of straight corridors" }, mainScript.pathType);
                            useWeights = EditorGUILayout.Toggle(new GUIContent() { text = "Use weights", tooltip = "" }, useWeights);
                            break;

                        case 1:   // djistra 
                            DjAvoidWalls = EditorGUILayout.Toggle(new GUIContent() { text = "Avoid Walls", tooltip = "" }, DjAvoidWalls);
                            break;
                        case 4:   // beizier 

                            bezierOndulation = (int)EditorGUILayout.Slider(new GUIContent() { text = "Curve Multiplier", tooltip = "A higher multiplier is going to equal to a a more extreme curver" }, bezierOndulation, 10, 40);

                            GeneralUtil.SpacesUILayout(1);
                            mainScript.pathType = EditorGUILayout.Toggle(new GUIContent() { text = "Use Straight corridors", tooltip = "Pathfinding will prioritize the creation of straight corridors" }, mainScript.pathType);

                            break;

                        default:
                            break;
                    }


                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    GeneralUtil.SpacesUILayout(3);

                    switch (selGridConnectionType)
                    {
                        case 0:   // prims ran

                            if (mainScript.rooms.Count >= 4)
                            {
                                randomAddCorr = (int)EditorGUILayout.Slider(new GUIContent() { text = "Additional random connections", tooltip = "Add another random connection. This number dictates how many times the script is going to TRY to add a new corridor" }, randomAddCorr, 0, mainScript.rooms.Count / 2);
                                GeneralUtil.SpacesUILayout(2);
                            }
                            break;

                        case 2:

                            if (mainScript.rooms.Count >= 4)
                            {
                                randomAddCorr = (int)EditorGUILayout.Slider(new GUIContent() { text = "Additional random connections", tooltip = "Add another random connection. This number dictates how many times the script is going to TRY to add a new corridor" }, randomAddCorr, 0, mainScript.rooms.Count / 2);
                                GeneralUtil.SpacesUILayout(2);
                            }
                            break;

                        default:
                            break;
                    }


                    GeneralUtil.SpacesUILayout(1);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    GeneralUtil.SpacesUILayout(1);


                    deadEndAmount = (int)EditorGUILayout.Slider(new GUIContent() { text = "Amount of dead end corridors", tooltip = "Dead end corridors start from somewhere in the dungeon and lead to nowhere" }, deadEndAmount, 0, 5);

                    deadEndCorridorThickness = (int)EditorGUILayout.Slider(new GUIContent() { text = "Thickness of the dead end corridor", tooltip = "How wide should the corridor be" }, deadEndCorridorThickness, 3, 6);

                    deadEndOndulation = (int)EditorGUILayout.Slider(new GUIContent() { text = "Curve Multiplier for dead end", tooltip = "A higher multiplier is going to equal to a a more extreme curver" }, deadEndOndulation, 10, 40);

                    GeneralUtil.SpacesUILayout(2);
                    if (GUILayout.Button("Connect all the rooms"))// dfor the corridor making
                    {

                        mainScript.allowedForward = true;

                        mainScript.pcgManager.CreateBackUpGrid();

                        mainScript.rooms = AlgosUtils.GetAllRooms(mainScript.pcgManager.gridArray2D, true);
                        var centerPoints = new List<Vector2>();
                        var roomDict = new Dictionary<Vector2, List<Tile>>();
                        foreach (var room in mainScript.rooms)
                        {
                            roomDict.Add(AlgosUtils.FindMiddlePoint(room), room);
                            centerPoints.Add(AlgosUtils.FindMiddlePoint(room));
                        }

                        switch (selGridConnectionType)
                        {
                            case 0:
                                mainScript.edges = AlgosUtils.PrimAlgoNoDelu(centerPoints);
                                if (randomAddCorr > 0)
                                {
                                    int len = mainScript.edges.Count - 1;

                                    for (int i = 0; i < randomAddCorr; i++)
                                    {
                                        var pointA = mainScript.edges[Random.Range(0, len)].edge[0];
                                        var pointBEdgeCheck = mainScript.edges[Random.Range(0, len)];

                                        Vector3 pointB;

                                        if (pointA == pointBEdgeCheck.edge[0])
                                            pointB = pointBEdgeCheck.edge[1];
                                        else if (pointA == pointBEdgeCheck.edge[1])
                                            pointB = pointBEdgeCheck.edge[0];
                                        else
                                            pointB = pointBEdgeCheck.edge[1];


                                        Edge newEdge = new Edge(pointA, pointB);

                                        bool toAdd = true;

                                        foreach (var primEdge in mainScript.edges)
                                        {
                                            if (AlgosUtils.LineIsEqual(primEdge, newEdge))
                                            {
                                                toAdd = false;
                                                break;
                                            }
                                        }


                                        if (toAdd)
                                        {
                                            mainScript.edges.Add(newEdge);
                                        }
                                    }
                                }
                                break;

                            case 1:
                                mainScript.edges = AlgosUtils.DelunayTriangulation2D(centerPoints).Item2;
                                break;

                            case 2://ran
                                {
                                    AlgosUtils.ShuffleList(mainScript.rooms);

                                    centerPoints = new List<Vector2>();
                                    roomDict = new Dictionary<Vector2, List<Tile>>();
                                    foreach (var room in mainScript.rooms)
                                    {
                                        roomDict.Add(AlgosUtils.FindMiddlePoint(room), room);
                                        centerPoints.Add(AlgosUtils.FindMiddlePoint(room));
                                    }

                                    for (int i = 0; i < centerPoints.Count; i++)
                                    {
                                        if (i == centerPoints.Count - 1) { continue; }
                                        mainScript.edges.Add(new Edge(new Vector3(centerPoints[i].x, centerPoints[i].y, 0), new Vector3(centerPoints[i + 1].x, centerPoints[i + 1].y, 0)));
                                    }

                                    if (randomAddCorr > 0)
                                    {
                                        int len = mainScript.edges.Count - 1;

                                        for (int i = 0; i < randomAddCorr; i++)
                                        {
                                            int ranStarter = Random.Range(0, len);
                                            int ranEnder = Random.Range(0, len);

                                            if (ranStarter == ranEnder) { continue; }
                                            else if (Mathf.Abs(ranStarter - ranEnder) == 1) { continue; }
                                            else
                                            {
                                                mainScript.edges.Add(new Edge(new Vector3(centerPoints[ranStarter].x, centerPoints[ranStarter].y, 0), new Vector3(centerPoints[ranEnder].x, centerPoints[ranEnder].y, 0)));
                                            }
                                        }
                                    }
                                }
                                break;
                        }

                        switch (selGridPathGenType)
                        {
                            case 0:   //A* pathfingin

                                foreach (var edge in mainScript.edges)
                                {
                                    //use where so we get soemthing its not the wall but not necessary
                                    var tileA = roomDict[edge.edge[0]][Random.Range(0, roomDict[edge.edge[0]].Count)].position;
                                    var tileB = roomDict[edge.edge[1]][Random.Range(0, roomDict[edge.edge[1]].Count)].position;

                                    var path = AlgosUtils.A_StarPathfinding2DNorm(mainScript.pcgManager.gridArray2D, new Vector2Int(tileA.x, tileA.y), new Vector2Int(tileB.x, tileB.y), !mainScript.pathType, useWeights: useWeights, arrWeights: mainScript.pcgManager.tileCosts);

                                    AlgosUtils.SetUpCorridorWithPath(path.Item1);
                                }

                                break;
                            case 1:  //dijistra
                                foreach (var edge in mainScript.edges)
                                {
                                    var tileA = roomDict[edge.edge[0]][Random.Range(0, roomDict[edge.edge[0]].Count)].position;
                                    var tileB = roomDict[edge.edge[1]][Random.Range(0, roomDict[edge.edge[1]].Count)].position;

                                    var path = AlgosUtils.DijstraPathfinding(mainScript.pcgManager.gridArray2D, new Vector2Int(tileA.x, tileA.y), new Vector2Int(tileB.x, tileB.y), DjAvoidWalls);

                                    AlgosUtils.SetUpCorridorWithPath(path);
                                }

                                break;
                            case 2://   bfs
                                break;
                            case 3://  dfs
                                break;
                            case 4://  beizier curve
                                foreach (var edge in mainScript.edges)
                                {
                                    var tileA = roomDict[edge.edge[0]][Random.Range(0, roomDict[edge.edge[0]].Count)].position;
                                    var tileB = roomDict[edge.edge[1]][Random.Range(0, roomDict[edge.edge[1]].Count)].position;

                                    AlgosUtils.BezierCurvePathing(new Vector2Int(tileA.x, tileA.y), new Vector2Int(tileB.x, tileB.y), bezierOndulation, mainScript.pcgManager.gridArray2D, !mainScript.pathType);

                                }
                                break;

                            default:
                                break;
                        }

                        for (int i = 0; i < deadEndAmount; i++)
                        {
                            var room = mainScript.rooms[GeneralUtil.ReturnRandomFromList(mainScript.rooms)];

                            var randomTileInRoom = room[GeneralUtil.ReturnRandomFromList(room)];

                            Tile randomTileOutsideOfRoom;

                            while (true)
                            {
                                var tile = mainScript.pcgManager.gridArray2D[Random.Range(0, mainScript.pcgManager.gridArray2D.Length)][Random.Range(0, mainScript.pcgManager.gridArray2D[0].Length)];

                                if (tile.tileWeight == 0)
                                {
                                    randomTileOutsideOfRoom = tile;

                                    var tileA = randomTileOutsideOfRoom.position;
                                    var tileB = randomTileInRoom.position;

                                    AlgosUtils.BezierCurvePathing(new Vector2Int(tileA.x, tileA.y), new Vector2Int(tileB.x, tileB.y), bezierOndulation, mainScript.pcgManager.gridArray2D);

                                    break;
                                }
                            }
                        }

                        AlgosUtils.SetUpTileCorridorTypesUI(mainScript.pcgManager.gridArray2D, corridorThickness);

                        mainScript.pcgManager.Plane.GetComponent<Renderer>().sharedMaterial.mainTexture = GeneralUtil.SetUpTextBiColShade(mainScript.pcgManager.gridArray2D, 0, 1, true);
                    }
                }
                else
                {
                    GUILayout.Label("To access the corridor making function you need to\nGenerate the rooms first");
                }

                #endregion

                break;

            case GeneralUtil.UISTATE.GENERATION:
                {
                    mainScript.allowedBack = true;

                    GeneralUtil.GenerateMeshEditorSection(mainScript.pcgManager,  saveMapFileName, out saveMapFileName);
                }

                break;

            default:
                break;
        }



        if (mainScript.currUiState != GeneralUtil.UISTATE.GENERATION)
        {
            GeneralUtil.SpacesUILayout(4);

            EditorGUI.BeginDisabledGroup(mainScript.allowedBack == false);

            if (GUILayout.Button(new GUIContent() { text = "Go Back", tooltip = mainScript.allowedForward == true ? "Press this to go back one step" : "You cant go back" }))// gen something
            {
                mainScript.pcgManager.ClearUndos();
                mainScript.allowedBack = false;
                mainScript.currStateIndex--;
                mainScript.currUiState = (GeneralUtil.UISTATE)mainScript.currStateIndex;
            }

            EditorGUI.EndDisabledGroup();



            EditorGUI.BeginDisabledGroup(mainScript.allowedForward == false);

            if (GUILayout.Button(new GUIContent() { text = "Continue", tooltip = mainScript.allowedForward == true ? "Press this to continue to the next step" : "You need to finish this step to continue" }))// gen something
            {
                mainScript.pcgManager.ClearUndos();
                mainScript.allowedForward = false;
                mainScript.currStateIndex++;
                mainScript.currUiState = (GeneralUtil.UISTATE)mainScript.currStateIndex;

            }

            EditorGUI.EndDisabledGroup();
        }












    }
}
