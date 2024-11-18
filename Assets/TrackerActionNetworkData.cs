public struct TrackerActionNetworkData
{
	public byte attachedTracker;
	public byte buttonIndex;
	public byte type;
	public byte target;
	public ushort data;

	public readonly bool IsValid => type != 0;
}