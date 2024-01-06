using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using UnityEngine.Tilemaps;
using System.IO;
using UnityEngine;

public class LandShape
{
    public string name;
    public int enterCount = 0;
    public int height = 0;

    Dictionary<FixData,Tuple<FixWay,int>> Adjust = new Dictionary<FixData,Tuple<FixWay,int>>();

    public LandShape(XmlNode root)
    {

        XmlNode tmp = root.SelectSingleNode("name");
        if (tmp != null)//录入名称
        {
            name = tmp.InnerText;
        }

        tmp = root.SelectSingleNode("Data");//录入数值
        if (tmp != null)
        {
            XmlNode Data = tmp;
            tmp = Data.SelectSingleNode("enterCost");//移入固定花费
            if (tmp != null)
            {
                try
                {
                    enterCount = int.Parse(tmp.InnerText);
                }
                catch (Exception)
                {
                    enterCount = -1;
                }
            }

            tmp = Data.SelectSingleNode("height");//录入地形高度
            if (tmp != null)
            {
                try
                {
                    height = int.Parse(tmp.InnerText);
                }
                catch (Exception)
                {
                    height = -1;
                }
            }

            tmp = Data.SelectSingleNode("battleAdjust");//录入修正值
            if(tmp != null && tmp.Attributes["target"].Value == "All")
            {
                foreach(XmlNode node in tmp.ChildNodes)
                {
                    Adjust.Add(
                        (FixData)Enum.Parse(typeof(FixData), node.Attributes["target"].Value),
                        new Tuple<FixWay, int>(
                            (FixWay)Enum.Parse(typeof(FixWay), node.Attributes["way"].Value),
                            int.Parse(node.Attributes["number"].Value)
                            )
                        );
                }
            }
        }
    

    }
}

public class MiddleLandShape : LandShape//位于格上的地形
{
    public Tile Top;

    public string id;

    public MiddleLandShape(XmlNode root) : base(root)
    {
        //加载瓦片
        string path = FixSystemData.TerrainDirectory + "\\img\\"+id+".png";
        string[] files = Directory.GetFiles(path);
        byte[] data = File.ReadAllBytes(files[0]);
        
        Texture2D texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
        texture.LoadImage(data);
        
        Top = new Tile();
        Top.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
        Top.name = id;
    }
    
}

public class SideLandShape : LandShape //位于边上的地形
{
    public Tile Left;
    public Tile Top;
    public Tile Right;

    public string id;

    public SideLandShape(XmlNode root) : base(root)
    {
        string path = FixSystemData.TerrainDirectory + "\\img\\" + id;
        //加载中间
        #region
        string[] files = Directory.GetFiles(path+"_T.png");
        byte[] data = File.ReadAllBytes(files[0]);

        Texture2D texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
        texture.LoadImage(data);

        Top = new Tile();
        Top.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
        Top.name = id + "_Top";
        #endregion
        //加载左
        #region
        files = Directory.GetFiles(path + "_L.png");
        data = File.ReadAllBytes(files[0]);

        texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
        texture.LoadImage(data);

        Left = new Tile();
        Left.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
        Left.name = id + "_Left";
        #endregion
        //加载右
        #region
        files = Directory.GetFiles(path + "_R.png");
        data = File.ReadAllBytes(files[0]);

        texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
        texture.LoadImage(data);

        Right = new Tile();
        Right.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
        Right.name = id + "_Right";
        #endregion
    }
}

