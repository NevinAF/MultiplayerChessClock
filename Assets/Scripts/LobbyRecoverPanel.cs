using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class LobbyRecoverPanel : MonoBehaviour
{
	public PrefabLayoutList<LobbyRecoverEntry> Entries;

	private (string, LobbySaves.Entry)[] m_entries;

	public SDispatcher<bool> HasEntries;

	public UnityEvent OnFileSelected;

	public void OnEnable()
	{
		m_entries = LobbySaves.GetSavedEntries();

		// Create new entries if needed
		for (int counter = Entries.Count; counter < m_entries.Length; counter++)
		{
			int index = counter;
			var entry = Entries.Initialize();
			entry.Recover.onClick.AddListener(() =>
			{
				OnFileSelected?.Invoke();
				LobbySaves.LoadFromFile(m_entries[index].Item1);

				if (NetworkServer.active || NetworkClient.active)
					return;

				NetworkManager.singleton.StartHost();
			});
			entry.Delete.onClick.AddListener(() =>
			{
				ConfirmationPanel.Instance.Show("Delete Lobby?", "Are you sure you want to delete this lobby?\nThis action cannot be undone.", "Delete", () =>
				{
					LobbySaves.DeleteFile(m_entries[index].Item1);
					OnEnable();
				});
			});
		}

		// Update existing entries
		for (int i = 0; i < m_entries.Length; i++)
		{
			var entry = Entries[i];
			entry.SetData(m_entries[i].Item2);
			entry.gameObject.SetActive(true);
		}

		// Hide extra entries
		for (int i = m_entries.Length; i < Entries.Count; i++)
			Entries[i].gameObject.SetActive(false);

		HasEntries.Value = m_entries.Length > 0;
	}

	public void OnDeleteAll()
	{
		ConfirmationPanel.Instance.Show("Delete All Lobbies?", "Are you sure you want to delete all saved lobbies?\nThis action cannot be undone.", "Delete All", () =>
		{
			LobbySaves.DeleteAllSavedLobbies();
			OnEnable();
		});
	}
}