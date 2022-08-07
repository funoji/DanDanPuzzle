using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    enum RotState
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,

        Invalid = -1,
    };
    AnimationController _animationController = new AnimationController();
    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2] { default!, default! };
    [SerializeField] BoardController boardController = default!;
    // Start is called before the first frame update

    Vector2Int _position;
    Vector2Int _last_position;
    RotState _last_rotate = RotState.Up;
    RotState _rotate = RotState.Up;
    const int TRANS_TIME = 3;
    const int ROT_TIME = 3;
    const int FALL_COUNT_UNIT = 120;
    const int FALL_COUNT_SPD = 10;
    const int FALL_COUNT_FAST_SPD = 20;
    const int GROUND_FRAMES = 50;

    int _fallCount = 0;
    int _groundFrame = GROUND_FRAMES;
    LogicalInput logical = new();
    static readonly KeyCode[] key_Code_tbl = new KeyCode[(int)LogicalInput.Key.Max]
    {
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.X,
        KeyCode.Z,
        KeyCode.UpArrow,
        KeyCode.DownArrow
    };
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetLogicalInput(LogicalInput reference)
    {
        logical = reference;
    }
    public bool Spawn(PuyoType axis,PuyoType child)
    {
        Vector2Int postion = new(2, 12);
        RotState rotate = RotState.Up;
        if (!CanMove(postion, rotate)) return false;

        _position = _last_position = postion;
        _rotate = _last_rotate = rotate;
        _animationController.Set(1);
        _fallCount = 0;
        _groundFrame = GROUND_FRAMES;

        _puyoControllers[0].SetPuyoType(axis);
        _puyoControllers[1].SetPuyoType(child);

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));

        gameObject.SetActive(true);

        return true;
    }

    void SetTransition(Vector2Int pos, RotState rot, int time)
    {
        _last_position = _position;
        _last_rotate = _rotate;

        _position = pos;
        _rotate = rot;

        _animationController.Set(time);
    }

    private bool Translate(bool is_right)
    {
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate)) return false;

        SetTransition(pos, _rotate, TRANS_TIME);

        return true;
    }

    static readonly Vector2Int[] rotate_tbl = new Vector2Int[]
    {
        Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left
    };

    private static Vector2Int CalcChildPuyoPos(Vector2Int pos, RotState rot)
    {
        return pos + rotate_tbl[(int)rot];
    }
    //void Start()
    //{
    //    _puyoControllers[0].SetPuyoType(PuyoType.Green);
    //    _puyoControllers[1].SetPuyoType(PuyoType.Red);

    //    _position = new Vector2Int(2, 12);
    //    _rotate = RotState.Up;

    //    _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
    //    Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
    //    _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y + 1.0f, 0.0f));
    //}

    private bool CanMove(Vector2Int pos, RotState rot)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;
        return true;
    }

    //public bool Translate(bool is_right)
    //{
    //    Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);

    //    if (!CanMove(pos, _rotate)) return false;

    //    _position = pos;

    //    _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
    //    Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
    //    _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y + 1.0f, 0.0f));
    //    return true;
    //}
    bool Rotate(bool is_right)
    {
        RotState rot = (RotState)(((int)_rotate + (is_right ? +1 : +3)) & 3);

        Vector2Int pos = _position;
        switch (rot)
        {
            case RotState.Down:
                if (!boardController.CanSettle(pos + Vector2Int.down) ||
                    !boardController.CanSettle(pos + new Vector2Int(is_right ? 1 : 1, -1)))
                {
                    pos += Vector2Int.up;
                }
                break;
            case RotState.Right:
                if (!boardController.CanSettle(pos + Vector2Int.right)) pos += Vector2Int.left;
                break;
            case RotState.Left:
                // ç∂: ç∂Ç™Ç§Ç‹Ç¡ÇƒÇ¢ÇÍÇŒÅAâEÇ…à⁄ìÆ
                if (!boardController.CanSettle(pos + Vector2Int.left)) pos += Vector2Int.right;
                break;
            case RotState.Up:
                break;
            default:
                Debug.Assert(false);
                break;
        }

        if (!CanMove(pos, rot)) return false;

        _rotate = rot;
        _position = pos;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));

        return true;
    }
    void Settle()
    {
        bool is_set0 = boardController.Settle(_position, (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_set0);
        bool is_set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate), (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_set1);

        gameObject.SetActive(false);
    }
    void QuickDrop()
    {
        Vector2Int pos = _position;
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;
        _position = pos;
        Settle();
    }

    bool Fall(bool is_fsat)
    {
        _fallCount -= is_fsat ? FALL_COUNT_FAST_SPD : FALL_COUNT_SPD;

        while(_fallCount < 0)
        {
            if(!CanMove(_position + Vector2Int.down, _rotate))
            {
                _fallCount = 0;
                if (0 < --_groundFrame) return true;

                Settle();
                return false;
            }
            _position += Vector2Int.down;
            _last_position += Vector2Int.down;
            _fallCount += FALL_COUNT_UNIT;
        }
        return true;
    }
    void Control()
    {
        if (!Fall(logical.IsRaw(LogicalInput.Key.Down))) return;
        if (_animationController.Update()) return;

        if (logical.IsRepeat(LogicalInput.Key.Right))
        {
            if (Translate(true)) return;
        }
        if (logical.IsRepeat(LogicalInput.Key.Left))
        {
            if (Translate(false)) return;
        }
        if (logical.IsTrigger(LogicalInput.Key.RotR))
        {
            if (Rotate(true)) return;
        }
        if (logical.IsTrigger(LogicalInput.Key.RotL))
        {
            if (Rotate(false)) return;
        }
        if (logical.IsRelease(LogicalInput.Key.QuickDrop))
        {
            QuickDrop();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Control();
        Vector3 dy = Vector3.up * (float)_fallCount / (float)FALL_COUNT_UNIT;
        float anim_rate = _animationController.GetNormalized();
        _puyoControllers[0].SetPos(Interpolate(_position, RotState.Invalid, _last_position, RotState.Invalid, anim_rate));
        _puyoControllers[1].SetPos(Interpolate(_position, _rotate, _last_position, _last_rotate, anim_rate));

    }
    static Vector3 Interpolate(Vector2Int pos, RotState rot, Vector2Int pos_last, RotState rot_last, float rate)
    {
        Vector3 p = Vector3.Lerp(
            new Vector3((float)pos.x, (float)pos.y, 0.0f),
            new Vector3((float)pos_last.x, (float)pos_last.y, 0.0f),
            rate);

        if (rot == RotState.Invalid) return p;

        float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
        float thetal = 0.5f * Mathf.PI * (float)(int)rot_last;
        float theta = thetal - theta0;

        if (+Mathf.PI < theta) theta = theta - 2.0f * Mathf.PI;
        if (theta < -Mathf.PI) theta = theta + 2.0f * Mathf.PI;

        theta = theta0 + rate * theta;
        return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
    }
}
