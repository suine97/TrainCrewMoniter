using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using TrainCrew;

namespace TrainCrewMoniter
{
    /// <summary>
    /// TASC演算クラス
    /// </summary>
    public class TASC
    {
        /// <summary>
        /// 停車パターン用減速度
        /// </summary>
        private readonly float stoppingDeceleration = 3.0f;

        /// <summary>
        /// 停車パターン用減速度(タイムアタック用)
        /// </summary>
        private readonly float stoppingDecelerationForTimeAttack = 3.5f;

        /// <summary>
        /// 軽減パターン用減速度
        /// </summary>
        private readonly float stoppingReductionDeceleration = 2.0f;

        /// <summary>
        /// 停車パターン
        /// </summary>
        private float stoppingPattern = 0.0f;

        /// <summary>
        /// 停車軽減パターン
        /// </summary>
        private float stoppingReductionPattern = 0.0f;

        /// <summary>
        /// 停車パターン用オフセット距離
        /// </summary>
        private readonly float offset = 2.00f;

        /// <summary>
        /// 勾配係数
        /// </summary>
        private readonly int K = 31;

        /// <summary>
        /// 停止位置範囲
        /// </summary>
        private readonly float stopRange = 3.00f;

        /// <summary>
        /// 制動待機距離[m]
        /// </summary>
        private readonly float standbyBreakingDistance = 700.0f;

        /// <summary>
        /// 空走時間[s]
        /// </summary>
        private readonly float[] freeRunningTime = new float[10]
        {
            2.0f, //None
            2.0f, //5320形
            2.0f, //5300形
            2.0f, //4300形
            2.0f, //4321F
            2.0f, //4000形
            2.0f, //4000形更新車
            2.0f, //50000形
            2.0f, //3300形VVVF
            2.0f, //3020形
        };

        /// <summary>
        /// 最大減速度[km/h/s]
        /// </summary>
        private readonly float[] maxDeceleration = new float[10]
        {
            4.60f, //None
            4.60f, //5320形
            4.60f, //5300形
            4.60f, //4300形
            4.60f, //4321F
            4.20f, //4000形
            4.20f, //4000形更新車
            4.60f, //50000形
            4.60f, //3300形VVVF
            3.50f, //3020形
        };

        /// <summary>
        /// 最大SAP圧力値[kPa]
        /// </summary>
        private readonly float[] maxPressure = new float[10]
        {
            400.00f, //None
            400.00f, //5320形
            400.00f, //5300形
            400.00f, //4300形
            400.00f, //4321F
            400.00f, //4000形
            400.00f, //4000形更新車
            400.00f, //50000形
            400.00f, //3300形VVVF
            400.00f, //3020形
        };

        /// <summary>
        /// 減速度係数(%)
        /// </summary>
        private readonly float[][] constDeceleration = new float[10][]
        {
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //None
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //5320形
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //5300形
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //4300形
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //4321F
            new float[] { 0.00f, 0.11f, 0.23f, 0.36f, 0.50f, 0.64f, 0.77f, 0.91f, 1.00f }, //4000形
            new float[] { 0.00f, 0.11f, 0.23f, 0.36f, 0.50f, 0.64f, 0.77f, 0.91f, 1.00f }, //4000形更新車
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //50000形
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //3300形VVVF
            new float[] { 0.00f, 0.11f, 0.22f, 0.33f, 0.45f, 0.56f, 0.67f, 0.78f, 0.89f, 1.00f }, //3020形
        };

        /// <summary>
        /// TASC 残り距離判定フェーズ
        /// [制動待機, 600, 500, 400, 300, 200, 100, 50, 25]
        /// </summary>
        private string sTASCDistancePhase = "制動待機";

        /// <summary>
        /// TASC 有効判定
        /// </summary>
        public bool IsTASCEnable = true;

        /// <summary>
        /// TASC 動作開始判定
        /// </summary>
        public bool IsTASCOperation = false;

        /// <summary>
        /// TASC ブレーキ動作判定
        /// </summary>
        public bool IsTASCBraking = false;

        /// <summary>
        /// TASC 動作フェーズ
        /// [制動待機, 制動中, 制動中, 停車, 解除]
        /// </summary>
        public string sTASCPhase = "解除";

        /// <summary>
        /// TASC 停車演算パターン(km/h)
        /// </summary>
        public float fTASCPatternSpeed = 0.0f;

        /// <summary>
        /// TASC 演算減速度(km/h/s)
        /// </summary>
        public float fTASCDeceleration = 0.0f;

        /// <summary>
        /// TASC ハンドル段数
        /// </summary>
        public int iTASCNotch = 0;

        /// <summary>
        /// TASC SAP圧力値(kPa)
        /// </summary>
        public float fTASCSAPPressure = 0.0f;

        /// <summary>
        /// 平均勾配値(‰)
        /// </summary>
        public float gradientAverage = 0.0f;

        /// <summary>
        /// Xml 上り勾配情報
        /// </summary>
        public XElement UpSideGradient;

        /// <summary>
        /// Xml 下り勾配情報
        /// </summary>
        public XElement DownSideGradient;

        /// <summary>
        /// 車両形式
        /// </summary>
        public TrainModel trainModel= TrainModel.None;

        /// <summary>
        /// データクラス
        /// </summary>
        private readonly Data data = new Data();

        /// <summary>
        /// ツーハンドル運転台判定
        /// </summary>
        public bool IsTwoHandle = false;

        /// <summary>
        /// 電磁直通ブレーキ車判定
        /// </summary>
        public bool IsSMEEBrake = false;

        /// <summary>
        /// 車両形式
        /// </summary>
        public enum TrainModel : int
        {
            None = 0,
            series5320 = 1,
            series5300 = 2,
            series4300 = 3,
            Car4321F = 4,
            series4000 = 5,
            Car4000R = 6,
            series50000 = 7,
            Car3300V = 8,
            series3020 = 9,
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TASC()
        {
            sTASCPhase = "停車";
            IsTASCEnable = true;
            IsTASCOperation = false;
            IsTASCBraking = false;
            fTASCPatternSpeed = 120f;
            fTASCDeceleration = 0f;
            iTASCNotch = 0;
            fTASCSAPPressure = 0.0f;
            trainModel = TrainModel.None;
            IsTwoHandle = false;
            IsSMEEBrake = false;

            //xmlファイル読み込み
            UpSideGradient = XElement.Load(@"Xml\UpSideGradient.xml");
            DownSideGradient = XElement.Load(@"Xml\DownSideGradient.xml");
        }

        /// <summary>
        /// TASC 演算メソッド
        /// </summary>
        /// <param name="state">列車の状態</param>
        public void TASC_Update(TrainState state)
        {
            float speed = state.Speed;
            float remainigDistance = state.nextStaDistance;
            string stopType = state.nextStopType;
            float dist = remainigDistance > 0.0f ? remainigDistance : 0.0f;

            //車両形式判定
            if (trainModel == TrainModel.None)
            {
                switch (state.CarStates[0].CarModel)
                {
                    case "5320":
                        trainModel = TrainModel.series5320;
                        IsTwoHandle = false;
                        IsSMEEBrake = false;
                        break;
                    case "5300":
                        trainModel = TrainModel.series5300;
                        IsTwoHandle = false;
                        IsSMEEBrake = false;
                        break;
                    case "4300":
                        trainModel = TrainModel.series4300;
                        IsTwoHandle = false;
                        IsSMEEBrake = false;
                        break;
                    case "4000":
                        trainModel = TrainModel.series4000;
                        IsTwoHandle = true;
                        IsSMEEBrake = false;
                        break;
                    case "50000":
                        trainModel = TrainModel.series50000;
                        IsTwoHandle = false;
                        IsSMEEBrake = false;
                        break;
                    case "4321":
                        trainModel = TrainModel.Car4321F;
                        IsTwoHandle = false;
                        IsSMEEBrake = false;
                        break;
                    case "4000R":
                        trainModel = TrainModel.Car4000R;
                        IsTwoHandle = true;
                        IsSMEEBrake = false;
                        break;
                    case "3300V":
                        trainModel = TrainModel.Car3300V;
                        IsTwoHandle = false;
                        IsSMEEBrake = false;
                        break;
                    case "3020":
                        trainModel = TrainModel.series3020;
                        IsTwoHandle = true;
                        IsSMEEBrake = true;
                        break;
                    default:
                        trainModel = TrainModel.series5320;
                        IsTwoHandle = false;
                        IsSMEEBrake = false;
                        break;
                }
            }

            //TASC有効判定
            if (!IsTASCEnable)
            {
                fTASCPatternSpeed = 120.0f;
                fTASCDeceleration = 0.0f;
                gradientAverage = 0.0f;
                stoppingPattern = 0.0f;
                stoppingReductionPattern = 0.0f;
                sTASCDistancePhase = "制動待機";
                sTASCPhase = "解除";
                IsTASCOperation = false;
                IsTASCBraking = false;
                return;
            }

            //勾配平均値演算
            if (stopType.Contains("停車") && remainigDistance < standbyBreakingDistance)
            {
                var distancePhases = new Dictionary<string, float>
                {
                    { "制動待機", 600.0f },
                    { "600", 500.0f },
                    { "500", 400.0f },
                    { "400", 300.0f },
                    { "300", 200.0f },
                    { "200", 100.0f },
                    { "100", 50.0f },
                    { "50", 25.0f }
                };

                if (distancePhases.TryGetValue(sTASCDistancePhase, out var nextDistance))
                {
                    if (remainigDistance < nextDistance)
                    {
                        gradientAverage = CalcTASCAverageGradient(
                            data.IsEven(int.Parse(Regex.Replace(state.diaName, @"[^0-9]", ""))) ? UpSideGradient : DownSideGradient,
                            state.CarStates.Count(),
                            state.nextStaName,
                            dist
                        );

                        sTASCDistancePhase = nextDistance.ToString();
                    }
                }
                else if (sTASCDistancePhase == "25" && sTASCPhase == "停車")
                {
                    sTASCDistancePhase = "制動待機";
                }
            }

            //TASCパターン演算
            if (stopType.Contains("停車") && remainigDistance < standbyBreakingDistance)
            {
                if (TrainCrewInput.gameState.driveMode == DriveMode.RTA)
                    stoppingPattern = CalcTASCStoppingPattern(dist, stoppingDecelerationForTimeAttack);
                else
                    stoppingPattern = CalcTASCStoppingPattern(dist, stoppingDeceleration);
                stoppingReductionPattern = CalcTASCStoppingReductionPattern(dist, stoppingReductionDeceleration);

                if (stoppingPattern > stoppingReductionPattern)
                    fTASCPatternSpeed = stoppingPattern;
                else
                    fTASCPatternSpeed = stoppingReductionPattern;
            }
            else
            {
                fTASCPatternSpeed = 120.0f;
                fTASCDeceleration = 0.0f;
                gradientAverage = 0.0f;
                stoppingPattern = 0.0f;
                stoppingReductionPattern = 0.0f;
                sTASCDistancePhase = "制動待機";
            }

            //出力ノッチ演算
            switch (sTASCPhase)
            {
                case "解除":
                    //次駅停車かつ残り距離が制動待機距離を下回ったら制動待機
                    if (stopType.Contains("停車") && remainigDistance < standbyBreakingDistance)
                    {
                        sTASCPhase = "制動待機";
                    }
                    break;
                case "制動待機":
                    //現在速度が停車パターンを下回ったら制動開始
                    if (fTASCPatternSpeed < speed)
                    {
                        sTASCPhase = "制動中";
                    }
                    break;
                case "制動中":
                    //停車軽減パターンに移行したら制動(低減)
                    if (stoppingReductionPattern >= stoppingPattern)
                    {
                        sTASCPhase = "制動中(低減)";
                    }
                    //停止位置範囲外で停車した場合は制動待機へ遷移
                    else if (stopRange <= Math.Abs(remainigDistance) && speed.IsZero())
                    {
                        iTASCNotch = 0;
                        fTASCSAPPressure = 0.0f;
                        sTASCPhase = "制動待機";
                    }
                    else
                    {
                        iTASCNotch = CalcTASCNotch(speed, dist);
                        fTASCSAPPressure = CalcTASCSAP(speed, dist);
                    }
                    break;
                case "制動中(低減)":
                    //停止位置範囲内かつ速度が0km/h、ドア開になったら停車判定
                    if (Math.Abs(remainigDistance) <= stopRange && speed.IsZero() && !state.AllClose)
                    {
                        //ツーハンドル車は別処理
                        if (IsTwoHandle)
                            iTASCNotch = -4;
                        else
                            iTASCNotch = -5;

                        fTASCSAPPressure = 0.0f;

                        sTASCPhase = "停車";
                    }
                    //停止位置範囲外で停車した場合は制動待機へ遷移
                    else if (stopRange <= Math.Abs(remainigDistance) && speed.IsZero())
                    {
                        iTASCNotch = 0;
                        sTASCPhase = "制動待機";
                    }
                    else
                    {
                        iTASCNotch = CalcTASCNotch(speed, dist);
                        fTASCSAPPressure = CalcTASCSAP(speed, dist);
                    }
                    break;
                case "停車":
                    //力行段投入で解除
                    if (0 < state.Pnotch)
                    {
                        iTASCNotch = 0;
                        fTASCSAPPressure = 0.0f;
                        sTASCPhase = "解除";
                    }
                    break;
                default:
                    break;
            }

            //TASC動作判定更新
            IsTASCOperation = !sTASCPhase.Contains("解除");
            IsTASCBraking = sTASCPhase.Contains("制動中");
        }

        /// <summary>
        /// TASC 出力ノッチ演算メソッド
        /// </summary>
        /// <param name="nowSpeed">現在速度[km/h]</param>
        /// <param name="distance">停止位置までの残り距離[m]</param>
        /// <returns></returns>
        private int CalcTASCNotch(float nowSpeed, float distance)
        {
            //距離オフセット設定
            float dist = distance;
            if (stoppingPattern > stoppingReductionPattern) dist = distance - offset;

            //勾配値算出
            float gradientDec = gradientAverage.IsZero() ? 0.0f : (gradientAverage / K);

            //減速度[km/h/s]に変換した配列を生成
            float[] strCoinstDeceration = constDeceleration[(int)trainModel];
            List<float> notchDist = strCoinstDeceration.Select(i => i * maxDeceleration[(int)trainModel] + gradientDec).ToList();

            //現在速度における停止位置までの減速度算出
            fTASCDeceleration = CalcTASCDeceleration(nowSpeed, 0.0f, dist);

            //演算減速度に最も近い段数のインデックスを出力
            int index = data.FindClosestIndex(notchDist, fTASCDeceleration);

            //TASC演算速度との差が大きければ出力ノッチを+1
            if ((fTASCPatternSpeed + 2.0f) < nowSpeed)
            {
                index++;
            }

            //ツーハンドル車は別処理
            if (IsTwoHandle)
            {
                //B1以下は出力しない
                if (index == 0) index = 1;
                //低速時はB4以上出力しない
                if (5.0f > nowSpeed && index > 4) index = 4;
                //停止直前にB2へ移行
                if (2.5f > nowSpeed && index > 2) index = 2;
                //停止時にB1へ移行
                if (1.0f > nowSpeed && index > 1) index = 1;
            }
            else
            {
                //B1以下は出力しない
                if (index == 1 || index == 0) index = 2;
                //低速時はB4以上出力しない
                if (5.0f > nowSpeed && index > 5) index = 5;
                //停止直前にB2へ移行
                if (2.5f > nowSpeed && index > 3) index = 3;
                //停止時にB1へ移行
                if (1.0f > nowSpeed && index > 2) index = 2;
            }
            //非常ブレーキはB6扱い
            if (index == 8) index = 7;

            return -index;
        }

        /// <summary>
        /// TASC 出力SAP圧力演算メソッド
        /// </summary>
        /// <param name="nowSpeed">現在速度[km/h]</param>
        /// <param name="distance">停止位置までの残り距離[m]</param>
        /// <returns></returns>
        private float CalcTASCSAP(float nowSpeed, float distance)
        {
            //距離オフセット設定
            float dist = distance;
            if (stoppingPattern > stoppingReductionPattern) dist = distance - offset;

            //勾配値算出
            float gradientDec = gradientAverage.IsZero() ? 0.0f : (gradientAverage / K);

            //最大減速度[km/h/s]を取得
            float strMaxDeceration = maxDeceleration[(int)trainModel];

            //最大SAP圧力値[kPa]を取得
            float strMaxPressure = maxPressure[(int)trainModel];

            //現在速度における停止位置までの減速度算出
            fTASCDeceleration = CalcTASCDeceleration(nowSpeed, 0.0f, dist);

            //減速度からSAP圧力値を算出
            float fPressure = (fTASCDeceleration / strMaxDeceration) * strMaxPressure;

            //50kPa以下は出力しない
            if (fPressure <= 50.0f) fPressure = 50.0f;
            //低速時は200kPa以上出力しない
            if (5.0f > nowSpeed && fPressure > 200.0f) fPressure = 200.0f;
            //停止直前に100kPaへ移行
            if (2.5f > nowSpeed && fPressure > 100.0f) fPressure = 100.0f;
            //停止時に50kPaへ移行
            if (1.0f > nowSpeed && fPressure > 50.0f) fPressure = 50.0f;
            //非常ブレーキは最大圧力扱い
            if (fPressure > strMaxPressure) fPressure = strMaxPressure;

            return fPressure;
        }

        /// <summary>
        /// TASC 停車パターン演算メソッド
        /// </summary>
        /// <param name="distance">停止位置までの残り距離[m]</param>
        /// <param name="deceleration">パターン減速度[km/h/s]</param>
        /// <returns></returns>
        private float CalcTASCStoppingPattern(float distance, float deceleration)
        {
            float dist = distance - offset;
            float dec = deceleration;
            float time = freeRunningTime[(int)trainModel];
            if (!gradientAverage.IsZero()) dec += (gradientAverage / K);

            //停車パターン演算
            float v = (-2.0f * dec * time + (float)Math.Sqrt((float)Math.Pow(2.0f * dec * time, 2) - 4 * (-7.2 * dec * dist))) / 2;

            return v;
        }

        /// <summary>
        /// TASC 停車軽減パターン演算メソッド
        /// </summary>
        /// <param name="distance">停止位置までの残り距離[m]</param>
        /// <param name="deceleration">パターン減速度[km/h/s]</param>
        /// <returns></returns>
        private float CalcTASCStoppingReductionPattern(float distance, float deceleration)
        {
            float dist = distance;
            float dec = deceleration;
            float time = freeRunningTime[(int)trainModel];
            if (!gradientAverage.IsZero()) dec += (gradientAverage / K);

            //軽減パターン演算
            float v = (-2.0f * dec * time + (float)Math.Sqrt((float)Math.Pow(2.0f * dec * time, 2) - 4 * (-7.2 * dec * dist))) / 2;

            return v;
        }

        /// <summary>
        /// TASC 指定速度における指定距離までの減速度演算メソッド
        /// </summary>
        /// <param name="nowSpeed">速度[km/h]</param>
        /// <param name="distance">距離[m]</param>
        /// <returns></returns>
        private float CalcTASCDeceleration(float nowSpeed, float distance)
        {
            //減速度演算
            float b = (float)(Math.Pow(nowSpeed, 2) / (7.2 * distance));
            if (!gradientAverage.IsZero()) b -= (gradientAverage / K);

            if (nowSpeed.IsZero()) b = 0.0f;
            if (b < 0f) b = 0.0f;

            return b;
        }
        /// <summary>
        /// TASC 指定速度における指定距離までの減速度演算メソッド
        /// </summary>
        /// <param name="nowSpeed">現在速度[km/h]</param>
        /// <param name="targetSpeed">目標速度[km/h]</param>
        /// <param name="distance">距離[m]</param>
        /// <returns></returns>
        private float CalcTASCDeceleration(float nowSpeed, float targetSpeed, float distance)
        {
            //減速度演算
            float b = (float)((Math.Pow(nowSpeed, 2) - Math.Pow(targetSpeed, 2)) / (7.2 * distance));
            if (!gradientAverage.IsZero()) b -= (gradientAverage / K);

            if (nowSpeed.IsZero()) b = 0.0f;
            if (b < 0f) b = 0.0f;

            return b;
        }

        /// <summary>
        /// TASC 勾配平均値計算メソッド(停止位置まで)
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="station"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private float CalcTASCAverageGradient(XElement element, int carLength, string station, float distance)
        {
            float average = 0.0f;

            var str = element.Elements("Value")
                .Where(s => s.Element("StationName").Value == station)
                .Where(s => float.Parse(s.Element("Distance").Value) < (distance + (carLength * 20.0f)))
                .Select(s => float.Parse(s.Element("Gradient").Value));

            if (str != null && str.Any()) average = str.Average();
            
            return average;
        }
    }
}
