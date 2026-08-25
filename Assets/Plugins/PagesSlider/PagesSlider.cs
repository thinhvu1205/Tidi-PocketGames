using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PagesSlider : MonoBehaviour
{
    [SerializeField] private GameObject m_ButtonUp, m_ButtonDown, m_ButtonLeft, m_ButtonRight;
    [SerializeField] private RectTransform m_MaskRT, m_PagesRT, m_PfPageRT, m_TestRT, m_HorizontalDotsRT, m_VerticalDotsRT, m_PfDotRT;
    [SerializeField][Min(0.1f)] private float m_MovingDuration = 1f;
    private List<PageInfo> _DataPIs = new();
    private int _PageIdNow, _FirstPageId, _LastPageId;
    private bool _IsHorizontal, _IsLoop;

    #region Button
    public void DoClickUp() => _DoClick(_IsLoop ? _PageIdNow - 1 : Mathf.Max(_PageIdNow - 1, _FirstPageId), m_MovingDuration);
    public void DoClickDown() => _DoClick(_IsLoop ? _PageIdNow + 1 : Mathf.Min(_PageIdNow + 1, _LastPageId), m_MovingDuration);
    public void DoClickLeft() => _DoClick(_IsLoop ? _PageIdNow - 1 : Mathf.Max(_PageIdNow - 1, _FirstPageId), m_MovingDuration);
    public void DoClickRight() => _DoClick(_IsLoop ? _PageIdNow + 1 : Mathf.Min(_PageIdNow + 1, _LastPageId), m_MovingDuration);
    public void DoClickClose() => gameObject.SetActive(false);
    #endregion

    [ContextMenu("Demo Usage")]
    public void DemoUsage()
    {
        List<Transform> contentsTfs = new();
        List<Color> colorCs = new() { Color.white, Color.blue, Color.yellow, Color.red, Color.green };
        for (int i = 0; i < colorCs.Count; i++)
        {
            Transform aTf = Instantiate(m_TestRT);
            aTf.gameObject.SetActive(true);
            aTf.name = i.ToString();
            aTf.GetComponent<Image>().color = colorCs[i];
            contentsTfs.Add(aTf);
        }
        Init(contentsTfs, new(true, true, true, true));
    }
    public async Awaitable Init(List<Transform> _contentTfs, PagesSliderProps _aPSP)
    {
        if (_contentTfs == null || _contentTfs.Count <= 0) return;
        _IsHorizontal = _aPSP.IsHorizontal;
        _IsLoop = _aPSP.IsLoop;
        m_ButtonUp.SetActive(!_IsHorizontal);
        m_ButtonDown.SetActive(!_IsHorizontal);
        m_ButtonLeft.SetActive(_IsHorizontal);
        m_ButtonRight.SetActive(_IsHorizontal);
        int countClonesEachSide = 1;
        if (_aPSP.IsShow1Page) setSizesSameAsMaskSizes(m_PfPageRT);
        else countClonesEachSide = (int)MathF.Max(2, Mathf.FloorToInt(_IsHorizontal ?
            m_MaskRT.rect.width / m_PfPageRT.rect.width : m_MaskRT.rect.height / m_PfPageRT.rect.height));
        Transform firstContentTf = _contentTfs[0], lastContentTf = _contentTfs[^1];
        if (_aPSP.IsLoop)
        {
            List<Transform> cloneHeadTfs = new(), cloneTailTfs = new();
            for (int i = 0; i < countClonesEachSide; i++)
            {
                cloneHeadTfs.Add(_contentTfs[i]);
                cloneTailTfs.Add(_contentTfs[_contentTfs.Count - i - 1]);
            }
            foreach (Transform aTf in cloneHeadTfs) _contentTfs.Add(Instantiate(aTf));
            foreach (Transform aTf in cloneTailTfs) _contentTfs.Insert(0, Instantiate(aTf));
        }
        foreach (Transform pageTf in m_PagesRT)
        {
            foreach (Transform aTf in pageTf) Destroy(aTf.gameObject);
            pageTf.gameObject.SetActive(false);
        }
        foreach (Transform dotTf in m_HorizontalDotsRT) dotTf.gameObject.SetActive(false);
        foreach (Transform dotTf in m_VerticalDotsRT) dotTf.gameObject.SetActive(false);
        RectTransform firstPageRT = null, lastPageRT = null;
        for (int i = 0; i < _contentTfs.Count; i++)
        {
            Transform thisTf = _contentTfs[i];
            RectTransform pageRT = getFromPool(m_PagesRT, m_PfPageRT);
            pageRT.name = "item " + i;
            if (_aPSP.IsShow1Page) setSizesSameAsMaskSizes(pageRT);
            thisTf.SetParent(pageRT);
            thisTf.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            thisTf.localScale = Vector3.one;
            if (thisTf == firstContentTf)
            {
                firstPageRT = pageRT;
                _FirstPageId = i;
            }
            if (thisTf == lastContentTf)
            {
                lastPageRT = pageRT;
                _LastPageId = i;
            }
        }
        if (m_PagesRT.TryGetComponent(out HorizontalOrVerticalLayoutGroup aHOVLG)) Destroy(aHOVLG);
        m_PagesRT.localPosition = Vector3.zero;
        await Awaitable.NextFrameAsync();

        aHOVLG = _IsHorizontal ? m_PagesRT.AddComponent<HorizontalLayoutGroup>() : m_PagesRT.AddComponent<VerticalLayoutGroup>();
        if (_IsHorizontal) aHOVLG.childAlignment = _aPSP.IsFromTopOrLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
        else aHOVLG.childAlignment = _aPSP.IsFromTopOrLeft ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;
        aHOVLG.childControlWidth = false;
        aHOVLG.childControlHeight = false;
        aHOVLG.childForceExpandWidth = false;
        aHOVLG.childForceExpandHeight = false;
        foreach (Transform aTf in _contentTfs) aTf.gameObject.SetActive(false);
        _DataPIs.Clear();
        await Awaitable.NextFrameAsync();

        Vector3 focusV3 = getPageLocalPosToMask((_aPSP.IsFromTopOrLeft ? firstPageRT : lastPageRT).position);
        RectTransform dotsRT = _IsHorizontal ? m_HorizontalDotsRT : m_VerticalDotsRT;
        for (int i = 0; i < _contentTfs.Count; i++)
        {
            PageInfo aPI = new() { Id = i, LocalPosToMaskV3 = getPageLocalPosToMask(_contentTfs[i].parent.position) };
            if (i >= _FirstPageId && i <= _LastPageId) aPI.DotRT = getFromPool(dotsRT, m_PfDotRT);
            if (aPI.DotRT != null)
            {
                aPI.DotActiveRT = aPI.DotRT.GetChild(0).GetComponent<RectTransform>();
                aPI.DotActiveRT.gameObject.SetActive(false);
            }
            _DataPIs.Add(aPI);
            if (aPI.LocalPosToMaskV3 == focusV3) _MoveToPage(i);
        }
        await Awaitable.NextFrameAsync();
        foreach (Transform aTf in _contentTfs) aTf.gameObject.SetActive(true);
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        RectTransform getFromPool(RectTransform _parentRT, RectTransform _templateRT)
        {
            RectTransform returnedRT = null;
            foreach (RectTransform aRT in _parentRT) if (!aRT.gameObject.activeSelf) { returnedRT = aRT; break; }
            if (returnedRT == null) returnedRT = Instantiate(_templateRT, _parentRT);
            returnedRT.SetAsLastSibling();
            returnedRT.gameObject.SetActive(true);
            return returnedRT;
        }
        Vector3 getPageLocalPosToMask(Vector3 _aV3) => m_PagesRT.localPosition - m_MaskRT.InverseTransformPoint(_aV3);
        void setSizesSameAsMaskSizes(RectTransform _aRT)
        {
            _aRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_MaskRT.rect.width);
            _aRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_MaskRT.rect.height);
        }
    }
    private void _MoveToPage(int _id, float _duration = 0)
    {
        m_PagesRT.DOLocalMove(_DataPIs[_id].LocalPosToMaskV3, _duration).SetEase(Ease.OutCirc).OnComplete(() =>
        {
            _PageIdNow = _id;
            if (_IsLoop)
            {
                if (_id < _FirstPageId) _PageIdNow = _LastPageId - (_FirstPageId - 1 - _id);
                else if (_id > _LastPageId) _PageIdNow = _FirstPageId + (_id - _LastPageId - 1);
                m_PagesRT.localPosition = _DataPIs[_PageIdNow].LocalPosToMaskV3;
            }
            foreach (PageInfo aPI in _DataPIs) if (aPI.DotActiveRT != null) aPI.DotActiveRT.gameObject.SetActive(aPI.Id == _PageIdNow);
        });
    }
    private void _DoClick(int _id, float _durration) { m_PagesRT.DOComplete(); _MoveToPage(_id, _durration); }

    private class PageInfo
    {
        public RectTransform DotRT, DotActiveRT;
        public Vector3 LocalPosToMaskV3;
        public int Id;
    }
    public class PagesSliderProps
    {
        public bool IsHorizontal, IsFromTopOrLeft, IsShow1Page, IsLoop;
        public PagesSliderProps(bool _isHorizontal, bool _isFromTopOrLeft, bool _isShow1Page, bool _isLoop)
        {
            IsHorizontal = _isHorizontal;
            IsFromTopOrLeft = _isFromTopOrLeft;
            IsShow1Page = _isShow1Page;
            IsLoop = _isLoop;
        }
    }
}