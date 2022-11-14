using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiHomeState : UiBaseState
{


    public Vector2 scrollPosition = Vector2.zero;

    private int scrollX = 0;
    private int scrollY = 0;
    private int scrollZ = 0;

    private bool scaleToggle;
    private bool ghostToggle;


    private string hover;


    public override void onExit(StateUIManager currentMenu)
    {

    }

    public override void onGUI(StateUIManager currentMenu)
    {

        GUI.Box(new Rect(5, 10, 230, 650),"");

        scrollX = (int)GUI.HorizontalSlider(new Rect(10, 25, 100, 20), scrollX, 3, 125);
        GUI.Label(new Rect(140, 20, 100, 30), "X Length: "+ scrollX);

        
        scrollY = (int)GUI.HorizontalSlider(new Rect(10, 50, 100, 20), scrollY, 3, 25);
        GUI.Label(new Rect(140, 45, 100, 30), "Y Height: " + scrollY);

        
        scrollZ = (int)GUI.HorizontalSlider(new Rect(10, 75, 100, 20), scrollZ, 3, 125);
        GUI.Label(new Rect(140, 70, 100, 30), "Z Width: " + scrollZ);

        ghostToggle = GUI.Toggle(new Rect(10, 100, 100, 30), ghostToggle, "toggle ghost");
        scaleToggle = GUI.Toggle(new Rect(10, 130, 100, 30), scaleToggle, "toggle scale");

       

        if (GUI.Button(new Rect(10, 160, 120, 30), new GUIContent("Gen TileSet 2D", "Button 1"))) 
        {
            currentMenu.DestroyAllTiles();
            currentMenu.Gen2DVolume(scrollX, scrollZ, ghostToggle, scaleToggle);
        }

        if (GUI.Button(new Rect(10, 200, 120, 30), "Gen TileSet 3D"))
        {
            currentMenu.DestroyAllTiles();
            currentMenu.Gen3DVolume(scrollZ, scrollY,scrollX, ghostToggle, scaleToggle);
        }

        if (GUI.Button(new Rect(10, 240, 120, 30), "Gen Plane"))
        {
            currentMenu.DestroyAllTiles();
            currentMenu.CreatePlane(scrollX, scrollZ);
        }

        if (GUI.Button(new Rect(10, 280, 100, 30), "Destroy"))
        {
            GeneralUitlInstance.instance.SpawnMessagePrefab("this is a test spawn for the error or info message", true);
            currentMenu.DestroyAllTiles();
        }



        hover = GUI.tooltip;

        //if (hover == "Button 1")
        //{ Debug.Log(hover); }

        GUI.Label(new Rect(10, 340, 140, 30), "Choose algo to use");

        if (GUI.Button(new Rect(10, 360, 100, 30), "L-System"))
            currentMenu.ChangeState(5);
        if (GUI.Button(new Rect(10, 400, 100, 30), "Voroni"))
            currentMenu.ChangeState(2);
        if (GUI.Button(new Rect(10, 440, 100, 30), "Perlin"))
            currentMenu.ChangeState(3);
        if (GUI.Button(new Rect(10, 480, 100, 30), "Cell Automata"))
            currentMenu.ChangeState(6);
        if (GUI.Button(new Rect(10, 520, 100, 30), "Drunk Walk"))
            currentMenu.ChangeState(7);
        if (GUI.Button(new Rect(10, 560, 100, 30), "Room Based"))
            currentMenu.ChangeState(1);
        if (GUI.Button(new Rect(10, 600, 120, 30), "Diamond Square"))
            currentMenu.ChangeState(4);







    }

    public override void onStart(StateUIManager currentMenu)
    {
    }

    public override void onUpdate(StateUIManager currentMenu)
    {

    }





}
