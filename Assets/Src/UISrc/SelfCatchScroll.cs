using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelfCatchScroll : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    RectTransform ContentPannel;
    [SerializeField]
    RectTransform Item;

    [SerializeField]
    VerticalLayoutGroup ItemGroup;

    List<GameObject> pieceList;

    bool catching = false;
    float snapingSpeed;
    float snapingForce = 200;
    Vector2 lastPos;

    int currentItem;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < ContentPannel.childCount; i++)
        {
            GameObject.Destroy(ContentPannel.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentItem = Mathf.RoundToInt(ContentPannel.localPosition.y / (Item.rect.height + ItemGroup.spacing));
        float Velo = Mathf.Min(Mathf.Abs((ContentPannel.localPosition.y - lastPos.y) / Time.deltaTime), 300);
        lastPos = ContentPannel.localPosition;

        

        //if (scrollRect.velocity.magnitude < 200 && catching)
        if (Velo < 200 && catching)
        {
            float tarY = currentItem * (Item.rect.height + ItemGroup.spacing);
            scrollRect.velocity = Vector2.zero;
            snapingSpeed += snapingForce * Time.deltaTime;
            ContentPannel.localPosition = new Vector3(
                ContentPannel.localPosition.x,
                Mathf.MoveTowards(ContentPannel.localPosition.y, tarY, snapingSpeed),
                ContentPannel.localPosition.z);
            
            lastPos = ContentPannel.localPosition;
            if (ContentPannel.localPosition.y == tarY)
            {
                catching = false;
                SelectPiece();
            }
            
        }

        //if(scrollRect.velocity.magnitude >= 200)
        if (Velo >= 200)
        {
            catching = true;
            snapingSpeed = 0;
        }

    }

    public void UpdateCellChilds(List<GameObject> Datas,List<LandShape> basicTerrain)
    {
        //清空记录
        for (int i = 0; i < ContentPannel.childCount; i++)
        {
            GameObject.Destroy(ContentPannel.GetChild(i).gameObject);
        }
        //放置地块
        GameObject newPiece = Object.Instantiate(Item.gameObject, ContentPannel);
        newPiece.name = "芝士地形";
        UIPieceDataCell ps = newPiece.GetComponent<UIPieceDataCell>();
        ps.SetData(null, basicTerrain);

        //放置棋子
        pieceList = Datas;
        foreach (GameObject Data in Datas)
        {
            newPiece = Object.Instantiate(Item.gameObject, ContentPannel);
            newPiece.name = "芝士棋子";
            ps = newPiece.GetComponent<UIPieceDataCell>();
            ps.SetData(Data.GetComponent<OB_Piece>().getPieceData(), null);
        }

        currentItem = Mathf.RoundToInt(ContentPannel.localPosition.y / (Item.rect.height + ItemGroup.spacing));
        SelectPiece();
    }

    void SelectPiece()
    {
        Debug.Log(currentItem);
        //return;

        UIPieceDataCell tar = ContentPannel.GetChild(ContentPannel.childCount - currentItem).gameObject.GetComponent<UIPieceDataCell>();
        Debug.Log(tar.gameObject.name);

        if (tar.Data != null && tar.Data.LoyalTo == GameManager.GM.ActionSide)
        {
            GameManager.GM.currentPiece = pieceList[currentItem].GetComponent<OB_Piece>();
            GameManager.GM.SetMachineState(MachineState.FocusOnPiece);
        }
        else
        {
            GameManager.GM.currentPiece = null;
            GameManager.GM.SetMachineState(MachineState.WaitForcuse);
        }
    }
}
