public static class Formatting
{
	public static string Time(double time)
	{
		if (time < 0)
			return "-" + Time(-time);
		if (time < 60)
			return time.ToString("F1") + "s";
		if (time < 3600)
			return $"{(int)time / 60}m {(int)(time % 60):00}s";
		return $"{(int)time / 3600}h {(int)(time / 60) % 60}m {time % 60:00}s";
	}
}