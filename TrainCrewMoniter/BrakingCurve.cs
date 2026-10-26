using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TrainCrewMoniter
{
    /// <summary>
    /// ブレーキ曲線データクラス
    /// </summary>
    /// <remarks>
    /// Bin\(駅名).bin から読み込んだ、勾配地形情報を織り込み済みのブレーキ曲線。
    /// ファイルの構造は MessagePack で以下の通り。
    ///   ルート    map  : キーは方向(0=下り, 1=上り)
    ///   第2階層   map  : キーは車両形式(TASC.TrainModel の値)
    ///   第3階層   map  : キーはブレーキノッチ(TrainState.Bnotch の値)
    ///   第4階層   array: [停止位置までの距離[m], 速度[km/h]] の組
    /// 距離は等間隔なので、速度だけを配列で保持して距離から添字を直接計算する。
    /// </remarks>
    public sealed class BrakingCurveSet
    {
        /// <summary>
        /// ブレーキノッチの最大数
        /// </summary>
        public const int NotchCount = 9;

        /// <summary>
        /// 駅名
        /// </summary>
        public string StationName { get; }

        /// <summary>
        /// 方向(0=下り, 1=上り)
        /// </summary>
        public int DirectionKey { get; }

        /// <summary>
        /// 車両形式
        /// </summary>
        public int TrainModelIndex { get; }

        /// <summary>
        /// ノッチ別の速度[km/h](距離の昇順、該当データが無いノッチはnull)
        /// </summary>
        private readonly float[][] speeds;

        /// <summary>
        /// 距離の下限[m]
        /// </summary>
        private readonly float minDistance;

        /// <summary>
        /// 距離の上限[m]
        /// </summary>
        private readonly float maxDistance;

        /// <summary>
        /// 距離の刻み幅[m]
        /// </summary>
        private readonly float step;

        private BrakingCurveSet(string stationName, int directionKey, int trainModelIndex,
                               float[][] speeds, float minDistance, float maxDistance, float step)
        {
            StationName = stationName;
            DirectionKey = directionKey;
            TrainModelIndex = trainModelIndex;
            this.speeds = speeds;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
            this.step = step;
        }

        /// <summary>
        /// 指定した条件のデータかどうかを判定するメソッド
        /// </summary>
        public bool Matches(string stationName, int directionKey, int trainModelIndex)
        {
            return DirectionKey == directionKey
                && TrainModelIndex == trainModelIndex
                && string.Equals(StationName, stationName, StringComparison.Ordinal);
        }

        /// <summary>
        /// 指定ノッチのデータ有無判定メソッド
        /// </summary>
        /// <param name="bnotch">ブレーキノッチ(TrainState.Bnotch と同じ値)</param>
        public bool HasNotch(int bnotch)
        {
            return bnotch >= 0 && bnotch < NotchCount && speeds[bnotch] != null && speeds[bnotch].Length > 0;
        }

        /// <summary>
        /// 速度取得メソッド
        /// </summary>
        /// <param name="bnotch">ブレーキノッチ(TrainState.Bnotch と同じ値)</param>
        /// <param name="distance">停止位置までの距離[m]</param>
        /// <param name="speed">その地点の速度[km/h]</param>
        /// <returns>該当するデータがあればtrue</returns>
        /// <remarks>
        /// 下限より手前の距離は下限の値に丸める。
        /// 上限を超える距離は制御に使うと危険なため、該当データ無しとして扱う。
        /// </remarks>
        public bool TryGetSpeed(int bnotch, float distance, out float speed)
        {
            speed = 0.0f;
            if (!HasNotch(bnotch)) return false;
            if (distance > maxDistance) return false;

            float[] values = speeds[bnotch];

            float clamped = distance;
            if (clamped < minDistance) clamped = minDistance;

            int index = (int)Math.Round((clamped - minDistance) / step);
            if (index < 0) index = 0;
            else if (index >= values.Length) index = values.Length - 1;

            speed = values[index];
            return true;
        }

        /// <summary>
        /// 読み込みメソッド
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="stationName">駅名</param>
        /// <param name="directionKey">方向(0=下り, 1=上り)</param>
        /// <param name="trainModelIndex">車両形式</param>
        /// <returns>読み込んだ曲線(該当データが無ければnull)</returns>
        /// <remarks>
        /// ファイルは30MB近くあるが、必要なのは1方向×1形式の分だけ(全体の約4%)なので、
        /// 該当しない方向・形式はデコードせず読み飛ばす。
        /// </remarks>
        public static BrakingCurveSet Load(string filePath, string stationName, int directionKey, int trainModelIndex)
        {
            var reader = new MessagePackReader(File.ReadAllBytes(filePath));

            int directionCount = reader.ReadMapHeader();
            for (var d = 0; d < directionCount; d++)
            {
                if (reader.ReadInt() != directionKey)
                {
                    reader.Skip();
                    continue;
                }

                int modelCount = reader.ReadMapHeader();
                for (var m = 0; m < modelCount; m++)
                {
                    if (reader.ReadInt() != trainModelIndex)
                    {
                        reader.Skip();
                        continue;
                    }

                    return ReadNotches(reader, stationName, directionKey, trainModelIndex);
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// ノッチ別曲線の読み込みメソッド
        /// </summary>
        private static BrakingCurveSet ReadNotches(MessagePackReader reader, string stationName, int directionKey, int trainModelIndex)
        {
            var speeds = new float[NotchCount][];
            float minDistance = 0.0f;
            float maxDistance = 0.0f;
            float step = 0.0f;

            int notchCount = reader.ReadMapHeader();
            for (var n = 0; n < notchCount; n++)
            {
                int notch = reader.ReadInt();
                int pointCount = reader.ReadArrayHeader();

                var values = new float[pointCount];
                float firstDistance = 0.0f;
                float lastDistance = 0.0f;
                bool hasSpeed = false;

                for (var i = 0; i < pointCount; i++)
                {
                    int fieldCount = reader.ReadArrayHeader();
                    float distance = reader.ReadSingle();
                    float speed = reader.ReadSingle();
                    for (var f = 2; f < fieldCount; f++) reader.Skip();

                    if (i == 0) firstDistance = distance;
                    lastDistance = distance;
                    values[i] = speed;
                    if (speed > 0.0f) hasSpeed = true;
                }

                // 距離の昇順に揃える
                if (firstDistance > lastDistance) Array.Reverse(values);

                // 全区間で0のノッチはその車両で使われていないため保持しない
                if (!hasSpeed || pointCount < 2 || notch < 0 || notch >= NotchCount) continue;

                speeds[notch] = values;
                minDistance = Math.Min(firstDistance, lastDistance);
                maxDistance = Math.Max(firstDistance, lastDistance);
                step = (maxDistance - minDistance) / (pointCount - 1);
            }

            return (step > 0.0f)
                ? new BrakingCurveSet(stationName, directionKey, trainModelIndex, speeds, minDistance, maxDistance, step)
                : null;
        }
    }

    /// <summary>
    /// ブレーキ曲線データ管理クラス
    /// </summary>
    /// <remarks>
    /// 駅・方向・車両形式が変わったときだけ、バックグラウンドで読み込む。
    /// ファイルが30MB近くあるためUIスレッドでは読み込まない。
    /// </remarks>
    public sealed class BrakingCurveRepository
    {
        /// <summary>
        /// ブレーキ曲線データの格納フォルダ
        /// </summary>
        private const string DirectoryName = "Bin";

        private readonly object sync = new object();

        /// <summary>
        /// 読み込みを試みた条件(成否に関わらず記録し、同じ条件で読み直さない)
        /// </summary>
        private string attemptedStationName;
        private int attemptedDirectionKey = -1;
        private int attemptedTrainModelIndex = -1;

        /// <summary>
        /// 読み込み済みの曲線
        /// </summary>
        private BrakingCurveSet current;

        /// <summary>
        /// 読み込み済みの曲線
        /// </summary>
        public BrakingCurveSet Current => Volatile.Read(ref current);

        /// <summary>
        /// ブレーキ曲線データのファイルパス解決メソッド
        /// </summary>
        /// <param name="stationName">駅名</param>
        /// <returns>存在するファイルのパス(見つからなければnull)</returns>
        /// <remarks>
        /// カレントディレクトリは起動方法によって変わるため、まずexeの位置を基準に探す。
        /// </remarks>
        private static string ResolveFilePath(string stationName)
        {
            string fileName = stationName + ".bin";

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DirectoryName, fileName);
            if (File.Exists(path)) return path;

            path = Path.Combine(DirectoryName, fileName);
            return File.Exists(path) ? path : null;
        }

        /// <summary>
        /// 曲線の読み込み要求メソッド
        /// </summary>
        /// <param name="stationName">駅名</param>
        /// <param name="directionKey">方向(0=下り, 1=上り)</param>
        /// <param name="trainModelIndex">車両形式</param>
        public void Request(string stationName, int directionKey, int trainModelIndex)
        {
            if (string.IsNullOrEmpty(stationName)) return;

            lock (sync)
            {
                if (attemptedDirectionKey == directionKey
                    && attemptedTrainModelIndex == trainModelIndex
                    && string.Equals(attemptedStationName, stationName, StringComparison.Ordinal))
                {
                    return;
                }
                attemptedStationName = stationName;
                attemptedDirectionKey = directionKey;
                attemptedTrainModelIndex = trainModelIndex;
            }

            Task.Run(() =>
            {
                try
                {
                    string path = ResolveFilePath(stationName);
                    if (path == null) return;

                    var set = BrakingCurveSet.Load(path, stationName, directionKey, trainModelIndex);
                    if (set == null) return;

                    Volatile.Write(ref current, set);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex}");
                }
            });
        }
    }
}
