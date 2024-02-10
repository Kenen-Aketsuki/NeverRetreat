using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GameUtility
{
    public static Vector2Int mapSize;
    public static Tuple<int, int> columRange;
    public static Tuple<int, int> rowRange;

    public static float TileMapCellStep { get { return 2.16f; } }

    public static bool fromSave;
    public static string Save;

    public static void ��Ϸ��ʼ��()
    {
        ��Ԥ���ж�ȡ��ͼ(fromSave, Save);

        ��Ԥ���ж�ȡ����(fromSave, Save);

        //¼��غ���Ϣ
        if (fromSave)
        {

        }
        else
        {

        }

        //�������Ӷѵ���־
        //�������Ӷѵ���־();
        Map.UpdatePieceStackSign();

    }

    public static void ��Ԥ���ж�ȡ��ͼ(bool fromSave, string Save)//������˼
    {
        
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FixSystemData.GameInitDirectory + "\\Terrain.xml");
        XmlNode TerrainRoot = xmlDoc.DocumentElement;

        if (fromSave)
        {
            xmlDoc.Load(FixSystemData.SaveDirectory + "\\" + Save + "\\Facility.xml");
        }
        else
        {
            xmlDoc.Load(FixSystemData.GameInitDirectory + "\\Facility.xml");
        }
        
        XmlNodeList FacilityColum = xmlDoc.DocumentElement.LastChild.ChildNodes;
        XmlNodeList FacilityRow = FacilityColum[0].ChildNodes;

        int columFac = int.Parse(TerrainRoot.FirstChild.InnerText.Split("*")[0]);
        int rowFac = int.Parse(TerrainRoot.FirstChild.InnerText.Split("*")[1]);

        mapSize = new Vector2Int(columFac, rowFac);
        columRange = new Tuple<int,int>(-(int)math.floor(mapSize.y / 2), (int)math.floor(mapSize.y / 2));
        rowRange = new Tuple<int, int>(-(int)math.floor(mapSize.x / 2), (int)math.floor(mapSize.x / 2));


        TerrainRoot = TerrainRoot.LastChild;

        
        int columNo = 0;
        int rowNo;

        //��ʩ�ڵ��ַ������ʵ��λ�á�
        columFac = 0;
        rowFac = 0;

        //���ԭ�е�ͼ
        foreach (Tilemap map in FixGameData.FGD.MapList) map.ClearAllTiles();
        FixGameData.FGD.InteractMap.ClearAllTiles();

        foreach (XmlNode colum in TerrainRoot.ChildNodes)
        {
            rowNo = 0;
            foreach (XmlNode row in colum.ChildNodes)
            {
                string tileName;
                string[] sideSplit;
                Vector3Int tarPos = FixGameData.MapToWorld(columNo, rowNo);

                //���û�������
                tileName = row.SelectSingleNode("basicTerrain").InnerText;
                FixGameData.FGD.MapList[0].SetTile(
                    tarPos,
                    FixSystemData.GlobalBasicTerrainList[tileName].Top);
                //��佻��
                FixGameData.FGD.InteractMap.SetTile(tarPos,
                    FixGameData.FGD.InteractFill);

                //���ú���
                if (row.SelectSingleNode("river") != null)
                {
                    sideSplit = row.SelectSingleNode("river").InnerText.Split("-");
                    if (sideSplit[0] == "1")
                    {
                        FixGameData.FGD.MapList[1].SetTile(
                            tarPos,
                            FixSystemData.GlobalBasicTerrainList["River"].Left);
                    }
                    if (sideSplit[1] == "1")
                    {
                        FixGameData.FGD.MapList[2].SetTile(
                            tarPos,
                            FixSystemData.GlobalBasicTerrainList["River"].Top);
                    }
                    if (sideSplit[2] == "1")
                    {
                        FixGameData.FGD.MapList[3].SetTile(
                            tarPos,
                            FixSystemData.GlobalBasicTerrainList["River"].Right);
                    }
                }

                //���õ�·
                if (row.SelectSingleNode("road") != null)
                {
                    string[] roadName = { "1st", "2nd", "3rd" };
                    sideSplit = row.SelectSingleNode("road").InnerText.Split("-");
                    if (sideSplit[0] != "0")
                    {
                        FixGameData.FGD.MapList[4].SetTile(
                            tarPos,
                            FixSystemData.GlobalFacilityList["Road" + roadName[int.Parse(sideSplit[0]) - 1]].Left);
                    }
                    if (sideSplit[1] != "0")
                    {
                        FixGameData.FGD.MapList[5].SetTile(
                            tarPos,
                            FixSystemData.GlobalFacilityList["Road" + roadName[int.Parse(sideSplit[1]) - 1]].Top);
                    }
                    if (sideSplit[2] != "0")
                    {
                        FixGameData.FGD.MapList[6].SetTile(
                            tarPos,
                            FixSystemData.GlobalFacilityList["Road" + roadName[int.Parse(sideSplit[2]) - 1]].Right);
                    }
                }

                if (rowFac < FacilityRow.Count &&
                    columFac < FacilityColum.Count &&
                    FacilityColum[columFac].Attributes["CNo"].Value == columNo.ToString() &&
                    FacilityRow[rowFac].Attributes["RNo"].Value == rowNo.ToString())
                {

                    //�������˽ڵ�ƥ���λ��
                    //������ʩ(������)
                    if (FacilityRow[rowFac].SelectSingleNode("facilityC") != null)
                    {
                        tileName = FacilityRow[rowFac].SelectSingleNode("facilityC").InnerText;
                        if (FixSystemData.GlobalFacilityList.ContainsKey(tileName))
                        {
                            FixGameData.FGD.MapList[7].SetTile(
                                tarPos,
                                FixSystemData.GlobalFacilityList[tileName].Top);
                            FixGameData.FGD.FacilityList.Add(new FacilityDataCell(
                                tileName,
                                tarPos,
                                0,
                                FacilityRow[rowFac].SelectSingleNode("facilityC").Attributes["stayTime"] == null? FixSystemData.GlobalFacilityList[tileName].defaultExistTime:int.Parse(FacilityRow[rowFac].SelectSingleNode("facilityC").Attributes["stayTime"].Value),
                                false
                                ));
                        }
                        else
                        {
                            FixGameData.FGD.MapList[7].SetTile(
                                tarPos,
                                FixSystemData.GlobalSpFacilityList[tileName].Close);
                            FixGameData.FGD.SpecialFacilityList.Add(new FacilityDataCell(
                                tileName,
                                tarPos,
                                0,
                                int.MaxValue,
                                false
                                ));
                        }
                        
                    }
                    //������ʩ�����ӱߣ�
                    if (FacilityRow[rowFac].SelectSingleNode("facilityS") != null)
                    {
                        sideSplit = FacilityRow[rowFac].SelectSingleNode("facilityS").InnerText.Split("-");
                        if (sideSplit[0] != "X")
                        {
                            sideSplit[0] = sideSplit[0].Replace("_L", "");
                            FixGameData.FGD.MapList[8].SetTile(
                                tarPos,
                                FixSystemData.GlobalFacilityList[sideSplit[0]].Left);
                            FixGameData.FGD.FacilityList.Add(new FacilityDataCell(
                                sideSplit[0],
                                tarPos,
                                1,
                                FacilityRow[rowFac].SelectSingleNode("facilityS").Attributes["stayTime"] == null ? FixSystemData.GlobalFacilityList[sideSplit[0]].defaultExistTime : int.Parse(FacilityRow[rowFac].SelectSingleNode("facilityS").Attributes["stayTime"].Value.Split("-")[0]),
                                false
                                ));
                        }
                        if (sideSplit[1] != "X")
                        {
                            FixGameData.FGD.MapList[9].SetTile(
                                tarPos,
                                FixSystemData.GlobalFacilityList[sideSplit[1]].Top);
                            FixGameData.FGD.FacilityList.Add(new FacilityDataCell(
                                sideSplit[1],
                                tarPos,
                                2,
                                FacilityRow[rowFac].SelectSingleNode("facilityS").Attributes["stayTime"] == null ? FixSystemData.GlobalFacilityList[sideSplit[1]].defaultExistTime : int.Parse(FacilityRow[rowFac].SelectSingleNode("facilityS").Attributes["stayTime"].Value.Split("-")[1]),
                                true
                                ));
                        }
                        if (sideSplit[2] != "X")
                        {
                            sideSplit[2] = sideSplit[2].Replace("_R", "");
                            FixGameData.FGD.MapList[10].SetTile(
                                tarPos,
                                FixSystemData.GlobalFacilityList[sideSplit[2]].Right);
                            FixGameData.FGD.FacilityList.Add(new FacilityDataCell(
                                sideSplit[2],
                                tarPos,
                                3,
                                FacilityRow[rowFac].SelectSingleNode("facilityS").Attributes["stayTime"] == null ? FixSystemData.GlobalFacilityList[sideSplit[2]].defaultExistTime : int.Parse(FacilityRow[rowFac].SelectSingleNode("facilityS").Attributes["stayTime"].Value.Split("-")[2]),
                                true
                                ));
                        }
                    }
                    //����������Σ����ӱߣ�
                    if(fromSave && FacilityRow[rowFac].SelectSingleNode("specialTerrainS") != null)
                    {
                        sideSplit = FacilityRow[rowFac].SelectSingleNode("specialTerrainS").InnerText.Split("-");
                        if (sideSplit[0] != "X")
                        {
                            sideSplit[0] = sideSplit[0].Replace("_L", "");
                            FixGameData.FGD.MapList[11].SetTile(
                                tarPos,
                                FixSystemData.GlobalSpecialTerrainList[sideSplit[0]].Left);
                            FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell(
                                sideSplit[0],
                                tarPos,
                                1,
                                FacilityRow[rowFac].SelectSingleNode("specialTerrainS").Attributes["stayTime"] == null ? FixSystemData.GlobalSpecialTerrainList[sideSplit[0]].defaultExistTime : int.Parse(FacilityRow[rowFac].SelectSingleNode("specialTerrainS").Attributes["stayTime"].Value.Split("-")[0]),
                                true
                                ));
                        }
                        if (sideSplit[1] != "X")
                        {
                            FixGameData.FGD.MapList[12].SetTile(
                                tarPos,
                                FixSystemData.GlobalSpecialTerrainList[sideSplit[1]].Top);
                            FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell(
                                sideSplit[1],
                                tarPos,
                                2,
                                FacilityRow[rowFac].SelectSingleNode("specialTerrainS").Attributes["stayTime"] == null ? FixSystemData.GlobalSpecialTerrainList[sideSplit[1]].defaultExistTime : int.Parse(FacilityRow[rowFac].SelectSingleNode("specialTerrainS").Attributes["stayTime"].Value.Split("-")[1]),
                                true
                                ));
                        }
                        if (sideSplit[2] != "X")
                        {
                            sideSplit[2] = sideSplit[2].Replace("_R", "");
                            FixGameData.FGD.MapList[13].SetTile(
                                tarPos,
                                FixSystemData.GlobalSpecialTerrainList[sideSplit[2]].Right);
                            FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell(
                                sideSplit[2],
                                tarPos,
                                3,
                                FacilityRow[rowFac].SelectSingleNode("specialTerrainS").Attributes["stayTime"] == null ? FixSystemData.GlobalSpecialTerrainList[sideSplit[2]].defaultExistTime : int.Parse(FacilityRow[rowFac].SelectSingleNode("specialTerrainS").Attributes["stayTime"].Value.Split("-")[2]),
                                true
                                ));
                        }
                    }
                    //����������Σ������ڣ�
                    if (fromSave && FacilityRow[rowFac].SelectSingleNode("specialTerrain") != null)
                    {
                        tileName = FacilityRow[rowFac].SelectSingleNode("specialTerrain").InnerText;
                        FixGameData.FGD.MapList[14].SetTile(
                                tarPos,
                                FixSystemData.GlobalFacilityList[tileName].Top);
                        FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell(
                                tileName,
                                tarPos,
                                0,
                                FacilityRow[rowFac].SelectSingleNode("specialTerrain").Attributes["stayTime"] == null ? FixSystemData.GlobalSpecialTerrainList[tileName].defaultExistTime : int.Parse(FacilityRow[rowFac].SelectSingleNode("specialTerrain").Attributes["stayTime"].Value),
                                false
                                ));
                    }

                    //ǰ������һ��
                    rowFac++;
                    if(rowFac >= FacilityColum[columFac].ChildNodes.Count)
                    {
                        rowFac = 0;
                        columFac++;
                        if(columFac < FacilityColum.Count)
                        {
                            FacilityRow = FacilityColum[columFac].ChildNodes;
                        }
                    }
                }

                rowNo++;
            }
            columNo++;
        }

        
        
        

    }

    public static void ��Ԥ���ж�ȡ����(bool fromSave,string Save)
    {
        XmlDocument xmlDoc = new XmlDocument();
        if (fromSave)
        {
            xmlDoc.Load(FixSystemData.SaveDirectory + "\\" + Save + "\\Piece.xml");
        }
        else
        {
            xmlDoc.Load(FixSystemData.GameInitDirectory + "\\Piece.xml");
        }
        XmlNodeList HumanPieceList = xmlDoc.DocumentElement.FirstChild.ChildNodes;
        XmlNodeList CrashPieceList = xmlDoc.DocumentElement.LastChild.ChildNodes;
        Vector3Int pos;

        //��ռ�����
        foreach(GameObject fake in GameObject.FindGameObjectsWithTag("FakePiece"))
        {
            GameObject.Destroy(fake);
            
        }
        
        //¼�����෽����
        foreach (XmlNode HumanPiece in HumanPieceList)
        {
            pos = FixGameData.MapToWorld(int.Parse(HumanPiece.Attributes["xPos"].Value), int.Parse(HumanPiece.Attributes["yPos"].Value));
            try
            {
                _ = HumanPiece.Attributes["stability"].Value;
                BasicUtility.SpawnPiece(HumanPiece.Attributes["troopName"].Value, pos, HumanPiece, false);
            }
            catch (Exception)
            {
                BasicUtility.SpawnPiece(HumanPiece.Attributes["troopName"].Value, pos, null, false);
            }
            
        }
        //¼�����������
        foreach (XmlNode CrashPiece in CrashPieceList)
        {
            pos = FixGameData.MapToWorld(int.Parse(CrashPiece.Attributes["xPos"].Value), int.Parse(CrashPiece.Attributes["yPos"].Value));
            try
            {
                _ = CrashPiece.Attributes["stability"].Value;
                BasicUtility.SpawnPiece(CrashPiece.Attributes["troopName"].Value, pos, CrashPiece, false);
            }
            catch (Exception)
            {
                BasicUtility.SpawnPiece(CrashPiece.Attributes["troopName"].Value, pos, null, false);
            }

        }

        //����������Ŀ¼
        QuickSortColumNo(ref FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().childList);
        GenColumIndex(FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().childList,
            ref FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().listIndex);

        QuickSortColumNo(ref FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().childList);
        GenColumIndex(FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().childList,
            ref FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().listIndex);
    }

    public static void �������Ӷѵ���־()
    {
        //������Ӷѵ���־
        FixGameData.FGD.MultiPieceMap.ClearAllTiles();
        //���෽
        foreach(Tuple<string,int,int> child in FixGameData.FGD.HumanPiecePool.childList)
        {
            Vector3Int tmp = new Vector3Int(child.Item2, child.Item3, 0);
            List<GameObject> lst = FixGameData.FGD.HumanPiecePool.getChildByPos(tmp);
            if (FixGameData.FGD.MultiPieceMap.GetTile(tmp) == null && 
                lst.Count > 1)
            {
                //Debug.Log(FixGameData.FGD.InteractMap.CellToWorld(tmp));
                FixGameData.FGD.MultiPieceMap.SetTile(tmp, FixGameData.FGD.MultiPieceIcon);
                
            }
        }
        //������
        foreach (Tuple<string, int, int> child in FixGameData.FGD.CrashPiecePool.childList)
        {
            Vector3Int tmp = new Vector3Int(child.Item2, child.Item3, 0);
            if (FixGameData.FGD.MultiPieceMap.GetTile(tmp) == null &&
                FixGameData.FGD.CrashPiecePool.getChildByPos(tmp).Count > 1)
            {
                Debug.Log(FixGameData.FGD.InteractMap.CellToWorld(tmp));
                FixGameData.FGD.MultiPieceMap.SetTile(tmp, FixGameData.FGD.MultiPieceIcon);
            }
        }
    }

    //������Ŀ¼
    static public void GenColumIndex(List<Tuple<string, int, int>> childList,ref Dictionary<int, Tuple<int, int>> listIndex)
    {
        listIndex = new Dictionary<int, Tuple<int, int>>();
        int sta = 0;
        foreach (Tuple<string, int, int> tup in childList)
        {
            if (listIndex.ContainsKey(tup.Item3))
            {
                listIndex[tup.Item3] = new Tuple<int, int>(listIndex[tup.Item3].Item1, listIndex[tup.Item3].Item2 + 1);
            }
            else
            {
                listIndex.Add(tup.Item3, new Tuple<int, int>(sta, 1));
            }
            sta++;
        }
    }
    //�����ȿ�������
    public static void QuickSortColumNo(ref List<Tuple<string, int, int>> numList)//����������
    {
        //��������
        int anix;//��������
        Tuple<string, int, int> anixKey;//����ֵ
        List<Tuple<int, int>> toSortQ = new List<Tuple<int, int>>();//�������Ƭ����ֹ����
        toSortQ.Add(new Tuple<int, int>(0, numList.Count));//��ʼ�������
                                                           //��ʼ����
        for (; toSortQ.Count > 0;)
        {
            //��ǰ��ʼ�����Ƭ�γ���
            Tuple<int, int> onSort = toSortQ[0];
            toSortQ.RemoveAt(0);

            //��ʼ��
            int frontP = onSort.Item1;//ǰָ��
            int backP = onSort.Item2;//��ָ��

            bool onReverse = false;//��ǰ�Ƿ���ָ��,falseΪ�Ӻ���ǰ,true Ϊ��ǰ����

            anix = frontP;
            anixKey = numList[anix];
            for (; frontP != backP;)
            {
                int length = backP - frontP - 1;
                if (onReverse)
                {
                    if (forwardSerch(ref frontP, ref numList, anixKey.Item3, length))
                    {
                        numList[backP] = numList[frontP];
                        onReverse = false;
                    }
                }
                else
                {
                    if (backwardSerch(ref backP, ref numList, anixKey.Item3, length))
                    {
                        numList[frontP] = numList[backP];
                        onReverse = true;
                    }
                }
            }
            numList[backP] = anixKey;
            //һ�����,��ʼ��Ӻ���
            //���
            if (backP - onSort.Item1 > 1)
            {
                toSortQ.Add(new Tuple<int, int>(onSort.Item1, backP));
            }
            //�Ҳ�
            if (onSort.Item2 - backP - 1 > 1)
            {
                toSortQ.Add(new Tuple<int, int>(backP + 1, onSort.Item2));
            }

        }
    }
    //�������򡪡���ǰ����
    static bool forwardSerch(ref int pin, ref List<Tuple<string, int, int>> list, int anixKey, int length)//�����Ƿ�Ӧ�÷���
    {
        pin += 1;
        int mem = pin;
        for (; pin - mem < length; pin++)
        {
            if (list[pin].Item3 > anixKey) return true;
        }
        return false;
    }
    //�������򡪡��������
    static bool backwardSerch(ref int pin, ref List<Tuple<string, int, int>> list, int anixKey, int length)//�����Ƿ�Ӧ�÷���
    {
        pin -= 1;
        int mem = pin;
        for (; mem - pin < length; pin--)
        {
            if (list[pin].Item3 < anixKey) return true;
        }
        return false;
    }
    

}
