using UnityEngine;

public class PlayerListEntry : MonoBehaviour
{
	public SDispatcher<string> Username;
	public SDispatcher<Sprite> Icon;
	public SDispatcher<string> Description;

	private Player connection;

	public void Connect(Player player)
	{
		Disconnect();

		connection = player;
		connection.OnUsernameChanged.AddListener(Username.SetValue);
		connection.OnIconChanged.AddListener(Icon.SetValue);
		connection.OnDescriptionChanged.AddListener(Description.SetValue);

		Username.SetValue(connection.Username);
		Icon.SetValue(GlobalSpriteList.GetSprite(connection.IconIndex));
		Description.SetValue(connection.Description);
	}

	public void Disconnect()
	{
		if (connection != null)
		{
			connection.OnUsernameChanged.RemoveListener(Username.SetValue);
			connection.OnIconChanged.RemoveListener(Icon.SetValue);
			connection.OnDescriptionChanged.RemoveListener(Description.SetValue);
			connection = null;
		}
	}
}