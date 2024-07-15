using System;
using System.Drawing;
using System.Windows.Forms;
using TrainCrew;

namespace TrainCrewMoniter
{
    /// <summary>
    /// MainFormクラス
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly Timer timer;
        private readonly Timer interval500Timer;
        private readonly Data data = new Data();
        private readonly TASC tasc = new TASC();
        private bool IsInterval = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            FormClosing += MainForm_FormClosing;
            this.KeyPreview = true;

            //Timer設定
            timer = InitializeTimer(50, Timer_Tick);
            interval500Timer = InitializeTimer(500, Interval500Timer_Tick);

            //初期化。起動時のみの呼び出しで大丈夫です。
            TrainCrewInput.Init();
        }

        /// <summary>
        /// Timer初期化メソッド
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="tickEvent"></param>
        /// <returns></returns>
        private Timer InitializeTimer(int interval, EventHandler tickEvent)
        {
            var timer = new Timer
            {
                Interval = interval
            };
            timer.Tick += tickEvent;
            timer.Start();
            return timer;
        }

        /// <summary>
        /// LabelCarDoor初期化メソッド
        /// </summary>
        private void InitializeLabelCarDoor()
        {
            label_Car1_Door.BackColor = Color.White;
            label_Car1_Door.ForeColor = Color.LightGray;
            label_Car2_Door.BackColor = Color.White;
            label_Car2_Door.ForeColor = Color.LightGray;
            label_Car3_Door.BackColor = Color.White;
            label_Car3_Door.ForeColor = Color.LightGray;
            label_Car4_Door.BackColor = Color.White;
            label_Car4_Door.ForeColor = Color.LightGray;
            label_Car5_Door.BackColor = Color.White;
            label_Car5_Door.ForeColor = Color.LightGray;
            label_Car6_Door.BackColor = Color.White;
            label_Car6_Door.ForeColor = Color.LightGray;
        }
        /// <summary>
        /// LabelCarBCPress初期化メソッド
        /// </summary>
        private void InitializeLabelCarBCPress()
        {
            label_Car1_BCPress.Text = "0.00kPa";
            label_Car2_BCPress.Text = "0.00kPa";
            label_Car3_BCPress.Text = "0.00kPa";
            label_Car4_BCPress.Text = "0.00kPa";
            label_Car5_BCPress.Text = "0.00kPa";
            label_Car6_BCPress.Text = "0.00kPa";
        }
        /// <summary>
        /// LabelCarAmpere初期化メソッド
        /// </summary>
        private void InitializeLabelCarAmpere()
        {
            label_Car1_Ampere.Text = "0.00A";
            label_Car2_Ampere.Text = "0.00A";
            label_Car3_Ampere.Text = "0.00A";
            label_Car4_Ampere.Text = "0.00A";
            label_Car5_Ampere.Text = "0.00A";
            label_Car6_Ampere.Text = "0.00A";
            label_Car1_Ampere.ForeColor = Color.Black;
            label_Car2_Ampere.ForeColor = Color.Black;
            label_Car3_Ampere.ForeColor = Color.Black;
            label_Car4_Ampere.ForeColor = Color.Black;
            label_Car5_Ampere.ForeColor = Color.Black;
            label_Car6_Ampere.ForeColor = Color.Black;
        }

        /// <summary>
        /// Timer_Tickイベント (Interval:50ms)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            var state = TrainCrewInput.GetTrainState();
            TrainCrewInput.RequestStaData();
            if (state == null || state.CarStates.Count == 0 || state.stationList.Count == 0) { return; }

            //運転画面遷移なら処理
            if (TrainCrewInput.gameState.gameScreen == GameScreen.MainGame
                || TrainCrewInput.gameState.gameScreen == GameScreen.MainGame_Pause
                || TrainCrewInput.gameState.gameScreen == GameScreen.MainGame_Loading)
            {
                SuspendLayout();

                //TASC演算
                tasc.IsTASCEnable = check_TASCEnable.Checked;
                tasc.TASC_Update(state);

                //列車番号
                label_CarInfo_DiaName.Text = state.diaName;
                //種別
                label_CarInfo_Class.Text = state.Class;
                //行先
                label_CarInfo_BoundFor.Text = state.BoundFor;
                //両数
                label_CarInfo_CarLength.Text = state.CarStates.Count.ToString() + "両";
                //速度
                label_CarInfo_Speed.Text = state.Speed.ToString("F2") + "km/h";
                //MR圧
                label_CarInfo_MRPress.Text = state.MR_Press.ToString("F2") + "kPa";

                //表示灯[●戸閉]
                if (state.Lamps[PanelLamp.DoorClose])
                {
                    label_Lamp_Door.BackColor = Color.Green;
                    label_Lamp_Door.ForeColor = Color.White;
                }
                else
                {
                    label_Lamp_Door.BackColor = Color.White;
                    label_Lamp_Door.ForeColor = Color.LightGray;
                }
                //表示灯[ATS正常]
                if (state.Lamps[PanelLamp.ATS_Ready])
                {
                    label_Lamp_ATSNormal.BackColor = Color.Green;
                    label_Lamp_ATSNormal.ForeColor = Color.White;
                }
                else
                {
                    label_Lamp_ATSNormal.BackColor = Color.White;
                    label_Lamp_ATSNormal.ForeColor = Color.LightGray;
                }
                //表示灯[ATS動作]
                if (state.Lamps[PanelLamp.ATS_BrakeApply])
                {
                    label_Lamp_ATSOperation.BackColor = Color.Red;
                    label_Lamp_ATSOperation.ForeColor = Color.White;
                }
                else
                {
                    label_Lamp_ATSOperation.BackColor = Color.White;
                    label_Lamp_ATSOperation.ForeColor = Color.LightGray;
                }
                //表示灯[ATS開放]
                if (state.Lamps[PanelLamp.ATS_Open])
                {
                    label_Lamp_ATSRelease.BackColor = Color.Orange;
                    label_Lamp_ATSRelease.ForeColor = Color.Black;
                }
                else
                {
                    label_Lamp_ATSRelease.BackColor = Color.White;
                    label_Lamp_ATSRelease.ForeColor = Color.LightGray;
                }
                //表示灯[TASC動作]
                if (tasc.IsTASCOperation)
                {
                    label_Lamp_TASCOperation.BackColor = Color.Orange;
                    label_Lamp_TASCOperation.ForeColor = Color.Black;
                }
                else
                {
                    label_Lamp_TASCOperation.BackColor= Color.White;
                    label_Lamp_TASCOperation.ForeColor = Color.LightGray;
                }
                //表示灯[TASCブレーキ]
                if (tasc.IsTASCBraking)
                {
                    label_Lamp_TASCBrake.BackColor = Color.Orange;
                    label_Lamp_TASCBrake.ForeColor = Color.Black;
                }
                else
                {
                    label_Lamp_TASCBrake.BackColor= Color.White;
                    label_Lamp_TASCBrake.ForeColor = Color.LightGray;
                }
                //表示灯[回生]
                if (state.Lamps[PanelLamp.RegenerativeBrake])
                {
                    label_Lamp_Regeneration.BackColor = Color.Orange;
                    label_Lamp_Regeneration.ForeColor = Color.Black;
                }
                else
                {
                    label_Lamp_Regeneration.BackColor= Color.White;
                    label_Lamp_Regeneration.ForeColor = Color.LightGray;
                }
                //表示灯[EB]
                if (state.Lamps[PanelLamp.EB_Timer])
                {
                    label_Lamp_EB.BackColor = Color.Red;
                    label_Lamp_EB.ForeColor = Color.White;
                }
                else
                {
                    label_Lamp_EB.BackColor= Color.White;
                    label_Lamp_EB.ForeColor = Color.LightGray;
                }
                //表示灯[非常ブレーキ]
                if (state.Lamps[PanelLamp.EmagencyBrake])
                {
                    label_Lamp_EmergencyBrake.BackColor = Color.Red;
                    label_Lamp_EmergencyBrake.ForeColor = Color.White;
                }
                else
                {
                    label_Lamp_EmergencyBrake.BackColor= Color.White;
                    label_Lamp_EmergencyBrake.ForeColor = Color.LightGray;
                }
                //表示灯[過負荷]
                if (state.Lamps[PanelLamp.Overload])
                {
                    label_Lamp_Overload.BackColor = Color.Red;
                    label_Lamp_Overload.ForeColor = Color.White;
                }
                else
                {
                    label_Lamp_Overload.BackColor= Color.White;
                    label_Lamp_Overload.ForeColor = Color.LightGray;
                }

                //ATS表示灯[種別]
                label_ATSLamp_Class.Text = state.ATS_Class.ToString();
                //ATS表示灯[制限]
                if (state.ATS_Speed == "112")
                    label_ATSLamp_Speed.Text = "110";
                else if (state.ATS_Speed == "300")
                    label_ATSLamp_Speed.Text = "F";
                else
                    label_ATSLamp_Speed.Text = state.ATS_Speed.ToString();
                //ATS表示灯[状態]
                label_ATSLamp_State.Text = state.ATS_State.ToString();

                //車両[●戸閉]
                InitializeLabelCarDoor();
                for (int i = 0; i < state.CarStates.Count; i++)
                {
                    Control[] cDoor = this.Controls.Find($"label_Car{i + 1}_Door", true);
                    if (state.CarStates[i].DoorClose)
                    {
                        cDoor[0].BackColor = Color.Green;
                        cDoor[0].ForeColor = Color.White;
                    }
                    else
                    {
                        cDoor[0].BackColor = Color.White;
                        cDoor[0].ForeColor = Color.LightGray;
                    }
                }
                //車両[BC圧力]
                InitializeLabelCarBCPress();
                for (int i = 0; i < state.CarStates.Count; i++)
                {
                    Control[] cBCPress = this.Controls.Find($"label_Car{i + 1}_BCPress", true);
                    cBCPress[0].Text = state.CarStates[i].BC_Press.ToString("F2") + "KPa";
                }
                //車両[電流]
                InitializeLabelCarAmpere();
                for (int i = 0; i < state.CarStates.Count; i++)
                {
                    Control[] cAmpere = this.Controls.Find($"label_Car{i + 1}_Ampere", true);
                    cAmpere[0].Text = state.CarStates[i].Ampare.ToString("F2") + "A";
                    if (state.CarStates[i].Ampare < 0.0f)
                        cAmpere[0].ForeColor = Color.Red;
                    else
                        cAmpere[0].ForeColor = Color.Black;
                }

                //TASC情報[TASC 状態]
                label_TASC_State.Text = tasc.sTASCPhase;
                //TASC情報[TASC 停車P]
                if (state.Speed < 2.5f)
                    label_TASC_Speed.Text = "0.00km/h";
                else
                    label_TASC_Speed.Text = tasc.fTASCPatternSpeed.ToString("F2") + "km/h";
                //TASC情報[TASC 制限P]
                label_TASC_LimitSpeed.Text = tasc.fTASCLimitPatternSpeed.ToString("F2") + "km/h";
                //TASC情報[TASC 減速度]
                if (state.Speed < 2.5f)
                    label_TASC_Deceleration.Text = "0.00km/h/s";
                else
                    label_TASC_Deceleration.Text = tasc.fTASCDeceleration.ToString("F2") + "km/h/s";
                //TASC情報[TASC SAP圧]
                label_TASC_SAPPressure.Text = tasc.fTASCSAPPressure.ToString("F2") + "kPa";
                //TASC情報[TASC B段数]
                if (tasc.IsTwoHandle)
                    label_TASC_Notch.Text = "B" + (tasc.iTASCNotch == 0 ? 0 : -tasc.iTASCNotch);
                else
                    label_TASC_Notch.Text = "B" + (tasc.iTASCNotch == 0 ? 0 : -(tasc.iTASCNotch + 1));
                //TASC情報[TASC 勾配値]
                label_TASC_Gradient.Text = tasc.gradientAverage.ToString("F2") + "‰";
                //TASC情報[TASC 開始距離]
                label_TASC_Distance.Text = tasc.standbyBreakingDistance.ToString("F2") + "m";

                //TASCノッチ出力処理
                if (tasc.IsSMEEBrake)
                {
                    if (!tasc.IsTASCEnable && tasc.fTASCSAPPressure > 0.0f)
                    {
                        //SAP圧初期化
                        tasc.fTASCSAPPressure = 0.0f;
                        TrainCrewInput.SetBrakeSAP(tasc.fTASCSAPPressure);
                    }
                    else if (tasc.IsTASCEnable && tasc.IsSAPReset)
                    {
                        //SAP圧初期化
                        tasc.IsSAPReset = false;
                        tasc.fTASCSAPPressure = 0.0f;
                        TrainCrewInput.SetBrakeSAP(tasc.fTASCSAPPressure);
                    }
                    else if (tasc.IsTASCEnable && tasc.fTASCSAPPressure > 0.0f)
                    {
                        //SAP圧出力
                        TrainCrewInput.SetBrakeSAP(tasc.fTASCSAPPressure);
                    }
                }
                else
                {
                    if (!tasc.IsTASCEnable && tasc.iTASCNotch < 0)
                    {
                        //ATOノッチ初期化
                        tasc.iTASCNotch = 0;
                        TrainCrewInput.SetATO_Notch(tasc.iTASCNotch);
                    }
                    else
                    {
                        //ATOノッチ出力
                        TrainCrewInput.SetATO_Notch(tasc.iTASCNotch);
                    }
                }

                //EB装置を周期的にリセット処理
                if (tasc.IsTASCEnable)
                {
                    TrainCrewInput.SetButton(InputAction.EBReset, IsInterval);
                }

                ResumeLayout();
            }
            else
            {
                SuspendLayout();

                //列車番号
                label_CarInfo_DiaName.Text = "0000";
                //種別
                label_CarInfo_Class.Text = "普通";
                //行先
                label_CarInfo_BoundFor.Text = "館浜";
                //両数
                label_CarInfo_CarLength.Text = "4両";
                //速度
                label_CarInfo_Speed.Text = "0.00km/h";
                //MR圧
                label_CarInfo_MRPress.Text = "0.00kPa";

                //表示灯[●戸閉]
                label_Lamp_Door.BackColor = Color.White;
                label_Lamp_Door.ForeColor = Color.LightGray;
                //表示灯[ATS正常]
                label_Lamp_ATSNormal.BackColor = Color.White;
                label_Lamp_ATSNormal.ForeColor = Color.LightGray;
                //表示灯[ATS動作]
                label_Lamp_ATSOperation.BackColor = Color.White;
                label_Lamp_ATSOperation.ForeColor = Color.LightGray;
                //表示灯[ATS開放]
                label_Lamp_ATSRelease.BackColor = Color.White;
                label_Lamp_ATSRelease.ForeColor = Color.LightGray;
                //表示灯[TASC動作]
                label_Lamp_TASCOperation.BackColor = Color.White;
                label_Lamp_TASCOperation.ForeColor = Color.LightGray;
                //表示灯[TASCブレーキ]
                label_Lamp_TASCBrake.BackColor = Color.White;
                label_Lamp_TASCBrake.ForeColor = Color.LightGray;
                //表示灯[回生]
                label_Lamp_Regeneration.BackColor = Color.White;
                label_Lamp_Regeneration.ForeColor = Color.LightGray;
                //表示灯[EB]
                label_Lamp_EB.BackColor = Color.White;
                label_Lamp_EB.ForeColor = Color.LightGray;
                //表示灯[非常ブレーキ]
                label_Lamp_EmergencyBrake.BackColor= Color.White;
                label_Lamp_EmergencyBrake.ForeColor = Color.LightGray;
                //表示灯[過負荷]
                label_Lamp_Overload.BackColor = Color.White;
                label_Lamp_Overload.ForeColor = Color.LightGray;

                //ATS表示灯[種別]
                label_ATSLamp_Class.Text = "普通";
                //ATS表示灯[制限]
                label_ATSLamp_Speed.Text = "110";
                //ATS表示灯[状態]
                label_ATSLamp_State.Text = "無表示";

                //車両[●戸閉]
                InitializeLabelCarDoor();
                //車両[BC圧力]
                InitializeLabelCarBCPress();
                //車両[電流]
                InitializeLabelCarAmpere();

                //TASC情報[TASC 状態]
                label_TASC_State.Text = "制動待機";
                //TASC情報[TASC 停車P]
                label_TASC_Speed.Text = "0.00km/h";
                //TASC情報[TASC 制限P]
                label_TASC_LimitSpeed.Text = "0.00km/h";
                //TASC情報[TASC 減速度]
                label_TASC_Deceleration.Text = "0.00km/h/s";
                //TASC情報[TASC SAP圧]
                label_TASC_SAPPressure.Text = "0.00kPa";
                //TASC情報[TASC B段数]
                label_TASC_Notch.Text = "B0";
                //TASC情報[TASC 勾配値]
                label_TASC_Gradient.Text = "0.00‰";
                //TASC情報[TASC 開始距離]
                label_TASC_Distance.Text = "0.00m";
                //TASC変数
                tasc.fTASCPatternSpeed = 120.0f;
                tasc.fTASCLimitPatternSpeed = 120.0f;
                tasc.fTASCDeceleration = 0.0f;
                tasc.iTASCNotch = 0;
                tasc.trainModel = TASC.TrainModel.None;

                ResumeLayout();
            }
        }

        /// <summary>
        /// Interval500Timer_Tickイベント (Interval:500ms)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Interval500Timer_Tick(object sender, EventArgs e)
        {
            IsInterval = !IsInterval;
        }

        /// <summary>
        /// MainForm_KeyDownイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                        Application.Exit();
                    break;
            }
        }

        /// <summary>
        /// MainForm_FormClosingイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            TrainCrewInput.Dispose();
        }

        /// <summary>
        /// check_TopMost_CheckedChangedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Check_TopMost_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = check_TopMost.Checked;
        }
    }
}
