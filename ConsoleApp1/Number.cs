using ConsoleApp1.BitLists;

namespace ConsoleApp1;

public class Number : IMultiply<Number>, ISum<Number>
{
    private readonly List<long> _bitList;
    
    public Number()
    {
        _bitList = new List<long>();
        BitCount = 0;
    }

    private Number(List<long> bits)
    {
        _bitList = bits;
        BitCount = 0;
    }

    public int BitCount { get; private set; }

    public Number Multiply(Number other)
    {
        var result = new ByteBitList(BitCount + other.BitCount);
        
        throw new NotImplementedException();
    }

    public Number Sum(Number other)
    {
        var resultBitCount = Math.Max(BitCount, other.BitCount) + 1;
        var result = new List<long>(resultBitCount / 64);
        
        throw new NotImplementedException();
    }
}