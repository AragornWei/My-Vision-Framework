using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoga.ImageControl;
using Yoga.Common;
using Yoga.Tools;
using System.IO;
using HalconDotNet;

namespace Yoga.HILTI.AutoPacking.GeneralTool
{

    public delegate void ExeInfo(string value);
    [Serializable]
    public class CommonTool : ToolBase, IToolEnable, IToolRun
    {
        [NonSerialized]
        public ExeInfo NotifyExcInfo;

        #region 反射调用
        protected static string toolType = "喜利得螺杆-皮带线自动抓取通用程序";
        public static string ToolType
        {
            get
            {
                return toolType;
            }
        }
        public static string ToolExplanation
        {
            get
            {
                return toolType + "\r\n版本:2020\r\n说明:";
            }
        }
        public override string GetToolType()
        {
            return toolType;
        }
        #endregion

        #region 创建模板

        private CreateShapeModel createShapeModel = new CreateShapeModel();
        public CreateShapeModel MCreateShapeModel
        {
            get
            {
                if (createShapeModel == null)
                    createShapeModel = new CreateShapeModel();
                return createShapeModel;
            }
            set
            {
                createShapeModel = value;
            }

        }
        #endregion

        #region 查找模板
        private FindShapeMode findShapeMode = new FindShapeMode();
        public FindShapeMode MFindShapeMode
        {
            get
            {
                if (findShapeMode == null)
                {
                    findShapeMode = new FindShapeMode();
                }
                return findShapeMode;
            }
            set
            {
                findShapeMode = value;
            }
        }
        [NonSerialized]
        bool bFindShapeMode = false;

        #endregion

        #region 抓取点设定
        public bool bIsCalibration = false;
        GrabPointSetting mGrabPointSetting = new GrabPointSetting();
        public GrabPointSetting MGrabPointSetting
        {
            get
            {
                if (mGrabPointSetting == null)
                    mGrabPointSetting = new GrabPointSetting();
                return mGrabPointSetting;
            }
            set
            {
                mGrabPointSetting = value;
            }
        }
        public string cameraParamPath;
        public string worldPosePath;
        public HTuple cameraParam;
        public HTuple worldPose;

        public double x_Compensation, y_Compensation, angle_Compensation;

        [NonSerialized]
        public bool bGrabPointSetting = false;

        #endregion

        #region 工程
        [NonSerialized]
        public HDevEngine MyEngine = new HDevEngine();
        [NonSerialized]
        public bool engineIsnitial;
        public void InitialEngine()
        {
            if (MyEngine == null)
            {
                MyEngine = new HDevEngine();
            }
            MyEngine.SetProcedurePath(Environment.CurrentDirectory + "\\Engine\\");
            MyEngine.SetEngineAttribute("execute_procedures_jit_compiled", "true");
            engineIsnitial = true;
        }
        public void StartDebugMode()
        {
            MyEngine.SetEngineAttribute("execute_procedures_jit_compiled", "false");
            MyEngine.SetEngineAttribute("debug_port", 57786);
            MyEngine.StartDebugServer();
        }
        public void StopDebugMode()
        {
            MyEngine.StopDebugServer();
            MyEngine.SetEngineAttribute("execute_procedures_jit_compiled", "true");
        }

        #endregion

        #region 膨胀套有无测量
        PZT_Detection mPZT_Detection;
        public PZT_Detection MPZT_Detection
        {
            get
            {
                if (mPZT_Detection == null)
                    mPZT_Detection = new PZT_Detection();
                return mPZT_Detection;
            }
            set
            {
                mPZT_Detection = value;
            }
        }
        public bool bPZT_Detection_Enble = true;
        public bool bIsBackflow_PZT = true;
        [NonSerialized]
        public bool bPZT_Detection_Result = false;

        #endregion

        #region 抓取位置干涉
        Grab_Intervene mGrab_Intervene;
        public Grab_Intervene MGrab_Intervene
        {
            get
            {
                if (mGrab_Intervene == null)
                {
                    mGrab_Intervene = new Grab_Intervene();
                }
                return mGrab_Intervene;
            }
            set
            {
                mGrab_Intervene = value;
            }
        }
        public bool bGrab_Intervene_Enble = true;
        [NonSerialized]
        public bool bGrab_Intervene_Result = false;
        #endregion

        #region 螺杆螺母有无检测
        LM_DP mLM_DP;
        public LM_DP MLM_DP
        {
            get
            {
                if (mLM_DP == null)
                {
                    mLM_DP = new LM_DP();
                }
                return mLM_DP;
            }
            set
            {
                mLM_DP = value;
            }
        }
        public bool bLM_DP_Enble = true;
        public bool bIsBackflow_LMDP = true;
        [NonSerialized]
        public bool bLM_DP_Result = false;

        #endregion

        #region 防呆
        FangDai mFangDai;
        public FangDai MFangDai
        {
            get
            {
                if (mFangDai == null)
                {
                    mFangDai = new FangDai();
                }
                return mFangDai;
            }
            set
            {
                mFangDai = value;
            }
        }
        public bool bFangDai_Enable = true;
        [NonSerialized]
        public bool bFangDai_Result = false;

        #endregion

        #region 蓝圈
        LanQuan mLanQuan;
        public LanQuan MLanQuan
        {
            get
            {
                if (mLanQuan == null)
                {
                    mLanQuan = new LanQuan();
                }
                return mLanQuan;
            }
            set
            {
                mLanQuan = value;
            }
        }
        public bool bLanQuan_Enable = true;
        [NonSerialized]
        public bool bLanQuan_Result = false;
        public bool bIsBackflow_LQ = true;
        #endregion

        public CommonTool(int settingIndex)
        {
            base.settingIndex = settingIndex;
            IsOutputResults = true;
            InitialEngine();
            createShapeModel.Initial(ImageRefIn);
        }
        public override ToolsSettingUnit GetSettingUnit()
        {
            if (settingUnit == null)
            {
                settingUnit = new CommonToolParamSetting(this);
            }
            return settingUnit;
        }
        public override void ClearTestData()
        {
            base.ClearTestData();
        }
        //序列化
        public override void SerializeCheck()
        {
            NotifyExcInfo = null;
            MCreateShapeModel.SerializeCheck();
            MFindShapeMode.SerializeCheck();
            MGrabPointSetting.SerializeCheck();
            MPZT_Detection.SerializeCheck();
            MGrab_Intervene.SerializeCheck();
            MLM_DP.SerializeCheck();
            MFangDai.SerializeCheck();
            MLanQuan.SerializeCheck();
            using (Stream objectStream = new MemoryStream())
            {
                System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(objectStream, this);
            }
        }
        //主执行程序
        protected override void RunAct()
        {
            base.RunAct();
            if (!engineIsnitial)
            {
                MyEngine = new HDevEngine();
                InitialEngine();
            }
            #region 初始化
            RuningFinish = false;
            base.Result = string.Empty;
            base.IsOk = false;
            base.isRealOk = false;
            //功能块
            bFindShapeMode = false;
            bGrabPointSetting = false;
            bPZT_Detection_Result = false;
            bGrab_Intervene_Result = false;
            bLM_DP_Result = false;
            bFangDai_Result = false;
            bLanQuan_Result = false;
            #endregion

            if (ImageTestIn == null || !ImageTestIn.IsInitialized())
                return;
            HImage R, G, B;
            R = ImageTestIn.Decompose3(out G, out B);

            #region 创建模板
            if (MCreateShapeModel.hShapeModel == null || !MCreateShapeModel.hShapeModel.IsInitialized())
            {
                if (!MCreateShapeModel.CreateShapeModelAct(ImageRefIn))
                {
                    Util.Notify("创建模板异常");
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo("创建模板异常");
                    }
                    return;
                }

            }
            #endregion

            HTuple t1;
            HOperatorSet.CountSeconds(out t1);

            #region 模板匹配

            bFindShapeMode = MFindShapeMode.FindShapeModeAct(ImageRefIn, MCreateShapeModel, ImageTestIn);
            HTuple t2;
            HOperatorSet.CountSeconds(out t2);
            double time = (t2 - t1).D * 1000;
            Util.Notify("模板匹配用时:" + time.ToString("F2") + "ms");
            if (NotifyExcInfo != null)
            {
                NotifyExcInfo("模板匹配用时:" + time.ToString("F2") + "ms");
            }

            if (!bFindShapeMode)
            {
                Util.Notify("查找模板异常");
                if (NotifyExcInfo != null)
                {
                    NotifyExcInfo("查找模板异常");
                }
                return;
            }
            List<HHomMat2D> homMat2Ds = new List<HHomMat2D>();
            homMat2Ds = MFindShapeMode.GetHHomMat2Ds();

            if (MFindShapeMode.row==null || MFindShapeMode.row.Length < 1 || homMat2Ds==null || homMat2Ds.Count<1)
            {
                if (NotifyExcInfo != null)
                {
                    NotifyExcInfo("未找到模板");
                }
                base.IsOk = true;
                base.isRealOk = true;
                return;
            }
            if (bFindShapeMode)
            {
                StringBuilder strb = new StringBuilder();
                strb.Append("查找结果:");
                strb.Append(Environment.NewLine);
                int count = MFindShapeMode.row_temp.Length;
                for (int i = 0; i < count; i++)
                {
                    HTuple phi;
                    HOperatorSet.TupleDeg(MFindShapeMode.angle_temp[i], out phi);
                    strb.Append("第" + (i + 1) + "个:\r\n");
                    string mes = string.Format(
                   "Row:{0:F2}\r\n" + "Col:{1:F2}\r\n" + "角度:{2:F2}\r\n" + "得分:{3:F2}"
                   , MFindShapeMode.row_temp[i].D, MFindShapeMode.column_temp[i].D, phi.D, MFindShapeMode.score_temp[i].D);
                    strb.Append(mes);
                    strb.Append(Environment.NewLine);
                }

                int blob_feature_lengt = MFindShapeMode.LG_Area.Length;
                for (int i = 0; i < blob_feature_lengt; i++)
                {
                    strb.Append("第" + (i + 1) + "个:\r\n");
                    string mes = string.Format(
                   "螺杆面积:{0:F2}\r\n" + "螺杆长:{1:F2}\r\n" + "垫片宽:{2:F2}\r\n"
                   , MFindShapeMode.LG_Area[i].D, MFindShapeMode.LG_lenght[i].D,MFindShapeMode.DP_width[i].D);
                    strb.Append(mes);
                    strb.Append(Environment.NewLine);
                }

                if (NotifyExcInfo != null)
                {
                    NotifyExcInfo(strb.ToString());
                }
                Util.Notify(strb.ToString());
            }
            



            #endregion

            

            #region 抓取点
            bGrabPointSetting = mGrabPointSetting.setTarget(homMat2Ds);
            if (!bGrabPointSetting)
            {
                Util.Notify("抓取点异常");
                if (NotifyExcInfo != null)
                {
                    NotifyExcInfo("抓取点异常");
                }
            }
            else
            {
                StringBuilder strd = new StringBuilder();
                for (int i = 0; i < MGrabPointSetting.GrabRowTarget.Length; i++)
                {
                    string mes = string.Format("抓取点像素坐标："+"第" + (i + 1) + "个像素坐标:\r\n" +
                   "Row: {0:F2}\r\n" +
                   "Col: {1:F2}"
                   , MGrabPointSetting.GrabRowTarget[i].D, MGrabPointSetting.GrabColTarget[i].D);
                    strd.Append(mes);
                    strd.Append(Environment.NewLine);
                }
                Util.Notify(strd.ToString());
                if (NotifyExcInfo != null)
                {
                    NotifyExcInfo(strd.ToString());
                }
            }
            #endregion     

            HTuple t3;
            HOperatorSet.CountSeconds(out t3);
            time = (t3 - t2).D * 1000;
            Util.Notify("抓取点用时:" + time.ToString("F2") + "ms");
            if (NotifyExcInfo != null)
            {
                NotifyExcInfo("抓取点用时:" + time.ToString("F2") + "ms");
            }

            #region 膨胀套检测
            if (bPZT_Detection_Enble)
            {
                bPZT_Detection_Result = MPZT_Detection.PZT_Detection_Act(R, homMat2Ds, MFindShapeMode.angle);
                if (bPZT_Detection_Result)
                {
                    StringBuilder strb = new StringBuilder();
                    strb.Append("膨胀套:");
                    strb.Append(Environment.NewLine);
                    int count = MPZT_Detection.dist_PZT.Length;
                    for (int i = 0; i < count; i++)
                    {
                        string mes = string.Format("第" + (i + 1) + "个:\r\n" +
                       "长度: {0:F2}"
                       , MPZT_Detection.dist_PZT[i].D);
                        strb.Append(mes);
                        strb.Append(Environment.NewLine);
                    }
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo(strb.ToString());
                    }
                    Util.Notify(strb.ToString());
                }
                else
                {
                    Util.Notify("膨胀套检测异常");
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo("膨胀套检测异常");
                    }
                }
            }
            else
            {
                bPZT_Detection_Result = true;
            }

            #endregion

            HTuple t4;
            HOperatorSet.CountSeconds(out t4);
            time = (t4 - t3).D * 1000;
            Util.Notify("膨胀套用时:" + time.ToString("F2"));
            if (NotifyExcInfo != null)
            {
                NotifyExcInfo("膨胀套用时:" + time.ToString("F2") + "ms");
            }

            #region 抓取干涉
            if (bGrab_Intervene_Enble)
            {
                bGrab_Intervene_Result = MGrab_Intervene.Grab_Intervene_Act(G, homMat2Ds);
                if (bGrab_Intervene_Result)
                {
                    StringBuilder strb = new StringBuilder();
                    strb.Append("抓取干涉:");
                    strb.Append(Environment.NewLine);
                    int count = MGrab_Intervene.grabInterveneOkNg.Length;
                    for (int i = 0; i < count; i++)
                    {
                        string mes = string.Format("第" + (i + 1) + "个:\r\n" +
                       "是否干涉: {0}"
                       , MGrab_Intervene.grabInterveneOkNg[i].I > 0 ? "不干涉" : "干涉");
                        strb.Append(mes);
                        strb.Append(Environment.NewLine);
                    }
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo(strb.ToString());
                    }
                    Util.Notify(strb.ToString());
                }
                else
                {
                    Util.Notify("抓取干涉检测异常");
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo("抓取干涉检测异常");
                    }
                }
            }
            else
            {
                bGrab_Intervene_Result = true;
            }

            #endregion

            HTuple t5;
            HOperatorSet.CountSeconds(out t5);
            time = (t5 - t4).D * 1000;
            Util.Notify("抓取干涉用时:" + time.ToString("F2") + "ms");
            if (NotifyExcInfo != null)
            {
                NotifyExcInfo("抓取干涉用时:" + time.ToString("F2") + "ms");
            }


            #region 螺母垫片有无检测
            if (bLM_DP_Enble)
            {
                bLM_DP_Result = MLM_DP.LM_DP_Act(G, homMat2Ds);
                if (bLM_DP_Result)
                {
                    StringBuilder strb = new StringBuilder();
                    strb.Append("螺母垫片:");
                    strb.Append(Environment.NewLine);
                    int count = MLM_DP.lM_DP_OkNg.Length;
                    for (int i = 0; i < count; i++)
                    {
                        string mes = string.Format("第" + (i + 1) + "个:\r\n" +
                        "螺母垫片有无: {0}" , MLM_DP.lM_DP_OkNg[i].I > 0 ? "有" : "无");
                        strb.Append(mes);
                        strb.Append(Environment.NewLine);
                        string temp= string.Format("第" + (i + 1) + "个:\r\n" +
                        "区块数量{0}; 最大区块面积{1}", MLM_DP.lM_DP_Num[i].I.ToString(), MLM_DP.lM_DP_Area[i].I.ToString());
                        strb.Append(temp);
                        strb.Append(Environment.NewLine);
                    }
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo(strb.ToString());
                    }
                    Util.Notify(strb.ToString());
                }
                else
                {
                    Util.Notify("螺母垫片有无: 检测异常");
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo("螺母垫片有无: 检测异常");
                    }
                }
            }
            else
            {
                bLM_DP_Result = true;
            }
            #endregion

            HTuple t6;
            HOperatorSet.CountSeconds(out t6);
            time = (t6 - t5).D * 1000;
            Util.Notify("螺母垫片用时:" + time.ToString("F2") + "ms");
            if (NotifyExcInfo != null)
            {
                NotifyExcInfo("螺母垫片用时:" + time.ToString("F2") + "ms");
            }

            #region 防呆
            if (bFangDai_Enable)
            {
                bFangDai_Result = MFangDai.FangDai_Act(G, homMat2Ds);
                if (bFangDai_Result)
                {
                    StringBuilder strb = new StringBuilder();
                    strb.Append("防呆:");
                    strb.Append(Environment.NewLine);
                    int count = MFangDai.Area.Length;
                    for (int i = 0; i < count; i++)
                    {
                        string mes = string.Format("第" + (i + 1) + "个:\r\n" +
                       "面积: {0:F2}"

                       , MFangDai.Area[i].D);
                        strb.Append(mes);
                        strb.Append(Environment.NewLine);
                    }
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo(strb.ToString());
                    }
                    Util.Notify(strb.ToString());
                }
                else
                {
                    Util.Notify("防呆检测异常");
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo("防呆检测异常");
                    }
                }
            }
            else
            {
                bFangDai_Result = true;
            }
            #endregion
            HTuple t7;
            HOperatorSet.CountSeconds(out t7);
            time = (t7 - t6).D * 1000;
            Util.Notify("防呆用时:" + time.ToString("F2") + "ms");
            if (NotifyExcInfo != null)
            {
                NotifyExcInfo("防呆用时:" + time.ToString("F2") + "ms");
            }

            #region 蓝圈
            if (bLanQuan_Enable)
            {
                bLanQuan_Result = MLanQuan.LanQuan_Act(R, B, homMat2Ds);
                if (bLanQuan_Result)
                {
                    StringBuilder strb = new StringBuilder();
                    strb.Append("蓝圈:");
                    strb.Append(Environment.NewLine);
                    int count = MLanQuan.Area.Length;
                    for (int i = 0; i < count; i++)
                    {
                        string mes = string.Format("第" + (i + 1) + "个:\r\n" +
                       "面积: {0:F2}\r\n"
                       , MLanQuan.Area[i].D);
                        strb.Append(mes);
                        strb.Append(Environment.NewLine);
                    }
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo(strb.ToString());
                    }
                    Util.Notify(strb.ToString());
                }
                else
                {
                    Util.Notify("蓝圈检测异常");
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo("蓝圈检测异常");
                    }
                }
            }
            else
            {
                bLanQuan_Result = true;
            }
            #endregion
            HTuple t8;
            HOperatorSet.CountSeconds(out t8);
            time = (t8 - t7).D * 1000;
            Util.Notify("蓝圈用时:" + time.ToString("F2")+"ms");
            if (NotifyExcInfo != null)
            {
                NotifyExcInfo("蓝圈用时:" + time.ToString("F2") + "ms");
            }


            RuningFinish = true;
            base.IsOk = bFindShapeMode && bGrabPointSetting && bPZT_Detection_Result && bGrab_Intervene_Result && bLM_DP_Result && bFangDai_Result && bLanQuan_Result;
            base.isRealOk = true;
            if (R != null && R.IsInitialized())
                R.Dispose();
            if (G != null && G.IsInitialized())
                G.Dispose();
            if (B != null && B.IsInitialized())
                B.Dispose();
        }
        [NonSerialized]
        HTuple row_Send, col_Send, Angle_Send, id_Send;
        HTuple row_NG, col_NG, NG_Reason;

        public override string GetSendResult()
        {
            if (MFindShapeMode.row == null || MFindShapeMode.row.Length < 1)
            {
                //return string.Empty;
                return ("Image" + Environment.NewLine + "Done" + Environment.NewLine);
            }
            row_Send = new HTuple();
            col_Send = new HTuple();
            Angle_Send = new HTuple();
            id_Send = new HTuple();

            row_NG = new HTuple();
            col_NG = new HTuple();
            NG_Reason = new HTuple();

            if (!bFindShapeMode || !bGrabPointSetting)
            {
                return ("Image" + Environment.NewLine + "Done" + Environment.NewLine);
            }


            int count = MFindShapeMode.angle.Length;

            #region 工具运行是否异常
            //膨胀套
            if (bPZT_Detection_Enble == false)
            {
                HOperatorSet.TupleGenConst(count, 1, out MPZT_Detection.pZTOkNg);
            }
            else if (bPZT_Detection_Enble && bPZT_Detection_Result == false)
            {
                HOperatorSet.TupleGenConst(count, 0, out MPZT_Detection.pZTOkNg);
            }

            //抓取干涉
            if (bGrab_Intervene_Enble == false)
            {
                HOperatorSet.TupleGenConst(count, 1, out MGrab_Intervene.grabInterveneOkNg);
            }
            else if (bGrab_Intervene_Enble && bGrab_Intervene_Result == false)
            {
                HOperatorSet.TupleGenConst(count, 0, out MGrab_Intervene.grabInterveneOkNg);
            }
            //螺母垫片

            if (bLM_DP_Enble == false)
            {
                HOperatorSet.TupleGenConst(count, 1, out MLM_DP.lM_DP_OkNg);
            }
            else if (bLM_DP_Enble && bLM_DP_Result == false)
            {
                HOperatorSet.TupleGenConst(count, 0, out MLM_DP.lM_DP_OkNg);
            }

            //防呆

            if (bFangDai_Enable == false)
            {
                HOperatorSet.TupleGenConst(count, 1, out MFangDai.FangDai_OkNg);
            }
            else if (bFangDai_Enable && bFangDai_Result == false)
            {
                HOperatorSet.TupleGenConst(count, 0, out MFangDai.FangDai_OkNg);
            }

            //蓝圈

            if (bLanQuan_Enable == false)
            {
                HOperatorSet.TupleGenConst(count, 1, out MLanQuan.LanQuan_OkNg);
            }
            else if (bLanQuan_Enable && bLanQuan_Result == false)
            {
                HOperatorSet.TupleGenConst(count, 0, out MLanQuan.LanQuan_OkNg);
            }
            #endregion


            if (count != MPZT_Detection.pZTOkNg.Length ||
               count != MGrab_Intervene.grabInterveneOkNg.Length ||
               count != MLM_DP.lM_DP_OkNg.Length ||
               count != MFangDai.FangDai_OkNg.Length ||
               count != MLanQuan.LanQuan_OkNg.Length)
            {
                return ("Image" + Environment.NewLine + "Done" + Environment.NewLine);
            }

            for (int i = 0; i < MFindShapeMode.angle.Length; i++)
            {
                //NG
                if (MPZT_Detection.pZTOkNg[i] != 1 ||
                    MGrab_Intervene.grabInterveneOkNg[i] != 1 ||
                    MLM_DP.lM_DP_OkNg[i] != 1 ||
                    MFangDai.FangDai_OkNg[i] != 1 ||
                    MLanQuan.LanQuan_OkNg[i] != 1
                    )
                {


                        //DateTime dt = DateTime.Now;
                        //string timeNow = dt.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                        //string NGImagePath = "D:\\data\\" + "\\NgImage\\" + "\\工具组" + settingIndex + "\\";
                        //SaveImage(NGImagePath + timeNow + ".png", ImageTestIn);

                    if (row_NG.Length == 0)
                    {
                        row_NG = MGrabPointSetting.GrabRowTarget[i];
                        col_NG = MGrabPointSetting.GrabColTarget[i];
                    }
                    else
                    {
                        row_NG = row_NG.TupleConcat(MGrabPointSetting.GrabRowTarget[i]);
                        col_NG = col_NG.TupleConcat(MGrabPointSetting.GrabColTarget[i]);
                    }
                    StringBuilder strd_temp = new StringBuilder();
                    if (MPZT_Detection.pZTOkNg[i] != 1)
                    {
                        strd_temp.Append("膨胀套；");
                    }
                    if (MGrab_Intervene.grabInterveneOkNg[i] != 1)
                    {
                        strd_temp.Append("抓取干涉;");
                    }
                    if (MLM_DP.lM_DP_OkNg[i] != 1)
                    {
                        strd_temp.Append("螺母垫片;");
                    }
                    if (MFangDai.FangDai_OkNg[i] != 1)
                    {
                        strd_temp.Append("防呆;");
                    }
                    if (MLanQuan.LanQuan_OkNg[i] != 1)
                    {
                        strd_temp.Append("蓝圈;");
                    }
                    if (NG_Reason.Length == 0)
                    {
                        NG_Reason = strd_temp.ToString();

                    }
                    else
                    {
                        NG_Reason = NG_Reason.TupleConcat(strd_temp.ToString());
                    }
                    //是否回流//确保抓取不干涉
                    if (MFangDai.FangDai_OkNg[i] != 1 || MGrab_Intervene.grabInterveneOkNg[i] == 0)
                    {
                        continue;
                       // No_Backflow(i);
                    }
                    else if (
                        (MPZT_Detection.pZTOkNg[i] != 1 && !bIsBackflow_PZT) ||
                        (MLM_DP.lM_DP_OkNg[i] != 1 && !bIsBackflow_LMDP) ||
                        (MLanQuan.LanQuan_OkNg[i] != 1 && !bIsBackflow_LQ))
                    {
                        if (MGrab_Intervene.grabInterveneOkNg[i] == 1)//确保抓取不干涉
                        {
                            No_Backflow(i);
                        }
                        else
                        {
                            continue;
                        }

                    }


                }
                //OK 
                else
                {
                    if (row_Send.Length == 0)
                    {
                        row_Send = MGrabPointSetting.GrabRowTarget[i];
                        col_Send = MGrabPointSetting.GrabColTarget[i];
                        Angle_Send = MFindShapeMode.angle[i];
                        id_Send = new HTuple((int)1);
                    }
                    else
                    {
                        row_Send = row_Send.TupleConcat(MGrabPointSetting.GrabRowTarget[i]);
                        col_Send = col_Send.TupleConcat(MGrabPointSetting.GrabColTarget[i]);
                        Angle_Send = Angle_Send.TupleConcat(MFindShapeMode.angle[i]);
                        id_Send = id_Send.TupleConcat((int)1);
                    }
                }
            }
            if (row_Send.Length == 0)
            {
                return ("Image" + Environment.NewLine + "Done" + Environment.NewLine);
            }

            StringBuilder outCoord = new StringBuilder();
            //坐标转换
            if (bIsCalibration)
            {
                outCoord.Clear();
                if (cameraParam == null || cameraParam.Length != 9)
                {
                    Util.Notify("相机参数异常");
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo("相机参数异常");
                    }
                }
                if (worldPose == null || worldPose.Length != 7)
                {
                    Util.Notify("世界坐标系异常");
                    if (NotifyExcInfo != null)
                    {
                        NotifyExcInfo("世界坐标系异常");
                    }
                }

                if (cameraParam != null && cameraParam.Length == 9 && worldPose != null && worldPose.Length == 7)
                {
                    outCoord.Append("Image" + Environment.NewLine);

                    for (int i = 0; i < row_Send.Length; i++)
                    {
                        HTuple X, Y;
                        HOperatorSet.ImagePointsToWorldPlane(cameraParam, worldPose, row_Send[i], col_Send[i], 1, out X, out Y);
                        HTuple angle;
                        HOperatorSet.TupleDeg(Angle_Send[i].D, out angle);
                        if (angle > 180)
                        {
                            angle -= 180;
                        }
                        if (angle >= 90 && angle <= 180)
                        {
                            angle -= 180;
                        }
                        if (angle < -90 && angle > -180)
                        {
                            angle += 180;
                        }
                        outCoord.Append("[");
                        string temp =
                              "X:" + (1000 * X.D + x_Compensation).ToString("F2") + ";"
                            + "Y:" + (1000 * Y.D + y_Compensation).ToString("F2") + ";"
                            + "A:" + (angle.D + angle_Compensation).ToString("F2") + ";"
                            + "ATTR:" + "0" + ";"
                            + "ID:" + id_Send.I.ToString();
                        outCoord.Append(temp);
                        outCoord.Append("]" + Environment.NewLine);
                    }
                    outCoord.Append("Done" + Environment.NewLine);
                }
                else
                {
                    return ("Image" + Environment.NewLine + "Done" + Environment.NewLine);
                }


            }
            else
            {
                outCoord.Clear();
                outCoord.Append("Image" + Environment.NewLine);

                for (int i = 0; i < row_Send.Length; i++)
                {

                    HTuple angle;
                    HOperatorSet.TupleDeg(Angle_Send[i].D, out angle);
                    outCoord.Append("[");
                    string temp =
                        "X:" + (row_Send[i].D + x_Compensation).ToString("F2") + ";" +
                        "Y:" + (col_Send[i].D + y_Compensation).ToString("F2") + ";" +
                        "A:" + (angle.D + angle_Compensation).ToString("F2") + ";" +
                        "ID:" + id_Send.I.ToString() + ";";
                    outCoord.Append(temp);
                    outCoord.Append("];" + Environment.NewLine);
                }
                outCoord.Append("Done" + Environment.NewLine);
            }
            return (outCoord.ToString());

        }

        private void No_Backflow(int i)
        {
            if (row_Send.Length == 0)
            {
                row_Send = MGrabPointSetting.GrabRowTarget[i];
                col_Send = MGrabPointSetting.GrabColTarget[i];
                Angle_Send = MFindShapeMode.angle[i];
                id_Send = new HTuple((int)0);
            }
            else
            {
                row_Send = row_Send.TupleConcat(MGrabPointSetting.GrabRowTarget[i]);
                col_Send = col_Send.TupleConcat(MGrabPointSetting.GrabColTarget[i]);
                Angle_Send = Angle_Send.TupleConcat(MFindShapeMode.angle[i]);
                id_Send = id_Send.TupleConcat((int)0);
            }
        }

        public override void ShowResult(HWndCtrl viewCtrl)
        {
            if (!bFindShapeMode)
                return;
            if (MFindShapeMode.row == null || MFindShapeMode.row.Length < 1)
            {
                return;
            }
            if (RuningFinish == false)
                return;
            MFindShapeMode.ShowResult(viewCtrl);
            if (bGrabPointSetting)
            {
                MGrabPointSetting.ShowGrabPoint(viewCtrl);
            }
            if (bPZT_Detection_Result && bPZT_Detection_Enble)
            {
                MPZT_Detection.Show(viewCtrl);
            }

            if (bLM_DP_Enble && bLM_DP_Result)
            {
                MLM_DP.Show(viewCtrl);
            }

            if (bGrab_Intervene_Result && bGrab_Intervene_Enble)
            {
                MGrab_Intervene.Show(viewCtrl);
            }
            if (bFangDai_Enable && bFangDai_Result)
            {
                MFangDai.Show(viewCtrl);
            }
            if (bLanQuan_Enable && bLanQuan_Result)
            {
                MLanQuan.Show(viewCtrl);
            }

            string temp = GetSendResult();
            if (NotifyExcInfo != null)
            {
                NotifyExcInfo("发送结果：" + temp);
            }

            if (row_Send != null && row_Send.Length > 0)
            {
                for (int i = 0; i < row_Send.Length; i++)
                {
                    if (id_Send[i].I == 1)
                    {
                        viewCtrl.AddText("OK", (int)(row_Send[i].D), (int)(col_Send[i].D), 80, "green");
                    }

                }
            }

            if (row_NG == null || row_NG.Length < 1)
                return;
            for (int i = 0; i < row_NG.Length; i++)
            {
                viewCtrl.AddText("NG:" + NG_Reason[i].S, (int)(row_NG[i].D), (int)(col_NG[i].D), 40, "red");
            }

        }
        public override void Close()
        {
            try
            {
                base.Close();
                MCreateShapeModel.Close();
                MCreateShapeModel = null;
                MFindShapeMode.Close();
                MFindShapeMode = null;
                MGrabPointSetting.Close();
                MGrabPointSetting = null;
                MPZT_Detection.Close();
                MPZT_Detection = null;
                MGrab_Intervene.Close();
                MGrab_Intervene = null;
                MLM_DP.Close();
                MLM_DP = null;
                MFangDai.Close();
                MFangDai = null;
                MLanQuan.Close();
                MLanQuan = null;
                NotifyExcInfo = null;
            }
            catch (Exception ex)
            {
                Util.WriteLog(this.GetType(), ex);
                Util.Notify("工具删除异常");
            }

        }
        public void SetGrabPoint(double x, double y)
        {
            //转换camera 、 projection
            MGrabPointSetting.X = x;
            MGrabPointSetting.Y = y;
            HTuple Cx, Cy, Cz, homMat3d;
            HOperatorSet.PoseToHomMat3d(worldPose, out homMat3d);
            HOperatorSet.AffineTransPoint3d(homMat3d, x, y, 0, out Cx, out Cy, out Cz);
            HTuple row, column;
            HOperatorSet.Project3dPoint(Cx, Cy, Cz, cameraParam, out row, out column);
            MGrabPointSetting.SetGrabPoint(row.D, column.D);
        }
        private void SaveImage(string files, HImage ngImage)
        {

            if (ngImage == null || ngImage.IsInitialized() == false)
            {
                Util.WriteLog(this.GetType(), "异常图像数据丢失");
                Util.Notify("异常图像数据丢失");
                return;
            }
            HImage imgSave = ngImage.CopyImage();
            Task.Run(() =>
            {
                try
                {
                    FileInfo fi = new FileInfo(files);
                    if (!fi.Directory.Exists)
                    {
                        fi.Directory.Create();
                    }                    
                    imgSave.WriteImage("png", 0, files);
                    imgSave.Dispose();
                }
                catch (Exception ex)
                {
                    Util.WriteLog(this.GetType(), ex);
                    Util.Notify(string.Format("相机{0}异常图像保存异常", settingIndex));
                }
            });

        }
    }
}
