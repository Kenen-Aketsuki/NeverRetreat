using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class UIAiAction : MonoBehaviour
{
    public static TurnStage currentStage;
    //无效指令最大数量
    int maxCommandTime = 10;
    //无效指令次数
    public Dictionary<string, int> FaildCommandCount = new Dictionary<string, int>();

    private void OnEnable()
    {
        switch (currentStage)
        {
            case TurnStage.Strategy:
                ActiveEvent();
                AddSupport();
                ActiveSpecialFac();
                break;
            case TurnStage.ModBattle:
                ModeBattleMove();
                ModeAttack();
                break;
            case TurnStage.Action:
                ActionMove();
                ActionSpecial();
                break;
        }

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        GameManager.GM.currentPiece = null;
        GameManager.GM.NextStage();
    }
    #region // 策略阶段
    void ActiveEvent()
    {
        List<Tuple<SpecialEvent, Vector3Int>> SelectedEvent = new List<Tuple<SpecialEvent, Vector3Int>>();

        for (int i = 0; i < 2; i++)
        {
            int radX = UnityEngine.Random.Range(GameUtility.columRange.Item1, GameUtility.columRange.Item2);
            int radY = UnityEngine.Random.Range(GameUtility.rowRange.Item1, GameUtility.rowRange.Item2);
            int radEvn = UnityEngine.Random.Range(0, 4);
            Vector3Int pos = new Vector3Int(radX, radY);

            SelectedEvent.Add(new Tuple<SpecialEvent, Vector3Int>(FixGameData.FGD.CrashSpecialEventList[radEvn], pos));
        }

    }

    void ActiveSpecialFac()
    {
        //IEnumerable SpecialList = FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == "DimensionFissure");
        List<FacilityDataCell> SpecialList = FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == "DimensionFissure").ToList();

        int amount = FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == "DimensionFissure").Count();

        for (int i = 0; i < GameManager.GM.MaxActiveFissureCount; i++)
        {
            int addr = UnityEngine.Random.Range(0, amount);
            SpecialList[addr].SetActive(true);
        }
    }

    void AddSupport()
    {
        int usableBind = GameManager.GM.MaxCrashBandwidth - GameManager.GM.CrashBandwidth;
        int refCount = FixGameData.FGD.CrashDeathList.Count;
        int usedBind = 0;

        while (usableBind > 0)
        {
            int radEvn = UnityEngine.Random.Range(0, refCount);
            string troopNam = FixGameData.FGD.CrashDeathList[radEvn];

            FixGameData.FGD.CrashLoadList.Add(new Tuple<string, string, int>(troopNam, "DimensionFissure", 0));
            Piece PData = new Piece(FixSystemData.CrashOrganizationList[troopNam], null);
            usableBind -= PData.crashLoad;
            usedBind += PData.crashLoad;
        }

        GameManager.GM.CrashBandwidth += usedBind;

    }
    #endregion

    #region // 模组战阶段
    void ModeBattleMove()
    {
        //获取可用棋子
        for(int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            OB_Piece pic = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
            if (pic.getPieceData().canFixMod)
            {
                CommandControl.CC.ToActionPiece.Enqueue(pic);
            }
        }

        //依次行动
        StartCoroutine(AIAgentWork(false));

    }

    void ModeAttack()
    {
        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            OB_Piece pic = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
            if (pic.getPieceData().canFixMod)
            {
                CommandControl.CC.ToActionPiece.Enqueue(pic);
            }
        }
        OB_Piece currentPic;
        while(CommandControl.CC.ToActionPiece.Count > 0)
        {
            currentPic = CommandControl.CC.ToActionPiece.Dequeue();
            List<CellInfo> attackArea = Map.PowerfulBrickAreaSearch(currentPic.piecePosition, 3);
            for(int i = 0; i < 3; i++)
            {
                Vector3Int attackTar = attackArea[UnityEngine.Random.Range(0, attackArea.Count)].Positian;
                ModBattle.CommitAttack("DataDeconstruct", attackTar);
            }
            
        }
    }
    #endregion

    #region // 行动阶段
    void ActionMove()
    {
        // 获取可用棋子
        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            OB_Piece pic = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
            if (!pic.getPieceData().canFixMod)
            {
                CommandControl.CC.ToActionPiece.Enqueue(pic);
            }
        }

        //依次行动
        StartCoroutine(AIAgentWork(true));
    }

    void ActionSpecial()
    {
        //收集可用棋子
        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            OB_Piece pic = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
            if (!pic.getPieceData().canFixMod &&
                pic.isSpecialPiece)
            {
                CommandControl.CC.ToActionPiece.Enqueue(pic);
            }
        }

        //执行特殊行动
        StartCoroutine(SpecialAction());


    }

    void MentalAttack(OB_Piece pic)
    {
        List<CellInfo> area = Map.PowerfulBrickAreaSearch(pic.piecePosition, 2);
        Vector3Int totalPos = Vector3Int.zero;
        int count = 0;
        foreach(CellInfo cell in area)
        {
            List<GameObject> picList = GameManager.GM.EnemyPool.getChildByPos(cell.Positian);
            totalPos += cell.Positian * picList.Count;
            count += picList.Count;
        }

        totalPos = totalPos / count;
        ActionStage.CommitMentalAttack(totalPos);
        pic.SpecialActPoint--;
    }

    void FireStrick(OB_Piece pic)
    {
        PiecePool pool = GameManager.GM.EnemyPool;
        List<CellInfo> area = Map.PowerfulBrickAreaSearch(pic.piecePosition, pic.getPieceData().activeArea);
        area = area.Where(x => pool.getChildByPos(x.Positian).Count > 0).ToList();
        while(pic.SpecialActPoint > 0)
        {
            int addr = UnityEngine.Random.Range(0,area.Count);
            ActionStage.CommitFireStrick(area[addr].Positian);
        }
    }
    #endregion

    IEnumerator AIAgentWork(bool canAttack)
    {
        CommandControl.CC.AttackList.Clear();
        CommandControl.CC.EndMovePiece.Clear();
        FaildCommandCount.Clear();

        bool isHttpWait = false;
        while (CommandControl.CC.ToActionPiece.Count > 0)
        {
            while (CommandControl.CC.ToActionPiece.Count > 0)
            {
                //提取棋子
                OB_Piece currentPiece = CommandControl.CC.ToActionPiece.Dequeue();
                string command = "N/A";
                currentPiece.gameObject.SetActive(true);

                if (FaildCommandCount.ContainsKey(currentPiece.name) && FaildCommandCount[currentPiece.name] >= maxCommandTime)
                {
                    CommandControl.CC.EndMovePiece.Add(currentPiece);
                    continue;
                }
                //前向传播
                isHttpWait = true;
                HttpConnect.instance.SendBattleFieldEnv(currentPiece, x => {
                    command = x;
                    isHttpWait = false;
                });
                while (isHttpWait) yield return null; // 阻塞

                //执行指令
                bool canBack;
                bool commandState = CommandControl.CC.CommandTranslate(command, currentPiece, out canBack, canAttack);
                if (!commandState && !FaildCommandCount.TryAdd(currentPiece.name, 1))
                {
                    FaildCommandCount[currentPiece.name]++;
                }

                yield return null;
            }

            CommandControl.CC.AttackCauclate();
            while (isHttpWait) yield return null;
        }
    }

    IEnumerator SpecialAction()
    {
        while (CommandControl.CC.ToActionPiece.Count > 0)
        {
            OB_Piece currentPiece = CommandControl.CC.ToActionPiece.Dequeue();
            GameManager.GM.currentPiece = currentPiece;
            if (currentPiece.getPieceData().canStrike)
            {
                FireStrick(currentPiece);
            }
            else if (currentPiece.getPieceData().canMental)
            {
                MentalAttack(currentPiece);
            }
            
            yield return null;

            if(currentPiece.SpecialActPoint > 0) CommandControl.CC.ToActionPiece.Enqueue(currentPiece);

            yield return null;
        }
    }
}
