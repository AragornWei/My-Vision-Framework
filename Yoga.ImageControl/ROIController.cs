using HalconDotNet;
using System;
using System.Collections.Generic;

namespace Yoga.ImageControl
{

    public delegate void FuncROIDelegate();
    [Serializable]
    /// <summary>
    /// ROI控制类
    /// </summary>
    public class ROIController
    {
        #region 类的属性
        [NonSerialized]
        private ROI roiSeed;
        private ROIOperation stateROIOperation;
        [NonSerialized]
        private double currX, currY;
        [NonSerialized]
        /// <summary>Index of the active ROI object</summary>
        private int activeROIidx;
        [NonSerialized]
        private int deletedIdx;

        /// <summary>List containing all created ROI objects so far</summary>
        public List<ROI> ROIList = new List<ROI>();
        [NonSerialized]
        /// <summary>
        /// Region obtained by summing up all negative 
        /// and positive ROI objects from the ROIList 
        /// </summary>
        private HRegion modelROI;
        private string activeCol = "green";
        private string activeHdlCol = "red";
        private string inactiveCol = "yellow";
        private string serachRegionCol = "blue";
        private string tuyaColor = "magenta";
        [NonSerialized]
        /// <summary>
        /// Reference to the HWndCtrl, the ROI Controller is registered to
        /// </summary>
        public HWndCtrl viewController;
        #endregion 

        /// <summary>
        /// 当前活动的roi序列
        /// </summary>
        public int ActiveRoiIdx
        {
            get
            {
                return activeROIidx;
            }

            set
            {
                activeROIidx = value;
                TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.UpdateROI));
            }
        }
        [field: NonSerialized()]
        /// <summary>
        /// Delegate that notifies about changes made in the model region
        /// </summary>
        public event EventHandler<ViewEventArgs> ROINotifyEvent;
        /// <summary>Constructor</summary>
        public ROIController()
        {
            stateROIOperation = ROIOperation.Positive;
            activeROIidx = -1;
            modelROI = new HRegion();
            deletedIdx = -1;
            currX = currY = -1;
        }

        public void TiggerROINotifyEvent(ViewEventArgs e)
        {
            if (ROINotifyEvent != null)
            {
                ROINotifyEvent(this, e);
            }
        }
        /// <summary>Registers the HWndCtrl to this ROIController instance</summary>
        public void setViewController(HWndCtrl view)
        {
            viewController = view;
        }

        /// <summary>Gets the ModelROI object</summary>
        public HRegion GetModelRegion()
        {
            return modelROI;
        }
        /// <summary>Get the active ROI</summary>
        public ROI getActiveROI()
        {
            if (activeROIidx != -1)
                return ROIList[activeROIidx];

            return null;
        }

        public int getDelROIIdx()
        {
            return deletedIdx;
        }

        /// <summary>
        /// 为了创建一个新的ROI对象，应用程序类初始化一个“种子”ROI实例并将其传递给ROIController。
        /// ROIController现在通过操纵这个新的ROI实例进行响应。
        /// </summary>
        /// <param name="r">
        /// 'Seed' ROI object forwarded by the application forms class.
        /// </param>
        public void SetROIShape(ROI r)
        {
            roiSeed = r;
            roiSeed.OperatorFlag = stateROIOperation;
        }

        public void SetROIShapeNoOperator(ROI r)
        {
            roiSeed = r;
            roiSeed.OperatorFlag = ROIOperation.None;
            //只能有一个无标志的roi作为搜索框
            for (int i = 0; i < ROIList.Count; i++)
            {
                if (ROIList[i].OperatorFlag == ROIOperation.None)
                {
                    ROIList.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Sets the sign of a ROI object to the value 'mode' (MODE_ROI_NONE,
        /// MODE_ROI_POS,MODE_ROI_NEG)
        /// </summary>
        public void SetROISign(ROIOperation mode)
        {
            stateROIOperation = mode;
            if (activeROIidx != -1)
            {
                if(stateROIOperation== ROIOperation.Tuya)
                {
                    return;
                }
                ROIList[activeROIidx].OperatorFlag = stateROIOperation;
                if(viewController!=null)
                    viewController.Repaint();
                TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.ChangedROISign));
            }
        }
        public void SetROIList(List<ROI> roiList)
        {
            ROIList = roiList;
            foreach (ROI roi in ROIList)
            {
                roi.ReCreateROI();
            }
        }
        /// <summary>
        /// Removes the ROI object that is marked as active. 
        /// If no ROI object is active, then nothing happens. 
        /// </summary>
        public void RemoveActive()
        {
            if (activeROIidx != -1)
            {
                if (ROIList[activeROIidx].OperatorFlag == ROIOperation.Tuya)
                {
                    ResetTuYa();
                }        
                ROIList.RemoveAt(activeROIidx);
                deletedIdx = activeROIidx;
                activeROIidx = -1;
                if (viewController != null)
                    viewController.Repaint();
                TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.DeletedActROI));
            }
        }

        public void RemoveROI(int index)
        {
            if (index<0|| ROIList.Count<index)
            {
                return;
            }
            ROIList.RemoveAt(index);
            activeROIidx = -1;
            if (viewController != null)
                viewController.Repaint();
            TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.DeletedActROI));
        }
        /// <summary>
        /// 获取+-运算后的roi轮廓
        /// </summary>
        public bool DefineModelROI()
        {
            HRegion tmpAdd, tmpDiff, tmp;
            double row, col;

            if (stateROIOperation == ROIOperation.None)  //ROI搜索模式
                return true;

            tmpAdd = new HRegion();
            tmpDiff = new HRegion();
            tmpAdd.GenEmptyRegion();
            tmpDiff.GenEmptyRegion();

            for (int i = 0; i < ROIList.Count; i++)
            {
                switch (ROIList[i].OperatorFlag)
                {
                    case ROIOperation.Positive:
                        tmp = ROIList[i].GetRegion();
                        tmpAdd = tmp.Union2(tmpAdd);   //把所有求和模式的ROI Region联合在一起
                        break;
                    case ROIOperation.Negative:
                        tmp = ROIList[i].GetRegion();
                        tmpDiff = tmp.Union2(tmpDiff);  //把所有求差模式的ROI Region联合在一起。
                        break;

                    case ROIOperation.Tuya:
                        tmp = ROIList[i].GetRegion();
                        if(tmp!=null && tmp.IsInitialized())
                        {
                            tmpDiff = tmp.Union2(tmpDiff);  //把所有求差模式的ROI Region联合在一起。
                        }                        
                        break;

                    default:
                        break;
                }//end of switch
            }//end of for

            modelROI = null;

            if (tmpAdd.AreaCenter(out row, out col) > 0)
            {
                tmp = tmpAdd.Difference(tmpDiff);  
                if (tmp.AreaCenter(out row, out col) > 0) //如果tmpAdd > tmpDiff;
                    modelROI = tmp;                 //把差值赋给modelROI。
            }

            //in case the set of positiv and negative ROIs dissolve 
            if (modelROI == null || ROIList.Count == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Clears all variables managing ROI objects
        /// </summary>
        public void Reset()
        {
            ROIList.Clear();
            ResetTuYa();
            activeROIidx = -1;
            modelROI = null;
            roiSeed = null;
            TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.DelectedAllROIs));
        }

        public void ResetTuYa()
        {
            for(int i = 0; i < ROIList.Count; i++)
            {
                if (ROIList[i].OperatorFlag == ROIOperation.Tuya)
                {
                    ROIList[i].ClearTuYa();
                }
            }
            TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.UpdateROI));
        }

        /// <summary>
        /// Deletes this ROI instance if a 'seed' ROI object has been passed
        /// to the ROIController by the application class.
        /// 
        /// </summary>
        public void resetROI()
        {
            activeROIidx = -1;
            roiSeed = null;
        }

        /// <summary>Defines the colors for the ROI objects</summary>
        /// <param name="aColor">Color for the active ROI object</param>
        /// <param name="inaColor">Color for the inactive ROI objects</param>
        /// <param name="aHdlColor">
        /// Color for the active handle of the active ROI object
        /// </param>
        public void setDrawColor(string aColor, string aHdlColor, string inaColor)
        {
            if (aColor != "")
                activeCol = aColor;
            if (aHdlColor != "")
                activeHdlCol = aHdlColor;
            if (inaColor != "")
                inactiveCol = inaColor;
        }

        /// <summary>
        /// Paints all objects from the ROIList into the HALCON window
        /// </summary>
        /// <param name="window">HALCON window</param>
        public void PaintData(HWindow window, int imageWidth, double txtScale)
        {
            window.SetDraw("margin");
            window.SetLineWidth(1);

            if (ROIList.Count > 0)
            {
                window.SetColor(inactiveCol);

                for (int i = 0; i < ROIList.Count; i++)
                {
                    window.SetDraw("margin");
                    window.SetLineStyle(ROIList[i].FlagLineStyle);
                    ROIList[i].ImageWidth = imageWidth;
                    ROIList[i].TxtScale = txtScale;
                    if (ROIList[i].OperatorFlag == ROIOperation.None)
                    {
                        window.SetColor(serachRegionCol);     //采用搜索区域颜色。
                    }
                    else if(ROIList[i].OperatorFlag == ROIOperation.Tuya)
                    {
                        window.SetDraw("fill");
                        window.SetColor(tuyaColor);
                    }
                    else
                    {
                        window.SetColor(inactiveCol);
                    }
                    ROIList[i].Draw(window);
                }

                if (activeROIidx != -1)
                {
                    if (ROIList[activeROIidx].OperatorFlag == ROIOperation.Tuya)
                    {

                    }
                    else
                    {
                        window.SetDraw("margin");
                        window.SetColor(activeCol);
                        window.SetLineStyle(ROIList[activeROIidx].FlagLineStyle);
                        ROIList[activeROIidx].Draw(window);
                        window.SetColor(activeHdlCol);
                        ROIList[activeROIidx].DisplayActive(window);
                    }   

                }
            }
        }

        /// <summary>
        /// 鼠标按下后对应的roi行为,判断是否在roi区域并记录
        /// </summary>
        /// <param name="imgX">鼠标坐标x</param>
        /// <param name="imgY">鼠标坐标y</param>
        /// <returns>激活的roi序号</returns>
        public int MouseDownAction(double imgX, double imgY)
        {
            int idxROI = -1;
            double max = 10000, dist = 0;
            double epsilon = 0;
            //判断是否是新建roi
            if (roiSeed != null)             //either a new ROI object is created
            {
                roiSeed.ImageWidth = viewController.ImageWidth;
                roiSeed.CreateROI(imgX, imgY);
                ROIList.Add(roiSeed);
                roiSeed = null;
                activeROIidx = ROIList.Count - 1;
                if (viewController != null)
                    viewController.Repaint();

                TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.CreatedROI));
            }
            else if (ROIList.Count > 0)     // ... or an existing one is manipulated
            {
                for (int i = 0; i < ROIList.Count; i++)
                {
                    dist = ROIList[i].DistToClosestHandle(imgX, imgY);
                    epsilon = ROIList[i].GetHandleWidth() + 2.0;
                    if ((dist < max) && (dist < epsilon))
                    {
                        max = dist;
                        idxROI = i;
                    }
                    
                }//end of for

                if ((max > 9999 || max > epsilon)&& activeROIidx !=-1  && ROIList[activeROIidx].OperatorFlag != ROIOperation.Tuya)
                {
                    activeROIidx = -1;
                }

                if (idxROI >= 0)
                {
                    activeROIidx = idxROI;
                    TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.ActivatedROI));
                }

                if (viewController != null)
                    viewController.Repaint();
            }
            return activeROIidx;
        }

        /// <summary>
        /// Reaction of ROI objects to the 'mouse button move' event: moving
        /// the active ROI.
        /// </summary>
        /// <param name="newX">x coordinate of mouse event</param>
        /// <param name="newY">y coordinate of mouse event</param>
        public void MouseMoveAction(double newX, double newY)
        {
            if ((newX == currX) && (newY == currY))
                return;

            ROIList[activeROIidx].MoveByHandle(newX,newY);
            if (viewController != null)
                viewController.Repaint();
            currX = newX;
            currY = newY;
            TiggerROINotifyEvent(new ViewEventArgs(ViewMessage.MovingROI));
        }
    }//end of class
}//end of namespace
