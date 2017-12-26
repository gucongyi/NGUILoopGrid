using System.Collections;
using UnityEngine;

public interface IParamData {
    void SetWidth(float newWidth);
    float GetWidth();
    void SetHeight(float newHeight);
    float GetHeight();
    void SetPosx(float newPosx);
    float GetPosx();
    void SetPosy(float newPosy);
    float GetPosy();
    void Reset();

    //本来Math.Abs(Bounds.Center.Y)-(Bounds.Extent.y)=0，但是这里可能有设计需求!=0，所以这里要计算offset
    //如果有这种offset,直接在初始化的fillitem里修改，存到这里没必要
}

public class LoopGridBaseItem : MonoBehaviour {

    #region member
    protected UILoopTitleGrid m_grid;
    protected IList m_datas;
    //private IList m_datas;
    protected int m_index;
    protected object m_titleData;

    private UIScrollView.Movement m_moveType;
    private UIScrollView m_scrollView;
    #endregion

    #region property
    public int gridIndex { private set; get; }
    public int lineIndex { private set; get; }    
    public UILoopTitleGrid grid {
        set {
            m_grid = value;
            m_scrollView = m_grid.transform.parent.GetComponent<UIScrollView>();
            m_moveType = m_scrollView.movement;
        }
        protected get {
            return m_grid;
        }
    }
    protected MonoBehaviour handleUI {
        get {
            return m_grid.handleUI;
        }
    }
    #endregion

    #region virtual
    public virtual void SetFirstItemData(IList datas, int index) {
        m_datas = datas;
        m_index = index;
    }
    public virtual void FindItem() {
    }
    public virtual void FillItem(IList datas, int index, int gridIndex, int lineIndex) {
        m_index = index;
        this.gridIndex = gridIndex;
        this.lineIndex = lineIndex;        
        float space = m_grid.GetSpliteSpaceByIndex(gridIndex);
        //if(m_moveType == UIScrollView.Movement.Horizontal) {
        //    transform.localPosition = new Vector3(m_grid.cellWidth * gridIndex + space, -m_grid.cellHeight * lineIndex, 0);
        //} else if(m_moveType == UIScrollView.Movement.Vertical) {
        //    transform.localPosition = new Vector3(m_grid.cellWidth * lineIndex, -m_grid.cellHeight * gridIndex - space, 0);
        //}
        IParamData paramData = datas[index] as IParamData;
        SetDynamicPosition(paramData, gridIndex, lineIndex, space);
#if DEBUG
        gameObject.name = index.ToStringNoGC();
#endif
        //GetComponentInChildren<UILabel>().text = index.ToStringNoGC();

        gameObject.UnRegistUIButton();
        m_titleData = null;
        bool hasTileData = lineIndex == 0 && m_grid.TryGetTitleData(gridIndex, out m_titleData);
        gameObject.SetActive(datas[index] != null || hasTileData);
    }
    #endregion

    #region public api
    public bool UpdateItem() {
        bool result = false;
        do {
            if(m_datas == null) break;
            if(m_datas.Count <= 0) break;
            if(!IsIndexExit()) break;
            FillItem(m_datas, m_index, gridIndex, lineIndex);
            result = true;
        } while(false);

        return result;
    }
    #endregion

    #region private
    private void SetItemPositionByIndex(int gridIndex, int lineIndex) {
        this.gridIndex = gridIndex;
        this.lineIndex = lineIndex;
        float space = m_grid.GetSpliteSpaceByIndex(gridIndex);
        if(m_moveType == UIScrollView.Movement.Horizontal) {
            transform.localPosition = new Vector3(m_grid.cellWidth * gridIndex + space, -m_grid.cellHeight * lineIndex, 0);
        } else if(m_moveType == UIScrollView.Movement.Vertical) {
            transform.localPosition = new Vector3(m_grid.cellWidth * lineIndex, -m_grid.cellHeight * gridIndex - space, 0);
        }
    }
    private bool IsIndexExit() {
        return m_datas.Count > m_index;
    }
    private void SetDynamicPosition(IParamData paramData, int gridIndex, int lineIndex, float space) {
        if (m_moveType == UIScrollView.Movement.Horizontal) {
            if (paramData == null) {
                transform.localPosition = new Vector3(m_grid.cellWidth * gridIndex + space, -m_grid.cellHeight * lineIndex, 0);
            }
            else {
                float totalCellWidth = paramData.GetPosx();
                if (totalCellWidth <= 0) {
                    if (m_index == 0) {
                        totalCellWidth = 0;
                    }
                    else {
                        totalCellWidth = (m_datas[m_index - 1] as IParamData).GetPosx() + (m_datas[m_index - 1] as IParamData).GetWidth() + space;
                    }
                }
                paramData.SetPosx(totalCellWidth);
                transform.localPosition = new Vector3(totalCellWidth , -m_grid.cellHeight * lineIndex, 0);
            }
        }
        else if (m_moveType == UIScrollView.Movement.Vertical) {
            if (paramData == null) {
                transform.localPosition = new Vector3(m_grid.cellWidth * lineIndex, -m_grid.cellHeight * gridIndex - space, 0);
            }
            else {
                float totalCellHeight = paramData.GetPosy();
                if (totalCellHeight <= 0)
                {
                    if (m_index == 0)
                    {
                        totalCellHeight = 0;
                    }
                    else
                    {
                        totalCellHeight = (m_datas[m_index - 1] as IParamData).GetPosy() +
                                          (m_datas[m_index - 1] as IParamData).GetHeight() - space;
                    }
                }
                else
                {
                    totalCellHeight = (m_datas[m_index - 1] as IParamData).GetPosy() +
                                      (m_datas[m_index - 1] as IParamData).GetHeight() - space;
                }
                paramData.SetPosy(totalCellHeight);
                transform.localPosition = new Vector3(m_grid.cellWidth * lineIndex, -totalCellHeight - space, 0);
            }
        }
    }

    protected virtual void ChangeDynamicPosition(IParamData paramData, int gridIndex, int lineIndex, float space ,bool autoRefreshData = true,bool resetFront =false) {
        if (m_moveType == UIScrollView.Movement.Horizontal) {
            if(resetFront)
            {
                for (int i = 0; i < m_datas.Count; i++)
                {
                    //if(i< m_index + 1)
                    //{
                    //    IParamData now = m_datas[i] as IParamData;
                    //    now.SetPosx(0);
                    //}
                    //else
                    //{
                    //    IParamData now = m_datas[i] as IParamData;
                    //    IParamData front = m_datas[i - 1] as IParamData;
                    //    now.SetPosx(front.GetPosx() + front.GetWidth());
                    //}
                    IParamData now = m_datas[i] as IParamData;
                    if (i < m_index)
                    {
                        now.Reset();
                    }
                    if (i >= 1)
                    {
                        IParamData front = m_datas[i - 1] as IParamData;
                        now.SetPosx(front.GetPosx() + front.GetWidth());
                    }
                }
            }
            else
            {
                for (int i = m_index + 1; i < m_datas.Count; i++)
                {
                    IParamData now = m_datas[i] as IParamData;
                    IParamData front = m_datas[i - 1] as IParamData;
                    now.SetPosx(front.GetPosx() + front.GetWidth());
                }
            }
            
        }
        else if (m_moveType == UIScrollView.Movement.Vertical) {
            if(resetFront)
            {
                for (int i = 0; i < m_datas.Count; i++)
                {
                    //if (i < m_index + 1)
                    //{
                    //    IParamData now = m_datas[i] as IParamData;
                    //    now.SetPosy(0);
                    //}
                    //else
                    //{
                    //    IParamData now = m_datas[i] as IParamData;
                    //    IParamData front = m_datas[i - 1] as IParamData;
                    //    now.SetPosy(front.GetPosy() + front.GetHeight());
                    //}
                    IParamData now = m_datas[i] as IParamData;
                    if (i < m_index )
                    {
                        now.Reset();
                    }
                    if (i >= 1)
                    {
                        IParamData front = m_datas[i - 1] as IParamData;
                        now.SetPosy(front.GetPosy() + front.GetHeight());
                    }
                }
            }
            else
            {
                for (int i = m_index + 1; i < m_datas.Count; i++)
                {
                    IParamData now = m_datas[i] as IParamData;
                    IParamData front = m_datas[i - 1] as IParamData;
                    now.SetPosy(front.GetPosy() + front.GetHeight());
                }
            }
        }
        if(autoRefreshData)
        {
            grid.RefreshData();
        }
    }
    #endregion


    public virtual void Selected() {

    }

    public virtual void UnSelected() {

    }
}