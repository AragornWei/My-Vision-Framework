using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using Yoga.ImageControl;
using System.IO;

namespace Yoga.HILTI.AutoPacking.GeneralTool

{
    [Serializable]
    public class GrabPointSetting
    {
        public double GrabRowOrg = -1;
        public double GrabColOrg = -1;
        public double X = -1;
        public double Y = -1;
        public bool fromPictrue = true;

        [NonSerialized]
        public HTuple GrabRowTarget;
        [NonSerialized]
        public HTuple GrabColTarget;
        [NonSerialized]
        HXLDCont GrabXld;
        public void SetGrabPoint(double row, double col)
        {
            GrabRowOrg = row;
            GrabColOrg = col;
        }
        public bool setTarget(List<HHomMat2D> mat2DsList)
        {
            if (mat2DsList==null || mat2DsList.Count < 1)
            {
                return false;
            }

            if (GrabXld != null && GrabXld.IsInitialized())
            {
                GrabXld.Dispose();
            }
            if (GrabXld == null)
            {
                GrabXld = new HXLDCont();
                GrabXld.GenEmptyObj();
            }
            
            GrabRowTarget = new HTuple();
            GrabColTarget = new HTuple();
            HTuple rowTemp, colTemp;
            for (int i = 0; i < mat2DsList.Count; i++)
            {
                HOperatorSet.AffineTransPoint2d(mat2DsList[i], new HTuple(GrabRowOrg), new HTuple(GrabColOrg), out rowTemp, out colTemp);
                GrabRowTarget = GrabRowTarget.TupleConcat(rowTemp);
                GrabColTarget = GrabColTarget.TupleConcat(colTemp);
            }
            return true;
        }
        public void ShowGrabPoint(HWndCtrl viewCtrl)
        {
            if (GrabRowTarget!=null && GrabRowTarget.Length > 0)
            {
                GrabXld.GenCrossContourXld(GrabRowTarget, GrabColTarget, 50, 0);
            }
            if (GrabXld != null && GrabXld.IsInitialized())
            {
                viewCtrl.ChangeGraphicSettings(Mode.COLOR, "red");
                viewCtrl.ChangeGraphicSettings(Mode.LINEWIDTH, 3);
                viewCtrl.AddIconicVar(GrabXld);
            }
        }
        public void SerializeCheck()
        {
            if (GrabXld != null && GrabXld.IsInitialized())
            {
                GrabXld.Dispose();
            }
            GrabXld = null;
            using (Stream objectStream = new MemoryStream())
            {
                System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(objectStream, this);
            }
        }
        public void Close()
        {
            if (GrabXld != null && GrabXld.IsInitialized())
            {
                GrabXld.Dispose();
            }
            GrabXld = null;
        }
    }
}
