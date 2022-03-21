internal struct TriangleCut
{
    public EdgeCut Cut1;
    public EdgeCut Cut2;
}

public struct EdgeCut
{
    public int From;
    public int To;
    public float Intersection;
}