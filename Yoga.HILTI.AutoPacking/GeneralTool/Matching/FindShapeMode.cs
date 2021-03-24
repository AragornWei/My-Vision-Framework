using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using Yoga.ImageControl;
using System.IO;
using Yoga.Common;
using System.Threading;

namespace Yoga.HILTI.AutoPacking.GeneralTool
{
    [Serializable]
    public class FindShapeMode
    {
        [NonSerialized]
        CreateShapeModel createShapeModel;
        [NonSerialized]
        HImage refImage;


        public List<ROI> FindShapeModeRoiList = new List<ROI>();
        public HRegion SearchRegion;
        /// <summary>
        /// 查找模板参数
        /// </summary>
        public double minScore = 0.7;
        public int numMatches = 0;
        public double maxOverlap = 0.1;
        public string subPixel = "least_squares";
        public int numLevels = -1;
        public double greediness = 0.75;

        /// <summary>
        /// 模板区域过滤参数
        /// </summary>
        public int LG_lenght_min =500;
        public int LG_lenght_max = 2000;
        public int DP_width_min = 100;
        public int DP_width_max = 400;
        public int LG_Area_min = 2000;
        public int LG_Area_max = 190000;
        public int LG_threshould = 60;
        public int Closing_width = 10;
        public int Closing_height = 40;

        /// <summary>
        /// 测量实际面积、长度、宽度
        /// </summary>
        [NonSerialized]
        public HTuple LG_lenght, LG_Area,DP_width;

        [NonSerialized]
        public HTuple row, column, angle, scale, score;
        [NonSerialized]
        public HTuple row_temp, column_temp, angle_temp, scale_temp, score_temp;
        [NonSerialized]
        public HXLDCont resultXLDCont;
        [NonSerialized]
        public HRegion resultRegion , scan_regions;

        public bool FindShapeModeAct(HImage refImage, CreateShapeModel createShapeModel, HImage image)
        {
            this.createShapeModel = createShapeModel;
            this.refImage = refImage;

            if (createShapeModel.hShapeModel == null || !createShapeModel.hShapeModel.IsInitialized() || createShapeModel.createNewModelID)
            {
                if (!createShapeModel.CreateShapeModelAct(refImage))
                    return false;
            }
            try
            {
                HImage searchImage;
                if (SearchRegion != null && SearchRegion.IsInitialized())
                {
                    searchImage = image.ReduceDomain(SearchRegion);
                }
                else
                {
                    searchImage = image.Clone();
                }


                ////阈值切割出预选框///      
                LG_lenght = new HTuple();
                LG_Area = new HTuple();
                DP_width = new HTuple();
                HRegion threshold_region, closing_region,fill_up_region,connection;
                threshold_region = searchImage.Threshold(new HTuple(LG_threshould), 255).OpeningCircle(5.0);
                
                closing_region=threshold_region.ClosingRectangle1(Closing_width, Closing_height);
                fill_up_region = closing_region.FillUp();
                connection=fill_up_region.Connection();
                if(scan_regions!=null && scan_regions.IsInitialized())
                {
                    scan_regions.Dispose();
                }
                scan_regions = connection.SelectShape
                    (
                    new HTuple("area").TupleConcat( "rect2_len1").TupleConcat("rect2_len2"),
                    "and", 
                    new HTuple(LG_Area_min).TupleConcat( LG_lenght_min/2).TupleConcat(DP_width_min/2), 
                    new HTuple(LG_Area_max).TupleConcat(LG_lenght_max/2).TupleConcat(DP_width_max / 2)
                    )  ;

                if (threshold_region != null && threshold_region.IsInitialized())
                {
                    threshold_region.Dispose();
                }
                if (closing_region != null && closing_region.IsInitialized())
                {
                    closing_region.Dispose();
                }
                if (fill_up_region != null && fill_up_region.IsInitialized())
                {
                    fill_up_region.Dispose();
                }
                if (connection != null && connection.IsInitialized())
                {
                    connection.Dispose();
                }
                ////阈值切割出预选框///  


                row = new HTuple();
                column = new HTuple();
                angle = new HTuple();
                scale = new HTuple();
                score = new HTuple();

                int Num = scan_regions.CountObj();
                if (Num == 0)
                {
                    return true;
                }
                

                for (int i=1; i <= Num; i++)
                {
                    HRegion scan_region_temp = scan_regions.SelectObj(i);

                    double row_rect2, col_rect2, phi_rect2, leght1_rect2, lenght2_rect2;
                    scan_region_temp.SmallestRectangle2(out row_rect2, out col_rect2, out phi_rect2, out leght1_rect2, out lenght2_rect2);
                    if (LG_lenght == null || LG_lenght.Length == 0)
                    {
                        LG_lenght = new HTuple(2 * leght1_rect2);
                    }
                    else
                    {
                        LG_lenght = LG_lenght.TupleConcat(new HTuple(2 * leght1_rect2));
                    }
                    if (DP_width == null || DP_width.Length == 0)
                    {
                        DP_width = new HTuple(2 * lenght2_rect2);
                    }
                    else
                    {
                        DP_width = DP_width.TupleConcat(new HTuple(2 * lenght2_rect2));
                    }
                    if (LG_Area == null || LG_Area.Length == 0)
                    {
                        LG_Area = scan_region_temp.Area;
                    }
                    else
                    {
                        LG_Area = LG_Area.TupleConcat(scan_region_temp.Area);
                    }


                    row_temp = new HTuple();
                    column_temp = new HTuple();
                    angle_temp = new HTuple();
                    scale_temp = new HTuple();
                    score_temp = new HTuple();

                    //HRegion dilation_Diff = image.GetDomain().Difference(scan_region_temp.DilationCircle(10.0));
                    //HImage souce_image_temp = image.Clone();
                    //HImage paint_image = souce_image_temp.PaintRegion(dilation_Diff, new HTuple(10).TupleConcat(10).TupleConcat(10), "fill");
                    //HImage temp = paint_image.ReduceDomain(scan_region_temp.DilationCircle(10.0));
                    HImage temp = image.ReduceDomain(scan_region_temp.DilationCircle(10.0));
                    HImage guassImage = temp.GaussFilter(7);

                    //HOperatorSet.SetSystem("thread_num", 4);

                    HTuple t1;
                    HOperatorSet.CountSeconds(out t1);
                    
                    try
                    {
                        createShapeModel.hShapeModel.FindScaledShapeModel(
                        guassImage,
                        createShapeModel.angleStart, createShapeModel.angleExtent,
                        createShapeModel.scaleMin, createShapeModel.scaleMax,
                        minScore, numMatches,
                        maxOverlap,
                        new HTuple(subPixel).TupleConcat("max_deformation 2"),
                        new HTuple(new int[] { createShapeModel.numLevels, numLevels }),
                        greediness,
                        out row_temp, out column_temp, out angle_temp, out scale_temp, out score_temp);
                    }
                    catch
                    {

                    }
                    HTuple t2;
                    HOperatorSet.CountSeconds(out t2);
                    double time = (t2 - t1).D * 1000;
                    Util.Notify("每次模板匹配用时:" + time.ToString("F2") + "ms");
                    guassImage.Dispose();
                    temp.Dispose();
                    //souce_image_temp.Dispose();
                    //paint_image.Dispose();
                    //dilation_Diff.Dispose();
                    scan_region_temp.Dispose();

                    if (row_temp != null && row_temp.Length > 0)
                    {
                        if (row.Length == 0)
                        {
                            row = row_temp;
                            column = column_temp;
                            angle = angle_temp;
                            scale = scale_temp;
                            score = score_temp;
                        }
                        else
                        {
                            row = row.TupleConcat(row_temp);
                            column = column.TupleConcat(column_temp);
                            angle = angle.TupleConcat(angle_temp);
                            scale = scale.TupleConcat(scale_temp);
                            score = score.TupleConcat(score_temp);
                        }

                    }

                }
                searchImage.Dispose();                  
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void FindScaleShapeMode(CreateShapeModel createShapeModel, HImage temp)
        {
            
            createShapeModel.hShapeModel.SetShapeModelParam(new HTuple("timeout"), new HTuple(80.0));
            try
            {
                createShapeModel.hShapeModel.FindScaledShapeModel(
                temp,
                createShapeModel.angleStart, createShapeModel.angleExtent,
                createShapeModel.scaleMin, createShapeModel.scaleMax,
                minScore, numMatches,
                maxOverlap,
                new HTuple(subPixel).TupleConcat("max_deformation 2"),
                new HTuple(new int[] { createShapeModel.numLevels, numLevels }),
                greediness,
                out row_temp, out column_temp, out angle_temp, out scale_temp, out score_temp);
            }
            catch
            {
                
            }
        }

            

        #region 仿射变换矩阵
        [NonSerialized]
        List<HHomMat2D> mat2Ds;
        public List<HHomMat2D> GetHHomMat2Ds()
        {
            if (mat2Ds == null)
            {
                mat2Ds = new List<HHomMat2D>();
            }
            else
            {
                mat2Ds.Clear();
            }
            for (int i = 0; i < row.Length; i++)
            {
                HHomMat2D homMat2D = new HHomMat2D();
                homMat2D.VectorAngleToRigid(
                    createShapeModel.refCoordinates[0].D, createShapeModel.refCoordinates[1].D, createShapeModel.refCoordinates[2].D,
                    row[i].D, column[i].D, angle[i].D);
                mat2Ds.Add(homMat2D);

            }
            return mat2Ds;
        }
        private void DefineMat2Ds()
        {
            if (mat2Ds == null)
            {
                mat2Ds = new List<HHomMat2D>();
            }
            else
            {
                mat2Ds.Clear();
            }
            for (int i = 0; i < row_temp.Length; i++)
            {
                HHomMat2D homMat2D = new HHomMat2D();
                homMat2D.VectorAngleToRigid(
                    createShapeModel.refCoordinates[0].D, createShapeModel.refCoordinates[1].D, createShapeModel.refCoordinates[2].D,
                    row_temp[i].D, column_temp[i].D, angle_temp[i].D);
                HRegion boundary_region;
                if (SearchRegion == null | !SearchRegion.IsInitialized())
                {
                    boundary_region = refImage.GetDomain().Boundary("inner");
                }
                else
                {
                    boundary_region = SearchRegion.Boundary("inner");
                }

                HTuple row_rect2, col_rect2, phi_rect2, lenght1_rect2, lenght2_rect2;
                if (this.createShapeModel.modelRegion == null || !this.createShapeModel.modelRegion.IsInitialized())
                {
                    return; 
                }
                this.createShapeModel.modelRegion.SmallestRectangle2(out row_rect2, out col_rect2, out phi_rect2, out lenght1_rect2, out lenght2_rect2);

                HRegion small_rect2_region = new HRegion();
                small_rect2_region.GenRectangle2(row_rect2, col_rect2, phi_rect2, lenght1_rect2, lenght2_rect2);

                HRegion small_rect2_region_affine = homMat2D.AffineTransRegion(small_rect2_region, "nearest_neighbor");

                HObject Area_temp;
                HOperatorSet.Intersection(boundary_region, small_rect2_region_affine, out Area_temp);
                HTuple area, row_tt, col_tt;
                HOperatorSet.AreaCenter(Area_temp, out area, out row_tt, out col_tt);
                if (area.D == 0)
                {
                    mat2Ds.Add(homMat2D);
                    row = row.TupleConcat(row_temp[i]);
                    column = column.TupleConcat(column_temp[i]);
                    angle = angle.TupleConcat(angle_temp[i]);
                    scale = scale.TupleConcat(scale_temp[i]);
                    score = score.TupleConcat(score_temp[i]);
                }
                if (boundary_region != null && boundary_region.IsInitialized())
                {
                    boundary_region.Dispose();
                }
                if (small_rect2_region_affine != null && small_rect2_region_affine.IsInitialized())
                {
                    small_rect2_region_affine.Dispose();
                }
                if (small_rect2_region != null && small_rect2_region.IsInitialized())
                {
                    small_rect2_region.Dispose();
                }
                if (Area_temp != null && Area_temp.IsInitialized())
                {
                    Area_temp.Dispose();
                }

            }
        }
        #endregion

        private void SaveImage(string files, HImage ngImage)
        {

            if (ngImage == null || ngImage.IsInitialized() == false)
            {
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
                }
            });

        }
        public void ShowResult(HWndCtrl viewCtrl)
        {
            viewCtrl.ChangeGraphicSettings(Mode.DRAWMODE, "margin");
            if (SearchRegion != null && SearchRegion.IsInitialized())
            {
                viewCtrl.ChangeGraphicSettings(Mode.COLOR, "blue");
                viewCtrl.ChangeGraphicSettings(Mode.LINEWIDTH, 2);
                viewCtrl.AddIconicVar(SearchRegion);
            }

            if (scan_regions != null && scan_regions.IsInitialized())
            {
                viewCtrl.ChangeGraphicSettings(Mode.COLOR, "cyan");
                viewCtrl.ChangeGraphicSettings(Mode.LINEWIDTH, 2);
                viewCtrl.AddIconicVar(scan_regions);
            }

            if (createShapeModel.hShapeModel == null || !createShapeModel.hShapeModel.IsInitialized() || createShapeModel.createNewModelID)
            {
                if (!createShapeModel.CreateShapeModelAct(refImage))
                    return;
            }

            HXLDCont modelXldCont = createShapeModel.ModelXLDCont;

            if (row.Length < 1)
                return;
            GenDetectionXLDResults(modelXldCont);
            if (resultXLDCont != null && resultXLDCont.IsInitialized())
            {
                viewCtrl.ChangeGraphicSettings(Mode.COLOR, "blue");
                viewCtrl.ChangeGraphicSettings(Mode.LINEWIDTH, 2);
                viewCtrl.AddIconicVar(resultXLDCont);
            }
            //GenDetectionRegionResult(createShapeModel.modelRegion);
            //if (resultRegion != null && resultRegion.IsInitialized())
            //{
            //    viewCtrl.ChangeGraphicSettings(Mode.COLOR, "green");
            //    viewCtrl.ChangeGraphicSettings(Mode.LINEWIDTH, 2);
            //    viewCtrl.AddIconicVar(resultRegion);
            //}

        }
        public void GenDetectionXLDResults(HXLDCont modelXldCont)
        {
            if (resultXLDCont == null)
            {
                resultXLDCont = new HXLDCont();
            }
            if (resultXLDCont != null && resultXLDCont.IsInitialized())
            {
                resultXLDCont.Dispose();
            }
            resultXLDCont.GenEmptyObj();

            HXLDCont rContours;

            for (int i = 0; i < row.Length; i++)
            {
                HHomMat2D mat1 = new HHomMat2D();
                mat1.VectorAngleToRigid(0, 0, 0, row[i].D, column[i].D, angle[i].D);
                mat1 = mat1.HomMat2dScale(scale[i].D, scale[i].D, row[i].D, column[i].D);
                //图像偏移
                rContours = mat1.AffineTransContourXld(modelXldCont);
                //获取模板集合
                resultXLDCont = resultXLDCont.ConcatObj(rContours);
                rContours.Dispose();
                rContours.GenCrossContourXld(row[i].D, column[i].D, 6, angle[i].D);
                resultXLDCont = resultXLDCont.ConcatObj(rContours);
                rContours.Dispose();
            }

        }
        public void GenDetectionRegionResult(HRegion modelRegion)
        {
            if (resultRegion == null)
            {
                resultRegion = new HRegion();
            }
            if (resultRegion != null && resultRegion.IsInitialized())
            {
                resultRegion.Dispose();
            }
            resultRegion.GenEmptyObj();

            HRegion temp = new HRegion();
            for (int i = 0; i < row.Length; i++)
            {
                HHomMat2D mat1 = new HHomMat2D();
                mat1.VectorAngleToRigid(
                    createShapeModel.refCoordinates[0].D,
                    createShapeModel.refCoordinates[1].D,
                    createShapeModel.refCoordinates[2].D,
                    row[i].D, column[i].D, angle[i].D);
                mat1 = mat1.HomMat2dScale(scale[i].D, scale[i].D, row[i].D, column[i].D);
                //图像偏移
                temp = mat1.AffineTransRegion(modelRegion, "nearest_neighbor");
                //获取模板集合
                resultRegion = resultRegion.ConcatObj(temp);
                temp.Dispose();

            }
        }

        public void SerializeCheck()
        {
            if (scan_regions != null && scan_regions.IsInitialized())
            {
                scan_regions.Dispose();
            }
            if (resultXLDCont != null && resultXLDCont.IsInitialized())
            {
                resultXLDCont.Dispose();
            }
            if (resultRegion != null && resultRegion.IsInitialized())
            {
                resultRegion.Dispose();
            }
            refImage = null;
            createShapeModel = null;
            if (SearchRegion != null && !SearchRegion.IsInitialized())
                SearchRegion = null;
            using (Stream objectStream = new MemoryStream())
            {
                System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(objectStream, this);
            }
        }
        public void Close()
        {
            if (scan_regions != null && scan_regions.IsInitialized())
            {
                scan_regions.Dispose();
            }
            if (resultXLDCont != null && resultXLDCont.IsInitialized())
            {
                resultXLDCont.Dispose();
            }
            resultXLDCont = null;
            if (resultRegion != null && resultRegion.IsInitialized())
            {
                resultRegion.Dispose();
            }
            resultRegion = null;
            refImage = null;
            createShapeModel = null;
            if (SearchRegion != null && SearchRegion.IsInitialized())
                SearchRegion.Dispose();
            SearchRegion = null;

        }
        public void Reset()
        {
            if (SearchRegion != null && !SearchRegion.IsInitialized())
            {
                SearchRegion.Dispose();
            }
            SearchRegion = null;
            refImage = null;
            createShapeModel = null;
            FindShapeModeRoiList = new List<ROI>();
        }
    }
}
