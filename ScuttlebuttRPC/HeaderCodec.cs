// Copyright © 2020 Pedro Gómez Martín <zentauro@riseup.net>
//
// This file is part of the library Scuttlebutt.RPC which
// is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this library. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;

namespace Scuttlebutt.RPC.Codec
{
    public enum BodyType
    {
        Binary = 0b_00,
        UTFStr = 0b_01,
          JSON = 0b_10,
    }

    public class Header
    {
        public sbyte[] Inner;

        public const sbyte IS_ENDERR = 0b_0000_0100;
        public const sbyte IS_STREAM = 0b_0000_1000;

        const uint FLAGS_SIZE   = 1;
        const uint BLENGTH_SIZE = 4;
        const uint REQNUM_SIZE  = 4;

        public Header()
        {
            this.Inner = new sbyte[FLAGS_SIZE + BLENGTH_SIZE + REQNUM_SIZE];
        }

        public Header SetStreamBit(bool isstream)
        {
            if (isstream)
            {
                this.Inner[0] |= IS_STREAM;
            }
            else
            {
                this.Inner[0] &= ~IS_STREAM;
            }

            return this;
        }

        public bool IsStream()
        {
            var res = this.Inner[0] & IS_STREAM;
            return res != 0;
        }

        public Header SetEndErr(bool enderr)
        {
            if (enderr)
            {
                this.Inner[0] |= IS_ENDERR;
            }
            else
            {
                this.Inner[0] &= ~IS_ENDERR;
            }

            return this;
        }

        public bool IsEndErr()
        {
            var res = this.Inner[0] & IS_ENDERR;
            return res != 0;
        }

        public Header SetBodyType(BodyType type)
        {
            this.Inner[0] &= ~0b_11;
            this.Inner[0] |= (sbyte)type;

            return this;
        }

        public BodyType GetBodyType()
        {
            var btype = this.Inner[0] & 0b_11;
            return (BodyType) btype;
        }

        public Header SetBodyLength(uint length)
        {
            var bytes = BitConverter.GetBytes(length);

            // Check for endianness and transmit in Network byte order
            // i.e. Big Endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            // We copy the length as bytes in the 2nd to 5th bytes of
            // the header
            Buffer.BlockCopy(bytes, 0, this.Inner, 1, 4);

            return this;
        }

        public uint GetBodyLength()
        {
            var bytes = new byte[4];
            Buffer.BlockCopy(this.Inner, 1, bytes, 0, 4);

            // Bytes that come from network are Big Endian, if the
            // machine is LE, we reverse them
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        public Header SetRequestNumber(uint reqnum)
        {
            var bytes = BitConverter.GetBytes(reqnum);

            // Check for endianness and transmit in Network byte order
            // i.e. Big Endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            // We copy the length as bytes in the 6th to 9th bytes of
            // the header
            Buffer.BlockCopy(bytes, 0, this.Inner, 5, 4);

            return this;
        }

        public uint GetRequestNumber()
        {
            var bytes = new byte[4];
            Buffer.BlockCopy(this.Inner, 5, bytes, 0, 4);

            // Bytes that come from network are Big Endian, if the
            // machine is LE, we reverse them
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

    }
}
