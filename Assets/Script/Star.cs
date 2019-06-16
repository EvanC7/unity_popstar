using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour {

	public Sprite normalSprite;

    public Sprite selectedSprite;

    public enum StateType
    {
        STATE_NORMAL,
        STATE_SELECTED
    };

    public int colorType;

    private StateType _state;

    public StateType state
    {
        get
        {
            return _state;
        }
        set
        {
            this._state = value;
            if (state == StateType.STATE_NORMAL)
                SetNormal();
            else
                SetSelected();
        }
    }

    private SpriteRenderer render;

    void Start()
    {
        render = GetComponent<SpriteRenderer>();

        state = StateType.STATE_NORMAL;
    }

    private void SetNormal()
    {
        render.sprite = normalSprite;
    }

    private void SetSelected()
    {
        render.sprite = selectedSprite;
    }
}
