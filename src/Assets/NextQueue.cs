using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextQueue
{

    private enum Contants
    { 
        PUYO_TYPE_MAX = 4,
        PUYO_NEXT_HISTORISE =2,
    };
    Queue<Vector2Int> _next = new();

    Vector2Int CreateNext()
    {
        return new Vector2Int(
            Random.Range(0, (int)Contants.PUYO_TYPE_MAX) + 1,
            Random.Range(0, (int)Contants.PUYO_TYPE_MAX) + 1
        );
    }

    public void Initialize()
    {
        for (int t = 0; t < (int)Contants.PUYO_NEXT_HISTORISE; t++)
        {
            _next.Enqueue(CreateNext());
        }
    }
    public Vector2Int Update()
    {
        Vector2Int next = _next.Dequeue();
        _next.Enqueue(CreateNext());

        return next;
    }
    public void Each(System.Action<int, Vector2Int> cb)
    {
        int idx = 0;
        foreach (Vector2Int n in _next)
        {
            cb(idx++, n);
        }
    }
}
