using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GameUtility
{
    public static Vector2Int mapSize;
    public static Tuple<int, int> columRange;
    public static Tuple<int, int> rowRange;

    public static TurnData saveTurn;

    public static bool fromSave = false;
    public static string Save = "";

    public static SaveData saveData;

    public static void 游戏初始化()
    {
        从预设中读取地图(fromSave, Save);

        从预设中读取棋子(fromSave, Save);

        //录入回合信息
        从预设中读取回合信息();
        saveTurn = null;
        if (fromSave && File.Exists(FixSystemData.SaveDirectory +  Save + "\\Turns.xml"))
        {
            //读取存档的回合信息
            读取存档的回合信息();
        }

        if(saveTurn?.currentResu != null)
        {
            FixGameData.FGD.resultMem = saveTurn.currentResu;
        }

        //布设棋子堆叠标志
        Map.UpdatePieceStackSign();

        //加载回合信息
        GameManager.GM.LoadTurnData(saveTurn);

        FixGameData.FGD.CrashDeathList = FixSystemData.CrashOrganizationList.Keys.ToList();

        GameManager.GM.SetMachineState(MachineState.JustReady);
        //开始游戏
        //GameManager.GM.StageStart();
        Debug.Log(saveData?.gameMode);
        switch (saveData?.gameMode)
        {
            case "PVP":
                GameManager.GM.TControl = new PVPTurn();
                break;
            case "PVE":

                break;
            case "TRA":
                GameManager.GM.TControl = new TRATurn();
                break;
        }

    }

    public static void 从预设中读取地图(bool fromSave, string Save)//字面意思
    {
        
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FixSystemData.GameInitDirectory + "\\Terrain.xml");
        XmlNode TerrainRoot = xmlDoc.DocumentElement;

        if (fromSave && File.Exists(FixSystemData.SaveDirectory + Save + "\\Facility.xml"))
        {
            xmlDoc.Load(FixSystemData.SaveDirectory +  Save + "\\Facility.xml");
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

        //设施节点地址，不是实际位置。
        columFac = 0;
        rowFac = 0;

        //清空原有地图
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

                //放置基础地形
                tileName = row.SelectSingleNode("basicTerrain").InnerText;
                FixGameData.FGD.MapList[0].SetTile(
                    tarPos,
                    FixSystemData.GlobalBasicTerrainList[tileName].Top);
                //填充交互
                FixGameData.FGD.InteractMap.SetTile(tarPos,
                    FixGameData.FGD.InteractFill);

                //放置河流
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

                //放置道路
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

                    //进入代表此节点匹配此位置
                    //放置设施(格子内)
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
                                false,
                                FacilityRow[rowFac].SelectSingleNode("facilityC").Attributes["active"] == null ? false : bool.Parse(FacilityRow[rowFac].SelectSingleNode("facilityC").Attributes["active"].Value)
                                ));
                        }
                        
                    }
                    //放置设施（格子边）
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
                                true,
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
                                true,
                                false
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
                                true,
                                false
                                ));
                        }
                    }
                    //放置特殊地形（格子边）
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
                                true,
                                false
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
                                true,
                                false
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
                                true,
                                false
                                ));
                        }
                    }
                    //放置特殊地形（格子内）
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
                                false,
                                false
                                ));
                    }

                    //前进到下一行
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

    public static void 从预设中读取棋子(bool fromSave,string Save)
    {
        XmlDocument xmlDoc = new XmlDocument();
        if (fromSave && File.Exists(FixSystemData.SaveDirectory  + Save + "\\Piece.xml"))
        {
            xmlDoc.Load(FixSystemData.SaveDirectory + Save + "\\Piece.xml");
        }
        else
        {
            xmlDoc.Load(FixSystemData.GameInitDirectory + "\\Piece.xml");
        }
        XmlNodeList HumanPieceList = xmlDoc.DocumentElement.FirstChild.ChildNodes;
        XmlNodeList CrashPieceList = xmlDoc.DocumentElement.LastChild.ChildNodes;
        Vector3Int pos;

        //清空假棋子
        foreach(GameObject fake in GameObject.FindGameObjectsWithTag("FakePiece"))
        {
            GameObject.Destroy(fake);
            
        }
        
        //录入人类方棋子
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
        //录入崩坏方棋子
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

        //排序并生成行目录
        QuickSortColumNo(ref FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().childList);
        GenColumIndex(FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().childList,
            ref FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().listIndex);

        QuickSortColumNo(ref FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().childList);
        GenColumIndex(FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().childList,
            ref FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().listIndex);
    }

    public static void 初始化支援签列表(XmlNode SupportList)
    {
        FixGameData.FGD.HumanSupportDic.Clear();
        FixGameData.FGD.CrashSupportDic.Clear();

        XmlNode HumanList = SupportList.FirstChild;
        XmlNode CrashList = SupportList.LastChild;
        foreach(XmlNode item in HumanList.ChildNodes)
        {
            string TroopName = item.Attributes["TroopName"].Value;
            int useTime = int.Parse(item.Attributes["UseableTime"].Value);
            Piece tmpPic = new Piece(FixSystemData.HumanOrganizationList[TroopName], null);
            
            FixGameData.FGD.HumanSupportDic.Add(TroopName,new Tuple<Piece, int>(tmpPic,useTime));
        }

        foreach (XmlNode item in CrashList.ChildNodes)
        {
            string TroopName = item.Attributes["TroopName"].Value;
            int useTime = int.Parse(item.Attributes["UseableTime"].Value);
            Piece tmpPic = new Piece(FixSystemData.CrashOrganizationList[TroopName], null);

            FixGameData.FGD.CrashSupportDic.Add(TroopName, new Tuple<Piece, int>(tmpPic, useTime));
        }
    }

    public static void 从预设中读取回合信息()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FixSystemData.GameInitDirectory + "\\Turns.xml");
        
        XmlNode TurnRoot = xmlDoc.DocumentElement;
        List<TurnData> turnList = new List<TurnData>();
        foreach(XmlNode node in TurnRoot.ChildNodes)
        {
            if (node.Name != "Turn") continue;
            turnList.Add(new TurnData(node));
        }
        FixGameData.FGD.TurnDatas = turnList.OrderBy(x => x.TurnNo).ToList();
        FixGameData.FGD.MaxRoundCount = FixGameData.FGD.TurnDatas[turnList.Count - 1].TurnNo;

        初始化支援签列表(TurnRoot.SelectSingleNode("BegianSupportList"));
    }

    public static void 读取存档的回合信息()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FixSystemData.SaveDirectory + Save + "\\Turns.xml");

        XmlNode TurnRoot = xmlDoc.DocumentElement;

        saveTurn = new TurnData(TurnRoot.FirstChild);

        初始化支援签列表(TurnRoot.FirstChild.SelectSingleNode("BegianSupportList"));
    }

    public static void 保存游戏()
    {
        BasicUtility.saveTurnData(FixSystemData.SaveDirectory + Save + "\\Turns.xml");
        BasicUtility.saveFacillitys(FixSystemData.SaveDirectory + Save + "\\Facility.xml");
        BasicUtility.savePiece(FixSystemData.SaveDirectory + Save + "\\Piece.xml");
        BasicUtility.saveSaveData(FixSystemData.SaveDirectory + Save + "\\SaveInfo.xml");
    }
    //生成行目录
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
    //行优先快速排序
    public static void QuickSortColumNo(ref List<Tuple<string, int, int>> numList)//行坐标排序
    {
        if (numList.Count == 0) return;

        //快速排序
        int anix;//快排轴枢
        Tuple<string, int, int> anixKey;//轴数值
        List<Tuple<int, int>> toSortQ = new List<Tuple<int, int>>();//待排序的片段起止队列
        toSortQ.Add(new Tuple<int, int>(0, numList.Count));//初始区间入队
                                                           //开始排序
        for (; toSortQ.Count > 0;)
        {
            //当前开始排序的片段出队
            Tuple<int, int> onSort = toSortQ[0];
            toSortQ.RemoveAt(0);

            //初始化
            int frontP = onSort.Item1;//前指针
            int backP = onSort.Item2;//后指针

            bool onReverse = false;//当前是否反向指针,false为从后往前,true 为从前往后

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
            //一次完成,开始入队后续
            //左侧
            if (backP - onSort.Item1 > 1)
            {
                toSortQ.Add(new Tuple<int, int>(onSort.Item1, backP));
            }
            //右侧
            if (onSort.Item2 - backP - 1 > 1)
            {
                toSortQ.Add(new Tuple<int, int>(backP + 1, onSort.Item2));
            }

        }
    }
    //快速排序——向前搜索
    static bool forwardSerch(ref int pin, ref List<Tuple<string, int, int>> list, int anixKey, int length)//返回是否应该反向
    {
        pin += 1;
        int mem = pin;
        for (; pin - mem < length; pin++)
        {
            if (list[pin].Item3 > anixKey) return true;
        }
        return false;
    }
    //快速排序——向后搜索
    static bool backwardSerch(ref int pin, ref List<Tuple<string, int, int>> list, int anixKey, int length)//返回是否应该反向
    {
        pin -= 1;
        int mem = pin;
        for (; mem - pin < length; pin--)
        {
            if (list[pin].Item3 < anixKey) return true;
        }
        return false;
    }
    
    public static string GetEventName(SpecialEvent evn)
    {
        switch (evn)
        {
            case SpecialEvent.DataStrom:
                return "模组数据暴";
            case SpecialEvent.SpaceSplit:
                return "空间撕裂";
            case SpecialEvent.SpaceFix:
                return "空间修补";
            case SpecialEvent.PosConfuse:
                return "坐标映射乱流";
            case SpecialEvent.MentalAD:
                return "心理学宣传";
            case SpecialEvent.TrainTroop:
                return "预备役训练";
            case SpecialEvent.RetreatCiv:
                return "平民撤离";
            default:
                return "未知事件";
        }
    }

    public static string GetEventInfo(SpecialEvent evn)
    {
        switch (evn)
        {
            case SpecialEvent.DataStrom:
                return "大量异常数据席卷一片区域，其中的敌方单位受到 1 点安定度伤害。\n每回合仅触发1次。";
            case SpecialEvent.SpaceSplit:
                return "占据一定带宽在某个处部署一个先兆态的空间裂隙。\n每回合可用多次，但需要留有带宽。";
            case SpecialEvent.SpaceFix:
                return "选择一个裂隙移除，并立刻释放其占据的带宽。\n每回合可用多次，并立刻释放带宽。";
            case SpecialEvent.PosConfuse:
                return "将某个区域内的全部敌军坐标向随机方向移动0~3格距离，如果进入到不可进入的地块则直接消灭。此次移动不计算堆叠和控制区。\n每回合仅触发1次。";
            case SpecialEvent.MentalAD:
                return "本回合免疫敌方心理战部队的煽动（处于“孤立”和“失联”状态下的部队除外），同时有概率解散叛军与迎回被策反的部队。解散的叛军直接回归平民（动员率下降）。\n每回合仅触发1次。";
            case SpecialEvent.TrainTroop:
                return "以动员率上升为代价，增加预备役储量。每个不处于“失联”状态下的庇护所都可以提供1动员率，每个猎人公会都可以将1动员率转化为2预备役储量。\n每回合可用多次，但请留意动员率。";
            case SpecialEvent.RetreatCiv:
                return "从机场撤离平民，以动员率上升为代价获得分数。每个不处于“失联”状态下的庇护所都可以提供1动员率，每个机场最多可以把2动员率1：5转化为撤离点数。\n每回合可用多次，但请留意动员率。";
            default:
                return "未知事件";
        }
    }
}
