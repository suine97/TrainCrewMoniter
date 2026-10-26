using System;

namespace TrainCrewMoniter
{
    /// <summary>
    /// MessagePack読み込みクラス
    /// </summary>
    /// <remarks>
    /// ブレーキ曲線データの読み込みに必要な範囲だけを実装した簡易リーダー。
    /// 不要な要素を読み飛ばせるように Skip を備える。
    /// </remarks>
    internal sealed class MessagePackReader
    {
        private readonly byte[] buffer;
        private int position;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="buffer">読み込み対象</param>
        public MessagePackReader(byte[] buffer)
        {
            this.buffer = buffer;
            position = 0;
        }

        /// <summary>
        /// マップの要素数読み込みメソッド
        /// </summary>
        public int ReadMapHeader()
        {
            byte code = ReadByte();
            if (code >= 0x80 && code <= 0x8f) return code & 0x0f;
            if (code == 0xde) return ReadUInt16();
            if (code == 0xdf) return checked((int)ReadUInt32());
            throw new FormatException($"map ではありません (0x{code:x2} at {position - 1})");
        }

        /// <summary>
        /// 配列の要素数読み込みメソッド
        /// </summary>
        public int ReadArrayHeader()
        {
            byte code = ReadByte();
            if (code >= 0x90 && code <= 0x9f) return code & 0x0f;
            if (code == 0xdc) return ReadUInt16();
            if (code == 0xdd) return checked((int)ReadUInt32());
            throw new FormatException($"array ではありません (0x{code:x2} at {position - 1})");
        }

        /// <summary>
        /// 整数読み込みメソッド
        /// </summary>
        public int ReadInt()
        {
            byte code = ReadByte();
            if (code <= 0x7f) return code;
            if (code >= 0xe0) return unchecked((sbyte)code);
            switch (code)
            {
                case 0xcc: return ReadByte();
                case 0xcd: return ReadUInt16();
                case 0xce: return checked((int)ReadUInt32());
                case 0xd0: return unchecked((sbyte)ReadByte());
                case 0xd1: return unchecked((short)ReadUInt16());
                case 0xd2: return unchecked((int)ReadUInt32());
            }
            throw new FormatException($"整数ではありません (0x{code:x2} at {position - 1})");
        }

        /// <summary>
        /// 浮動小数点数読み込みメソッド
        /// </summary>
        public float ReadSingle()
        {
            byte code = ReadByte();
            //.NET Framework には BitConverter.Int32BitsToSingle が無いため、バイト列経由で変換する
            if (code == 0xca) return BitConverter.ToSingle(BitConverter.GetBytes(unchecked((int)ReadUInt32())), 0);
            if (code == 0xcb) return (float)BitConverter.Int64BitsToDouble(unchecked((long)ReadUInt64()));

            //整数で格納されている場合にも対応する
            position--;
            return ReadInt();
        }

        /// <summary>
        /// 読み飛ばしメソッド
        /// </summary>
        public void Skip()
        {
            byte code = ReadByte();

            //positive fixint / negative fixint
            if (code <= 0x7f || code >= 0xe0) return;
            //fixmap
            if (code >= 0x80 && code <= 0x8f) { SkipElements((code & 0x0f) * 2); return; }
            //fixarray
            if (code >= 0x90 && code <= 0x9f) { SkipElements(code & 0x0f); return; }
            //fixstr
            if (code >= 0xa0 && code <= 0xbf) { position += code & 0x1f; return; }

            switch (code)
            {
                case 0xc0: case 0xc2: case 0xc3: return;             //nil, false, true
                case 0xc4: position += ReadByte(); return;            //bin8
                case 0xc5: position += ReadUInt16(); return;          //bin16
                case 0xc6: position += checked((int)ReadUInt32()); return; //bin32
                case 0xc7: position += ReadByte() + 1; return;        //ext8
                case 0xc8: position += ReadUInt16() + 1; return;      //ext16
                case 0xc9: position += checked((int)ReadUInt32()) + 1; return; //ext32
                case 0xca: position += 4; return;                     //float32
                case 0xcb: position += 8; return;                     //float64
                case 0xcc: case 0xd0: position += 1; return;          //uint8, int8
                case 0xcd: case 0xd1: position += 2; return;          //uint16, int16
                case 0xce: case 0xd2: position += 4; return;          //uint32, int32
                case 0xcf: case 0xd3: position += 8; return;          //uint64, int64
                case 0xd4: position += 2; return;                     //fixext1
                case 0xd5: position += 3; return;                     //fixext2
                case 0xd6: position += 5; return;                     //fixext4
                case 0xd7: position += 9; return;                     //fixext8
                case 0xd8: position += 17; return;                    //fixext16
                case 0xd9: position += ReadByte(); return;            //str8
                case 0xda: position += ReadUInt16(); return;          //str16
                case 0xdb: position += checked((int)ReadUInt32()); return; //str32
                case 0xdc: SkipElements(ReadUInt16()); return;        //array16
                case 0xdd: SkipElements(checked((int)ReadUInt32())); return; //array32
                case 0xde: SkipElements(ReadUInt16() * 2); return;    //map16
                case 0xdf: SkipElements(checked((int)ReadUInt32()) * 2); return; //map32
            }
            throw new FormatException($"未対応の型です (0x{code:x2} at {position - 1})");
        }

        /// <summary>
        /// 指定個数の要素を読み飛ばすメソッド
        /// </summary>
        private void SkipElements(int count)
        {
            for (var i = 0; i < count; i++) Skip();
        }

        private byte ReadByte()
        {
            return buffer[position++];
        }

        private ushort ReadUInt16()
        {
            ushort value = (ushort)((buffer[position] << 8) | buffer[position + 1]);
            position += 2;
            return value;
        }

        private uint ReadUInt32()
        {
            uint value = ((uint)buffer[position] << 24)
                       | ((uint)buffer[position + 1] << 16)
                       | ((uint)buffer[position + 2] << 8)
                       | buffer[position + 3];
            position += 4;
            return value;
        }

        private ulong ReadUInt64()
        {
            ulong value = ((ulong)ReadUInt32() << 32) | ReadUInt32();
            return value;
        }
    }
}
