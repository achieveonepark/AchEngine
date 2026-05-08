using System;

namespace AchEngine
{
	struct BitString
	{
		public byte[] stream;

		public long Length
		{
			get { return stream.LongLength * 8; }
		}

		public BitString(byte[] stream)
		{
			this.stream = stream;
		}

		public bool this[ulong index]
		{
			get
			{
				var byteIndex = (long)(index / 8);
				//This - 'appears to work'...
				return
					(stream[byteIndex]
					 & (1 << (int)(index % 8))
					) > 0;
			}
			//...and so does this
			set
			{
				var byteIndex = (long)(index / 8);
				var mask = (byte)(1 << (int)(index % 8));

				if (value)
				{
					stream[byteIndex] |= mask;
				}
				else
				{
					stream[byteIndex] &= (byte)~mask;
				}
			}
		}

	}
}
