using System;
using System.Text.RegularExpressions;
using Mirror;
using UnityEngine;

public static class LobbySaves
{
	public struct Entry
	{
		public const int CurrentVersion = 0;

		public string LobbyName;
		public int LobbyIcon;
		public string Address;
		public DateTime Date;
		public int TrackerCount;
		public int ActionCount;
		public TimeSpan Runtime;
		public int Version;

		public Entry(string filename)
		{
			int tryvalue;

			string[] parts = filename.Split('_');

			LobbyName = parts.Length > 0
				? parts[0]
				: filename;
			LobbyIcon = parts.Length > 1 && int.TryParse(parts[1], out tryvalue)
				? tryvalue
				: -1;
			Address = parts.Length > 2
				? parts[2]
				: "Unknown";
			TrackerCount = parts.Length > 3 && int.TryParse(parts[3], out tryvalue)
				? tryvalue
				: -1;
			ActionCount = parts.Length > 4 && int.TryParse(parts[4], out tryvalue)
				? tryvalue
				: -1;

			Runtime = parts.Length > 5 && parts[5].Length == 16 && long.TryParse(parts[5], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out long runtime)
				? new TimeSpan(runtime)
				: default;

			Date = parts.Length > 7 && parts[6].Length == 10 && parts[7].Length == 8 && DateTime.TryParseExact(parts[6] + "_" + parts[7], "yyyy-MM-dd_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime date)
				? date
				: default;

			Version = parts.Length > 8 && int.TryParse(parts[8][..^6], out tryvalue)
				? tryvalue
				: -1;
		}

		public override string ToString()
		{
			return $"V{Version}: {LobbyName} [{LobbyIcon}] ({Address}) - {Date} - {TrackerCount} trackers, {ActionCount} actions, {Runtime}";
		}

		public string FileName()
		{
			return $"{LobbyName}_{LobbyIcon}_{Address}_{TrackerCount}_{ActionCount}_{Runtime.Ticks:X16}_{Date:yyyy-MM-dd_HH-mm-ss}_{Version}.lobby";
		}
	}

	private static string SanitizeHostForFileName(string host)
	{
		return Regex.Replace(host, @"[^a-zA-Z0-9\-\.]", ".");
	}

	public static void Save()
	{
		Entry entry = new Entry
		{
			LobbyName = LobbyInfoDispatcher.Instance.Name,
			LobbyIcon = LobbyInfoDispatcher.Instance.IconIndex,
			Address = SanitizeHostForFileName(LobbyInfoDispatcher.Instance.Address),
			Date = DateTime.Now,
			TrackerCount = TrackerManager.Instance.ValidTrackerCount,
			ActionCount = LobbyNetworkManager.ActionCount,
			Runtime = TimeSpan.FromSeconds(LobbyNetworkManager.Instance.recoverTime),
			Version = Entry.CurrentVersion
		};

		if (entry.Runtime.TotalSeconds < 0)
		{
			UnityEngine.Debug.LogWarning("Saving lobby with negative runtime: " + entry);
		}

		var otherSaves = GetSavedEntries();

		// Save the file
		string path = Application.persistentDataPath + "/" + entry.FileName();
		using (var writer = NetworkWriterPool.Get())
		{
			LobbyNetworkManager.Instance.OnSerialize(writer, true);

			var segment = writer.ToArraySegment();

			using (var file = new System.IO.FileStream(path, System.IO.FileMode.Create))
				file.Write(segment.Array, segment.Offset, segment.Count);
		}

		// If a different save with the same lobby, icon, tracker count, and action count, and Address exists, delete it
		for (int i = 0; i < otherSaves.Length; i++)
		{
			var other = otherSaves[i].Item2;
			if (entry.LobbyName == other.LobbyName &&
				entry.LobbyIcon == other.LobbyIcon &&
				entry.TrackerCount == other.TrackerCount &&
				entry.ActionCount == other.ActionCount &&
				entry.Address == other.Address &&
				(entry.Runtime - other.Runtime).TotalSeconds < 600)
				DeleteFile(otherSaves[i].Item1);
		}
	}

	public static void LoadFromFile(string path)
	{
		// Load the file
		byte[] data = System.IO.File.ReadAllBytes(path);

		if (NetworkServer.active || NetworkClient.active)
			LobbyNetworkManager.Cmd_BroadcastReinitialize(data);
		else {
			try {
				// Deserialize the data
				using (var reader = NetworkReaderPool.Get(data))
					LobbyNetworkManager.Instance.OnDeserialize(reader, true);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to load lobby from file {path}: {e}");
				Debug.LogException(e);
			}
		}
	}

	public static (string, Entry)[] GetSavedEntries()
	{
		string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath, "*.lobby");
		(string, Entry)[] entries = new (string, Entry)[files.Length];

		Entry entry;
		for (int i = 0; i < files.Length; i++)
		{
			try {
				entry = new Entry(files[i].Substring(Application.persistentDataPath.Length + 1));
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to load entry from file {files[i]}: {e}");
				entry = new Entry
				{
					LobbyName = files[i],
					LobbyIcon = -1,
					Address = "Unknown",
					Date = default,
					TrackerCount = -1,
					ActionCount = -1,
					Runtime = default
				};
			}
			entries[i] = (files[i], entry);
		}

		Array.Sort(entries, (a, b) => b.Item2.Date.CompareTo(a.Item2.Date));

		return entries;
	}

	public static bool HasSavedLobbies()
	{
		return System.IO.Directory.GetFiles(Application.persistentDataPath, "*.lobby").Length > 0;
	}

	public static void DeleteFile(string path)
	{
		System.IO.File.Delete(path);
	}

	public static void DeleteAllSavedLobbies()
	{
		string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath, "*.lobby");
		foreach (string file in files)
			System.IO.File.Delete(file);
	}
}