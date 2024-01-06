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
using Unity.VisualScripting;
using static Unity.Burst.Intrinsics.X86.Avx;
using UnityEditor.Experimental.GraphView;

public class LandShape
{
    public string name;
    public int enterCount = 0;
    public int height = 0;

    Dictionary<FixData,Tuple<FixWay,float>> Adjust = new Dictionary<FixData,Tuple<FixWay, float>>();

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

            XmlNodeList tmpL = Data.SelectNodes("battleAdjust");//录入修正值
            tmp = null;
            foreach(XmlNode L in tmpL)
            {
                if (L.Attributes["target"] == null || L.Attributes["target"].Value == "All")
                {
                    addAdjestTo(ref Adjust, L);
                    break;
                }
            }
        }
    }

    internal void addAdjestTo(ref Dictionary<FixData, Tuple<FixWay, float>> tar,XmlNode battleAdjust)
    {
        foreach(XmlNode node in battleAdjust.ChildNodes)
        {
            if (node.Attributes["target"].Value == "NA") return;
            tar.Add(
                (FixData)Enum.Parse(typeof(FixData), node.Attributes["target"].Value),
                new Tuple<FixWay, float>(
                    (FixWay)Enum.Parse(typeof(FixWay), node.Attributes["way"].Value),
                    float.Parse(node.Attributes["number"].Value)
                    )
                );
        }
    }
}

//通常地形都是中立的，其效果对双方有效。
public class BasicLandShape : LandShape //基础地形
{
    public Tile Left = null;
    public Tile Top;
    public Tile Right = null;

    public string id;
    public bool atSide = false;

    public BasicLandShape(XmlNode root) : base(root)
    {
        id = root.Attributes["id"].Value;
        if(root.Attributes["atSide"] != null) atSide = bool.Parse(root.Attributes["atSide"].Value);

        string path = FixSystemData.TerrainDirectory + "\\img\\" ;
        Texture2D texture;
        byte[] data;
        //加载中间
        #region
        string[] files = Directory.GetFiles(path, id + ".png");
        if(files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Top = new Tile();
            Top.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Top.name = id + "_Top";
        }
        #endregion
        if (!atSide) return; //若是在边上的，则继续加载左右。
        //加载左
        #region
        files = Directory.GetFiles(path, id + "_L.png");
        if( files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Left = new Tile();
            Left.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Left.name = id + "_Left";
        }
        #endregion
        //加载右
        #region
        files = Directory.GetFiles(path, id + "_R.png");
        if(files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Right = new Tile();
            Right.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Right.name = id + "_Right";
        }
        #endregion
    }
}

//设施通常是有所属的，其效果仅对一方有效，或对双方的效果不同。
public class Facility : LandShape
{
    public Tile Left = null;
    public Tile Top;
    public Tile Right = null;

    public string id;
    public bool atSide = false;
    public bool isRoad = false;
    public bool canLeftRuin = false;//是否留下废墟

    Dictionary<FixData, Tuple<FixWay, float>> AdjustFriend = new Dictionary<FixData, Tuple<FixWay, float>>();
    Dictionary<FixData, Tuple<FixWay, float>> AdjustEnemy = new Dictionary<FixData, Tuple<FixWay, float>>();

    public Facility(XmlNode root) : base(root)
    {
        id = root.Attributes["id"].Value;
        if (root.Attributes["atSide"] != null) atSide = bool.Parse(root.Attributes["atSide"].Value);
        if (root.Attributes["isRoad"] != null) isRoad = bool.Parse(root.Attributes["isRoad"].Value);
        if (root.Attributes["Type"].Value == "FixFacility") canLeftRuin = true;
        //加载另外两个增益项
        #region
        XmlNode tmp = root.SelectSingleNode("Data");
        if (tmp != null)
        {
            foreach (XmlNode L in root.SelectSingleNode("Data").SelectNodes("battleAdjust"))
            {
                if (L.Attributes["target"] != null && L.Attributes["target"].Value == "Friend")
                {
                    addAdjestTo(ref AdjustFriend, L);
                    break;
                }else if (L.Attributes["target"] != null && L.Attributes["target"].Value == "Enemy")
                {
                    addAdjestTo(ref AdjustEnemy, L);
                    break;
                }
            }
            
        }
        #endregion
        string path = FixSystemData.TerrainDirectory + "\\img\\";
        //加载中间
        Texture2D texture;
        byte[] data;
        #region
        string[] files = Directory.GetFiles(path, id + ".png");
        if(files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Top = new Tile();
            Top.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Top.name = id + "_Top";
        }
        else
        {
            Top = null;
        }
        
        #endregion
        if (!atSide) return; //若是在边上的，则继续加载左右。
        //加载左
        #region
        files = Directory.GetFiles(path, id + "_L.png");
        if( files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Left = new Tile();
            Left.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Left.name = id + "_Left";
        }
        #endregion
        //加载右
        #region
        files = Directory.GetFiles(path, id + "_R.png");
        if(files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Right = new Tile();
            Right.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Right.name = id + "_Right";
        }
        #endregion
    }
}