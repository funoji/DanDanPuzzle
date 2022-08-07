using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! };
    PlayerController _playerController = null;
    LogicalInput _logicalInput = new();

    NextQueue _NextQueue = new();
    static readonly KeyCode[] key_Code_tbl = new KeyCode[(int)LogicalInput.Key.Max]
   {
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.X,
        KeyCode.Z,
        KeyCode.UpArrow,
        KeyCode.DownArrow
   };
    // Start is called before the first frame update
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);

        _NextQueue.Initialize();
        Spawn(_NextQueue.Update());
        UpdateNextsView();
    }

    void UpdateNextsView()
    {
        _NextQueue.Each((int idx, Vector2Int n) => {
            nextPuyoPairs[idx++].SetPuyoType((PuyoType)n.x, (PuyoType)n.y);
        });
    }
    void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;

        for (int i = 0; i < (int)LogicalInput.Key.Max; i++)
        {
            if (Input.GetKey(key_Code_tbl[i]))
            {
                inputDev |= (LogicalInput.Key)(1 << i);
            }
        }
        _logicalInput.Update(inputDev);
    }
    void FixedUpdate()
    {
        UpdateInput();
        if(!player.activeSelf)
        {
            Spawn(_NextQueue.Update());
            UpdateNextsView();
        }
    }

    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}
