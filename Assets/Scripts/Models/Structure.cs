public class Structure
{
    // Structure type is used to look up information from configuration etc.
    public StructureType StructureType { get; private set; }
    public float MovementCost { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool Connected { get; private set; }
    public Square BaseSquare { get; set; }

    // Returns true for each connection based on whether we are assigned to a square, should be connected to others,
    // and the square in the appropriate direction has an installed structure of the same type
    public bool ConnectedNorth { get => Connected
            && BaseSquare != null
            && BaseSquare.SquareNorth != null
            && BaseSquare.SquareNorth.InstalledStructure != null 
            && BaseSquare.SquareNorth.InstalledStructure.StructureType == StructureType; }
    public bool ConnectedEast { get => Connected
            && BaseSquare != null
            && BaseSquare.SquareEast != null && BaseSquare.SquareEast.InstalledStructure != null 
            && BaseSquare.SquareEast.InstalledStructure.StructureType == StructureType; }
    public bool ConnectedSouth { get => Connected
            && BaseSquare != null
            && BaseSquare.SquareSouth != null
            && BaseSquare.SquareSouth.InstalledStructure != null 
            && BaseSquare.SquareSouth.InstalledStructure.StructureType == StructureType; }
    public bool ConnectedWest { get => Connected
            && BaseSquare != null
            && BaseSquare.SquareWest != null
            && BaseSquare.SquareWest.InstalledStructure != null 
            && BaseSquare.SquareWest.InstalledStructure.StructureType == StructureType; }


    // Constructor takes all of the required information for creating a structure
    public Structure(StructureType structureType, float movementCost, int width, int height, bool connected = false)
    {
        this.StructureType = structureType;
        this.MovementCost = movementCost;
        this.Width = width;
        this.Height = height;
        this.Connected = connected;
    }
}
