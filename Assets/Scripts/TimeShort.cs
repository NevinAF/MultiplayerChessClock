// 1 bit for sign, 2 bits for unit type (0 = seconds, 1 = minutes, 2 = hours, 3 = days), 13 bits for value
public static class TimeShort
{
	public const int MaxValue = 0x1FFF * 60 * 60 * 24; // 13 bits

	public static int ToSeconds(ushort value)
	{
		int sec = value & 0x1FFF;
		if ((value & 0x2000) == 0x2000)
			sec *= 60;
		if ((value & 0x4000) == 0x4000)
			sec *= 60;
		if ((value & 0x6000) == 0x6000)
			sec *= 24;
		return (value & 0x8000) != 0 ? -sec : sec;
	}

	public static ushort FromSeconds(int sec)
	{
		bool sign = sec < 0;
		sec = System.Math.Abs(sec);
		if (sec < 0x1FFF)
			return (ushort)(sec | (sign ? 0x8000 : 0));

		if (sec < 0x1FFF * 60)
			return (ushort)((sec / 60) | 0x2000 | (sign ? 0x8000 : 0));
		
		if (sec < 0x1FFF * 60 * 60)
			return (ushort)((sec / 60 / 60) | 0x4000 | (sign ? 0x8000 : 0));

		if (sec < 0x1FFF * 60 * 60 * 24)
			return (ushort)((sec / 60 / 60 / 24) | 0x6000 | (sign ? 0x8000 : 0));

		throw new System.ArgumentOutOfRangeException("sec", "Value is too large to be stored in a TimeHalf");
	}
}