using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAirStrickSelect : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    RectTransform ContentPannel;
    [SerializeField]
    RectTransform Item;
    [SerializeField]
    HorizontalLayoutGroup ItemGroup;

    [SerializeField]
    TMP_Text MountShow;

    [SerializeField]
    TMP_Text Text;

    [SerializeField]
    TMP_Text turnShow;

    bool catching = false;
    float snapingSpeed;
    float snapingForce = 20;
    Vector2 lastPos;
    int currentItem;
    int halfFachInt;

    int EnyGroundForce = 0;
    IAirStrick airStrick;
    bool isModBattle;

    UIPieceDataCell SelectedPiece;
    Dictionary<string, int> SelectList = new Dictionary<string, int>();
    Dictionary<string, int> FriendList = new Dictionary<string, int>();
    Dictionary<string, int> EnemyList = new Dictionary<string, int>();

    Dictionary<string, Tuple<Piece, int>> pieceLst;

    private void OnEnable()
    {
        //收集范围内可防空的陆地单位
        EnyGroundForce = 0;
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, FixGameData.FGD.maxAirDefenceDis);
        List<OB_Piece> AAs = new List<OB_Piece>();
        foreach (CellInfo cell in Area)
        {
            List<GameObject> tmp = GameManager.GM.EnemyPool.getChildByPos(cell.Positian).Where(x => x.GetComponent<OB_Piece>().getPieceData().canAirBattle).ToList();
            if (tmp.Count != 0) AAs.AddRange(tmp.Where(
                x => Map.HexDistence(
                    x.GetComponent<OB_Piece>().piecePosition,
                    GameManager.GM.currentPosition)
                <= x.GetComponent<OB_Piece>().getPieceData().passiveArea)
                .Select(x => x.GetComponent<OB_Piece>()));
        }
        foreach (OB_Piece aa in AAs)
        {
            EnyGroundForce += aa.getPieceData().ATK;
        }

        //更新列表
        UpdatePieceList();

        //halfFachInt = Mathf.FloorToInt(ContentPannel.childCount / 2);
        //if (ContentPannel.childCount % 2 == 0)
        //{
        //    ItemGroup.padding.left = Mathf.RoundToInt((Item.rect.width + ItemGroup.spacing));
        //}

        
    }

    private void OnDisable()
    {
        FriendList.Clear();
        EnemyList.Clear();
    }

    void Update()
    {
        #region //自捕捉滚动视图
        //currentItem = Mathf.RoundToInt(ContentPannel.localPosition.x / (Item.rect.width + ItemGroup.spacing));
        float Velo = Mathf.Abs((ContentPannel.localPosition.x - lastPos.x) / Time.deltaTime);
        lastPos = ContentPannel.localPosition;

        //if (scrollRect.velocity.magnitude < 200 && catching)
        if (Velo < 2 && catching)
        {
            float tarX = currentItem * (Item.rect.width + ItemGroup.spacing);
            scrollRect.velocity = Vector2.zero;
            snapingSpeed += snapingForce * Time.deltaTime;
            ContentPannel.localPosition = new Vector3(
                Mathf.MoveTowards(ContentPannel.localPosition.x, tarX, snapingSpeed),
                ContentPannel.localPosition.y,
                ContentPannel.localPosition.z);

            lastPos = ContentPannel.localPosition;
            if (ContentPannel.localPosition.x == tarX)
            {
                //ContentPannel.localPosition = new Vector3(
                //    tarX,
                //    ContentPannel.localPosition.y,
                //    ContentPannel.localPosition.z);
                catching = false;
                SelectPiece();
            }

        }

        //if(scrollRect.velocity.magnitude >= 200)
        if (Velo >= 2)
        {
            currentItem = Mathf.RoundToInt(ContentPannel.localPosition.x / (Item.rect.width + ItemGroup.spacing));
            if (currentItem + 1 + halfFachInt > ContentPannel.childCount) currentItem = ContentPannel.childCount - 1 - halfFachInt;
            else if (currentItem + halfFachInt < 0) currentItem = -halfFachInt;
            catching = true;
            snapingSpeed = 0;
        }

        #endregion
    }

    void SelectPiece()
    {
        int addr = currentItem + halfFachInt;
        SelectedPiece = ContentPannel.GetChild(addr).GetComponent<UIPieceDataCell>();
        //Text.text = SelectedPiece.getPieceInfo();
        string txt = SelectedPiece.getPieceInfo();
        txt += "\n可用波次: " + pieceLst[SelectedPiece.Data.PDesignation].Item2;
        Text.text = txt;

        MountShow.text = SelectList[SelectedPiece.Data.PDesignation].ToString();
    }

    void UpdatePieceList()
    {
        if (FriendList.Count == 0)
        {
            if (GameManager.GM.ActionSide == ArmyBelong.Human)
            {
                pieceLst = FixGameData.FGD.HumanSupportDic.Where(x => FixGameData.FGD.HumanSupportDic[x.Key].Item1.canFixMod == isModBattle).ToDictionary(x => x.Key, x => x.Value); ;
            }
            else
            {
                pieceLst = FixGameData.FGD.CrashSupportDic.Where(x => FixGameData.FGD.CrashSupportDic[x.Key].Item1.canFixMod == isModBattle).ToDictionary(x => x.Key, x => x.Value); ; ;
            }
        }
        else
        {
            if (GameManager.GM.ActionSide == ArmyBelong.Human)
            {
                pieceLst = FixGameData.FGD.CrashSupportDic.Where(x => FixGameData.FGD.CrashSupportDic[x.Key].Item1.canAirBattle).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                pieceLst = FixGameData.FGD.HumanSupportDic.Where(x => FixGameData.FGD.HumanSupportDic[x.Key].Item1.canAirBattle).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        StartCoroutine(ResetCell());

        
    }

    public void closeWindow()
    {
        EnemyList.Clear();
        FriendList.Clear();

        gameObject.SetActive(false);
    }

    public void conformChoice()
    {
        if(FriendList.Count == 0)
        {
            FriendList = SelectList.ToDictionary(x => x.Key, x => x.Value);

            UpdatePieceList();
        }
        else
        {
            EnemyList = SelectList.ToDictionary(x => x.Key, x => x.Value);

            //执行空战程序
            Dictionary<Piece, int> FriendTmp;
            Dictionary<Piece, int> EnemyTmp;

            if(GameManager.GM.ActionSide == ArmyBelong.Human)
            {
                FriendTmp = FriendList.ToDictionary(x => FixGameData.FGD.HumanSupportDic[x.Key].Item1, x => x.Value);
                EnemyTmp = EnemyList.ToDictionary(x => FixGameData.FGD.CrashSupportDic[x.Key].Item1, x => x.Value);

                Tuple<Piece, int> tmp;
                foreach (KeyValuePair<string,int> par in FriendList)
                {
                    tmp = FixGameData.FGD.HumanSupportDic[par.Key];
                    FixGameData.FGD.HumanSupportDic[par.Key] = new Tuple<Piece, int>(tmp.Item1, tmp.Item2 - par.Value);
                }

                foreach (KeyValuePair<string, int> par in EnemyList)
                {
                    tmp = FixGameData.FGD.CrashSupportDic[par.Key];
                    FixGameData.FGD.CrashSupportDic[par.Key] = new Tuple<Piece, int>(tmp.Item1, tmp.Item2 - par.Value);
                }

            }
            else
            {
                FriendTmp = FriendList.ToDictionary(x => FixGameData.FGD.CrashSupportDic[x.Key].Item1, x => x.Value);
                EnemyTmp = EnemyList.ToDictionary(x => FixGameData.FGD.HumanSupportDic[x.Key].Item1, x => x.Value);

                Tuple<Piece, int> tmp;
                foreach (KeyValuePair<string, int> par in EnemyList)
                {
                    tmp = FixGameData.FGD.HumanSupportDic[par.Key];
                    FixGameData.FGD.HumanSupportDic[par.Key] = new Tuple<Piece, int>(tmp.Item1, tmp.Item2 - par.Value);
                }

                foreach (KeyValuePair<string, int> par in FriendList)
                {
                    tmp = FixGameData.FGD.CrashSupportDic[par.Key];
                    FixGameData.FGD.CrashSupportDic[par.Key] = new Tuple<Piece, int>(tmp.Item1, tmp.Item2 - par.Value);
                }
            }

            airStrick.AirStrickCall(FriendTmp, EnemyTmp, EnyGroundForce);

            //ActionStage.CommitAirStrick(FriendTmp, EnemyTmp, EnyGroundForce);

            gameObject.SetActive(false);
        }
    }

    public void PAdd()
    {
        int tmp = SelectList[SelectedPiece.Data.PDesignation];

        SelectList[SelectedPiece.Data.PDesignation] = Math.Min(tmp + 1, pieceLst[SelectedPiece.Data.PDesignation].Item2);

        MountShow.text = SelectList[SelectedPiece.Data.PDesignation].ToString();
        UpdateTextShow();

    }
    public void PSubtract()
    {
        int tmp = SelectList[SelectedPiece.Data.PDesignation];

        SelectList[SelectedPiece.Data.PDesignation] = Math.Max(tmp - 1, 0);

        MountShow.text = SelectList[SelectedPiece.Data.PDesignation].ToString();
        UpdateTextShow();
    }

    public void UpdateTextShow()
    {
        string str = "";
        str += "此处防空火力:" + EnyGroundForce + "\n";

        str += "打击队列:\n";
        foreach (KeyValuePair<string, int> par in SelectList)
        {
            str += "\n" + par.Key + "\n" + par.Value + "\n";
        }

        turnShow.text = str;
    }

    void ClearCell()
    {
        for (int i = ContentPannel.childCount; i > 0; i--)
        {
            Destroy(ContentPannel.GetChild(i - 1).gameObject);
        }
    }

    void FrashCell()
    {
        foreach (string troopNam in pieceLst.Keys)
        {
            Piece PData = pieceLst[troopNam].Item1;
            GameObject newPiece = GameObject.Instantiate(Item.gameObject, ContentPannel);
            newPiece.name = PData.PDesignation;
            UIPieceDataCell ps = newPiece.GetComponent<UIPieceDataCell>();
            ps.SetData(PData, null, null);
            if (!SelectList.ContainsKey(troopNam)) SelectList.Add(troopNam, 0);
        }
    }

    IEnumerator ResetCell()
    {
        SelectList.Clear();
        ClearCell();
        yield return 0;
        FrashCell();
        halfFachInt = Mathf.FloorToInt(ContentPannel.childCount / 2);
        if (ContentPannel.childCount % 2 == 0)
        {
            ItemGroup.padding.left = Mathf.RoundToInt(Item.rect.width + ItemGroup.spacing);
        }
        else
        {
            ItemGroup.padding.left = 0;
        }

        currentItem = 0;

        SelectPiece();

        UpdateTextShow();
    }

    public void SetData(IAirStrick ui,bool onModBattle)
    {
        airStrick = ui;
        isModBattle = onModBattle;
    }
}
