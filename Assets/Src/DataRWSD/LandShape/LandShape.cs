using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Tilemaps;
using System.IO;
using UnityEngine;

public class LandShape
{
    public string name;
    public int enterCount = 0;
    public int height = 0;

    //获取对应项加成
    public Tuple<FixWay, float> ATK_All { get { return Adjust[FixData.ATK]; } }
    public Tuple<FixWay, float> DEF_All { get { return Adjust[FixData.DEF]; } }
    public Tuple<FixWay, float> HP_All { get { return Adjust[FixData.HP]; } }
    public Tuple<FixWay, float> MOV_All { get { return Adjust[FixData.MOV]; } }
    public Tuple<FixWay, float> RRK_All { get { return Adjust[FixData.RRK]; } }


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
            Top.name = id;
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
            Left.name = id + "_L";
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
            Right.name = id + "_R";
        }
        #endregion
    }
}

//设施通常是有所属的，其效果仅对一方有效，或对双方的效果不同。特殊地形同样使用这个结构存储。
public class Facility : LandShape
{
    public Tile Left = null;
    public Tile Top;
    public Tile Right = null;

    public string id;
    public bool atSide = false;
    public bool isRoad = false;
    public bool canLeftRuin = false;//是否留下废墟
    public bool isSpecialLandShape = false;//是否为特殊地形(不包括特殊设施)
    public ArmyBelong Belone = ArmyBelong.Nutral;

    //获友方应项加成
    #region
    public Tuple<FixWay, float> ATK_Friend { get { return AdjustFriend[FixData.ATK]; } }
    public Tuple<FixWay, float> DEF_Friend { get { return AdjustFriend[FixData.DEF]; } }
    public Tuple<FixWay, float> HP_Friend { get { return AdjustFriend[FixData.HP]; } }
    public Tuple<FixWay, float> MOV_Friend { get { return AdjustFriend[FixData.MOV]; } }
    public Tuple<FixWay, float> RRK_Friend { get { return AdjustFriend[FixData.RRK]; } }
    #endregion
    //获取敌方加成
    #region
    public Tuple<FixWay, float> ATK_Enemy { get { return AdjustEnemy[FixData.ATK]; } }
    public Tuple<FixWay, float> DEF_Enemy { get { return AdjustEnemy[FixData.DEF]; } }
    public Tuple<FixWay, float> HP_Enemy { get { return AdjustEnemy[FixData.HP]; } }
    public Tuple<FixWay, float> MOV_Enemy { get { return AdjustEnemy[FixData.MOV]; } }
    public Tuple<FixWay, float> RRK_Enemy { get { return AdjustEnemy[FixData.RRK]; } }
    #endregion

    Dictionary<FixData, Tuple<FixWay, float>> AdjustFriend = new Dictionary<FixData, Tuple<FixWay, float>>();
    Dictionary<FixData, Tuple<FixWay, float>> AdjustEnemy = new Dictionary<FixData, Tuple<FixWay, float>>();

    public Facility(XmlNode root) : base(root)
    {
        id = root.Attributes["id"].Value;
        if (root.Attributes["atSide"] != null) atSide = bool.Parse(root.Attributes["atSide"].Value);
        if (root.Attributes["isRoad"] != null) isRoad = bool.Parse(root.Attributes["isRoad"].Value);
        if (root.Attributes["Type"].Value == "FixFacility") canLeftRuin = true;
        if (root.Attributes["Type"].Value == "SpecialTerrain") isSpecialLandShape = true;
        if (root.Attributes["belone"] != null) Belone = (ArmyBelong)Enum.Parse(typeof(ArmyBelong), root.Attributes["belone"].Value);
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
            Top.name = id;
        }
        else
        {
            Top = null;
        }
        
        #endregion
        if (!atSide && !isRoad) return; //若是在边上的，则继续加载左右。
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
            Left.name = id + "_L";
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
            Right.name = id + "_R";
        }
        #endregion
    }
}

public class SpecialFacility : LandShape
{
    public Tile Close;
    public Tile Active;

    public string id;
    public ArmyBelong Belone = ArmyBelong.Nutral;

    //获取友方加成
    #region
    public Tuple<FixWay, float> ATK_Friend { get { return AdjustFriend[FixData.ATK]; } }
    public Tuple<FixWay, float> DEF_Friend { get { return AdjustFriend[FixData.DEF]; } }
    public Tuple<FixWay, float> HP_Friend { get { return AdjustFriend[FixData.HP]; } }
    public Tuple<FixWay, float> MOV_Friend { get { return AdjustFriend[FixData.MOV]; } }
    public Tuple<FixWay, float> RRK_Friend { get { return AdjustFriend[FixData.RRK]; } }
    #endregion
    //获取敌方加成
    #region
    public Tuple<FixWay, float> ATK_Enemy { get { return AdjustEnemy[FixData.ATK]; } }
    public Tuple<FixWay, float> DEF_Enemy { get { return AdjustEnemy[FixData.DEF]; } }
    public Tuple<FixWay, float> HP_Enemy { get { return AdjustEnemy[FixData.HP]; } }
    public Tuple<FixWay, float> MOV_Enemy { get { return AdjustEnemy[FixData.MOV]; } }
    public Tuple<FixWay, float> RRK_Enemy { get { return AdjustEnemy[FixData.RRK]; } }
    #endregion

    Dictionary<FixData, Tuple<FixWay, float>> AdjustFriend = new Dictionary<FixData, Tuple<FixWay, float>>();
    Dictionary<FixData, Tuple<FixWay, float>> AdjustEnemy = new Dictionary<FixData, Tuple<FixWay, float>>();

    public SpecialFacility(XmlNode root) : base(root)
    {
        id = root.Attributes["id"].Value;
        if (root.Attributes["belone"] != null) Belone = (ArmyBelong)Enum.Parse(typeof(ArmyBelong), root.Attributes["belone"].Value);
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
                }
                else if (L.Attributes["target"] != null && L.Attributes["target"].Value == "Enemy")
                {
                    addAdjestTo(ref AdjustEnemy, L);
                    break;
                }
            }

        }
        #endregion
        string path = FixSystemData.TerrainDirectory + "\\img\\";
        //加载未激活
        Texture2D texture;
        byte[] data;
        #region
        string[] files = Directory.GetFiles(path, id + ".png");
        if (files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Close = new Tile();
            Close.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Close.name = id;
        }
        else
        {
            Close = null;
        }

        #endregion
        //加载激活
        #region
        files = Directory.GetFiles(path, id + "_Act.png");
        if (files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Active = new Tile();
            Active.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Active.name = id + "_Act";
        }
        #endregion
    }
}

public class Zone : LandShape
{
    public Tile Top;

    public string id;
    public ArmyBelong Belone = ArmyBelong.Nutral;

    //获友方应项加成
    #region
    public Tuple<FixWay, float> ATK_Friend { get { return AdjustFriend[FixData.ATK]; } }
    public Tuple<FixWay, float> DEF_Friend { get { return AdjustFriend[FixData.DEF]; } }
    public Tuple<FixWay, float> HP_Friend { get { return AdjustFriend[FixData.HP]; } }
    public Tuple<FixWay, float> MOV_Friend { get { return AdjustFriend[FixData.MOV]; } }
    public Tuple<FixWay, float> RRK_Friend { get { return AdjustFriend[FixData.RRK]; } }
    #endregion
    //获取敌方加成
    #region
    public Tuple<FixWay, float> ATK_Enemy { get { return AdjustEnemy[FixData.ATK]; } }
    public Tuple<FixWay, float> DEF_Enemy { get { return AdjustEnemy[FixData.DEF]; } }
    public Tuple<FixWay, float> HP_Enemy { get { return AdjustEnemy[FixData.HP]; } }
    public Tuple<FixWay, float> MOV_Enemy { get { return AdjustEnemy[FixData.MOV]; } }
    public Tuple<FixWay, float> RRK_Enemy { get { return AdjustEnemy[FixData.RRK]; } }
    #endregion

    Dictionary<FixData, Tuple<FixWay, float>> AdjustFriend = new Dictionary<FixData, Tuple<FixWay, float>>();
    Dictionary<FixData, Tuple<FixWay, float>> AdjustEnemy = new Dictionary<FixData, Tuple<FixWay, float>>();

    public Zone(XmlNode root) : base(root)
    {
        id = root.Attributes["id"].Value;
        
        if (root.Attributes["belone"] != null) Belone = (ArmyBelong)Enum.Parse(typeof(ArmyBelong), root.Attributes["belone"].Value);
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
                }
                else if (L.Attributes["target"] != null && L.Attributes["target"].Value == "Enemy")
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
        if (files.Length != 0)
        {
            data = File.ReadAllBytes(files[0]);

            texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);

            Top = new Tile();
            Top.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
            Top.name = id;
        }
        else
        {
            Top = null;
        }

        #endregion
    }
}