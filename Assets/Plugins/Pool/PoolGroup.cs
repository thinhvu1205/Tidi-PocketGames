using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
public class PoolGroup : MonoBehaviour
{
    private enum START { TOP_LEFT, TOP_RIGHT, BOT_LEFT, BOT_RIGHT }
    private enum SCROLL { NONE, HORIZONTAL, VERTICAL, FREE }
    [SerializeField] private START m_From = START.TOP_LEFT;
    [SerializeField][Min(1)] private int m_Constraint = 1; // limit max number of items in each row or column
    [SerializeField] private GameObject m_PfCell;
    [SerializeField] private Vector2 m_SpacingV2;
    [SerializeField] private float m_PaddingT, m_PaddingB, m_PaddingL, m_PaddingR;
    [SerializeField] private bool m_IsPriorPaddingTopOrLeft, m_IsPriorPaddingBotOrRight;
    private readonly List<PoolObject> _DataPOs = new();
    private List<PoolInfo> _ControlPIs = new();
    private ScrollRect _DataSR;
    private RectTransform _ViewportRT, _ContentRT;
    private UnityAction<RectTransform, PoolInfo> _OnApplyDataCb = (aRT, dataPI) => { }, _SetCellSizesInOnApplyDataCb = null;
    private SCROLL _ScrollType = SCROLL.NONE;
    private bool _IsCompleteCalculate;
    private float _BaseCellW, _BaseCellH, _StartPaddingT, _StartPaddingB, _StartPaddingL, _StartPaddingR, _StartSpacingX, _StartSpacingY;

    public void SetControlInfo(List<PoolInfo> _infoPIs)
    {
        foreach (PoolObject aPO in _DataPOs) aPO.PutBackToPool();
        if (_infoPIs == null || _infoPIs.Count <= 0) return;
        _CheckInitialize();
        _DataSR.StopMovement();
        m_PaddingT = _StartPaddingT; m_PaddingB = _StartPaddingB; m_PaddingL = _StartPaddingL; m_PaddingR = _StartPaddingR;
        m_SpacingV2 = new(_StartSpacingX, _StartSpacingY);
        for (int i = 0; i < _infoPIs.Count; i++)
        {
            PoolInfo aPI = _infoPIs[i];
            aPI.Id = i;
            if (aPI.Width <= 0) aPI.SetCellWidth(_BaseCellW).UpdateOldWidth();
            if (aPI.Height <= 0) aPI.SetCellHeight(_BaseCellH).UpdateOldHeight();
        }
        _ControlPIs = _infoPIs;
        _CalculateCellLocalV2sAndContentSizeDelta();
        if (_ScrollType == SCROLL.HORIZONTAL) _HandleOnHorizontalScroll();
        else if (_ScrollType == SCROLL.VERTICAL) _HandleOnVerticalScroll();
    }
    public void RefreshUI(bool _fixFocusItemTopOrLeft = true, int _focusItemId = -1)
    {   // if there is no cells change sizes and _SetCellSizesInOnApplyDataCb is null then just rerun _OnApplyDataCb, keep positions
        /* otherwise focus item will be found, default (_focusItemId = -1) is the closest to item 0 in viewport. It can be decided 
        to keep position of an edge of the focus item, default _fixFocusItemTopOrLeft is the top (vertical) and left (horizontal)*/
        bool hasCellsChangeSizes = _SetCellSizesInOnApplyDataCb != null;
        if (!hasCellsChangeSizes)
            foreach (PoolInfo aPI in _ControlPIs)
                if (aPI.Width != aPI.OldWidth || aPI.Height != aPI.OldHeight)
                {
                    hasCellsChangeSizes = true;
                    break;
                }
        if (!hasCellsChangeSizes)
        {
            foreach (PoolObject aPO in _DataPOs) if (!aPO.IsUnused) _OnApplyDataCb(aPO.DataRT, _GetInfo(aPO.Id));
            return;
        }
        bool isManuallyFocus = _focusItemId >= 0 && _DataPOs.Find(x => x.Id == _focusItemId && !x.IsUnused) != null;
        PoolInfo focusPI = null;
        if (isManuallyFocus) focusPI = _GetInfo(_focusItemId);
        else
        {
            PoolObject focusPO = _DataPOs.Find(x => !x.IsUnused);
            if (focusPO == null) return;
            focusPI = _GetInfo(focusPO.Id);
        }
        UnityAction endCb = null;
        if (_ScrollType == SCROLL.VERTICAL)
        {
            if (!isManuallyFocus) getAutoFocusItem(m_From == START.TOP_LEFT || m_From == START.TOP_RIGHT,
                (a) => { if (a.YBot > focusPI.YBot) focusPI = a; }, (a) => { if (a.YTop < focusPI.YTop) focusPI = a; });
            float offset = _ContentRT.localPosition.y + (_fixFocusItemTopOrLeft ? focusPI.YTop : focusPI.YBot); // from viewport top
            endCb = () => _ContentRT.localPosition = new(0, offset - (_fixFocusItemTopOrLeft ? focusPI.YTop : focusPI.YBot));
        }
        else if (_ScrollType == SCROLL.HORIZONTAL)
        {
            if (!isManuallyFocus) getAutoFocusItem(m_From == START.TOP_LEFT || m_From == START.BOT_LEFT,
                (a) => { if (a.XRight < focusPI.XRight) focusPI = a; }, (a) => { if (a.XLeft > focusPI.XLeft) focusPI = a; });
            float offset = _ContentRT.localPosition.x + (_fixFocusItemTopOrLeft ? focusPI.XLeft : focusPI.XRight); // from viewport left
            endCb = () => _ContentRT.localPosition = new(offset - (_fixFocusItemTopOrLeft ? focusPI.XLeft : focusPI.XRight), 0);
        }
        _CalculateCellLocalV2sAndContentSizeDelta();
        endCb();
        foreach (PoolObject aPO in _DataPOs) aPO.PutBackToPool();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        void getAutoFocusItem(bool _condition, UnityAction<PoolInfo> _trueCb, UnityAction<PoolInfo> _falseCb)
        {
            UnityAction<PoolInfo> getAutoFocusPICb = _condition ? _trueCb : _falseCb;
            foreach (PoolObject aPO in _DataPOs) if (!aPO.IsUnused) getAutoFocusPICb(_GetInfo(aPO.Id));
        }
    }
    public PoolGroup SetControlCbs(UnityAction<RectTransform, PoolInfo> _applyDataCb, UnityAction<RectTransform, PoolInfo> _setCellSizesCb = null)
    {   // _setCellSizesCb contains only codes that are calculating new cell sizes from _applyDataCb to maximize performance
        /* if _applyDataCb contains codes change cell sizes then _setCellSizesCb MUST NOT be null, otherwise leave it null.
        If _applyDataCb changes cell sizes but not sure which codes calculate them then set both callbacks same value*/
        if (_applyDataCb == null || m_PfCell == null)
        {
            _OnApplyDataCb = (aRT, dataPI) => { };
            _SetCellSizesInOnApplyDataCb = null;
            return null;
        }
        _CheckInitialize();
        _OnApplyDataCb = _applyDataCb;
        _SetCellSizesInOnApplyDataCb = _setCellSizesCb;
        return this;
    }
    public bool CheckAPoolInfoIsShown(PoolInfo _aPI) => _DataPOs.Find(x => !x.IsUnused && x.Id == _aPI.Id) != null;
    public void ScrollToItem(int _id, float _duration = 0, float _offset = 0, Ease _easeE = Ease.Linear, UnityAction _onEndCb = null)
    {   // _offset: distance item _id strips away from item 0; Ex: vertical pool starts from TOP then strip from TOP to BOT
        PoolInfo inputPI = _GetInfo(_id);
        if (inputPI == null) return;
        _DataSR.StopMovement();
        if (_ScrollType == SCROLL.VERTICAL)
        {
            bool fromTop = m_From == START.TOP_LEFT || m_From == START.TOP_RIGHT;
            float viewportH = _ViewportRT.rect.height, clampY = _ContentRT.sizeDelta.y - viewportH,
                targetY = Mathf.Min(fromTop ? -inputPI.YTop - _offset : -inputPI.YBot - viewportH + _offset, clampY);
            _ContentRT.DOLocalMoveY(targetY, _duration).SetEase(_easeE).OnComplete(() => _onEndCb?.Invoke());
        }
        else if (_ScrollType == SCROLL.HORIZONTAL)
        {
            bool fromLeft = m_From == START.TOP_LEFT || m_From == START.BOT_LEFT;
            float viewportW = _ViewportRT.rect.width, clampX = -(_ContentRT.sizeDelta.x - viewportW),
                targetX = Mathf.Max(fromLeft ? -inputPI.XLeft + _offset : -inputPI.XRight + viewportW - _offset, clampX);
            _ContentRT.DOLocalMoveX(targetX, _duration).SetEase(_easeE).OnComplete(() => _onEndCb?.Invoke());
        }
    }
    private PoolInfo _GetInfo(int _id) { return _id >= 0 && _id < _ControlPIs.Count ? _ControlPIs[_id] : null; }
    private void _CalculateCellLocalV2sAndContentSizeDelta()
    {
        if (_SetCellSizesInOnApplyDataCb != null)
        {   // _OnApplyDataCb changes cell sizes, prerun _SetCellSizesInOnApplyDataCb to prepare calculation
            m_PfCell.SetActive(false);
            RectTransform testRT = Instantiate(m_PfCell, _ViewportRT).GetComponent<RectTransform>();
            foreach (PoolInfo aPI in _ControlPIs) _SetCellSizesInOnApplyDataCb(testRT, aPI);
            Destroy(testRT.gameObject);
        }
        if (_ViewportRT.rect.width <= 0 || _ViewportRT.rect.height <= 0) Canvas.ForceUpdateCanvases();
        _IsCompleteCalculate = false;
        if (_ScrollType == SCROLL.VERTICAL)
        {
            if (m_Constraint > 1)
            {
                float rowMaxItems = Mathf.Min(_ControlPIs.Count, m_Constraint), countSpacing = rowMaxItems - 1, maxSpacingX =
                    (_ViewportRT.rect.width - rowMaxItems * _BaseCellW - m_PaddingL - m_PaddingR) / countSpacing;
                if (m_SpacingV2.x > maxSpacingX) { if (maxSpacingX > 0) m_SpacingV2.x = maxSpacingX; }
                else if (m_SpacingV2.x < maxSpacingX) _AdjustPaddings(countSpacing * (maxSpacingX - m_SpacingV2.x) / 2, ref m_PaddingL, ref m_PaddingR);
            }
            else _AdjustPaddings((_ViewportRT.rect.width - m_PaddingL - _BaseCellW) / 2, ref m_PaddingL, ref m_PaddingR);
            int countRows = Mathf.CeilToInt((float)_ControlPIs.Count / m_Constraint);
            List<List<PoolInfo>> rowsPIs = new();
            List<float> localXs = new(), maxRowHs = new();
            for (int i = 0; i < countRows; i++)
            {
                int startId = i * m_Constraint, endId = Mathf.Min(_ControlPIs.Count, startId + m_Constraint) - 1;
                float maxRowH = 0;
                List<PoolInfo> aRowPIs = new();
                for (int j = startId; j <= endId; j++)
                {
                    PoolInfo aPI = _ControlPIs[j];
                    if (maxRowH < aPI.Height) maxRowH = aPI.Height;
                    aRowPIs.Add(aPI);
                }
                maxRowHs.Add(maxRowH);
                rowsPIs.Add(aRowPIs);
            }
            float toRowYEdge = 0, contentHeight = 0;
            switch (m_From)
            {
                case START.TOP_LEFT:
                    for (int i = 0; i < m_Constraint; i++) localXs.Add(m_PaddingL + i * (_BaseCellW + m_SpacingV2.x) + _BaseCellW / 2);
                    toRowYEdge = -m_PaddingT;
                    for (int i = 0; i < rowsPIs.Count; i++)
                    {
                        float maxRowH = maxRowHs[i];
                        for (int j = 0; j < rowsPIs[i].Count; j++) rowsPIs[i][j].LocalV2 = new(localXs[j], toRowYEdge - maxRowH / 2);
                        toRowYEdge -= maxRowH + m_SpacingV2.y;
                    }
                    contentHeight = -(toRowYEdge + m_SpacingV2.y - m_PaddingB);
                    break;
                case START.TOP_RIGHT:
                    for (int i = 0; i < m_Constraint; i++) localXs.Insert(0, m_PaddingL + i * (_BaseCellW + m_SpacingV2.x) + _BaseCellW / 2);
                    toRowYEdge = -m_PaddingT;
                    for (int i = 0; i < rowsPIs.Count; i++)
                    {
                        float maxRowH = maxRowHs[i];
                        for (int j = 0; j < rowsPIs[i].Count; j++) rowsPIs[i][j].LocalV2 = new(localXs[j], toRowYEdge - maxRowH / 2);
                        toRowYEdge -= maxRowH + m_SpacingV2.y;
                    }
                    contentHeight = -(toRowYEdge + m_SpacingV2.y - m_PaddingB);
                    break;
                case START.BOT_LEFT:
                    for (int i = 0; i < m_Constraint; i++) localXs.Add(m_PaddingL + i * (_BaseCellW + m_SpacingV2.x) + _BaseCellW / 2);
                    toRowYEdge = m_PaddingB;
                    for (int i = 0; i < rowsPIs.Count; i++)
                    {
                        float maxRowH = maxRowHs[i];
                        for (int j = 0; j < rowsPIs[i].Count; j++) rowsPIs[i][j].LocalV2 = new(localXs[j], toRowYEdge + maxRowH / 2);
                        toRowYEdge += maxRowH + m_SpacingV2.y;
                    }
                    contentHeight = toRowYEdge - m_SpacingV2.y + m_PaddingT;
                    foreach (PoolInfo aPI in _ControlPIs) aPI.LocalV2 = new(aPI.LocalV2.x, -contentHeight + aPI.LocalV2.y);
                    break;
                case START.BOT_RIGHT:
                    for (int i = 0; i < m_Constraint; i++) localXs.Insert(0, m_PaddingL + i * (_BaseCellW + m_SpacingV2.x) + _BaseCellW / 2);
                    toRowYEdge = m_PaddingB;
                    for (int i = 0; i < rowsPIs.Count; i++)
                    {
                        float maxRowH = maxRowHs[i];
                        for (int j = 0; j < rowsPIs[i].Count; j++) rowsPIs[i][j].LocalV2 = new(localXs[j], toRowYEdge + maxRowH / 2);
                        toRowYEdge += maxRowH + m_SpacingV2.y;
                    }
                    contentHeight = toRowYEdge - m_SpacingV2.y + m_PaddingT;
                    foreach (PoolInfo aPI in _ControlPIs) aPI.LocalV2 = new(aPI.LocalV2.x, -contentHeight + aPI.LocalV2.y);
                    break;
            }
            _ContentRT.sizeDelta = new(_ViewportRT.rect.width, contentHeight);
        }
        else if (_ScrollType == SCROLL.HORIZONTAL)
        {
            if (m_Constraint > 1)
            {
                float colMaxItems = Mathf.Min(_ControlPIs.Count, m_Constraint), countSpacing = colMaxItems - 1, maxSpacingY =
                    (_ViewportRT.rect.height - colMaxItems * _BaseCellH - m_PaddingT - m_PaddingB) / countSpacing;
                if (m_SpacingV2.y > maxSpacingY) { if (maxSpacingY > 0) m_SpacingV2.y = maxSpacingY; }
                else if (m_SpacingV2.y < maxSpacingY) _AdjustPaddings(countSpacing * (maxSpacingY - m_SpacingV2.y) / 2, ref m_PaddingT, ref m_PaddingB);
            }
            else _AdjustPaddings((_ViewportRT.rect.height - m_PaddingT - _BaseCellH) / 2, ref m_PaddingT, ref m_PaddingB);
            int countCols = Mathf.CeilToInt((float)_ControlPIs.Count / m_Constraint);
            List<List<PoolInfo>> colsPIs = new();
            List<float> localYs = new(), maxColWs = new();
            for (int i = 0; i < countCols; i++)
            {
                int startId = i * m_Constraint, endId = Mathf.Min(_ControlPIs.Count, startId + m_Constraint) - 1;
                float maxColW = 0;
                List<PoolInfo> aColPIs = new();
                for (int j = startId; j <= endId; j++)
                {
                    PoolInfo aPI = _ControlPIs[j];
                    if (maxColW < aPI.Width) maxColW = aPI.Width;
                    aColPIs.Add(aPI);
                }
                maxColWs.Add(maxColW);
                colsPIs.Add(aColPIs);
            }
            float toColXEdge = 0, contentWidth = 0;
            switch (m_From)
            {
                case START.TOP_LEFT:
                    for (int i = 0; i < m_Constraint; i++) localYs.Add(-m_PaddingT - i * (_BaseCellH + m_SpacingV2.y) - _BaseCellH / 2);
                    toColXEdge = m_PaddingL;
                    for (int i = 0; i < colsPIs.Count; i++)
                    {
                        float maxColW = maxColWs[i];
                        for (int j = 0; j < colsPIs[i].Count; j++) colsPIs[i][j].LocalV2 = new(toColXEdge + maxColW / 2, localYs[j]);
                        toColXEdge += maxColW + m_SpacingV2.x;
                    }
                    contentWidth = toColXEdge - m_SpacingV2.x + m_PaddingR;
                    break;
                case START.TOP_RIGHT:
                    for (int i = 0; i < m_Constraint; i++) localYs.Add(-m_PaddingT - i * (_BaseCellH + m_SpacingV2.y) - _BaseCellH / 2);
                    toColXEdge = -m_PaddingR;
                    for (int i = 0; i < colsPIs.Count; i++)
                    {
                        float maxColW = maxColWs[i];
                        for (int j = 0; j < colsPIs[i].Count; j++) colsPIs[i][j].LocalV2 = new(toColXEdge - maxColW / 2, localYs[j]);
                        toColXEdge -= maxColW + m_SpacingV2.x;
                    }
                    contentWidth = -(toColXEdge + m_SpacingV2.x - m_PaddingL);
                    foreach (PoolInfo aPI in _ControlPIs) aPI.LocalV2 = new(contentWidth + aPI.LocalV2.x, aPI.LocalV2.y);
                    break;
                case START.BOT_LEFT:
                    for (int i = 0; i < m_Constraint; i++) localYs.Insert(0, -m_PaddingT - i * (_BaseCellH + m_SpacingV2.y) - _BaseCellH / 2);
                    toColXEdge = m_PaddingL;
                    for (int i = 0; i < colsPIs.Count; i++)
                    {
                        float maxColW = maxColWs[i];
                        for (int j = 0; j < colsPIs[i].Count; j++) colsPIs[i][j].LocalV2 = new(toColXEdge + maxColW / 2, localYs[j]);
                        toColXEdge += maxColW + m_SpacingV2.x;
                    }
                    contentWidth = toColXEdge - m_SpacingV2.x + m_PaddingR;
                    break;
                case START.BOT_RIGHT:
                    for (int i = 0; i < m_Constraint; i++) localYs.Insert(0, -m_PaddingT - i * (_BaseCellH + m_SpacingV2.y) - _BaseCellH / 2);
                    toColXEdge = -m_PaddingR;
                    for (int i = 0; i < colsPIs.Count; i++)
                    {
                        float maxColW = maxColWs[i];
                        for (int j = 0; j < colsPIs[i].Count; j++) colsPIs[i][j].LocalV2 = new(toColXEdge - maxColW / 2, localYs[j]);
                        toColXEdge -= maxColW + m_SpacingV2.x;
                    }
                    contentWidth = -(toColXEdge + m_SpacingV2.x - m_PaddingL);
                    foreach (PoolInfo aPI in _ControlPIs) aPI.LocalV2 = new(contentWidth + aPI.LocalV2.x, aPI.LocalV2.y);
                    break;
            }
            _ContentRT.sizeDelta = new(contentWidth, _ViewportRT.rect.height);
        }
        foreach (PoolInfo aPI in _ControlPIs) aPI.CalculateLocalEdges();
        _IsCompleteCalculate = true;
    }
    private void _AdjustPaddings(float _sharedSpace, ref float _padding1, ref float _padding2)
    {
        if (m_IsPriorPaddingTopOrLeft == m_IsPriorPaddingBotOrRight)
        {
            _padding1 += _sharedSpace;
            _padding2 += _sharedSpace;
            return;
        }
        if (m_IsPriorPaddingTopOrLeft) _padding2 += 2 * _sharedSpace;
        else _padding1 += 2 * _sharedSpace;
    }
    private void _CheckAndSetUIPoolObject(PoolInfo _aPI, Vector2 _localV2)
    {
        foreach (PoolObject aPO in _DataPOs) if (!aPO.IsUnused && aPO.Id == _aPI.Id) return;
        PoolObject foundPO = null;
        foreach (PoolObject aPO in _DataPOs) if (aPO.IsUnused) { foundPO = aPO; break; }
        if (foundPO == null)
        {
            m_PfCell.SetActive(false);
            foundPO = new() { DataRT = Instantiate(m_PfCell, _ContentRT).GetComponent<RectTransform>() };
            foundPO.DataRT.pivot = new(.5f, .5f);
            _DataPOs.Add(foundPO);
        }
        foundPO.Id = _aPI.Id;
        foundPO.IsUnused = false;
        foundPO.DataRT.name = "item " + _aPI.Id;
        foundPO.DataRT.gameObject.SetActive(true);
        _OnApplyDataCb(foundPO.DataRT, _aPI);
        foundPO.DataRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _aPI.UpdateOldWidth().Width);
        foundPO.DataRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _aPI.UpdateOldHeight().Height);
        foundPO.DataRT.localPosition = _localV2;
        foundPO.DataRT.SetAsLastSibling();
    }
    private void _HandleOnHorizontalScroll()
    {
        if (!_IsCompleteCalculate) return;
        float viewportXLeft = -_ContentRT.localPosition.x, viewportXRight = viewportXLeft + _ViewportRT.rect.width;
        foreach (PoolObject aPO in _DataPOs)
        {
            if (aPO.IsUnused) continue;
            PoolInfo aPI = _GetInfo(aPO.Id);
            if (aPI.XRight <= viewportXLeft || aPI.XLeft >= viewportXRight) aPO.PutBackToPool();
        }
        for (int i = 0; i < _ControlPIs.Count; i++)
        {
            PoolInfo aPI = _ControlPIs[i];
            if (aPI.XRight <= viewportXLeft || aPI.XLeft >= viewportXRight) continue;
            _CheckAndSetUIPoolObject(aPI, aPI.LocalV2);
        }
    }
    private void _HandleOnVerticalScroll()
    {
        if (!_IsCompleteCalculate) return;
        float viewportYTop = -_ContentRT.localPosition.y, viewportYBot = viewportYTop - _ViewportRT.rect.height;
        foreach (PoolObject aPO in _DataPOs)
        {
            if (aPO.IsUnused) continue;
            PoolInfo aPI = _GetInfo(aPO.Id);
            if (aPI.YBot >= viewportYTop || aPI.YTop <= viewportYBot) aPO.PutBackToPool();
        }
        for (int i = 0; i < _ControlPIs.Count; i++)
        {
            PoolInfo aPI = _ControlPIs[i];
            if (aPI.YBot >= viewportYTop || aPI.YTop <= viewportYBot) continue;
            _CheckAndSetUIPoolObject(aPI, aPI.LocalV2);
        }
    }
    private void _CheckInitialize()
    {
        if (_DataSR != null) return;
        _DataSR = GetComponentInParent<ScrollRect>();
        _ViewportRT = _DataSR.viewport;
        _ContentRT = _DataSR.content;
        _ViewportRT.pivot = Vector2.up;
        RectTransform thisRT = GetComponent<RectTransform>(), prefabCellRT = m_PfCell.GetComponent<RectTransform>();
        _BaseCellW = prefabCellRT.rect.width;
        _BaseCellH = prefabCellRT.rect.height;
        thisRT.anchorMin = Vector2.up;
        thisRT.anchorMax = Vector2.up;
        thisRT.pivot = Vector2.up;
        if (_DataSR.horizontal && _DataSR.vertical) _ScrollType = SCROLL.FREE;
        else _ScrollType = _DataSR.horizontal ? SCROLL.HORIZONTAL : (_DataSR.vertical ? SCROLL.VERTICAL : SCROLL.NONE);
        switch (_ScrollType)
        {
            case SCROLL.HORIZONTAL: _DataSR.onValueChanged.AddListener(aV2 => _HandleOnHorizontalScroll()); break;
            case SCROLL.VERTICAL: _DataSR.onValueChanged.AddListener(aV2 => _HandleOnVerticalScroll()); break;
            case SCROLL.FREE: break; // TODO: _HandleOnFreeScroll() later
            case SCROLL.NONE: Debug.LogError("|   ) )=3 Check axis scrolling."); return;
        }
        if (_DataSR.horizontalScrollbar != null && _DataSR.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
            _DataSR.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        if (_DataSR.verticalScrollbar != null && _DataSR.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
            _DataSR.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        _StartPaddingT = m_PaddingT; _StartPaddingB = m_PaddingB; _StartPaddingL = m_PaddingL; _StartPaddingR = m_PaddingR;
        _StartSpacingX = m_SpacingV2.x; _StartSpacingY = m_SpacingV2.y;
        m_PfCell.SetActive(false);
    }

    private void Awake() => _CheckInitialize();
}
public class PoolObject
{
    public RectTransform DataRT;
    public int Id;
    public bool IsUnused;

    public void PutBackToPool() { Id = -1; IsUnused = true; DataRT.gameObject.SetActive(false); }
}
public class PoolInfo
{
    public object Data;
    public int Id;
    public Vector2 LocalV2;
    public float XLeft { get; private set; }
    public float XRight { get; private set; }
    public float YTop { get; private set; }
    public float YBot { get; private set; }
    public float OldWidth { get; private set; }
    public float OldHeight { get; private set; }
    public float Width { get; private set; }
    public float Height { get; private set; }

    public void CalculateLocalEdges()
    {
        XLeft = LocalV2.x - Width / 2;
        XRight = XLeft + Width;
        YTop = LocalV2.y + Height / 2;
        YBot = YTop - Height;
    }
    public PoolInfo UpdateOldWidth() { OldWidth = Width; return this; }
    public PoolInfo UpdateOldHeight() { OldHeight = Height; return this; }
    public PoolInfo SetCellWidth(float _width)
    {
        if (Width == _width) return this;
        OldWidth = Width;
        Width = _width;
        return this;
    }
    public PoolInfo SetCellHeight(float _height)
    {
        if (Height == _height) return this;
        OldHeight = Height;
        Height = _height;
        return this;
    }
}
