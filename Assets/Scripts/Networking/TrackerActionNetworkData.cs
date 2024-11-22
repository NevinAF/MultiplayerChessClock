public struct TrackerActionNetworkData
{
	public byte attachedTracker;
	public byte buttonIndex;
	public byte type;
	public byte target;
	public ushort data;

	public readonly bool IsValid => type != 0;

	public override string ToString()
	{
		return $"{{{attachedTracker}-{buttonIndex}: {type} {target} {data:X4}}}";
	}
}