using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
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
        /// TASC 停車パターン用減速度
        /// </summary>
        private readonly float fTASCStoppingDeceleration = 3.0f;

        /// <summary>
        /// TASC 軽減パターン用減速度
        /// </summary>
        private readonly float fTASCStoppingReductionDeceleration = 2.0f;

        /// <summary>
        /// TASC 速度制限パターン用減速度
        /// </summary>
        private readonly float fTASCLimitSpeedDeceleration = 2.5f;

        /// <summary>
        /// TASC 停車パターン
        /// </summary>
        private float fTASCStoppingPattern = 0.0f;

        /// <summary>
        /// TASC 停車軽減パターン
        /// </summary>
        private float fTASCStoppingReductionPattern = 0.0f;

        /// <summary>
        /// TASC 停車パターン用オフセット距離
        /// </summary>
        private readonly float fTASCDistanceOffset = 2.00f;

        /// <summary>
        /// TASC 一時保存用動作フェーズ
        /// </summary>
        private string strTASCPhase = "解除";

        /// <summary>
        /// TASC 一時保存用目標制限速度
        /// </summary>
        private float strTargetLimitSpeed = 0.0f;

        /// <summary>
        /// TASC 一時保存用目標制限距離
        /// </summary>
        private float strTargetLimitDistance = 0.0f;

        /// <summary>
        /// TASC 駅停車判定
        /// </summary>
        private bool IsTASCStoppedStation = false;

        /// <summary>
        /// TASC 有効判定
        /// </summary>
        public bool IsTASCEnable = true;

        /// <summary>
        /// TASC 速度制御有効判定
        /// </summary>
        public bool IsTASCSpeedControlEnable = true;

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
        /// [制御待機, 停車制御, 停車制御(低減), 速度制御, 抑速制御, 停車, 解除]
        /// </summary>
        public string sTASCPhase = "解除";

        /// <summary>
        /// TASC パターンモード
        /// [平常, 高速, 低速]
        /// </summary>
        public string sTASCPatternMode = "平常";

        /// <summary>
        /// TASC 追加操作
        /// </summary>
        private string sTASCAdditionalOperation = "None";

        /// <summary>
        /// TASC 停車演算パターン(km/h)
        /// </summary>
        public float fTASCPatternSpeed = 0.0f;

        /// <summary>
        /// TASC 速度制限演算パターン(km/h)
        /// </summary>
        public float fTASCLimitPatternSpeed = 0.0f;

        /// <summary>
        /// TASC XML 制限速度(km/h)
        /// </summary>
        public float fTASCXmlLimitSpeed = 0.0f;

        /// <summary>
        /// TASC XML 制限速度までの残り距離(m)
        /// </summary>
        public float fTASCXmlLimitDistance = 0.0f;

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
        /// TASC 平均勾配値(‰)
        /// </summary>
        public float fTASCGradientAverage = 0.0f;

        /// <summary>
        /// TASC 制動待機距離[m]
        /// </summary>
        public float fTASCStandbyBreakingDistance = 700.0f;

        /// <summary>
        /// ATO 有効判定
        /// </summary>
        public bool IsATOEnable = false;

        /// <summary>
        /// ATO 最高速度到達フラグ
        /// </summary>
        public bool IsATOMaxSpeedReached = false;

        /// <summary>
        /// ATO 出発ボタン操作有効判定
        /// </summary>
        public bool IsATOStartButtonControl = false;

        /// <summary>
        /// ATO 出発ボタン押下判定
        /// </summary>
        public bool IsATOStartButtonActive = false;

        /// <summary>
        /// ATO 動作フェーズ
        /// [制御待機, 加速制御, 駅停車, 機外停車, 解除]
        /// </summary>
        public string sATOPhase = "解除";

        /// <summary>
        /// ATO パターンモード
        /// [平常, 回復, 遅速]
        /// </summary>
        public string sATOPatternMode = "平常";

        /// <summary>
        /// ATO 一時保存用動作フェーズ
        /// </summary>
        private string strATOPhase = "解除";

        /// <summary>
        /// ATO 一時保存用最高速度
        /// </summary>
        private float strATOMaxSpeed = 0.0f;

        /// <summary>
        /// ATO ハンドル段数
        /// </summary>
        public int iATONotch = 0;

        /// <summary>
        /// ATO 最高速度(km/h)
        /// </summary>
        public float fATOMaxSpeed = 0.0f;

        /// <summary>
        /// ツーハンドル運転台判定
        /// </summary>
        public bool IsTwoHandle = false;

        /// <summary>
        /// 電磁直通ブレーキ車判定
        /// </summary>
        public bool IsSMEEBrake = false;

        /// <summary>
        /// SAP圧リセット判定
        /// </summary>
        public bool IsSAPReset = false;

        /// <summary>
        /// 勾配起動スイッチ判定
        /// </summary>
        private bool IsGradientStart = false;

        /// <summary>
        /// 勾配係数
        /// </summary>
        private readonly int iGradientCoefficient = 35;

        /// <summary>
        /// 停止位置範囲
        /// </summary>
        private readonly float fStopRange = 3.00f;

        /// <summary>
        /// 停止位置オフセット距離
        /// </summary>
        private float fStopPositionOffset = 0.0f;

        /// <summary>
        /// 空走時間[s]
        /// </summary>
        private readonly float[] freeRunningTime = new float[12]
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
            2.0f, //4600形
            2.0f, //5600形
        };

        /// <summary>
        /// 最大減速度[km/h/s]
        /// </summary>
        private readonly float[] maxDeceleration = new float[12]
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
            4.60f, //4600形
            4.60f, //5600形
        };

        /// <summary>
        /// 最大SAP圧力値[kPa]
        /// </summary>
        private readonly float[] maxPressure = new float[12]
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
            400.00f, //4600形
            400.00f, //5600形
        };

        /// <summary>
        /// 減速度係数(%)
        /// </summary>
        private readonly float[][] constDeceleration = new float[12][]
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
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //4600形
            new float[] { 0.00f, 0.00f, 0.18f, 0.33f, 0.49f, 0.61f, 0.75f, 0.89f, 1.00f }, //5600形
        };

        /// <summary>
        /// 車両形式
        /// </summary>
        public TrainModel trainModel = TrainModel.None;

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
            series4600 = 10,
            series5600 = 11,
        }

        /// <summary>
        /// Xml 最高速度情報
        /// </summary>
        private List<MaxSpeedClass> MaxSpeedList;

        /// <summary>
        /// Xml 勾配情報
        /// </summary>
        private List<GradientClass> GradientList;

        /// <summary>
        /// Xml 速度制限情報
        /// </summary>
        private List<SpeedLimitClass> SpeedLimitList;

        /// <summary>
        /// Xml 追加操作情報
        /// </summary>
        private List<AdditionalOperationClass> OperationList;

        /// <summary>
        /// Xml 停止位置オフセット情報
        /// </summary>
        private List<StopPositionOffsetClass> StopPositionOffsetList;

        /// <summary>
        /// データクラス
        /// </summary>
        private readonly Data data = new Data();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TASC()
        {
            //Xmlファイル読み込み
            GradientList = LoadXmlData(@"Xml\Gradient.xml", element => new GradientClass
            {
                Direction = element.Element("Direction").Value,
                StationName = element.Element("StationName").Value,
                Distance = float.Parse(element.Element("Distance").Value),
                Gradient = float.Parse(element.Element("Gradient").Value)
            });

            MaxSpeedList = LoadXmlData(@"Xml\MaxSpeed.xml", element => new MaxSpeedClass
            {
                Direction = element.Element("Direction").Value,
                StationName = element.Element("StationName").Value,
                Class = element.Element("Class").Value,
                StartPos = float.Parse(element.Element("StartPos").Value),
                EndPos = float.Parse(element.Element("EndPos").Value),
                MaxSpeed = float.Parse(element.Element("MaxSpeed").Value)
            });

            SpeedLimitList = LoadXmlData(@"Xml\SpeedLimit.xml", element => new SpeedLimitClass
            {
                Direction = element.Element("Direction").Value,
                StartPos = float.Parse(element.Element("StartPos").Value),
                EndPos = float.Parse(element.Element("EndPos").Value),
                Limit = float.Parse(element.Element("Limit").Value),
                BackStopPosName = element.Element("BackStopPosName").Value,
                NextStopPosName = element.Element("NextStopPosName").Value
            });

            OperationList = LoadXmlData(@"Xml\Operation.xml", element => new AdditionalOperationClass
            {
                Direction = element.Element("Direction").Value,
                StationName = element.Element("StationName").Value,
                StartPos = float.Parse(element.Element("StartPos").Value),
                EndPos = float.Parse(element.Element("EndPos").Value),
                AdditionalOperation = element.Element("Operation").Value
            });

            StopPositionOffsetList = LoadXmlData(@"Xml\StopPositionOffset.xml", element => new StopPositionOffsetClass
            {
                Direction = element.Element("Direction").Value,
                StationName = element.Element("StationName").Value,
                Offset = new List<float>
                {
                    float.Parse(element.Element("Offset1").Value),
                    float.Parse(element.Element("Offset2").Value),
                    float.Parse(element.Element("Offset3").Value),
                    float.Parse(element.Element("Offset4").Value),
                    float.Parse(element.Element("Offset5").Value),
                    float.Parse(element.Element("Offset6").Value)
                }
            });

            //変数初期化
            sTASCPhase = "停車";
            strTASCPhase = "停車";
            sATOPhase = "解除";
            strATOPhase = "解除";
            sTASCAdditionalOperation = "None";
            IsATOEnable = false;
            IsTASCEnable = true;
            IsTASCSpeedControlEnable = true;
            IsTASCOperation = false;
            IsTASCBraking = false;
            IsTASCStoppedStation = true;
            IsATOMaxSpeedReached = false;
            IsATOStartButtonControl = false;
            IsATOStartButtonActive = false;
            fTASCPatternSpeed = 120.0f;
            fTASCLimitPatternSpeed = 120.0f;
            fTASCDeceleration = 0.0f;
            fTASCXmlLimitSpeed = 0.0f;
            fTASCXmlLimitDistance = 0.0f;
            iTASCNotch = 0;
            iATONotch = 0;
            fATOMaxSpeed = 0.0f;
            strATOMaxSpeed = 0.0f;
            fTASCSAPPressure = 0.0f;
            strTargetLimitSpeed = 0.0f;
            strTargetLimitDistance = 0.0f;
            fStopPositionOffset = 0.0f;
            trainModel = TrainModel.None;
            IsTwoHandle = false;
            IsSMEEBrake = false;
            IsSAPReset = false;
            IsGradientStart = false;
        }

        /// <summary>
        /// XmlData読み込みメソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<T> LoadXmlData<T>(string filePath, Func<XElement, T> selector)
        {
            try
            {
                return XElement.Load(filePath).Elements().Select(selector).ToList();
            }
            catch
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// TASC 演算メソッド
        /// </summary>
        /// <param name="state">列車の状態</param>
        /// <param name="signal">信号機状態</param>
        public void TASC_Update(TrainState state, string signalName)
        {
            float speed = state.Speed;
            float remainigDistance = state.nextStaDistance;
            string stopType = state.nextStopType;
            float dist = remainigDistance > 0.0f ? remainigDistance : 0.0f;

            //車両形式判定
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
                case "4600":
                    trainModel = TrainModel.series4600;
                    IsTwoHandle = false;
                    IsSMEEBrake = false;
                    break;
                case "5600":
                    trainModel = TrainModel.series5600;
                    IsTwoHandle = false;
                    IsSMEEBrake = false;
                    break;
                default:
                    trainModel = TrainModel.series5320;
                    IsTwoHandle = false;
                    IsSMEEBrake = false;
                    break;
            }

            //ATO有効判定
            if (!IsATOEnable)
            {
                iATONotch = 0;
                sATOPhase = "解除";
                strATOPhase = "解除";
                IsATOMaxSpeedReached = false;
            }
            //TASC有効判定
            if (!IsTASCEnable)
            {
                fTASCPatternSpeed = 120.0f;
                fTASCLimitPatternSpeed = 120.0f;
                fTASCDeceleration = 0.0f;
                fTASCXmlLimitSpeed = 0.0f;
                fTASCXmlLimitDistance = 0.0f;
                fTASCGradientAverage = 0.0f;
                fTASCStoppingPattern = 0.0f;
                fTASCStoppingReductionPattern = 0.0f;
                fTASCStandbyBreakingDistance = 700.0f;
                strTargetLimitSpeed = 0.0f;
                strTargetLimitDistance = 0.0f;
                sTASCPhase = "解除";
                strTASCPhase = "解除";
                sTASCAdditionalOperation = "None";
                IsTASCOperation = false;
                IsTASCBraking = false;
                IsTASCStoppedStation = false;
                IsSAPReset = false;
                IsGradientStart = false;
                return;
            }

            //停止位置オフセット取得
            fStopPositionOffset = GetStopPositionOffset(state);

            //TASCパターンモード設定
            float fTASCPatternOffset = (sTASCPatternMode == "高速") ? 0.4f : (sTASCPatternMode == "低速") ? -0.5f : 0.0f;

            //TASC制動待機距離演算
            fTASCStandbyBreakingDistance = CalcTASCStoppingDistance(speed, fTASCStoppingDeceleration) + 200.0f;

            //TASC駅停車判定(停止位置範囲内かつ速度が0km/h、ドア開(乗降駅のみ)になったら停車判定)
            if (state.stationList[state.nowStaIndex].stopType == StopType.StopForPassenger && Math.Abs(remainigDistance) <= fStopRange && speed.IsZero() && !state.AllClose)
                IsTASCStoppedStation = true;
            else if (state.stationList[state.nowStaIndex].stopType == StopType.StopForOperation && Math.Abs(remainigDistance) <= fStopRange && speed.IsZero())
                IsTASCStoppedStation = true;
            else
                IsTASCStoppedStation = false;

            //TASC制限速度取得
            GetTASCLimitSpeed(state, dist, fStopPositionOffset, out float xmlLimitSpeed, out float xmlLimitDistance);
            if ((state.nextSpeedLimit >= 0.0f) && state.nextSpeedLimit <= xmlLimitSpeed)
            {
                fTASCXmlLimitSpeed = state.nextSpeedLimit;
                fTASCXmlLimitDistance = state.nextSpeedLimitDistance;
            }
            else
            {
                fTASCXmlLimitSpeed = xmlLimitSpeed;
                fTASCXmlLimitDistance = xmlLimitDistance;
            }

            //TASC残距離取得用
            int tascFixedPointDistance = 1000;
            if (dist < 50) tascFixedPointDistance = 50;
            else if (dist < 150) tascFixedPointDistance = 150;
            else if (dist < 300) tascFixedPointDistance = 300;
            else if (dist < 600) tascFixedPointDistance = 600;

            //TASC勾配平均値演算
            if (IsTASCEnable)
                fTASCGradientAverage = CalcAverageGradientToAbsolutePosition(state, tascFixedPointDistance, 0.0f, fStopPositionOffset);
            else if (state.nextSpeedLimit >= 0.0f)
                fTASCGradientAverage = CalcAverageGradientToAbsolutePosition(state, dist, dist - fTASCXmlLimitDistance, fStopPositionOffset);
            else
                fTASCGradientAverage = CalcAverageGradientToRelativePosition(state, dist, 600.0f, fStopPositionOffset);

            //TASC追加操作取得
            sTASCAdditionalOperation = GetTASCAdditionalOperation(state, dist, fStopPositionOffset);

            //TASC速度制限パターン演算
            if (fTASCXmlLimitSpeed < state.speedLimit)
            {
                strTargetLimitSpeed = fTASCXmlLimitSpeed;

                //出発信号機以外の停止信号ならR0標識手前を目標にする
                float calcTargetLimitDistance = (fTASCXmlLimitDistance > 0.0f) ? fTASCXmlLimitDistance : 0.0f;
                if (strTargetLimitSpeed.IsZero() && !signalName.Contains("出発"))
                    strTargetLimitDistance = ((calcTargetLimitDistance - 15.0f) > 0.0f) ? (calcTargetLimitDistance - 15.0f) : 0.0f;
                //出発信号機の停止信号なら信号機建植位置を目標にする
                else if (strTargetLimitSpeed.IsZero() && signalName.Contains("出発"))
                    strTargetLimitDistance = calcTargetLimitDistance;
                //それ以外は10m手前を目標にする
                else
                    strTargetLimitDistance = calcTargetLimitDistance - 10.0f;

                //速度制限パターン演算
                float calcLimitSpeedPattern = CalcTASCLimitSpeedPattern(strTargetLimitSpeed, strTargetLimitDistance, fTASCLimitSpeedDeceleration + fTASCPatternOffset);
                //最も低い制限速度を選択
                if (calcLimitSpeedPattern > state.speedLimit)
                    fTASCLimitPatternSpeed = state.speedLimit;
                else if (calcLimitSpeedPattern > strTargetLimitSpeed)
                    fTASCLimitPatternSpeed = calcLimitSpeedPattern;
                else
                    fTASCLimitPatternSpeed = strTargetLimitSpeed;
            }
            else
            {
                strTargetLimitSpeed = state.speedLimit;
                strTargetLimitDistance = 0.0f;

                fTASCLimitPatternSpeed = strTargetLimitSpeed;
            }

            //TASC停車パターン演算
            if (stopType.Contains("停車") && remainigDistance < fTASCStandbyBreakingDistance)
            {
                //停車パターン演算
                fTASCStoppingPattern = CalcTASCStoppingPattern(dist, fTASCStoppingDeceleration + fTASCPatternOffset);
                //停車軽減パターン演算
                if (sTASCPatternMode == "高速")
                    fTASCStoppingReductionPattern = CalcTASCStoppingReductionPattern(dist, fTASCStoppingReductionDeceleration + fTASCPatternOffset);
                else
                    fTASCStoppingReductionPattern = CalcTASCStoppingReductionPattern(dist, fTASCStoppingReductionDeceleration);
                //停車軽減パターン移行判定
                if (fTASCStoppingPattern > fTASCStoppingReductionPattern)
                    fTASCPatternSpeed = fTASCStoppingPattern;
                else
                    fTASCPatternSpeed = fTASCStoppingReductionPattern;
            }
            else
            {
                fTASCPatternSpeed = 120.0f;
                fTASCDeceleration = 0.0f;
                fTASCStoppingPattern = 0.0f;
                fTASCStoppingReductionPattern = 0.0f;
            }

            //TASC制動ノッチ演算
            switch (sTASCPhase)
            {
                case "解除":
                    //次駅停車かつ残り距離が制動待機距離を下回ったら制御待機
                    if (stopType.Contains("停車") && remainigDistance < fTASCStandbyBreakingDistance)
                    {
                        sTASCPhase = "制御待機";
                    }
                    //速度制御が有効かつ現在速度が速度制限パターンを超えたら速度制御開始
                    else if (IsTASCSpeedControlEnable && (int)fTASCLimitPatternSpeed < (int)speed)
                    {
                        strTASCPhase = sTASCPhase;
                        sTASCPhase = "速度制御";
                    }
                    //速度制御が有効かつ追加操作が設定されていたら遷移
                    else if (IsTASCSpeedControlEnable && sTASCAdditionalOperation == "抑速" && state.Pnotch == 0 && iATONotch == 0)
                    {
                        strTASCPhase = sTASCPhase;
                        sTASCPhase = "抑速制御";
                    }
                    break;
                case "制御待機":
                    //現在速度が停車パターンを越えたら停車制御開始
                    if (fTASCPatternSpeed < speed)
                    {
                        sTASCPhase = "停車制御";
                    }
                    //速度制御が有効かつ現在速度が速度制限パターンを超えたら速度制御開始
                    else if (IsTASCSpeedControlEnable && (int)fTASCLimitPatternSpeed < (int)speed)
                    {
                        strTASCPhase = sTASCPhase;
                        sTASCPhase = "速度制御";
                    }
                    //速度制御が有効かつ追加操作が設定されていたら遷移
                    else if (IsTASCSpeedControlEnable && sTASCAdditionalOperation == "抑速" && state.Pnotch == 0 && iATONotch == 0)
                    {
                        strTASCPhase = sTASCPhase;
                        sTASCPhase = "抑速制御";
                    }
                    break;
                case "停車制御":
                    //停車軽減パターンに移行したら制動(低減)
                    if (fTASCStoppingReductionPattern >= fTASCStoppingPattern)
                    {
                        sTASCPhase = "停車制御(低減)";
                    }
                    //停止位置範囲外で停車した場合は制動待機へ遷移
                    else if (fStopRange <= Math.Abs(remainigDistance) && speed.IsZero())
                    {
                        iTASCNotch = 0;
                        IsSAPReset = true;
                        sTASCPhase = "制御待機";
                    }
                    else
                    {
                        iTASCNotch = CalcTASCNotch(speed, dist);
                        fTASCSAPPressure = CalcTASCSAP(speed, dist);
                    }
                    break;
                case "停車制御(低減)":
                    //停止位置範囲内かつ速度が0km/h、ドア開(乗降駅のみ)になったら停車判定
                    if (IsTASCStoppedStation)
                    {
                        //ツーハンドル車は別処理
                        if (IsTwoHandle)
                            iTASCNotch = -4;
                        else
                            iTASCNotch = -5;
                        IsSAPReset = true;
                        sTASCPhase = "停車";
                    }
                    //停止位置範囲外で停車した場合は制動待機へ遷移
                    else if (fStopRange <= Math.Abs(remainigDistance) && speed.IsZero())
                    {
                        iTASCNotch = 0;
                        IsSAPReset = true;
                        sTASCPhase = "制御待機";
                    }
                    else
                    {
                        iTASCNotch = CalcTASCNotch(speed, dist);
                        fTASCSAPPressure = CalcTASCSAP(speed, dist);
                    }
                    break;
                case "速度制御":
                    //現在速度が停車パターンを越えたら停車制御開始
                    if (fTASCPatternSpeed < speed)
                    {
                        sTASCPhase = "停車制御";
                        strTASCPhase = "解除";
                    }
                    //速度制御が無効または現在速度が速度制限パターンを下回ったら解除
                    else if (!IsTASCSpeedControlEnable || (int)fTASCLimitPatternSpeed > (int)speed)
                    {
                        iTASCNotch = 0;
                        IsSAPReset = true;
                        sTASCPhase = strTASCPhase;
                        strTASCPhase = "解除";
                    }
                    else
                    {
                        iTASCNotch = CalcTASCLimitSpeedNotch(speed, strTargetLimitSpeed, strTargetLimitDistance);
                        fTASCSAPPressure = CalcTASCLimitSpeedSAP(speed, strTargetLimitSpeed, strTargetLimitDistance);
                    }
                    break;
                case "抑速制御":
                    //現在速度が停車パターンを越えたら停車制御開始
                    if (fTASCPatternSpeed < speed)
                    {
                        sTASCPhase = "停車制御";
                    }
                    //速度制御が有効かつ現在速度が速度制限パターンを超えたら速度制御開始
                    else if (IsTASCSpeedControlEnable && (int)fTASCLimitPatternSpeed < (int)speed)
                    {
                        strTASCPhase = sTASCPhase;
                        sTASCPhase = "速度制御";
                    }
                    //速度制御が無効 or 力行操作 or 追加操作が消去されたら解除
                    else if (!IsTASCSpeedControlEnable || 0 < state.Pnotch || 0 < iATONotch || sTASCAdditionalOperation != "抑速")
                    {
                        iTASCNotch = 0;
                        IsSAPReset = true;
                        sTASCPhase = strTASCPhase;
                        strTASCPhase = "解除";
                    }
                    else
                    {
                        //ツーハンドル車は別処理
                        if (IsTwoHandle)
                        {
                            iTASCNotch = -1;
                            fTASCSAPPressure = 50.0f;
                        }
                        else
                        {
                            iTASCNotch = -1;
                        }
                    }
                    break;
                case "停車":
                    //力行段投入で解除
                    if (0 < state.Pnotch || 0 < iATONotch)
                    {
                        iTASCNotch = 0;
                        fTASCSAPPressure = 0.0f;
                        sTASCPhase = "解除";
                        strTASCPhase = "解除";
                    }
                    break;
                default:
                    break;
            }

            //TASC動作判定更新
            IsTASCOperation = !sTASCPhase.Contains("解除");
            IsTASCBraking = sTASCPhase.Contains("停車制御") || sTASCPhase.Contains("速度制御");

            //ATO最高速度取得
            strATOMaxSpeed = fATOMaxSpeed;
            fATOMaxSpeed = GetATOMaxSpeed(state, dist, fStopPositionOffset);

            //ATO力行ノッチ演算
            switch (sATOPhase)
            {
                case "解除":
                    //ATO有効判定なら制御待機へ遷移
                    if (IsATOEnable)
                    {
                        sATOPhase = "制御待機";
                    }
                    break;
                case "制御待機":
                    //停止位置範囲内かつ速度が0km/h、ドア開(乗降駅のみ)になったら駅停車判定
                    if (IsTASCStoppedStation)
                    {
                        iATONotch = 0;
                        sATOPhase = "駅停車";
                    }
                    //停止位置範囲外で停車した場合は機外停車判定
                    else if (fStopRange <= Math.Abs(remainigDistance) && speed.IsZero())
                    {
                        iATONotch = 0;
                        sATOPhase = "機外停車";
                    }
                    //最高速度が上方に更新されたら最高速度到達フラグを解除
                    else if (IsATOMaxSpeedReached && (strATOMaxSpeed < fATOMaxSpeed))
                    {
                        IsATOMaxSpeedReached = false;
                        sATOPhase = "加速制御";
                    }
                    //最高速度に到達していなければ加速制御へ遷移
                    else if (!IsATOMaxSpeedReached)
                    {
                        sATOPhase = "加速制御";
                    }
                    //TASCブレーキ未動作かつ指定速度を下回ったら加速制御へ遷移
                    else if (!IsTASCBraking && (speed <= (fATOMaxSpeed - 5.0f)))
                    {
                        IsATOMaxSpeedReached = false;
                        sATOPhase = "加速制御";
                    }
                    break;
                case "加速制御":
                    //停止位置範囲内かつ速度が0km/h、ドア開(乗降駅のみ)になったら駅停車判定
                    if (IsTASCStoppedStation)
                    {
                        iATONotch = 0;
                        sATOPhase = "駅停車";
                    }
                    //停止位置範囲外で停車した場合は機外停車判定
                    else if (!IsGradientStart && fStopRange <= Math.Abs(remainigDistance) && speed.IsZero())
                    {
                        iATONotch = 0;
                        sATOPhase = "機外停車";
                    }
                    //停車パターン・速度制御パターンに接近したら制御待機へ遷移
                    else if ((fTASCPatternSpeed <= speed && speed < fTASCPatternSpeed + 10.0f)
                        || (IsTASCSpeedControlEnable && (int)fTASCLimitPatternSpeed <= speed && speed < (int)fTASCLimitPatternSpeed + 10.0f))
                    {
                        iATONotch = 0;
                        IsATOMaxSpeedReached = true;
                        sATOPhase = "制御待機";
                    }
                    //最高速度に到達したら制御待機へ遷移
                    else if ((fATOMaxSpeed <= 70.0f && (fATOMaxSpeed - 2.0f) <= speed)
                        || (fATOMaxSpeed > 70.0f && (fATOMaxSpeed - 1.0f) <= speed))
                    {
                        iATONotch = 0;
                        IsATOMaxSpeedReached = true;
                        sATOPhase = "制御待機";
                    }
                    else
                    {
                        //勾配起動使用中なら解除
                        if (IsGradientStart && !speed.IsZero())
                        {
                            IsGradientStart = false;
                            TrainCrewInput.SetButton(InputAction.GradientStart, false);
                        }
                        iATONotch = 5;
                    }
                    break;
                case "駅停車":
                    //停止位置情報が更新されたら加速制御
                    if ((!IsATOStartButtonControl || (IsATOStartButtonControl && IsATOStartButtonActive))
                        && fStopRange < Math.Abs(remainigDistance)
                        && state.Bnotch == 0
                        && state.Pnotch == 0)
                    {
                        IsATOMaxSpeedReached = false;

                        if (IsATOStartButtonControl)
                        {
                            sATOPhase = "加速制御(P1)";
                            strATOPhase = "解除";
                        }
                        else
                        {
                            strATOPhase = "加速制御(P1)";
                            sATOPhase = "応答待機";
                            //0.5秒後に加速制御(P1)へ遷移
                            _ = WaitForAsync(0.5f, () => ReturnATOPhase());
                        }
                    }
                    break;
                case "機外停車":
                    //指定速度を下回ったら加速制御(P1)へ遷移
                    if ((!IsATOStartButtonControl || (IsATOStartButtonControl && IsATOStartButtonActive))
                        && ((speed <= (fATOMaxSpeed - 10.0f)) || ((sATOPatternMode == "回復") && (speed <= (fATOMaxSpeed - 5.0f))))
                        && state.Bnotch == 0
                        && state.Pnotch == 0)
                    {
                        IsATOMaxSpeedReached = false;
                        sATOPhase = "加速制御(P1)";
                    }
                    break;
                case "加速制御(P1)":
                    if (iATONotch < 1)
                    {
                        iATONotch = 1;
                        strATOPhase = "加速制御";
                        sATOPhase = "加速制御(P1)";

                        //上り勾配時は勾配起動
                        if (fTASCGradientAverage >= 15.0f)
                        {
                            IsGradientStart = true;
                            TrainCrewInput.SetButton(InputAction.GradientStart, true);
                        }
                        //1.5秒後に加速制御へ遷移
                        _ = WaitForAsync(1.5f, () => ReturnATOPhase());
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// TASC 停車用出力ノッチ演算メソッド
        /// </summary>
        /// <param name="nowSpeed">現在速度[km/h]</param>
        /// <param name="distance">停止位置までの残り距離[m]</param>
        /// <returns></returns>
        private int CalcTASCNotch(float nowSpeed, float distance)
        {
            //距離オフセット設定
            float dist = distance;
            if (fTASCStoppingPattern > fTASCStoppingReductionPattern) dist = distance - fTASCDistanceOffset;

            //勾配値算出
            float gradientDec = fTASCGradientAverage.IsZero() ? 0.0f : (fTASCGradientAverage / iGradientCoefficient);

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
                //停止時にB1へ移行(下り勾配以外)
                if (1.0f > nowSpeed && index > 1 && fTASCGradientAverage > -5.0f) index = 1;
            }
            else
            {
                //B1以下は出力しない
                if (index == 1 || index == 0) index = 2;
                //低速時はB4以上出力しない
                if (5.0f > nowSpeed && index > 5) index = 5;
                //停止直前にB2へ移行
                if (2.5f > nowSpeed && index > 3) index = 3;
                //停止時にB1へ移行(下り勾配以外)
                if (1.0f > nowSpeed && index > 2 && fTASCGradientAverage > -5.0f) index = 2;
            }
            //非常ブレーキはB6扱い
            if (index >= 8) index = 7;

            return -index;
        }

        /// <summary>
        /// TASC 速度制限用出力ノッチ演算メソッド
        /// </summary>
        /// <param name="nowSpeed">現在速度[km/h]</param>
        /// <param name="limitSpeed">制限速度[km/h]</param>
        /// <param name="distance">制限速度までの残り距離[m]</param>
        /// <returns></returns>
        private int CalcTASCLimitSpeedNotch(float nowSpeed, float limitSpeed, float distance)
        {
            float dist = distance;

            //勾配値算出
            float gradientDec = fTASCGradientAverage.IsZero() ? 0.0f : (fTASCGradientAverage / iGradientCoefficient);

            //減速度[km/h/s]に変換した配列を生成
            float[] strCoinstDeceration = constDeceleration[(int)trainModel];
            List<float> notchDist = strCoinstDeceration.Select(i => i * maxDeceleration[(int)trainModel] + gradientDec).ToList();

            //現在速度における制限速度までの減速度算出
            fTASCDeceleration = CalcTASCDeceleration(nowSpeed, limitSpeed, dist);

            //演算減速度に最も近い段数のインデックスを出力
            int index = data.FindClosestIndex(notchDist, fTASCDeceleration);

            //TASC演算速度との差が大きければ出力ノッチを+1
            if ((fTASCLimitPatternSpeed + 2.0f) < nowSpeed)
            {
                index++;
            }

            //ツーハンドル車は別処理
            if (IsTwoHandle)
            {
                //B1以下は出力しない
                if (index == 0) index = 1;
                //制限速度との差が小さい時はB4以下を出力
                if (Math.Abs(limitSpeed - nowSpeed) < 5.0f && index > 4) index = 4;
            }
            else
            {
                //B1以下は出力しない
                if (index == 1 || index == 0) index = 2;
                //制限速度との差が小さい時はB4以下を出力
                if (Math.Abs(limitSpeed - nowSpeed) < 5.0f && index > 5) index = 5;
            }
            //非常ブレーキはB6扱い
            if (index >= 8) index = 7;

            return -index;
        }

        /// <summary>
        /// TASC 停車用出力SAP圧力演算メソッド
        /// </summary>
        /// <param name="nowSpeed">現在速度[km/h]</param>
        /// <param name="distance">停止位置までの残り距離[m]</param>
        /// <returns></returns>
        private float CalcTASCSAP(float nowSpeed, float distance)
        {
            //距離オフセット設定
            float dist = distance;
            if (fTASCStoppingPattern > fTASCStoppingReductionPattern) dist = distance - fTASCDistanceOffset;

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
            //停止時に50kPaへ移行(下り勾配以外)
            if (1.0f > nowSpeed && fPressure > 50.0f && fTASCGradientAverage > -5.0f) fPressure = 50.0f;
            //非常ブレーキは最大圧力扱い
            if (fPressure > strMaxPressure) fPressure = strMaxPressure;

            return fPressure;
        }

        /// <summary>
        /// TASC 速度制限用出力SAP圧力演算メソッド
        /// </summary>
        /// <param name="nowSpeed">現在速度[km/h]</param>
        /// <param name="limitSpeed">制限速度[km/h]</param>
        /// <param name="distance">制限速度までの残り距離[m]</param>
        /// <returns></returns>
        private float CalcTASCLimitSpeedSAP(float nowSpeed, float limitSpeed, float distance)
        {
            float dist = distance;

            //最大減速度[km/h/s]を取得
            float strMaxDeceration = maxDeceleration[(int)trainModel];

            //最大SAP圧力値[kPa]を取得
            float strMaxPressure = maxPressure[(int)trainModel];

            //現在速度における制限速度までの減速度算出
            fTASCDeceleration = CalcTASCDeceleration(nowSpeed, limitSpeed, dist);

            //減速度からSAP圧力値を算出
            float fPressure = (fTASCDeceleration / strMaxDeceration) * strMaxPressure;

            //50kPa以下は出力しない
            if (fPressure <= 50.0f) fPressure = 50.0f;
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
            float dist = distance - fTASCDistanceOffset;
            float dec = deceleration;
            float time = freeRunningTime[(int)trainModel];
            if (dist < 0.0f) dist = 0.0f;
            if (!fTASCGradientAverage.IsZero()) dec += (fTASCGradientAverage / iGradientCoefficient);

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
            if (dist < 0.0f) dist = 0.0f;
            if (!fTASCGradientAverage.IsZero()) dec += (fTASCGradientAverage / iGradientCoefficient);

            //軽減パターン演算
            float v = (-2.0f * dec * time + (float)Math.Sqrt((float)Math.Pow(2.0f * dec * time, 2) - 4 * (-7.2 * dec * dist))) / 2;

            return v;
        }

        /// <summary>
        /// TASC 速度制限パターン演算メソッド
        /// </summary>
        /// <param name="limitSpeed">制限速度[km/h]</param>
        /// <param name="distance">制限速度までの残り距離[m]</param>
        /// <param name="deceleration">パターン減速度[km/h/s]</param>
        /// <returns></returns>
        private float CalcTASCLimitSpeedPattern(float limitSpeed, float distance, float deceleration)
        {
            float dist = distance - fTASCDistanceOffset;
            float dec = deceleration;
            float time = freeRunningTime[(int)trainModel];
            if (dist < 0.0f) dist = 0.0f;
            if (!fTASCGradientAverage.IsZero()) dec += (fTASCGradientAverage / iGradientCoefficient);

            //速度制限パターン演算
            float v = -time * dec + (float)Math.Sqrt((float)Math.Pow(time, 2) * (float)Math.Pow(dec, 2) + (float)Math.Pow(limitSpeed, 2) + 7.2f * dec * dist);
            
            if (v < limitSpeed) v = limitSpeed;

            return v;
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
            float b = 5.0f;
            float dist = distance;
            if (dist < 0.0f) dist = 0.0f;

            //減速度演算
            try
            {
                b = (float)((Math.Pow(nowSpeed, 2) - Math.Pow(targetSpeed, 2)) / (7.2 * dist));
                if (!fTASCGradientAverage.IsZero()) b -= (fTASCGradientAverage / iGradientCoefficient);

                if (nowSpeed.IsZero()) b = 5.0f;
                if (b < 0.0f) b = 5.0f;
                if (b > 5.0f) b = 5.0f;
            }
            catch
            {
                b = 5.0f;
            }
            return b;
        }

        /// <summary>
        /// TASC 指定速度における停止までの距離演算メソッド
        /// </summary>
        /// <param name="nowSpeed">現在速度[km/h]</param>
        /// <param name="deceleration">減速度[km/h/s]</param>
        /// <returns></returns>
        private float CalcTASCStoppingDistance(float nowSpeed, float deceleration)
        {
            if (!fTASCGradientAverage.IsZero()) deceleration += (fTASCGradientAverage / iGradientCoefficient);

            //停止距離演算
            float d = (float)(Math.Pow(nowSpeed, 2) / (7.2 * deceleration));

            if (nowSpeed.IsZero()) d = 0.0f;
            if (d < 0f) d = 0.0f;

            return d;
        }

        /// <summary>
        /// TASC 勾配平均値計算メソッド
        /// </summary>
        /// <param name="_state">列車の状態</param>
        /// <param name="distance">距離[m]</param>
        /// <param name="offset">距離オフセット[m]</param>
        /// <returns></returns>
        private float CalcTASCAverageGradient(TrainState _state, float distance, float offset)
        {
            string direction = data.IsEven(int.Parse(Regex.Replace(_state.diaName, @"[^0-9]", ""))) ? "上り" : "下り";
            float average = 0.0f;
            float dist = distance;
            if (dist < 0.0f) dist = 0.0f;
            try
            {
                var str = GradientList
                    .Where(s => s.Direction == direction)
                    .Where(s => s.StationName == _state.nextStaName)
                    .Where(s => (s.Distance - offset) < (dist + (_state.CarStates.Count() * 20.0f)))
                    .Select(s => s.Gradient);

                //一致したデータがあれば取得
                if (str != null && str.Any()) average = str.Average();
            }
            catch
            {
                return average;
            }
            return average;
        }

        /// <summary>
        /// 現在位置から相対位置までの区間における勾配平均値を計算するメソッド
        /// </summary>
        /// <param name="_state">列車の状態</param>
        /// <param name="distance">現在位置の距離[m]</param>
        /// <param name="targetDistance">目標位置までの相対距離[m]</param>
        /// <param name="offset">距離オフセット[m]</param>
        /// <returns></returns>
        private float CalcAverageGradientToRelativePosition(TrainState _state, float distance, float targetDistance, float offset)
        {
            string direction = data.IsEven(int.Parse(Regex.Replace(_state.diaName, @"[^0-9]", ""))) ? "上り" : "下り";
            float average = 0.0f;
            float startDist = Math.Max(distance, 0.0f);
            float endDist = Math.Max(distance - targetDistance, 0.0f);

            // 開始距離が終了距離を超えている場合、入れ替え
            if (startDist > endDist) (endDist, startDist) = (startDist, endDist);
            try
            {
                var gradientsInRange = GradientList
                    .Where(s => s.Direction == direction)
                    .Where(s => s.StationName == _state.nextStaName)
                    .Where(s => s.Distance - offset >= startDist && s.Distance - offset <= endDist)
                    .Select(s => s.Gradient);

                // 一致したデータがあれば平均を計算
                if (gradientsInRange.Any()) average = gradientsInRange.Average();
            }
            catch
            {
                return average;
            }
            return average;
        }

        /// <summary>
        /// 現在位置から絶対位置までの区間における勾配平均値を計算するメソッド
        /// </summary>
        /// <param name="_state">列車の状態</param>
        /// <param name="distance">現在位置の距離[m]</param>
        /// <param name="targetDistance">目標の絶対距離[m]</param>
        /// <param name="offset">距離オフセット[m]</param>
        /// <returns>指定区間の勾配平均値</returns>
        private float CalcAverageGradientToAbsolutePosition(TrainState _state, float distance, float targetDistance, float offset)
        {
            string direction = data.IsEven(int.Parse(Regex.Replace(_state.diaName, @"[^0-9]", ""))) ? "上り" : "下り";
            float average = 0.0f;
            float startDist = Math.Max(distance, 0.0f);
            float endDist = targetDistance;

            try
            {
                var gradientsInRange = GradientList
                    .Where(s => s.Direction == direction)
                    .Where(s => s.StationName == _state.nextStaName)
                    .Where(s => s.Distance - offset >= endDist && s.Distance - offset <= startDist)
                    .Select(s => s.Gradient);

                // 一致したデータがあれば平均を計算
                if (gradientsInRange.Any()) average = gradientsInRange.Average();
            }
            catch
            {
                return average;
            }
            return average;
        }

        /// <summary>
        /// TASC 制限速度取得メソッド
        /// </summary>
        /// <param name="_state">列車の状態</param>
        /// <param name="distance">距離[m]</param>
        /// <param name="offset">距離オフセット[m]</param>
        /// <param name="limitSpeed">制限速度[km/h]</param>
        /// <param name="limitDistance">制限速度までの距離[m]</param>
        public void GetTASCLimitSpeed(TrainState _state, float distance, float offset, out float limitSpeed, out float limitDistance)
        {
            string direction = data.IsEven(int.Parse(Regex.Replace(_state.diaName, @"[^0-9]", ""))) ? "上り" : "下り";
            int backStaIndex = (_state.nowStaIndex - 1 < 0) ? 0 : _state.nowStaIndex - 1;
            int nowStaIndex = _state.nowStaIndex;
            float strSystemSpeedLimit = (_state.nextSpeedLimit < 0.0f) ? _state.speedLimit : _state.nextSpeedLimit;
            float strSystemSpeedLimitDistance = (_state.nextSpeedLimit < 0.0f) ? 0.0f : _state.nextSpeedLimitDistance;
            float dist = (distance < 0.0f) ? 0.0f : distance;
            float strlimitSpeed = 120.0f;
            float strlimitDistance = 0.0f;
            try
            {
                var str = SpeedLimitList
                    .Where(s => s.Direction == direction)
                    .Where(s => s.BackStopPosName == _state.stationList[backStaIndex].StopPosName || s.NextStopPosName == _state.stationList[nowStaIndex].StopPosName)
                    .Where(s => (s.StartPos - offset) > dist && dist >= (s.EndPos - offset))
                    .FirstOrDefault();

                //一致したデータがあれば取得
                if (str != null)
                {
                    strlimitSpeed = str.Limit;
                    strlimitDistance = (str.EndPos - offset) > 0.0f ? (str.EndPos - offset) : 0.0f;
                }

                //最も低い制限速度を選択
                if (strlimitSpeed < strSystemSpeedLimit)
                {
                    limitSpeed = strlimitSpeed;
                    limitDistance = ((dist - strlimitDistance) > 0.0f) ? (dist - strlimitDistance) : 0.0f;
                }
                else
                {
                    limitSpeed = strSystemSpeedLimit;
                    limitDistance = strSystemSpeedLimitDistance;
                }
            }
            catch
            {
                limitSpeed = strSystemSpeedLimit;
                limitDistance = strSystemSpeedLimitDistance;
            }
        }

        /// <summary>
        /// TASC 追加操作取得メソッド
        /// </summary>
        /// <param name="_state">列車の状態</param>
        /// <param name="distance">距離[m]</param>
        /// <param name="offset">距離オフセット[m]</param>
        /// <returns></returns>
        private string GetTASCAdditionalOperation(TrainState _state, float distance, float offset)
        {
            string direction = data.IsEven(int.Parse(Regex.Replace(_state.diaName, @"[^0-9]", ""))) ? "上り" : "下り";
            float dist = distance;
            string operation = "None";
            if (dist < 0.0f) dist = 0.0f;
            try
            {
                var str = OperationList
                    .Where(s => s.Direction == direction)
                    .Where(s => s.StationName == _state.nextStaName)
                    .Where(s => (s.StartPos - offset) > dist && dist >= (s.EndPos - offset))
                    .Select(s => s.AdditionalOperation);

                //一致したデータがあれば取得
                if (str != null && str.Any()) operation = str.FirstOrDefault();
            }
            catch
            {
                return operation;
            }
            return operation;
        }

        /// <summary>
        /// ATO 最高速度取得メソッド
        /// </summary>
        /// <param name="_state">列車の状態</param>
        /// <param name="distance">距離[m]</param>
        /// <param name="offset">距離オフセット[m]</param>
        /// <returns></returns>
        private float GetATOMaxSpeed(TrainState _state, float distance, float offset)
        {
            string direction = data.IsEven(int.Parse(Regex.Replace(_state.diaName, @"[^0-9]", ""))) ? "上り" : "下り";
            float fATOPatternOffset = (sATOPatternMode == "遅速") ? -10.0f : 0.0f;
            float speedLimit = (_state.nextSpeedLimit < 0.0f) ? _state.speedLimit : _state.nextSpeedLimit;
            float maxSpeed = speedLimit;
            string trainClass = _state.Class;
            float dist = distance;
            if (dist < 0.0f) dist = 0.0f;
            try
            {
                var str = MaxSpeedList
                    .Where(s => s.StationName == _state.nextStaName)
                    .Where(s => s.Class.Contains(trainClass) || s.Class.Contains("その他"))
                    .Where(s => (s.StartPos - offset) > dist && dist >= (s.EndPos - offset))
                    .Select(s => s.MaxSpeed);

                //一致したデータがあれば取得
                if (str != null && str.Any()) maxSpeed = str.FirstOrDefault();

                //ATOモード判定
                if (sATOPatternMode == "回復")
                    maxSpeed = 110.0f;
                else
                    maxSpeed = maxSpeed + fATOPatternOffset;

                //制限速度と最高速度の低い方を選択
                if (speedLimit < maxSpeed) maxSpeed = speedLimit;
            }
            catch
            {
                return maxSpeed;
            }
            return maxSpeed;
        }

        /// <summary>
        /// 遅延実行呼び出しメソッド
        /// </summary>
        /// <param name="seconds">遅延秒数[s]</param>
        /// <param name="action">呼び出し処理</param>
        /// <returns></returns>
        private async Task WaitForAsync(float seconds, Action action)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            action();
        }

        /// <summary>
        /// ATO動作フェーズを呼び出し元へ戻す (遅延実行)
        /// </summary>
        private void ReturnATOPhase()
        {
            sATOPhase = strATOPhase;
            strATOPhase = "";
        }

        /// <summary>
        /// 停止位置オフセット取得メソッド
        /// </summary>
        /// <param name="_state">列車の状態</param>
        /// <returns></returns>
        private float GetStopPositionOffset(TrainState _state)
        {
            string direction = data.IsEven(int.Parse(Regex.Replace(_state.diaName, @"[^0-9]", ""))) ? "上り" : "下り";
            float offset = 0.0f;
            try
            {
                var str = StopPositionOffsetList
                    .Where(s => s.Direction == direction)
                    .Where(s => s.StationName == _state.nextStaName)
                    .Select(s => s.Offset[_state.CarStates.Count - 1]);

                //一致したデータがあれば取得
                if (str != null && str.Any() && _state.nextStopType.Contains("停車"))
                    offset = str.FirstOrDefault();
            }
            catch
            {
                return offset;
            }
            return offset;
        }
    }
}

/// <summary>
/// 勾配情報クラス
/// </summary>
public class GradientClass
{
    /// <summary>
    /// 上下
    /// </summary>
    public string Direction { get; set; }

    /// <summary>
    /// 駅名
    /// </summary>
    public string StationName { get; set; }

    /// <summary>
    /// 残り距離(m)
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// 勾配値(‰)
    /// </summary>
    public float Gradient { get; set; }
}

/// <summary>
/// 最高速度情報クラス
/// </summary>
public class MaxSpeedClass
{
    /// <summary>
    /// 上下
    /// </summary>
    public string Direction { get; set; }

    /// <summary>
    /// 駅名
    /// </summary>
    public string StationName { get; set; }

    /// <summary>
    /// 種別名
    /// </summary>
    public string Class { get; set; }

    /// <summary>
    /// 開始位置(m)
    /// </summary>
    public float StartPos { get; set; }

    /// <summary>
    /// 終了位置(m)
    /// </summary>
    public float EndPos { get; set; }

    /// <summary>
    /// 最高速度(km/h)
    /// </summary>
    public float MaxSpeed { get; set; }
}

/// <summary>
/// 速度制限クラス
/// </summary>
public class SpeedLimitClass
{
    /// <summary>
    /// 上下
    /// </summary>
    public string Direction { get; set; }

    /// <summary>
    /// 開始位置(m)
    /// </summary>
    public float StartPos { get; set; }

    /// <summary>
    /// 終了位置(m)
    /// </summary>
    public float EndPos { get; set; }

    /// <summary>
    /// 制限速度(km/h)
    /// </summary>
    public float Limit { get; set; }

    /// <summary>
    /// 前の停止位置名
    /// </summary>
    public string BackStopPosName { get; set; }

    /// <summary>
    /// 次の停止位置名
    /// </summary>
    public string NextStopPosName { get; set; }
}

/// <summary>
/// 追加操作クラス
/// </summary>
public class AdditionalOperationClass
{
    /// <summary>
    /// 上下
    /// </summary>
    public string Direction { get; set; }

    /// <summary>
    /// 駅名
    /// </summary>
    public string StationName { get; set; }

    /// <summary>
    /// 開始位置(m)
    /// </summary>
    public float StartPos { get; set; }

    /// <summary>
    /// 終了位置(m)
    /// </summary>
    public float EndPos { get; set; }

    /// <summary>
    /// 操作内容
    /// </summary>
    public string AdditionalOperation { get; set; }
}

/// <summary>
/// 停止位置オフセットクラス
/// </summary>
public class StopPositionOffsetClass
{
    /// <summary>
    /// 上下
    /// </summary>
    public string Direction { get; set; }

    /// <summary>
    /// 駅名
    /// </summary>
    public string StationName { get; set; }

    /// <summary>
    /// オフセット[m]
    /// </summary>
    public List<float> Offset { get; set; }
}
