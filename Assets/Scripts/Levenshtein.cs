using System;

public static class Levenshtein
{
	public static int Distance(string a, string b)
	{
		if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 0;
		if (string.IsNullOrEmpty(a)) return b.Length;
		if (string.IsNullOrEmpty(b)) return a.Length;

		int aLen = a.Length;
		int bLen = b.Length;
		int[,] d = new int[aLen + 1, bLen + 1];

		for (int i = 0; i <= aLen; d[i, 0] = i++) ;
		for (int j = 0; j <= bLen; d[0, j] = j++) ;

		for (int row = 1; row <= aLen; row++)
		{
			for (int col = 1; col <= bLen; col++)
			{
				int cost = (b[col - 1] == a[row - 1]) ? 0 : 1;
				d[row, col] = Math.Min(
					Math.Min(d[row - 1, col] + 1, d[row, col - 1] + 1),
					d[row - 1, col - 1] + cost
				);
			}
		}
		return d[aLen, bLen];
	}
}
