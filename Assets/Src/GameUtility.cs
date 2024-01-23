using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GameUtility
{
    public static Vector2Int mapSize;

    public static void ��Ԥ���ж�ȡ��ͼ(bool fromSave, string Save)//������˼
    {
        
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FixSystemData.GameInitDirectory + "\\Terrain.xml");
        XmlNode TerrainRoot = xmlDoc.DocumentElement;

        if (fromSave)
        {
            xmlDoc.Load(FixSystemData.GameInitDirectory + "\\" + Save + "\\Facility.xml");
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

        TerrainRoot = TerrainRoot.LastChild;

        
        int columNo = 0;
        int rowNo;

        //��ʩ�ڵ��ַ������ʵ��λ�á�
        columFac = 0;
        rowFac = 0;


        foreach (Tilemap map in FixGameData.FGD.MapList) map.ClearAllTiles();

        foreach (XmlNode colum in TerrainRoot.ChildNodes)
        {
            rowNo = 0;
            foreach (XmlNode row in colum.ChildNodes)
            {
                string tileName;
                string[] sideSplit;

                //���û�������
                tileName = row.SelectSingleNode("basicTerrain").InnerText;
                FixGameData.FGD.MapList[0].SetTile(
                    FixGameData.MapToWorld(columNo,rowNo ),
                    FixSystemData.GlobalBasicTerrainList[tileName].Top);

                //���ú���
                if(row.SelectSingleNode("river") != null)
                {
                    sideSplit = row.SelectSingleNode("river").InnerText.Split("-");
                    if (sideSplit[0] == "1")
                    {
                        FixGameData.FGD.MapList[1].SetTile(
                            FixGameData.MapToWorld(columNo, rowNo ),
                            FixSystemData.GlobalBasicTerrainList["River"].Left);
                    }
                    if (sideSplit[1] == "1")
                    {
                        FixGameData.FGD.MapList[2].SetTile(
                            FixGameData.MapToWorld(columNo, rowNo ),
                            FixSystemData.GlobalBasicTerrainList["River"].Top);
                    }
                    if (sideSplit[2] == "1")
                    {
                        FixGameData.FGD.MapList[3].SetTile(
                            FixGameData.MapToWorld(columNo, rowNo ),
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
                            FixGameData.MapToWorld(columNo, rowNo ),
                            FixSystemData.GlobalFacilityList["Road_" + roadName[int.Parse(sideSplit[0]) - 1]].Left);
                    }
                    if (sideSplit[1] != "0")
                    {
                        FixGameData.FGD.MapList[5].SetTile(
                            FixGameData.MapToWorld(columNo, rowNo ),
                            FixSystemData.GlobalFacilityList["Road_" + roadName[int.Parse(sideSplit[1]) - 1]].Top);
                    }
                    if (sideSplit[2] != "0")
                    {
                        FixGameData.FGD.MapList[6].SetTile(
                            FixGameData.MapToWorld(columNo, rowNo ),
                            FixSystemData.GlobalFacilityList["Road_" + roadName[int.Parse(sideSplit[2]) - 1]].Right);
                    }
                }

                if (rowFac < FacilityRow.Count &&
                    FacilityColum[columFac].Attributes["CNo"].Value == columNo.ToString() &&
                    FacilityRow[rowFac].Attributes["RNo"].Value == rowNo.ToString())
                {
                    //�������˽ڵ�ƥ���λ��
                    //������ʩ(������)
                    if (row.SelectSingleNode("FacilityC") != null)
                    {
                        tileName = row.SelectSingleNode("FacilityC").InnerText;
                        if (FixSystemData.GlobalFacilityList.ContainsKey(tileName))
                        {
                            FixGameData.FGD.MapList[7].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalFacilityList[tileName].Top);
                        }
                        else
                        {
                            FixGameData.FGD.MapList[7].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalSpFacilityList[tileName].Close);
                        }
                        
                    }
                    //������ʩ�����ӱߣ�
                    if (row.SelectSingleNode("FacilityS") != null)
                    {
                        sideSplit = row.SelectSingleNode("FacilityS").InnerText.Split("-");
                        if (sideSplit[0] == "1")
                        {
                            FixGameData.FGD.MapList[8].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalFacilityList[sideSplit[0]].Left);
                        }
                        if (sideSplit[1] == "1")
                        {
                            FixGameData.FGD.MapList[9].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalFacilityList[sideSplit[1]].Top);
                        }
                        if (sideSplit[2] == "1")
                        {
                            FixGameData.FGD.MapList[10].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalFacilityList[sideSplit[2]].Right);
                        }
                    }
                    //����������Σ����ӱߣ�
                    if(fromSave && row.SelectSingleNode("specialTerrainS") != null)
                    {
                        sideSplit = row.SelectSingleNode("specialTerrainS").InnerText.Split("-");
                        if (sideSplit[0] == "1")
                        {
                            FixGameData.FGD.MapList[11].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalSpecialTerrainList[sideSplit[0]].Left);
                        }
                        if (sideSplit[1] == "1")
                        {
                            FixGameData.FGD.MapList[12].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalSpecialTerrainList[sideSplit[1]].Top);
                        }
                        if (sideSplit[2] == "1")
                        {
                            FixGameData.FGD.MapList[13].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalSpecialTerrainList[sideSplit[2]].Right);
                        }
                    }
                    //����������Σ������ڣ�
                    if (fromSave && row.SelectSingleNode("specialTerrain") != null)
                    {
                        tileName = row.SelectSingleNode("specialTerrain").InnerText;
                        FixGameData.FGD.MapList[14].SetTile(
                                FixGameData.MapToWorld(columNo, rowNo),
                                FixSystemData.GlobalFacilityList[tileName].Top);

                    }

                    //ǰ������һ��
                    rowFac++;
                    if(rowFac >= FacilityColum[columFac].ChildNodes.Count)
                    {
                        rowFac = 0;
                        columFac++;
                        if(columFac < FacilityColum.Count) FacilityRow = FacilityColum[columFac].ChildNodes;
                    }
                }

                rowNo++;
            }
            columNo++;
        }

        

    }
}
