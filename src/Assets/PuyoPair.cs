using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuyoPair : MonoBehaviour
{
    [SerializeField] PuyoController[] Puyos = { default!, default! };

    public void SetPuyoType(PuyoType axis,PuyoType child)
    {
        Puyos[0].SetPuyoType(axis);
        Puyos[1].SetPuyoType(child);
    }
}
