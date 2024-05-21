using System;
using System.Collections.Generic;
using UnityEngine;

public class Guest : MonoBehaviour
{
    [SerializeField] private float _height;
    [SerializeField] private float _timeWait;
    private Floor _floorController;
    private Queue<(ItemHolder, int)> _items;
    private MoveGuest _mover;
    private float _time;
    private bool _waiting;
    void Start()
    {
        _floorController = FindAnyObjectByType<Floor>();
       
        _mover = transform.GetComponent<MoveGuest>();
        if (_floorController == null)
        {
            Debug.Log("Cannot find floor controller on scene");
        }
        _waiting = true;
        _mover.SetActionOnEnd(releaseNextItem);
        _mover.SetHeight(_height);
        _time = _timeWait;
    }

    // Update is called once per frame
    void Update()
    {
        if(_time <= 0f && _waiting)
        {
            _waiting = false;
            Vector3 point = transform.position;

            List<Vector3> points = new List<Vector3>();
            Queue<int> itemHolders = new Queue<int>();
            List<(ItemHolder, int)> holdersItems = new List<(ItemHolder, int)> ();
            int lengthWay = UnityEngine.Random.Range(1, 4);
            _items = new Queue<(ItemHolder, int)>();
            for(int l = 0; l < lengthWay; l++)
            {
                
                (List<Vector3>, ItemHolder) points2 = _floorController.GetWayToRandom(point);
                if (points2.Item1 == null || points2.Item2.getFreeItems() == 0)
                {
                    continue;
                }
                ItemHolder item = points2.Item2;
                points.AddRange(points2.Item1);
                point = points[points.Count - 1];
                itemHolders.Enqueue(points.Count - 1);
                int use = UnityEngine.Random.RandomRange(1, Math.Max(1, Math.Min(item.getFreeItems(), 6)));
                _items.Enqueue((points2.Item2, use));
                holdersItems.Add((item, item.getFreeItems() - use));
            }

            if(_items.Count == 0)
            {
                return;
            }

            for(int i = 0; i < 5; i++)
            {
                List<Vector3> points2 = _floorController.GetWayToRandomEndPoint(point);

                if (points2 != null)
                {
                    points.AddRange(points2);
                    point = points[points.Count - 1];
                    break;
                }

                if(points2 == null && i == 4)
                {
                    return;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                List<Vector3> points2 = _floorController.GetWayToExitPoint(point);

                if (points2 != null)
                {
                    points.AddRange(points2);
                    point = points[points.Count - 1];
                    break;
                }

                if (points2 == null && i == 4)
                {
                    return;
                }
            }
            foreach(var i in holdersItems)
            {
                i.Item1.setFreeItems(i.Item2);
            }
            _mover.Move(points, itemHolders);
        }

        _time -= Time.deltaTime;
    }

    private void setEnableToMove()
    {
        if(_timeWait == -1f) { return; }
        _waiting = true;
        _time = _timeWait;
    }
    public void releaseNextItem()
    {
        (ItemHolder, int) k = _items.Dequeue();
        for (int i = 0; i < k.Item2; i++)
        {
            k.Item1.DestroyLastItem();
        }
    }
    
}