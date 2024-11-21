using UnityEngine;

public class PlayerListPanel : SingletonMono<PlayerListPanel>
{
	public PrefabLayoutList<PlayerListEntry> PlayerEntryList;

	private void OnEnable()
	{
		UpdateListItems();
		Player.OnPlayerAdded += UpdateListItems;
		Player.OnPlayerRemoved += UpdateListItems;
	}

	private void OnDisable()
	{
		Player.OnPlayerAdded -= UpdateListItems;
		Player.OnPlayerRemoved -= UpdateListItems;

		for (int i = 0; i < PlayerEntryList.Count; i++)
			PlayerEntryList[i].Disconnect();
	}

	private void UpdateListItems(Player _ = null)
	{
		// Create new entries if needed
		for (int i = PlayerEntryList.Count; i < Player.Players.Count; i++)
			PlayerEntryList.Initialize();

		// Update existing entries
		for (int i = 0; i < Player.Players.Count; i++)
		{
			var entry = PlayerEntryList[i];
			entry.Connect(Player.Players[i]);
			entry.gameObject.SetActive(true);
		}

		// Hide extra entries
		for (int i = Player.Players.Count; i < PlayerEntryList.Count; i++)
			PlayerEntryList[i].gameObject.SetActive(false);
	}
}