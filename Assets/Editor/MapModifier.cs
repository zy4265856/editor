﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class MapModifier
{
    CustomMap cm;
    Vector3 mapsize;
    Vector3 maplefttopcenter;
     static MapModifier instance;
    public static MapModifier Instance
    {
        get
        {
            if (instance == null)
                instance = new MapModifier();
            return instance;
        }
    }

    public CustomMap CurMap
    {
        get
        {
            return cm;
        }
    }

    public Vector3 MapSize
    {
        get
        {
            return mapsize;
        }
    }
   

public void SetCustomMap(CustomMap cmap)
    {
        cm = cmap;
    }



    public void AddUnreachableIndex(int index)
    {

    }

    public void AddUnreachableIndexs(List<int> index)
    {

    }


    public void RemoveUnreachableIndex(int index)
    {

    }

    public void AddItem(CustomItemInfo iteminfo)
    { }

    public void RemoveItemInfo()
    { }

   public Vector3 CaculateCollisionPosFromGUIPoint(Vector2 guipoint)
    {
        var mousepos = Event.current.mousePosition;
        Ray screenTo = HandleUtility.GUIPointToWorldRay(mousepos);// SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousepos);
        Vector3 pos = new Vector3();
        RaycastHit hitinfo;
        if (Physics.Raycast(screenTo, out hitinfo))
        {
            pos = hitinfo.point;
        }
        return pos;
    }

    public Vector3 CaculateGameObjectSize(int selected)
    {
        int xlength = 1;
        int ylength = 1;
        int zlength = 1;
        if (selected != null)
        {
            var obj = ResourceCenter.Instance.prefabObjects[selected];
            //var size = obj.GetComponent<Collider>().bounds.size;
            var size = obj.GetComponent<Renderer>().bounds.size;
            xlength = (int)Mathf.Ceil(size.x / cm.unitlength);
            zlength = (int)Mathf.Ceil(size.z / cm.unitlength);
            ylength = (int)Mathf.Ceil(size.y / cm.unitlength);
        }
        return new Vector3(xlength, ylength, zlength);
    }

    public Vector3 CaculateCreateGameObjectCenter(Vector3 pos, Vector3 size)
    {
        int xlength = (int)size.x;
        int zlength = (int)size.z;
        Vector3 center = new Vector3();
        center.x = pos.x + (float)(xlength / 2f - 0.5) * cm.unitlength;
        center.y = pos.y;
        center.z = pos.z - (float)(zlength / 2f - 0.5) * cm.unitlength;
        return center;
    }

    public Vector3 CaculateCellCenterByPos(Vector3 pos)
    {
        Vector3 lefttop = new Vector3(cm.center.x - cm.mapwidth / 2.0f, 0, cm.center.z + cm.mapheight / 2.0f);
        int rank = (int)Mathf.Ceil((pos.x - lefttop.x) / (float)cm.unitlength);
        int row = (int)Mathf.Ceil(Mathf.Abs(pos.z - lefttop.z) / (float)cm.unitlength);
        // int index = (row - 1) * cm.mapwidth / cm.unitlength + (rank);
        var indexUnitpos = new Vector3(rank * cm.unitlength + lefttop.x - cm.unitlength / 2.0f, pos.y, lefttop.z - row * cm.unitlength + cm.unitlength / 2.0f);
        Debug.Log(indexUnitpos + "index");
        return indexUnitpos;
    }

    public int CaculateIndexForPos(Vector3 pos)
    {
        Vector3 lefttop = new Vector3(cm.center.x - cm.mapwidth / 2.0f, 0, cm.center.z + cm.mapheight / 2.0f);
        int rank = (int)Mathf.Ceil((pos.x - lefttop.x) / (float)cm.unitlength);
        int row = (int)Mathf.Ceil(Mathf.Abs(pos.z - lefttop.z) / (float)cm.unitlength);
        int index = (row - 1) * cm.mapwidth / cm.unitlength + (rank - 1);
        return index;
    }

    public bool CheckContainUnreachable(int siteindex, Vector3 size)
    {
        int xlength = (int)size.x / cm.unitlength;
        int zlength = (int)size.z / cm.unitlength;
        int num = xlength * zlength;
        var unreachable = cm.unreachable;
        for (int i = 0; i < num; ++i)
        {
            int index = siteindex + (i / xlength) * cm.mapwidth / cm.unitlength + i % xlength;
            if (unreachable.Contains(index))
            {
                return true;
            }
        }
        return false;
    }


    void CreateGameObject(Vector3 center, int index)
    {
        GameObject objTarget;
        objTarget = GameObject.Instantiate(ResourceCenter.Instance.prefabObjects[index]);
        if (objTarget)
            objTarget.transform.position = center;
    }

    public void AddNewItem(int posindex, Vector3 size, int itemindex)
    {
        // int index = CaculateIndexForPos(collisionPos);
        int xlength = (int)size.x;
        int zlength = (int)size.z;
        int num = (int)size.x * (int)size.z;
        for (int i = 0; i < num; ++i)
        {
            int index = posindex + (i / xlength) * cm.mapwidth / cm.unitlength + i % xlength;
            cm.unreachable.Add(index);
        }
        CustomItemInfo newitem = new CustomItemInfo();
       // newitem.type = itemtype;
        newitem.lefttopsite = posindex;
        newitem.prefab = ResourceCenter.Instance.prefabObjects[itemindex];
        cm.itemlist.Add(newitem);
    }

    public void AddObject(int posindex, Vector3 center,int itemindex)
    {
        e_ItemType itemtype = MapInspector.chooseType;// MapDesignerWindow.mapdesignerWind.chooseType;
        //GameObject  objTarget;
        switch (itemtype)
        {
            case e_ItemType.Tree:
            case e_ItemType.Box:
            case e_ItemType.Stone:
                //BuildOrNot(itemtype);
                CreateGameObject(center, itemindex);

                break;
            default:
                // AttempToSelect();
                break;
        }
        Debug.Log("mouse click");
    }


    public void GenerateBaseData()
    {
        mapsize = new Vector3();
        mapsize.x = cm.mapwidth / cm.unitlength;
        mapsize.y = 0;
        mapsize.z = cm.mapheight / cm.unitlength;
        maplefttopcenter = new Vector3(cm.center.x - cm.mapwidth / 2.0f + cm.unitlength/2f, 0, cm.center.z + cm.mapheight / 2.0f - cm.unitlength/2f);
        if(cm.hasGeneratedData == false)
        {
            GenerateBaseUnreachableData();
            cm.hasGeneratedData = true;
        }
    }

    List<int> Detect(List<Vector3> pos, Vector3 dir, float max)
    {
        int i = 0;
        List<int> unreachable = new List<int>();
        foreach (var p in pos)
        {
            bool res = CastLine(p, dir, max);
            if (res == false)
            {
                unreachable.Add(i); //保存的这个i是地图的小格子 从0开始计数
            }
            i++;
        }
        return unreachable;
    }

    bool CastLine(Vector3 pos, Vector3 dir, float max)
    {
        RaycastHit hit;
        bool flag = false;
        if (Physics.Raycast(pos, dir, out hit))
        {
            float depth = hit.point.y;
            if (depth <= max)
            {
                flag = true;
            }
        }
        return flag;
    }

    void GenerateBaseUnreachableData()
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < cm.mapwidth / cm.tilelength; ++i)
            for (int j = 0; j < cm.mapheight / cm.tilelength; ++j)
            {
                Vector3 pos = new Vector3((float)(i - cm.mapwidth / 2 + 0.5) * cm.tilelength, 1000, (float)(j - cm.mapheight / 2 + 0.5) * cm.tilelength);
                positions.Add(pos);
            }
        // AssetDatabase.Refresh();
        cm.unreachable = Detect(positions, cm.dir, cm.max);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    

    public Vector3 TranselateIndexToPostion(int index)
    {
        int xlength = (int)cm.mapwidth / cm.unitlength;
        //int zlength = (int)cm.mapheight / cm.unitlength;
        int xdelta = index % xlength;
        int zdelta = index / xlength;
        Vector3 pos = new Vector3(maplefttopcenter.x + xdelta * cm.unitlength, 0, maplefttopcenter.z - zdelta * cm.unitlength);
        
        return pos;
    }

      void UpdateItemInfo()
    {
        foreach (var i in cm.itemlist)
        {
            if (i != null)
                i.posy = i.prefab.transform.position.y;
        }
    }

    public void Save()
    {
        // SceneView.onSceneGUIDelegate -= OnSceneGUI;
        UpdateItemInfo();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

   public  void CreateGameObjectAndAddUnreachable(CustomItemInfo iteminfo)
    {
        int index = iteminfo.lefttopsite;
        Vector3 lefttop = new Vector3(cm.center.x - cm.mapwidth / 2.0f, 0, cm.center.z + cm.mapheight / 2.0f);
        int rank = index % (cm.mapwidth / cm.unitlength) + 1;//(int)Mathf.Ceil((pos.x - lefttop.x) / (float)cm.unitlength);
        int row = index / (cm.mapwidth / cm.unitlength) + 1;// (int)Mathf.Ceil(Mathf.Abs(pos.z - lefttop.z) / (float)cm.unitlength);
        // int index = (row - 1) * cm.mapwidth / cm.unitlength + (rank);
        var centerpos = new Vector3(rank * cm.unitlength + lefttop.x - cm.unitlength / 2.0f, 0, lefttop.z - row * cm.unitlength + cm.unitlength / 2.0f);

        GameObject objTarget;
        switch (iteminfo.type)
        {
            case e_ItemType.Tree:
                objTarget = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                break;
            case e_ItemType.Box:
                objTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case e_ItemType.Stone:
                objTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            default:
                return;
                break;

        }
        //if (objTarget)
        //objTarget.transform.position = centerpos;
        objTarget.transform.position = new Vector3(centerpos.x, iteminfo.posy, centerpos.z);
        cm.unreachable.Add(index);
        //return centerpos;
    }
}