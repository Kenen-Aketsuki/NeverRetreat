using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMov : MonoBehaviour
{
    float addSpd;
    [SerializeField]
    Vector2 borderLD;
    [SerializeField]
    Vector2 borderRT;
    [SerializeField]
    Camera thisCam;

    [SerializeField]
    Vector3 mouseNow;
    [SerializeField]
    bool isDraging = false;

    bool needCul = true;

    // Start is called before the first frame update
    void Start()
    {
        borderRT = new Vector2(2.16f * GameUtility.mapSize.x / 2, 2.16f * GameUtility.mapSize.y / 2);
        borderLD = borderRT * new Vector2(-1, -1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W) && transform.position.y < borderRT.y) transform.position += new Vector3(0, 0.05f + addSpd + thisCam.orthographicSize * 0.01f, 0);
        if (Input.GetKey(KeyCode.S) && transform.position.y > borderLD.y) transform.position += new Vector3(0, -0.05f - addSpd - thisCam.orthographicSize * 0.01f, 0);
        if (Input.GetKey(KeyCode.A) && transform.position.x > borderLD.x) transform.position = transform.position + new Vector3(-0.05f - addSpd - thisCam.orthographicSize * 0.01f, 0, 0);
        if (Input.GetKey(KeyCode.D) && transform.position.x < borderRT.x) transform.position = transform.position + new Vector3(0.05f + addSpd + thisCam.orthographicSize * 0.01f, 0, 0);

        if (Input.GetKeyDown(KeyCode.LeftShift)) addSpd = 0.05f;
        if (Input.GetKeyUp(KeyCode.LeftShift)) addSpd = 0;

        #region //мов╖
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            mouseNow = thisCam.ScreenToWorldPoint(Input.mousePosition);
            isDraging = true;
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            isDraging = false;
        }
        if (isDraging)
        {
            transform.position -= (thisCam.ScreenToWorldPoint(Input.mousePosition) - mouseNow);
            mouseNow = thisCam.ScreenToWorldPoint(Input.mousePosition);
        }
        #endregion

        if (EventSystem.current.IsPointerOverGameObject() || Input.GetKey(KeyCode.Mouse1)) return;

        if (Input.mouseScrollDelta.y < 0 && thisCam.orthographicSize < 20) thisCam.orthographicSize += 0.5f;
        if (Input.mouseScrollDelta.y > 0 && thisCam.orthographicSize > 2) thisCam.orthographicSize -= 0.5f;

        if (needCul && borderRT == Vector2.zero)
        {
            borderRT = new Vector2(2.16f * GameUtility.mapSize.x / 2.5f, 2.16f * GameUtility.mapSize.y / 2);
            borderLD = borderRT * new Vector2(-1, -1);
            needCul = false;
        }
    }
}
