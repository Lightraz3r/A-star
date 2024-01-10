using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PFStarter : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Transform start;
    [SerializeField] Transform end;
    [SerializeField] float moveSpeed = 0f;
    [SerializeField] float rotateSpeed = 0f;

    public UI UI;
    public bool DebugPathList = true;
    public bool ShowPath = true;

    Camera cam;
    List<Vector3> path = new List<Vector3>();
    Vector3 target { get => path[0] + new Vector3(0.5f, 0.5f, transform.position.z); }
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        UI.Instance.PathText(ShowPath);
    }

    private void Starter()
    {
        path = tilemap.FindPath(start.position, end.position, ShowPath);

        DebugPath(DebugPathList);
    }

    private void DebugPath(bool debug)
    {
        if (debug)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Debug.Log(path[i]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit(0);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {//By pressing tab you can change whether to debug path or not
            ShowPath = !ShowPath;
            UI.Instance.PathText(ShowPath);
        }


        bool lShiftPressed = Input.GetKey(KeyCode.LeftShift);
        UI.Instance.SwitcherText(path.Count == 0 && lShiftPressed);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {//Changes either goal or AI position depending on if LShift is pressed
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            if (lShiftPressed && path.Count == 0 && AStar.GetWalkable(mousePos.x, mousePos.y, tilemap))
            {
                transform.position = tilemap.WorldToCell(mousePos) + new Vector3(0.5f, 0.5f, transform.position.z);
            }
            else if (!lShiftPressed && AStar.GetWalkable(mousePos.x, mousePos.y, tilemap))
            {
                end.position = (Vector2)mousePos;
                Starter();
            }
        }


        if (path.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime) ;

            //roterar objekt, pekande mot vägen
            transform.eulerAngles = new Vector3(0, 0, GetSmoothAngle());

            if ((Vector2)transform.position == (Vector2)target)
            {
                path.RemoveAt(0);
            }
        }
    }

    private float GetSmoothAngle()
    {//Ger en vinkel, så att objekt kan rotera mjukt och inte på den första framen
        Vector2 direction = target - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Mathf.MoveTowardsAngle(transform.eulerAngles.z, angle, rotateSpeed * Time.deltaTime);
    }
}
