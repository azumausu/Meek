using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NestedScrollRect : ScrollRect
{
    private ScrollRect _parentScrollRect;
    private bool _isScrolling;
    private bool _isParentScroll;
    private bool _isParentStartScroll;
    private Vector2 _scrollStartPosition;

    protected override void Awake()
    {
        _parentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
    }
    
    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        _scrollStartPosition = eventData.position;
        base.OnInitializePotentialDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!_isScrolling)
        {
            // 子のスクロールか親のスクロールか判定
            Vector2 pos = eventData.position - _scrollStartPosition;
            float x = Mathf.Abs(pos.x);
            float y = Mathf.Abs(pos.y);
            if (Math.Abs(x - y) > 0.01)
            {
                if (horizontal)
                {
                    _isParentScroll = x < y; // 縦移動の方が多ければ親のイベントを発火
                    _isScrolling = true; // 親と子のどちらのイベントを実行するかをFix
                }

                if (vertical)
                {
                    _isParentScroll = x > y; // 横移動の方が多ければ親のイベントを発火
                    _isScrolling = true; // 親と子のどちらのイベントを実行するかをFix
                }
            }
        }
        else
        {
            // 親の場合、OnBeginDragをしてからOnDragする
            if (_isParentScroll)
            {
                if (!_isParentStartScroll)
                {
                    _parentScrollRect.OnInitializePotentialDrag(eventData);
                    _parentScrollRect.OnBeginDrag(eventData);
                    _isParentStartScroll = true;
                }

                _parentScrollRect.OnDrag(eventData);
            }
            else
            {
                base.OnDrag(eventData);
            }
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (_isParentScroll)
        {
            _parentScrollRect.OnEndDrag(eventData);
        }
        else
        {
            base.OnEndDrag(eventData);
        }

        // フラグ関連を初期化
        _isParentStartScroll = false;
        _isScrolling = false;
        _isParentScroll = false;
    }
}