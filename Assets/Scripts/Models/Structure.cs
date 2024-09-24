public class Structure
{
    // Structure type is used to look up information from configuration etc.
    public string StructureType { get; private set; }
    public float MovementCost { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Tile BaseTile { get; private set; }

    // Constructor takes all of the required information for creating a structure
    public Structure(string structureType, float movementCost, int width, int height)
    {
        this.StructureType = structureType;
        this.MovementCost = movementCost;
        this.Width = width;
        this.Height = height;
    }
}
