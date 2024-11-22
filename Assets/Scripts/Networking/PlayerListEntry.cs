using UnityEngine;

public class PlayerListEntry : MonoBehaviour
{
	public SDispatcher<string> Username;
	public SDispatcher<Sprite> Icon;
	public SDispatcher<string> Description;
	public SDispatcher<bool> IsHost;
	public SDispatcher<bool> IsLocal;

	private Player connection;

	public void Connect(Player player)
	{
		Disconnect();

		connection = player;
		connection.OnUsernameChanged.AddListener(Username.SetValue);
		connection.OnIconChanged.AddListener(Icon.SetValue);
		connection.OnDescriptionChanged.AddListener(Description.SetValue);
		connection.OnIsHostChanged.AddListener(IsHost.SetValue);
		IsLocal.Value = connection.isLocalPlayer;

		Username.SetValue(connection.Username);
		Icon.SetValue(GlobalSpriteList.GetSprite(connection.IconIndex));
		Description.SetValue(connection.Description);
		IsHost.SetValue(connection.IsHost);
	}

	public void Disconnect()
	{
		if (connection != null)
		{
			connection.OnUsernameChanged.RemoveListener(Username.SetValue);
			connection.OnIconChanged.RemoveListener(Icon.SetValue);
			connection.OnDescriptionChanged.RemoveListener(Description.SetValue);
			connection.OnIsHostChanged.RemoveListener(IsHost.SetValue);
			connection = null;
		}
	}
}