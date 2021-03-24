using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using Yoga.ImageControl;
using Yoga.Tools;
using System.IO;

namespace Yoga.HILTI.AutoPacking.GeneralTool
{

    public partial class CommonToolParamSetting : ToolsSettingUnit
    {
        CommonTool commonTool;
        public HWndCtrl mView;
        public ROIController ROIController;
        private HImage CurrentImg;
        public CommonToolParamSetting(CommonTool commonTool)
        {
            InitializeComponent();
            this.commonTool = commonTool;
            mView = hWndUnit1.HWndCtrl;
            ROIController = new ROIController();
            ROIController.ROINotifyEvent += new EventHandler<ViewEventArgs>(UpdateROI);
            roiActUnit1.RoiController = ROIController;
            mView.useROIController(ROIController);
            ROIController.SetROISign(ROIOperation.Positive);
            commonTool.NotifyExcInfo = new ExeInfo(ExecuteInformation);

            locked = true;
            if (!commonTool.engineIsnitial)
            {
                commonTool.InitialEngine();
            }
            base.Init(commonTool.Name, commonTool);
            Init();
            locked = false;
            commonTool.NotifyExcInfo = new ExeInfo(ExecuteInformation);
            ExecuteInformation("初始化完成。");
        }

        private void Init()
        {
            InitCreateShapeMode();
            InitFindShapeMode();
            InitGrabPointSetting();
            InitPzt_Detection();
            InitGrabIntervene();
            Init_LD_DP();
            Init_FangDai();
            Init_LanQuan();
        }
        int messageCount = 0;
        public void ExecuteInformation(string message)
        {
            StringBuilder strb = new StringBuilder();
            if (message == string.Empty)
            {
                return;
            }
            messageCount++;
            strb.Append(message);
            strb.Append(Environment.NewLine);
            txt_Log.AppendText(strb.ToString());
            //超过1000行就删除500行
            if (messageCount > 1000)
            {
                string[] lines = txt_Log.Lines;
                List<string> a = lines.ToList();
                a.RemoveRange(0, 500);           //删除之前必须转换成链表。
                txt_Log.Lines = a.ToArray();
                messageCount = 500;
            }
            txt_Log.ScrollToCaret();

        }
        #region 初始化控件
        private void Init_LanQuan()
        {
            nUpDown_LQ_filterRadius.Value = (decimal)commonTool.MLanQuan.filterRadiu;
            nUpDown_LQ_minThreshold.Value = (decimal)commonTool.MLanQuan.minThreshold;
            nUpDown_LQ__minArea.Value = (decimal)commonTool.MLanQuan.minArea;
            checkBox_LanQuan.Checked = commonTool.bLanQuan_Enable;
            groupBox_LanQuan.Enabled = commonTool.bLanQuan_Enable;
            cB_isBackflow_LQ.Checked = commonTool.bIsBackflow_LQ;
        }
        private void Init_FangDai()
        {
            nUpDown_FD_minThreshold.Value = (decimal)commonTool.MFangDai.minThreshold;
            nUpDown_FD_minArea.Value = (decimal)commonTool.MFangDai.minArea;
            nUpDown_FD_maxArea.Value = (decimal)commonTool.MFangDai.maxArea;
            checkBox_FangDai.Checked = commonTool.bFangDai_Enable;
            groupBox_FangDai.Enabled = commonTool.bFangDai_Enable;

        }
        private void Init_LD_DP()
        {
            NUpDownNUpDown_LM_DP_filterRadiu.Value = (decimal)commonTool.MLM_DP.filterRadiu;
            NUpDown_LD_DP_minAreaThreshold.Value = (decimal)commonTool.MLM_DP.minAreaThread;
            NUpDown_LM_DP_minThreshold.Value = (decimal)commonTool.MLM_DP.minThreshold;
            NUD_LD_DP_LuMuMaxArea.Value = (decimal)commonTool.MLM_DP.luoMuMaxArea;
            nUD_LMDP_LMWidth.Value = (decimal)commonTool.MLM_DP.luoMuWidth;
            checkBox_LM_DP.Checked = commonTool.bLM_DP_Enble;
            groupBox_LM_DP.Enabled = commonTool.bLM_DP_Enble;
            cB_IsBackflow_LMDP.Checked = commonTool.bIsBackflow_LMDP;
        }

        private void InitGrabIntervene()
        {
            nUpDown_GrabIntervene_filterRadiu.Value = (decimal)commonTool.MGrab_Intervene.filterRadiu;
            nUpDown_GrabIntervene_minArea.Value = (decimal)commonTool.MGrab_Intervene.minAreaThread;
            nUpDown_GrabIntervene_minThreshold.Value = (decimal)commonTool.MGrab_Intervene.minThreshold;
            checkBox_GrabIntervene_Enable.Checked = commonTool.bGrab_Intervene_Enble;
            groupBox_GrabIntervene.Enabled = commonTool.bGrab_Intervene_Enble;
        }

        public void InitPzt_Detection()
        {
            mesureNumber_UpDown.Value = (decimal)commonTool.MPZT_Detection.mesureNumber;
            mes_width_UpDown.Value = (decimal)commonTool.MPZT_Detection.mes_width;
            Sigma_UpDown.Value = (decimal)commonTool.MPZT_Detection.sigma;
            contrast_Threshold_UpDown.Value = (decimal)commonTool.MPZT_Detection.threshold;
            dist_min_PZT_UpDown.Value = (decimal)commonTool.MPZT_Detection.dist_STD;
            dist_max_PZT_UpDown.Value = (decimal)commonTool.MPZT_Detection.dist_max;
            PZT_checkBox.Checked = commonTool.bPZT_Detection_Enble;
            groupBox_PZT_Detection.Enabled = commonTool.bPZT_Detection_Enble;
            cB_IsBackflow_PZT.Checked = commonTool.bIsBackflow_PZT;

        }

        private void InitGrabPointSetting()
        {
            txt_Grab_Row.Text = commonTool.MGrabPointSetting.GrabRowOrg.ToString("f2");
            txt_Grab_Col.Text = commonTool.MGrabPointSetting.GrabColOrg.ToString("f2");
            txt_Grab_Row.ReadOnly = true;
            txt_Grab_Col.ReadOnly = true;
            txt_Robot_X.Text = commonTool.MGrabPointSetting.X.ToString("f2");
            txt_Robot_Y.Text = commonTool.MGrabPointSetting.Y.ToString("f2");
            rbtn_Grab_From_Pictrue.Checked = commonTool.MGrabPointSetting.fromPictrue;
            rbtn_from_Robot.Checked = !rbtn_Grab_From_Pictrue.Checked;
            ckb_IsCalibration.Checked = commonTool.bIsCalibration;
            gb_IsCalibration.Enabled = ckb_IsCalibration.Checked;
            if (ckb_IsCalibration.Checked)
            {
                txt_CameraPath.Text = commonTool.cameraParamPath;
                txt_worldPosePath.Text = commonTool.worldPosePath;
            }
            txt_X_Compensation.Text = commonTool.x_Compensation.ToString();
            txt_Y_Compensation.Text = commonTool.y_Compensation.ToString();
            txt_Angle_Compensation.Text = commonTool.angle_Compensation.ToString();

        }

        private void InitFindShapeMode()
        {
            MinScoreUpDown.Value = (decimal)commonTool.MFindShapeMode.minScore;
            NumMatchesUpDown.Value = (decimal)commonTool.MFindShapeMode.numMatches;
            GreedinessUpDown.Value = (decimal)commonTool.MFindShapeMode.greediness;
            MaxOverlapUpDown.Value = (decimal)commonTool.MFindShapeMode.maxOverlap;
            SubPixelBox.Text = commonTool.MFindShapeMode.subPixel;
            LastPyrLevUpDown.Value = (decimal)commonTool.MFindShapeMode.numLevels;

            ///模板过滤
            nud_LG_lenght_min.Value = (decimal)commonTool.MFindShapeMode.LG_lenght_min;
            nud_LG_lenght_max.Value = (decimal)commonTool.MFindShapeMode.LG_lenght_max;
            nud_DP_width_min.Value = (decimal)commonTool.MFindShapeMode.DP_width_min;
            nud_DP_width_max.Value = (decimal)commonTool.MFindShapeMode.DP_width_max;
            nud_LG_Area_min.Value = (decimal)commonTool.MFindShapeMode.LG_Area_min;
            nud_LG_Area_max.Value = (decimal)commonTool.MFindShapeMode.LG_Area_max;
            nud_LG_threshould.Value = (decimal)commonTool.MFindShapeMode.LG_threshould;
            nud_Closing_width.Value = (decimal)commonTool.MFindShapeMode.Closing_width;
            nud_Closing_height.Value = (decimal)commonTool.MFindShapeMode.Closing_height;

        }

        private void InitCreateShapeMode()
        {
            if (commonTool.MCreateShapeModel.shapeModelROIList != null)
            {
                ROIController.ROIList = commonTool.MCreateShapeModel.shapeModelROIList;
            }
            ContrastHighUpDown.Value = (decimal)commonTool.MCreateShapeModel.contrastHigh;
            ContrastLowUpDown.Value = (decimal)commonTool.MCreateShapeModel.contrastLow;
            MinLenghtUpDown.Value = (decimal)commonTool.MCreateShapeModel.minLength;
            MinScaleUpDown.Value = (decimal)commonTool.MCreateShapeModel.scaleMin;
            MaxScaleUpDown.Value = (decimal)commonTool.MCreateShapeModel.scaleMax;
            StartingAngleUpDown.Value = (decimal)(commonTool.MCreateShapeModel.angleStart * 180.0 / Math.PI);
            AngleExtentUpDown.Value = (decimal)(commonTool.MCreateShapeModel.angleExtent * 180.0 / Math.PI);
            AngleStepUpDown.Value = (decimal)(commonTool.MCreateShapeModel.angleStep * 180.0 / Math.PI);
            PyramidLevelUpDown.Value = (decimal)commonTool.MCreateShapeModel.numLevels;
            MetricBox.Text = commonTool.MCreateShapeModel.metric;
            OptimizationBox.Text = commonTool.MCreateShapeModel.optimization;
            MinContrastUpDown.Value = (decimal)commonTool.MCreateShapeModel.minContrast;
            ScaleStepUpDown.Value = (decimal)commonTool.MCreateShapeModel.scaleStep;

        }

        #endregion
        //功能设定切换
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {

            switch (tabControl.SelectedIndex)
            {
                case 0:
                    CreateShapeModeSetting();
                    break;
                case 1:
                    FindShapeModeSetting();
                    break;
                case 2:
                    GrabPointDispSetting();
                    break;
                case 3:
                    PZT_DetectionSetting();
                    break;
                case 4:
                    GrabInterveneSetting();
                    break;
                case 5:
                    LM_DP_Setting();
                    break;
                case 6:
                    FangDai_Setting();
                    break;
                case 7:
                    LanQuan_Setting();
                    break;
                default:
                    break;
            }
            mView.Repaint();
        }


        #region 功能切换设定

        private void FindShapeModeSetting()
        {
            roiActUnit1.Enabled = true;
            Panel_Test.Enabled = true;
            mView.SetDispLevel(ShowMode.IncludeROI);
            CurrentImg = commonTool.ImageRefIn;
            mView.ClearList();
            mView.AddIconicVar(commonTool.ImageRefIn);
            if (commonTool.MFindShapeMode.FindShapeModeRoiList != null && commonTool.MFindShapeMode.FindShapeModeRoiList.Count > 0)
            {
                ROIController.ROIList = commonTool.MFindShapeMode.FindShapeModeRoiList;
            }
            else
            {
                ROIController.ROIList = new List<ROI>();
            }
            ROIController.ActiveRoiIdx = -1;
            bool var = commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, commonTool.ImageRefIn);
            if (var)
                commonTool.MFindShapeMode.ShowResult(mView);
        }

        private void CreateShapeModeSetting()
        {
            roiActUnit1.Enabled = true;
            Panel_Test.Enabled = false;
            mView.SetDispLevel(ShowMode.IncludeROI);
            CurrentImg = commonTool.ImageRefIn;
            mView.ClearList();
            mView.AddIconicVar(CurrentImg);
            if (commonTool.MCreateShapeModel.shapeModelROIList != null && commonTool.MCreateShapeModel.shapeModelROIList.Count > 0)
            {
                ROIController.ROIList = commonTool.MCreateShapeModel.shapeModelROIList;
            }
            else
            {
                ROIController.ROIList = new List<ROI>(); ;
            }
            ROIController.ActiveRoiIdx = -1;
            commonTool.MCreateShapeModel.ShowShapeModel(mView);
        }

        private void PZT_DetectionSetting()
        {
            if (PZT_checkBox.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
            Panel_Test.Enabled = true;
            mView.SetDispLevel(ShowMode.IncludeROI);
            if (commonTool.MPZT_Detection.Pzt_ROIList != null && commonTool.MPZT_Detection.Pzt_ROIList.Count > 0)
            {
                ROIController.ROIList = commonTool.MPZT_Detection.Pzt_ROIList;
            }
            else
            {
                ROIController.ROIList = new List<ROI>(); ;
            }
            ROIController.ActiveRoiIdx = -1;
            CurrentImg = commonTool.ImageRefIn;
            mView.AddIconicVar(CurrentImg);
            commonTool.Run(CurrentImg);
            mView.AddIconicVar(CurrentImg);
            if (commonTool.bPZT_Detection_Result)
            {
                commonTool.MPZT_Detection.Show(mView);
            }
        }

        private void GrabInterveneSetting()
        {
            if (checkBox_GrabIntervene_Enable.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }

            Panel_Test.Enabled = true;
            mView.SetDispLevel(ShowMode.IncludeROI);
            ROIController.ROIList = null;
            if (commonTool.MGrab_Intervene.Grab_Intervene_ROIList != null && commonTool.MGrab_Intervene.Grab_Intervene_ROIList.Count > 0)
            {
                ROIController.ROIList = commonTool.MGrab_Intervene.Grab_Intervene_ROIList;
            }
            else
            {
                ROIController.ROIList = new List<ROI>();
            }
            ROIController.ActiveRoiIdx = -1;
            CurrentImg = commonTool.ImageRefIn;
            mView.AddIconicVar(CurrentImg);
            commonTool.Run(CurrentImg);
            mView.AddIconicVar(CurrentImg);
            if (commonTool.bGrab_Intervene_Result)
            {
                commonTool.MGrab_Intervene.Show(mView);
            }
        }

        private void LM_DP_Setting()
        {
            if (checkBox_LM_DP.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
            Panel_Test.Enabled = true;
            mView.SetDispLevel(ShowMode.IncludeROI);
            ROIController.ROIList = null;
            if (commonTool.MLM_DP.LM_DP_ROIList != null && commonTool.MLM_DP.LM_DP_ROIList.Count > 0)
            {
                ROIController.ROIList = commonTool.MLM_DP.LM_DP_ROIList;
            }
            else
            {
                ROIController.ROIList = new List<ROI>();
            }
            ROIController.ActiveRoiIdx = -1;
            CurrentImg = commonTool.ImageRefIn;
            mView.AddIconicVar(CurrentImg);
            commonTool.Run(CurrentImg);
            mView.AddIconicVar(CurrentImg);
            if (commonTool.bLM_DP_Result)
            {
                commonTool.MLM_DP.Show(mView);
            }
        }
        private void FangDai_Setting()
        {
            if (checkBox_FangDai.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
            Panel_Test.Enabled = true;
            mView.SetDispLevel(ShowMode.IncludeROI);
            ROIController.ROIList = null;
            if (commonTool.MFangDai.FangDai_ROIList != null && commonTool.MFangDai.FangDai_ROIList.Count > 0)
            {
                ROIController.ROIList = commonTool.MFangDai.FangDai_ROIList;
            }
            else
            {
                ROIController.ROIList = new List<ROI>();
            }
            ROIController.ActiveRoiIdx = -1;
            CurrentImg = commonTool.ImageRefIn;
            mView.AddIconicVar(CurrentImg);
            commonTool.Run(CurrentImg);
            mView.AddIconicVar(CurrentImg);
            if (commonTool.bFangDai_Result)
            {
                commonTool.MFangDai.Show(mView);
            }
        }

        private void LanQuan_Setting()
        {
            if (checkBox_LanQuan.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
            Panel_Test.Enabled = true;
            mView.SetDispLevel(ShowMode.IncludeROI);
            ROIController.ROIList = null;
            if (commonTool.MLanQuan.LanQuan_ROIList != null && commonTool.MLanQuan.LanQuan_ROIList.Count > 0)
            {
                ROIController.ROIList = commonTool.MLanQuan.LanQuan_ROIList;
            }
            else
            {
                ROIController.ROIList = new List<ROI>();
            }
            ROIController.ActiveRoiIdx = -1;
            CurrentImg = commonTool.ImageRefIn;
            mView.AddIconicVar(CurrentImg);
            commonTool.Run(CurrentImg);
            mView.AddIconicVar(CurrentImg);
            if (commonTool.bLanQuan_Result)
            {
                commonTool.MLanQuan.Show(mView);
            }
        }
        #endregion
        private void UpdateROI(object sender, ViewEventArgs e)
        {
            switch (e.ViewMessage)
            {
                case ViewMessage.DelectedAllROIs:
                    switch (tabControl.SelectedIndex)
                    {
                        case 0:
                            commonTool.MCreateShapeModel.Reset();
                            mView.AddIconicVar(commonTool.ImageRefIn);
                            mView.Repaint();
                            break;
                        case 1:

                            commonTool.MFindShapeMode.Reset();
                            mView.AddIconicVar(commonTool.ImageRefIn);
                            mView.Repaint();
                            break;

                        case 3:
                            commonTool.MPZT_Detection.Reset();
                            mView.AddIconicVar(commonTool.ImageRefIn);
                            mView.Repaint();
                            break;
                        case 4:
                            commonTool.MGrab_Intervene.Reset();
                            mView.AddIconicVar(commonTool.ImageRefIn);
                            mView.Repaint();
                            break;
                        case 5:
                            commonTool.MLM_DP.Reset();
                            mView.AddIconicVar(commonTool.ImageRefIn);
                            mView.Repaint();
                            break;
                        case 6:
                            commonTool.MFangDai.Reset();
                            mView.AddIconicVar(commonTool.ImageRefIn);
                            mView.Repaint();
                            break;
                        case 7:
                            commonTool.MLanQuan.Reset();
                            mView.AddIconicVar(commonTool.ImageRefIn);
                            mView.Repaint();
                            break;

                        default:
                            break;
                    }
                    break;
                case ViewMessage.ChangedROISign:
                case ViewMessage.DeletedActROI:
                case ViewMessage.UpdateROI:
                case ViewMessage.CreatedROI:
                    switch (tabControl.SelectedIndex)
                    {
                        case 0:
                            UpdateShapeModelROI();
                            ShowShapeModel();
                            break;
                        case 1:
                            if (!ROIController.DefineModelROI())
                                return;
                            if (commonTool.MFindShapeMode.SearchRegion != null && commonTool.MFindShapeMode.SearchRegion.IsInitialized())
                            {
                                commonTool.MFindShapeMode.SearchRegion.Dispose();
                            }
                            commonTool.MFindShapeMode.SearchRegion = ROIController.GetModelRegion();
                            commonTool.MFindShapeMode.FindShapeModeRoiList = ROIController.ROIList;

                            CurrentImg = commonTool.ImageRefIn;

                            if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                            {
                                mView.AddIconicVar(CurrentImg);
                                commonTool.MFindShapeMode.ShowResult(mView);
                                mView.Repaint();
                            }
                            break;

                        case 3:
                            if (!ROIController.DefineModelROI())
                                return;
                            if (commonTool.MPZT_Detection.Pzt_Region != null && commonTool.MPZT_Detection.Pzt_Region.IsInitialized())
                            {
                                commonTool.MPZT_Detection.Pzt_Region.Dispose();
                            }
                            commonTool.MPZT_Detection.Pzt_Region = ROIController.GetModelRegion();
                            commonTool.MPZT_Detection.Pzt_ROIList = ROIController.ROIList;
                            CurrentImg = commonTool.ImageRefIn;

                            commonTool.Run(CurrentImg);
                            PZT_DetectionShow();


                            break;
                        case 4:
                            if (!ROIController.DefineModelROI())
                                return;
                            commonTool.MGrab_Intervene.Grab_Intervene_ROIList = ROIController.ROIList;
                            if (commonTool.MGrab_Intervene.Grab_Intervene_Region != null && commonTool.MGrab_Intervene.Grab_Intervene_Region.IsInitialized())
                            {
                                commonTool.MGrab_Intervene.Grab_Intervene_Region.Dispose();
                            }
                            commonTool.MGrab_Intervene.Grab_Intervene_Region = ROIController.GetModelRegion();
                            commonTool.MGrab_Intervene.Grab_Intervene_ROIList = ROIController.ROIList;
                            CurrentImg = commonTool.ImageRefIn;
                            commonTool.Run(CurrentImg);
                            GrabIntervene_DetectionShow();
                            break;
                        case 5:
                            if (!ROIController.DefineModelROI())
                                return;
                            commonTool.MLM_DP.LM_DP_ROIList = ROIController.ROIList;
                            if (commonTool.MLM_DP.LM_DP_Region != null && commonTool.MLM_DP.LM_DP_Region.IsInitialized())
                            {
                                commonTool.MLM_DP.LM_DP_Region.Dispose();
                            }
                            commonTool.MLM_DP.LM_DP_Region = ROIController.GetModelRegion();
                            commonTool.MLM_DP.LM_DP_ROIList = ROIController.ROIList;
                            CurrentImg = commonTool.ImageRefIn;
                            commonTool.Run(CurrentImg);
                            LM_DP_DetectionShow();
                            break;
                        case 6:
                            if (!ROIController.DefineModelROI())
                                return;
                            commonTool.MFangDai.FangDai_ROIList = ROIController.ROIList;
                            if (commonTool.MFangDai.FangDai_Region != null && commonTool.MFangDai.FangDai_Region.IsInitialized())
                            {
                                commonTool.MFangDai.FangDai_Region.Dispose();
                            }
                            commonTool.MFangDai.FangDai_Region = ROIController.GetModelRegion();
                            commonTool.MFangDai.FangDai_ROIList = ROIController.ROIList;
                            CurrentImg = commonTool.ImageRefIn;
                            commonTool.Run(CurrentImg);
                            FangDai_DetectionShow();
                            break;
                        case 7:
                            if (!ROIController.DefineModelROI())
                                return;
                            commonTool.MLanQuan.LanQuan_ROIList = ROIController.ROIList;
                            if (commonTool.MLanQuan.LanQuan_Region != null && commonTool.MLanQuan.LanQuan_Region.IsInitialized())
                            {
                                commonTool.MLanQuan.LanQuan_Region.Dispose();
                            }
                            commonTool.MLanQuan.LanQuan_Region = ROIController.GetModelRegion();
                            commonTool.MLanQuan.LanQuan_ROIList = ROIController.ROIList;
                            CurrentImg = commonTool.ImageRefIn;
                            commonTool.Run(CurrentImg);
                            LanQuan_DetectionShow();
                            break;
                        default:
                            break;
                    }
                    break;
                case ViewMessage.ErrReadingImage:
                    break;
                default:
                    break;
            }
        }
        //启动时显示
        public override void ShowTranResult()
        {
            base.ShowTranResult();
            UpdateShapeModelROI();
            ShowShapeModel();
        }
        //窗口关闭退出时清除
        public override void Clear()
        {
            base.Clear();
            commonTool.StopDebugMode();
            if (modelPointXld != null && modelPointXld.IsInitialized())
                modelPointXld.Dispose();
            modelPointXld = null;
            CurrentImg = null;
        }

        #region 创建模板
        private void UpdateShapeModelROI()
        {
            if (!ROIController.DefineModelROI())
                return;
            if (commonTool.MCreateShapeModel.ModelRegion != null && commonTool.MCreateShapeModel.ModelRegion.IsInitialized())
            {
                commonTool.MCreateShapeModel.modelRegion.Dispose();
            }
            commonTool.MCreateShapeModel.modelRegion = ROIController.GetModelRegion();
            commonTool.MCreateShapeModel.shapeModelROIList = ROIController.ROIList;



            commonTool.MCreateShapeModel.CreateShapeModelAct(commonTool.ImageRefIn);
        }
        private void ShowShapeModel()
        {
            mView.ClearList();
            mView.SetDispLevel(ShowMode.IncludeROI);
            mView.ChangeGraphicSettings(Mode.LINESTYLE, new HTuple());
            mView.AddIconicVar(commonTool.ImageRefIn);
            mView.ChangeGraphicSettings(Mode.DRAWMODE, "margin");
            commonTool.MCreateShapeModel.ShowShapeModel(mView);
            mView.Repaint();
        }
        private void ContrastHighUpDown_ValueChanged(object sender, EventArgs e)
        {
            int val = (int)ContrastHighUpDown.Value;
            ContrastLowUpDown.Maximum = (decimal)val;
            if (!locked)
            {
                commonTool.MCreateShapeModel.contrastHigh = val;
                if (commonTool.MCreateShapeModel.modelRegion == null || !commonTool.MCreateShapeModel.modelRegion.IsInitialized())
                    return;
                mView.ClearList();
                mView.AddIconicVar(commonTool.ImageRefIn);
                if (commonTool.MCreateShapeModel.CreateShapeModelAct(commonTool.ImageRefIn))
                {
                    commonTool.MCreateShapeModel.ShowShapeModel(mView);
                }
                else
                {
                    MessageBox.Show("创建模板失败", "创建模板失败请检查参数", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                mView.Repaint();
            }

        }

        private void ContrastLowUpDown_ValueChanged(object sender, EventArgs e)
        {
            int val = (int)ContrastLowUpDown.Value;
            ContrastHighUpDown.Minimum = (decimal)val;
            MinContrastUpDown.Maximum = (decimal)val;
            if (!locked)
            {
                commonTool.MCreateShapeModel.contrastLow = val;
                if (commonTool.MCreateShapeModel.modelRegion == null || !commonTool.MCreateShapeModel.modelRegion.IsInitialized())
                    return;
                mView.ClearList();
                mView.AddIconicVar(commonTool.ImageRefIn);
                if (commonTool.MCreateShapeModel.CreateShapeModelAct(commonTool.ImageRefIn))
                {
                    commonTool.MCreateShapeModel.ShowShapeModel(mView);
                }
                else
                {
                    MessageBox.Show("创建模板失败", "创建模板失败请检查参数", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                mView.Repaint();
            }
        }

        private void MinContrastUpDown_ValueChanged(object sender, EventArgs e)
        {
            int val = (int)MinContrastUpDown.Value;
            ContrastLowUpDown.Minimum = (decimal)val;

            if (!locked)
            {
                commonTool.MCreateShapeModel.minContrast = val;
                if (commonTool.MCreateShapeModel.modelRegion == null || !commonTool.MCreateShapeModel.modelRegion.IsInitialized())
                    return;
                mView.ClearList();
                mView.AddIconicVar(commonTool.ImageRefIn);
                if (commonTool.MCreateShapeModel.CreateShapeModelAct(commonTool.ImageRefIn))
                {
                    commonTool.MCreateShapeModel.ShowShapeModel(mView);
                }
                else
                {
                    MessageBox.Show("创建模板失败", "创建模板失败请检查参数", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                mView.Repaint();
            }
        }

        private void MinLenghtUpDown_ValueChanged(object sender, EventArgs e)
        {
            int val = (int)MinLenghtUpDown.Value;
            if (!locked)
            {
                commonTool.MCreateShapeModel.minLength = val;
                mView.ClearList();
                mView.AddIconicVar(commonTool.ImageRefIn);
                if (commonTool.MCreateShapeModel.CreateShapeModelAct(commonTool.ImageRefIn))
                {
                    commonTool.MCreateShapeModel.ShowShapeModel(mView);
                }
                else
                {
                    MessageBox.Show("创建模板失败", "创建模板失败请检查参数", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                mView.Repaint();
            }
        }

        private void StartingAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!locked)
            {
                commonTool.MCreateShapeModel.angleStart = (double)(StartingAngleUpDown.Value) / 180 * Math.PI;
                commonTool.MCreateShapeModel.createNewModelID = true;
            }
        }

        private void AngleExtentUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!locked)
            {
                commonTool.MCreateShapeModel.angleExtent = (double)(AngleExtentUpDown.Value) / 180 * Math.PI;
                commonTool.MCreateShapeModel.createNewModelID = true;
            }
        }

        private void AngleStepUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!locked)
            {
                commonTool.MCreateShapeModel.angleStep = (double)(AngleStepUpDown.Value) / 180 * Math.PI;
                commonTool.MCreateShapeModel.createNewModelID = true;
            }
        }

        private void MinScaleUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = MinScaleUpDown.Value;
            //if (val > MaxScaleUpDown.Value)
            //    MaxScaleUpDown.Value = val;
            MaxScaleUpDown.Minimum = val;
            if (!locked)
            {
                commonTool.MCreateShapeModel.scaleMin = (double)val;
                commonTool.MCreateShapeModel.createNewModelID = true;
            }
        }

        private void PyramidLevelUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!locked)
            {
                commonTool.MCreateShapeModel.numLevels = (int)PyramidLevelUpDown.Value;
                commonTool.MCreateShapeModel.createNewModelID = true;
            }
        }

        private void MaxScaleUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = MaxScaleUpDown.Value;
            MinScaleUpDown.Maximum = val;
            //if (MaxScaleUpDown.Value < MinScaleUpDown.Value)
            //{
            //    MinScaleUpDown.Value = MaxScaleUpDown.Value;
            //}
            if (!locked)
            {
                commonTool.MCreateShapeModel.scaleMax = (double)MaxScaleUpDown.Value;
                commonTool.MCreateShapeModel.createNewModelID = true;
            }
        }
        private void ScaleStepUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = ScaleStepUpDown.Value;


            if (!locked)
            {
                commonTool.MCreateShapeModel.scaleStep = (double)val;
                commonTool.MCreateShapeModel.createNewModelID = true;
            }
        }

        private void MetricBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            commonTool.MCreateShapeModel.metric = MetricBox.Text;
            commonTool.MCreateShapeModel.createNewModelID = true;
        }

        private void OptimizationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            commonTool.MCreateShapeModel.optimization = OptimizationBox.Text;
            commonTool.MCreateShapeModel.createNewModelID = true;
        }

        private void btn_CreateModeShape_Click(object sender, EventArgs e)
        {
            if (commonTool.MCreateShapeModel.modelRegion != null && commonTool.MCreateShapeModel.modelRegion.IsInitialized())
            {
                mView.ClearList();
                mView.AddIconicVar(commonTool.ImageRefIn);
                if (commonTool.MCreateShapeModel.CreateShapeModelAct(commonTool.ImageRefIn))
                {
                    commonTool.MCreateShapeModel.ShowShapeModel(mView);
                }
                else
                {
                    MessageBox.Show("创建模板失败", "创建模板失败请检查参数", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                mView.Repaint();
                MessageBox.Show("创建模板成功", "创建模板成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("没有设置模板区域，不能创建模板", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #region 查找模板

        #region 模板过滤
        private void nud_LG_lenght_min_ValueChanged(object sender, EventArgs e)
        {
            decimal val;
            if (nud_LG_lenght_min.Value < nud_LG_lenght_max.Value)
            {
                val = nud_LG_lenght_min.Value;
            }
            else
            {
                val = nud_LG_lenght_max.Value;
            }

            commonTool.MFindShapeMode.LG_lenght_min = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void nud_LG_lenght_max_ValueChanged(object sender, EventArgs e)
        {
            decimal val;
            if (nud_LG_lenght_max.Value > nud_LG_lenght_min.Value)
            {
                val = nud_LG_lenght_max.Value;
            }
            else
            {
                val = nud_LG_lenght_min.Value;
            }
                
            commonTool.MFindShapeMode.LG_lenght_max = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void nud_DP_width_min_ValueChanged(object sender, EventArgs e)
        {
            decimal val;
            if (nud_DP_width_min.Value < nud_DP_width_max.Value)
            {
                val = nud_DP_width_min.Value;
            }
            else
            {
                val = nud_DP_width_max.Value;
            }

            commonTool.MFindShapeMode.DP_width_min = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void nud_DP_width_max_ValueChanged(object sender, EventArgs e)
        {
            decimal val;
            if (nud_DP_width_max.Value > nud_DP_width_min.Value)
            {
                val = nud_DP_width_max.Value;
            }
            else
            {
                val = nud_DP_width_min.Value;
            }
            commonTool.MFindShapeMode.DP_width_max = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void nud_LG_Area_min_ValueChanged(object sender, EventArgs e)
        {
            decimal val;
            if (nud_LG_Area_min.Value < nud_LG_Area_max.Value)
            {
                val = nud_LG_Area_min.Value;
            }
            else
            {
                val = nud_LG_Area_max.Value;
            }
            commonTool.MFindShapeMode.LG_Area_min = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void nud_LG_Area_max_ValueChanged(object sender, EventArgs e)
        {
            decimal val;
            if (nud_LG_Area_max.Value > nud_LG_Area_min.Value)
            {
                val = nud_LG_Area_max.Value;
            }
            else
            {
                val = nud_LG_Area_min.Value;
            }
            commonTool.MFindShapeMode.LG_Area_max = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void nud_LG_threshould_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nud_LG_threshould.Value;
            commonTool.MFindShapeMode.LG_threshould = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void nud_Closing_width_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nud_Closing_width.Value;
            commonTool.MFindShapeMode.Closing_width = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void nud_Closing_height_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nud_Closing_height.Value;
            commonTool.MFindShapeMode.Closing_height = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }
        #endregion

        #region 查找模板
        private void MinScoreUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = MinScoreUpDown.Value;
            commonTool.MFindShapeMode.minScore = (double)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void MaxOverlapUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = MaxOverlapUpDown.Value;
            commonTool.MFindShapeMode.maxOverlap = (double)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void NumMatchesUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = NumMatchesUpDown.Value;
            commonTool.MFindShapeMode.numMatches = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void LastPyrLevUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = LastPyrLevUpDown.Value;
            commonTool.MFindShapeMode.numLevels = (int)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void GreedinessUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = GreedinessUpDown.Value;
            commonTool.MFindShapeMode.greediness = (double)val;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }

        private void SubPixelBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            commonTool.MFindShapeMode.subPixel = SubPixelBox.Text;
            if (ckbCycleFind.Checked)
            {
                if (commonTool.MFindShapeMode.FindShapeModeAct(commonTool.ImageRefIn, commonTool.MCreateShapeModel, CurrentImg))
                {
                    mView.AddIconicVar(CurrentImg);
                    commonTool.MFindShapeMode.ShowResult(mView);
                    mView.Repaint();
                }
            }
        }
        #endregion

        #endregion

        #region 测试
        private void loadTestImgButton_Click(object sender, EventArgs e)
        {
            string[] files;
            int count = 0;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "选择图像文件";
            openFileDialog.Filter = "图像文件 |*.bmp;*.png;*.tif;*.jpg|all files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                files = openFileDialog.FileNames;
                count = files.Length;

                for (int i = 0; i < count; i++)
                {
                    if (commonTool.AddTestImages(files[i]))
                        testImgListBox.Items.Add(files[i]);
                }
                if (testImgListBox.Items.Count > 0)
                    testImgListBox.SelectedIndex = testImgListBox.Items.Count - 1;
                CurrentImg = commonTool.TestImageDic[(string)testImgListBox.SelectedItem];
                mView.SetDispLevel(ShowMode.ExcludeROI);
                mView.ClearList();
                mView.AddIconicVar(CurrentImg);
                mView.Repaint();
            }
        }

        private void deleteTestImgButton_Click(object sender, EventArgs e)
        {
            int count;
            if ((count = testImgListBox.SelectedIndex) < 0)
                return;

            string fileName = (string)testImgListBox.SelectedItem;

            if ((--count) < 0)
                count += 2;

            if ((count < testImgListBox.Items.Count))
            {
                testImgListBox.SelectedIndex = count;
            }

            commonTool.RemoveTestImage(fileName);
            testImgListBox.Items.Remove(fileName);
        }

        private void deleteAllTestImgButton_Click(object sender, EventArgs e)
        {
            if (testImgListBox.Items.Count > 0)
            {
                testImgListBox.Items.Clear();
                commonTool.RemoveTestImage();
                mView.ClearList();
                mView.SetDispLevel(ShowMode.ExcludeROI);
                mView.Repaint();
            }
        }
        bool isTestRunning;
        private int testImageIterator = 0;
        private void findModelButton_Click(object sender, EventArgs e)
        {
            if (testImgListBox.Items.Count == 0)
                return;
            if (isTestRunning)
                return;

            locked = true;
            isTestRunning = true;
            if (testImageIterator > testImgListBox.Items.Count - 1)
                testImageIterator = 0;
            testImgListBox.SelectedIndex = testImageIterator;

            string file;
            file = (string)testImgListBox.SelectedItem;
            CurrentImg = commonTool.TestImageDic[file];
            commonTool.ImageTestIn = CurrentImg;
            mView.SetDispLevel(ShowMode.ExcludeROI);
            mView.ClearList();
            mView.AddIconicVar(CurrentImg);
            commonTool.RunTest();
            commonTool.ShowResult(mView);
            mView.Repaint();
            if (ckbCycleFind.Checked && isTestRunning)
            {
                testImageIterator++;
            }
            isTestRunning = false;
            locked = false;
        }
        private void testImgListBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (testImgListBox.Items.Count == 0)
            {
                return;
            }
            if (e.Button != MouseButtons.Left)
                return;
            if (isTestRunning)
                return;

            string file;
            if (testImgListBox.Items.Count > 0)
            {
                file = (string)testImgListBox.SelectedItem;
                CurrentImg = commonTool.TestImageDic[file];
                mView.SetDispLevel(ShowMode.ExcludeROI);
                mView.ClearList();
                mView.AddIconicVar(CurrentImg);
                mView.Repaint();
            }
            else
            {
                mView.ClearList();
                mView.Repaint();
                return;
            }
        }
        private void testImgListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (testImgListBox.Items.Count == 0)
            {
                return;
            }
            if (isTestRunning)
                return;
            testImageIterator = testImgListBox.SelectedIndex;
            findModelButton_Click(null, null);
        }

        private void btn_Debug_Click(object sender, EventArgs e)
        {
            if (!commonTool.engineIsnitial)
            {
                commonTool.MyEngine = new HDevEngine();
                commonTool.InitialEngine();
            }
            if (btn_Debug.Text == "StartDebug")
            {
                commonTool.StartDebugMode();
                btn_Debug.Text = "StopDebug";
            }
            else
            {
                commonTool.StopDebugMode();
                btn_Debug.Text = "StartDebug";
            }

        }
        #endregion

        #region 抓取点设定
        private void GrabPointDispSetting()
        {
            roiActUnit1.Enabled = false;
            Panel_Test.Enabled = true;
            mView.SetDispLevel(ShowMode.ExcludeROI);
            CurrentImg = commonTool.ImageRefIn;
            mView.ClearList();
            mView.AddIconicVar(commonTool.ImageRefIn);

            if (modelPointXld == null)
                modelPointXld = new HXLDCont();
            if (modelPointXld != null && modelPointXld.IsInitialized())
                modelPointXld.Dispose();
            modelPointXld.GenCrossContourXld(commonTool.MGrabPointSetting.GrabRowOrg, commonTool.MGrabPointSetting.GrabColOrg, 30, 0);

            mView.ChangeGraphicSettings(Mode.COLOR, "red");
            mView.ChangeGraphicSettings(Mode.LINEWIDTH, 2);
            mView.AddIconicVar(modelPointXld);
        }
        private void rbtn_Grab_From_Pictrue_CheckedChanged(object sender, EventArgs e)
        {
            panel_From_Pitrue.Enabled = rbtn_Grab_From_Pictrue.Checked;
            commonTool.MGrabPointSetting.fromPictrue = panel_From_Pitrue.Enabled;
            panel_From_Robot.Enabled = !panel_From_Pitrue.Enabled;
            if (panel_From_Pitrue.Enabled)
            {
                panel_From_Pitrue.BackColor = Color.LightGreen;
            }
            else
            {
                panel_From_Pitrue.BackColor = Color.LightGray;
            }
            if (panel_From_Robot.Enabled)
            {
                panel_From_Robot.BackColor = Color.LightGreen;
            }
            else
            {
                panel_From_Robot.BackColor = Color.LightGray;
            }
        }
        private void rbtn_from_Robot_CheckedChanged(object sender, EventArgs e)
        {
            panel_From_Robot.Enabled = rbtn_from_Robot.Checked;
            panel_From_Pitrue.Enabled = !panel_From_Robot.Enabled;
            commonTool.MGrabPointSetting.fromPictrue = panel_From_Pitrue.Enabled;
            if (panel_From_Pitrue.Enabled)
            {
                panel_From_Pitrue.BackColor = Color.LightGreen;
            }
            else
            {
                panel_From_Pitrue.BackColor = Color.LightGray;
            }
            if (panel_From_Robot.Enabled)
            {
                panel_From_Robot.BackColor = Color.LightGreen;
            }
            else
            {
                panel_From_Robot.BackColor = Color.LightGray;
            }
        }
        private void btn_Set_Grab_P_Click(object sender, EventArgs e)
        {
            MessageBox.Show("使用鼠标在左侧图像窗口，选取模板图像上一点，作为抓取点", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            mView.viewPort.MouseDown += GetMousePoint;
            mView.viewPort.MouseMove += MouseMovePoint;
            mView.ClearList();
            mView.SetDispLevel(ShowMode.ExcludeROI);
            mView.ChangeGraphicSettings(Mode.LINESTYLE, new HTuple());
            mView.AddIconicVar(commonTool.ImageRefIn);
            mView.Repaint();
            btn_Set_Grab_P.Enabled = false;
        }
        double row, col;
        private void GetMousePoint(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            int state;
            try
            {
                mView.viewPort.HalconWindow.GetMpositionSubPix(out row, out col, out state);
                txt_Grab_Row.Text = string.Format("{0:F2}", row);
                txt_Grab_Col.Text = string.Format("{0:F2}", col);
                commonTool.MGrabPointSetting.SetGrabPoint(row, col);
            }
            catch (HalconException)
            {
                return;
            }
            mView.viewPort.MouseDown -= GetMousePoint;
            mView.viewPort.MouseMove -= MouseMovePoint;
            btn_Set_Grab_P.Enabled = true;
        }
        HXLDCont modelPointXld;
        private void MouseMovePoint(object sender, MouseEventArgs e)
        {
            int state;
            try
            {
                mView.viewPort.HalconWindow.GetMpositionSubPix(out row, out col, out state);
                txt_Grab_Row.Text = string.Format("{0:F2}", row);
                txt_Grab_Col.Text = string.Format("{0:F2}", col);
                if (modelPointXld == null)
                    modelPointXld = new HXLDCont();
                if (modelPointXld != null && modelPointXld.IsInitialized())
                    modelPointXld.Dispose();
                modelPointXld.GenCrossContourXld(row, col, 30, 0);
                HXLDCont CicleXld = new HXLDCont();
                CicleXld.GenCircleContourXld(row, col, 50, 0, 2 * Math.PI, "positive", 2);
                HXLDCont temp = new HXLDCont();
                temp = modelPointXld.ConcatObj(CicleXld);
                modelPointXld.Dispose();
                modelPointXld = temp;
                mView.ClearList();
                mView.SetDispLevel(ShowMode.ExcludeROI);
                mView.ChangeGraphicSettings(Mode.LINESTYLE, new HTuple());
                mView.AddIconicVar(commonTool.ImageRefIn);
                if (modelPointXld != null)
                {
                    mView.ChangeGraphicSettings(Mode.COLOR, "red");
                    mView.ChangeGraphicSettings(Mode.LINEWIDTH, 2);
                    mView.AddIconicVar(modelPointXld);
                }
                mView.Repaint();
            }

            catch (HalconException)
            {
                return;
            }

        }
        private void btn_Sure_RobotCoor_Click(object sender, EventArgs e)
        {
            double x, y;
            if (!double.TryParse(txt_Robot_X.Text, out x) || !double.TryParse(txt_Robot_Y.Text, out y))
            {
                MessageBox.Show("手臂坐标错误请检查", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (commonTool.cameraParam == null || commonTool.cameraParam.Length != 9)
            {
                MessageBox.Show("未有相机参数，请先标定，后导入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (commonTool.worldPose == null || commonTool.worldPose.Length != 7)
            {
                MessageBox.Show("未有世界坐标系，请先标定，后导入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            commonTool.SetGrabPoint(x, y);

            txt_Grab_Row.Text = string.Format("{0:F2}", commonTool.MGrabPointSetting.GrabRowOrg);
            txt_Grab_Col.Text = string.Format("{0:F2}", commonTool.MGrabPointSetting.GrabColOrg);
            GrabPointDispSetting();
            mView.Repaint();
        }
        private void btn_cameraParamPath_Click(object sender, EventArgs e)
        {
            string file;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "选择相机参数文件";
            openFileDialog.Filter = "相机参数(cal)文件 |*.cal|all files (*.*)|*.*";

            string path = Path.Combine(Environment.CurrentDirectory, "CalibrationData");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            openFileDialog.InitialDirectory = path;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                file = openFileDialog.FileName;
                txt_CameraPath.Text = file;
                commonTool.cameraParamPath = file;
                try
                {
                    HOperatorSet.ReadCamPar(file, out commonTool.cameraParam);
                }
                catch
                {
                    MessageBox.Show("读取相机参数失败", "读取相机参数失败，请检查文件该文件：" + file, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (commonTool.cameraParam == null || commonTool.cameraParam.Length != 9)
                {
                    MessageBox.Show("相机参数文件错误", "相机参数文件错误，请检查该文件", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    commonTool.cameraParam = null;
                }

            }
        }
        private void btn_worldPosePath_Click(object sender, EventArgs e)
        {
            string file;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "选择世界坐标系文件";
            openFileDialog.Filter = "世界坐标系(dat)文件 |*.dat|all files (*.*)|*.*";

            string path = Path.Combine(Environment.CurrentDirectory, "CalibrationData");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            openFileDialog.InitialDirectory = path;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                file = openFileDialog.FileName;
                txt_worldPosePath.Text = file;
                commonTool.worldPosePath = file;
                try
                {
                    HOperatorSet.ReadPose(file, out commonTool.worldPose);
                }
                catch
                {
                    MessageBox.Show("读取世界坐标系文件失败", "读取世界坐标系文件失败，请检查文件该文件：" + file, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (commonTool.worldPose == null || commonTool.worldPose.Length != 7)
                {
                    MessageBox.Show("世界坐标系文件错误", "世界坐标系文件错误，请检查该文件", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    commonTool.worldPose = null;
                }

            }
        }
        private void ckb_IsCalibration_CheckedChanged(object sender, EventArgs e)
        {
            if (locked)
                return;
            commonTool.bIsCalibration = ckb_IsCalibration.Checked;
            gb_IsCalibration.Enabled = ckb_IsCalibration.Checked;

            if (ckb_IsCalibration.Checked)
            {
                txt_CameraPath.Text = commonTool.cameraParamPath;
                txt_worldPosePath.Text = commonTool.worldPosePath;
            }
        }
        private void txt_X_Compensation_TextChanged(object sender, EventArgs e)
        {
            double a;
            if (double.TryParse(txt_X_Compensation.Text, out a))
            {
                if (a < 10 || a < -10)
                {
                    commonTool.x_Compensation = a;
                }
                else
                {
                    MessageBox.Show("输入数值过大", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show("请输入数值", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void txt_Y_Compensation_TextChanged(object sender, EventArgs e)
        {
            double a;
            if (double.TryParse(txt_Y_Compensation.Text, out a))
            {
                if (a < 10 || a < -10)
                {
                    commonTool.y_Compensation = a;
                }
                else
                {
                    MessageBox.Show("输入数值过大", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show("请输入数值", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void txt_Angle_Compensation_TextChanged(object sender, EventArgs e)
        {
            double a;
            if (double.TryParse(txt_Angle_Compensation.Text, out a))
            {
                if (a < 10 || a < -10)
                {
                    commonTool.angle_Compensation = a;
                }
                else
                {
                    MessageBox.Show("输入数值过大", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show("请输入数值", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 膨胀套参数设定
        private void dist_PZT_UpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = dist_min_PZT_UpDown.Value;
            commonTool.MPZT_Detection.dist_STD = (double)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                if (commonTool.bPZT_Detection_Result)
                {
                    PZT_DetectionShow();
                }
            }
        }
        private void dist_max_PZT_UpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = dist_max_PZT_UpDown.Value;
            commonTool.MPZT_Detection.dist_max = (double)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                if (commonTool.bPZT_Detection_Result)
                {
                    PZT_DetectionShow();
                }
            }
        }

        private void contrast_Threshold_UpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = contrast_Threshold_UpDown.Value;
            commonTool.MPZT_Detection.threshold = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                PZT_DetectionShow();
            }
        }

        private void PZT_DetectionShow()
        {
            if (commonTool.bPZT_Detection_Result)
            {
                mView.ClearList();
                mView.AddIconicVar(CurrentImg);
                commonTool.MPZT_Detection.Show(mView);
            }
            mView.Repaint();
        }

        private void Sigma_UpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = Sigma_UpDown.Value;
            commonTool.MPZT_Detection.sigma = (double)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                PZT_DetectionShow();
            }
        }

        private void mesureNumber_UpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = mesureNumber_UpDown.Value;
            commonTool.MPZT_Detection.mesureNumber = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                PZT_DetectionShow();
            }
        }
        private void PZT_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            commonTool.bPZT_Detection_Enble = PZT_checkBox.Checked;
            groupBox_PZT_Detection.Enabled = commonTool.bPZT_Detection_Enble;
            if (PZT_checkBox.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
        }

        private void mes_width_UpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = mesureNumber_UpDown.Value;
            commonTool.MPZT_Detection.mesureNumber = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                PZT_DetectionShow();
            }
        }

        private void cB_IsBackflow_PZT_CheckedChanged(object sender, EventArgs e)
        {
            if (locked)
                return;
            commonTool.bIsBackflow_PZT = cB_IsBackflow_PZT.Checked;
        }
        #endregion

        #region 抓取干涉参数设定

        private void nUpDown_GrabIntervene_minThreshold_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_GrabIntervene_minThreshold.Value;
            commonTool.MGrab_Intervene.minThreshold = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                GrabIntervene_DetectionShow();
            }
        }

        private void GrabIntervene_DetectionShow()
        {
            if (commonTool.bGrab_Intervene_Result)
            {
                mView.ClearList();
                mView.AddIconicVar(CurrentImg);
                commonTool.MGrab_Intervene.Show(mView);
            }
            mView.Repaint();
        }

        private void nUpDown_GrabIntervene_filterRadiu_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_GrabIntervene_filterRadiu.Value;
            commonTool.MGrab_Intervene.filterRadiu = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                GrabIntervene_DetectionShow();
            }
        }

        private void checkBox_GrabIntervene_Enable_CheckedChanged(object sender, EventArgs e)
        {
            commonTool.bGrab_Intervene_Enble = checkBox_GrabIntervene_Enable.Checked;
            groupBox_GrabIntervene.Enabled = commonTool.bGrab_Intervene_Enble;
            if (checkBox_GrabIntervene_Enable.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
        }


        private void nUpDown_GrabIntervene_minArea_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_GrabIntervene_minArea.Value;
            commonTool.MGrab_Intervene.minAreaThread = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                GrabIntervene_DetectionShow();
            }
        }
        #endregion

        #region 螺母垫片检测
        private void LM_DP_DetectionShow()
        {
            if (commonTool.bLM_DP_Result)
            {
                mView.ClearList();
                mView.AddIconicVar(CurrentImg);
                commonTool.MLM_DP.Show(mView);
            }
            mView.Repaint();
        }
        private void NUpDown_LM_DP_minThreshold_ValueChanged(object sender, EventArgs e)
        {
            decimal val = NUpDown_LM_DP_minThreshold.Value;
            commonTool.MLM_DP.minThreshold = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                LM_DP_DetectionShow();
            }
        }

        private void NUpDownNUpDown_LM_DP_filterRadiu_ValueChanged(object sender, EventArgs e)
        {
            decimal val = NUpDownNUpDown_LM_DP_filterRadiu.Value;
            commonTool.MLM_DP.filterRadiu = (double)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                LM_DP_DetectionShow();
            }
        }

        private void NUpDown_LD_DP_minAreaThreshold_ValueChanged(object sender, EventArgs e)
        {
            decimal val = NUpDown_LD_DP_minAreaThreshold.Value;
            commonTool.MLM_DP.minAreaThread = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                LM_DP_DetectionShow();
            }
        }

        private void NUD_LD_DP_LuMuMaxArea_ValueChanged(object sender, EventArgs e)
        {
            decimal val = NUD_LD_DP_LuMuMaxArea.Value;
            commonTool.MLM_DP.luoMuMaxArea = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                LM_DP_DetectionShow();
            }
        }

        private void nUD_LMDP_LMWidth_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUD_LMDP_LMWidth.Value;
            commonTool.MLM_DP.luoMuWidth = (double)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                LM_DP_DetectionShow();
            }
        }






        private void checkBox_LM_DP_CheckedChanged(object sender, EventArgs e)
        {
            commonTool.bLM_DP_Enble = checkBox_LM_DP.Checked;
            groupBox_LM_DP.Enabled = commonTool.bLM_DP_Enble;
            if (checkBox_LM_DP.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
        }

        private void cB_IsBackflow_LMDP_CheckedChanged(object sender, EventArgs e)
        {
            if (locked)
                return;
            commonTool.bIsBackflow_LMDP = cB_IsBackflow_LMDP.Checked;
        }
        #endregion

        #region 防呆
        private void FangDai_DetectionShow()
        {
            if (commonTool.bFangDai_Result)
            {
                mView.ClearList();
                mView.AddIconicVar(CurrentImg);
                commonTool.MFangDai.Show(mView);
            }
            mView.Repaint();
        }
        private void checkBox_FangDai_CheckedChanged(object sender, EventArgs e)
        {
            commonTool.bFangDai_Enable = checkBox_FangDai.Checked;
            groupBox_FangDai.Enabled = commonTool.bFangDai_Enable;
            if (checkBox_FangDai.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
        }
        private void nUpDown_FD_minThreshold_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_FD_minThreshold.Value;
            commonTool.MFangDai.minThreshold = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                FangDai_DetectionShow();
            }
        }

        private void nUpDown_FD_minArea_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_FD_minArea.Value;
            commonTool.MFangDai.minArea = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                FangDai_DetectionShow();
            }
        }

        private void nUpDown_FD_maxArea_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_FD_maxArea.Value;
            commonTool.MFangDai.maxArea = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                FangDai_DetectionShow();
            }
        }
        #endregion

        #region 蓝圈
        private void LanQuan_DetectionShow()
        {
            if (commonTool.bLanQuan_Result)
            {
                mView.ClearList();
                mView.AddIconicVar(CurrentImg);
                commonTool.MLanQuan.Show(mView);
            }
            mView.Repaint();
        }
        private void checkBox_LanQuan_CheckedChanged(object sender, EventArgs e)
        {
            commonTool.bLanQuan_Enable = checkBox_LanQuan.Checked;
            groupBox_LanQuan.Enabled = commonTool.bLanQuan_Enable;
            if (checkBox_LanQuan.Checked)
            {
                roiActUnit1.Enabled = true;
            }
            else
            {
                roiActUnit1.Enabled = false;
            }
        }

        private void nUpDown_LQ_minThreshold_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_LQ_minThreshold.Value;
            commonTool.MLanQuan.minThreshold = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                LanQuan_DetectionShow();
            }
        }

        private void nUpDown_LQ_filterRadius_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_LQ_filterRadius.Value;
            commonTool.MLanQuan.filterRadiu = (double)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                LanQuan_DetectionShow();
            }
        }

        private void cB_isBackflow_LQ_CheckedChanged(object sender, EventArgs e)
        {
            if (locked)
                return;
            commonTool.bIsBackflow_LQ = cB_isBackflow_LQ.Checked;
        }



        private void nUpDown_LQ__minArea_ValueChanged(object sender, EventArgs e)
        {
            decimal val = nUpDown_LQ__minArea.Value;
            commonTool.MLanQuan.minArea = (int)val;
            if (ckbCycleFind.Checked)
            {
                commonTool.Run(CurrentImg);
                LanQuan_DetectionShow();
            }
        }
        #endregion

    }
}
