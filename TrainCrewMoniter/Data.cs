using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TrainCrewMoniter
{
    /// <summary>
    /// データクラス
    /// </summary>
    public class Data
    {
        /// <summary>
        /// 画像の回転
        /// </summary>
        /// <param name="org_bmp"></param>
        /// <param name="angle"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Bitmap RotateBitmap(Bitmap org_bmp, float angle, int x, int y)
        {
            Bitmap result_bmp = new Bitmap((int)org_bmp.Width, (int)org_bmp.Height);
            Graphics g = Graphics.FromImage(result_bmp);

            g.TranslateTransform(-x, -y);
            g.RotateTransform(angle, System.Drawing.Drawing2D.MatrixOrder.Append);
            g.TranslateTransform(x, y, System.Drawing.Drawing2D.MatrixOrder.Append);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(org_bmp, 0, 0);
            g.Dispose();

            return result_bmp;
        }

        /// <summary>
        /// 整数nのd桁目を切り捨てする
        /// </summary>
        /// <param name="n">対象の整数</param>
        /// <param name="d">四捨五入する桁数</param>
        /// <returns></returns>
        public int TruncateInt(int n, int d)
        {
            // 一旦10^dの逆数で数を小数化してMath.Truncateで切り捨てしたのち、10^dの倍数で元に戻す
            double s = Math.Pow(10.0, d);

            return (int)(Math.Truncate(n / s) * s);
        }

        /// <summary>
        /// 整数nの1桁目を切り捨てする(5ずつ)
        /// </summary>
        /// <param name="n">対象の整数</param>
        /// <param name="d">四捨五入する桁数</param>
        /// <returns></returns>
        public int TruncateInt5(int n)
        {
            // 一旦5^dの逆数で数を小数化してMath.Truncateで切り捨てしたのち、5^dの倍数で元に戻す
            double s = Math.Pow(5.0, 1);

            return (int)(Math.Truncate(n / s) * s);
        }

        /// <summary>
        /// リストから指定した値に一番近い値を持つインデックスを取得する
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int FindClosestIndex(List<float> list, float value)
        {
            float closest = list.Aggregate((x, y) => Math.Abs(x - value) < Math.Abs(y - value) ? x : y);
            return list.IndexOf(closest);
        }

        /// <summary>
        /// 偶数かどうかを判定する
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool IsEven(int num)
        {
            return (num % 2 == 0);
        }
    }

    /// <summary>
    /// float型拡張クラス
    /// </summary>
    public static class FloatExtensions
    {
        public static bool IsZero(this float self)
        {
            return self.IsZero(float.Epsilon);
        }

        public static bool IsZero(this float self, float epsilon)
        {
            return Math.Abs(self) < epsilon;
        }
    }
}
