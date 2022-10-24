using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;
using Unity.Netcode;
using System;
using Random = UnityEngine.Random;
using Unity.Collections;

[Serializable]public class RandomMapGenerator : NetworkBehaviour
{
    [Range(0, 100)]
    public int iniChance = 25;
    [Range(1, 8)]
    public int birthLimit;
    [Range(1, 8)]
    public int deathLimit;

    [Range(1, 10)]
    public int numR;
    private int count = 0;

    private int[,] terrainMap;
    public Vector3Int tmpSize;
    public Tilemap baseMap;
    public Tilemap environmentMap;
    public Tilemap botMap;
    public Tilemap backgroundMap;

    [SerializeField] TileBase[] tilesToChooseFrom;


    [SerializeField] TileBase[] plainsEnvironmentTilesToChooseFrom;
    [SerializeField] TileBase[] islandEnvironmentTilesToChooseFrom;
    [SerializeField] TileBase[] swampEnvironmentTilesToChooseFrom;
    [SerializeField] TileBase[] mountainEnvironmentTilesToChooseFrom;
    [SerializeField] TileBase[] forestEnvironmentTilesToChooseFrom;

    public TileBase botTile;
    public TileBase backgroundTile;

    List<int> modeList = new List<int>();
    int? randomNeighborTile;
    int width;
    int height;

    NetworkVariable<int> seed = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            seed.Value = Random.Range(0, 2147483646);
        }
        if (IsClient)
        {
            Debug.Log(seed.Value);
            Random.InitState(seed.Value);
            doSim(numR);
        }
    }

    public void doSim(int nu)
    {
        clearMap(false);
        width = tmpSize.x;
        height = tmpSize.y;

        GameManager.singleton.startingX = (-width + 1) + (width / 2);
        GameManager.singleton.startingY = (-height + 1) + (height / 2);
        GameManager.singleton.endingY = height + GameManager.singleton.startingY;
        GameManager.singleton.endingX = width + GameManager.singleton.startingX;
        if (terrainMap == null)
        {
            terrainMap = new int[width, height];
            initPos();
        }


        for (int i = 0; i < nu; i++)
        {
            terrainMap = genTilePos(terrainMap);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrainMap[x, y] != 0)
                {
                    baseMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), tilesToChooseFrom[terrainMap[x, y] - 1]);

                    if (Random.Range(0, 100) > 75)
                    {
                        int tileValue = terrainMap[x, y];
                        if (tileValue == 1)
                        {
                            environmentMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), plainsEnvironmentTilesToChooseFrom[Random.Range(0, plainsEnvironmentTilesToChooseFrom.Length)]);
                        }
                        if (tileValue == 2)
                        {
                            environmentMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), swampEnvironmentTilesToChooseFrom[Random.Range(0, swampEnvironmentTilesToChooseFrom.Length)]);
                        }
                        if (tileValue == 3)
                        {
                            environmentMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), mountainEnvironmentTilesToChooseFrom[Random.Range(0, mountainEnvironmentTilesToChooseFrom.Length)]);
                        }
                        if (tileValue == 4)
                        {
                            environmentMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), forestEnvironmentTilesToChooseFrom[Random.Range(0, forestEnvironmentTilesToChooseFrom.Length)]);
                        }
                    }

                }

                botMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), botTile);

                backgroundMap.SetTile(new Vector3Int((-x + width / 2) + width, (-y + height / 2) + height, 0), backgroundTile);
                backgroundMap.SetTile(new Vector3Int((-x + width / 2) - width, (-y + height / 2) - height, 0), backgroundTile);
                backgroundMap.SetTile(new Vector3Int((-x + width / 2) + width, (-y + height / 2) - height, 0), backgroundTile);
                backgroundMap.SetTile(new Vector3Int((-x + width / 2) - width, (-y + height / 2) + height, 0), backgroundTile);
                backgroundMap.SetTile(new Vector3Int((-x + width / 2) + width, (-y + height / 2), 0), backgroundTile);
                backgroundMap.SetTile(new Vector3Int((-x + width / 2) - width, (-y + height / 2), 0), backgroundTile);
                backgroundMap.SetTile(new Vector3Int((-x + width / 2), (-y + height / 2) + height, 0), backgroundTile);
                backgroundMap.SetTile(new Vector3Int((-x + width / 2), (-y + height / 2) - height, 0), backgroundTile);
            }
        }


    }

    public void initPos()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                terrainMap[x, y] = Random.Range(1, 101) < iniChance ? Random.Range(1, 5) : 0;
            }

        }

    }


    public int[,] genTilePos(int[,] oldMap)
    {
        int[,] newMap = new int[width, height];
        int neighb;
        BoundsInt myB = new BoundsInt(-1, -1, 0, 3, 3, 1);


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                modeList.Clear();
                neighb = 0;
                foreach (var b in myB.allPositionsWithin)
                {
                    if (b.x == 0 && b.y == 0) continue;
                    if (x + b.x >= 0 && x + b.x < width && y + b.y >= 0 && y + b.y < height)
                    {
                        neighb += oldMap[x + b.x, y + b.y];
                        if (oldMap[x + b.x, y + b.y] != 0)
                        {
                            modeList.Add(oldMap[x + b.x, y + b.y]);
                            //randomNeighborTile = oldMap[x + b.x, y + b.y];
                        }
                    }
                    else
                    {
                        //neighb++; //this draws a border
                    }
                }
                //randomNeighborTile = modeList.
                randomNeighborTile = modeList.GroupBy(x => x).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).Select(x => (int?)x.Key).FirstOrDefault();


                if (oldMap[x, y] != 0)
                {
                    if (neighb < deathLimit) newMap[x, y] = 0;

                    else
                    {
                        newMap[x, y] = (int)randomNeighborTile;

                    }
                }

                if (oldMap[x, y] == 0)
                {
                    if (neighb > birthLimit) newMap[x, y] = (int)randomNeighborTile;

                    else
                    {
                        newMap[x, y] = 0;
                    }
                }

            }

        }



        return newMap;
    }



    public void Update()
    {
        /*if (Input.GetButtonDown("Fire2"))
        {
            clearMap(true);
            doSim(numR);
        }*/
    }

    public void SaveAssetMap()
    {
       /* string saveName = "tmapXY_" + count;
        var mf = GameObject.Find("Grid");

        if (mf)
        {
            var savePath = "Assets/" + saveName + ".prefab";
            if (PrefabUtility.SaveAsPrefabAsset(mf, savePath, out bool succesful))
            {
                EditorUtility.DisplayDialog("Tilemap saved", "Your Tilemap was saved under" + savePath, "Continue");
            }
            else
            {
                EditorUtility.DisplayDialog("Tilemap NOT saved", "An ERROR occured while trying to saveTilemap under" + savePath, "Continue");
            }


        }*/


    }

    public void clearMap(bool complete)
    {

        baseMap.ClearAllTiles();
        botMap.ClearAllTiles();
        environmentMap.ClearAllTiles();
        if (complete)
        {
            terrainMap = null;
        }


    }

}
