using System.Collections;

namespace ConsoleApp1.BitLists;

public class LongBitList : IList<bool>
{
    private readonly List<byte> _byteList;
        
    public LongBitList()
    {
        _byteList = new List<byte>();
    }
        
    public LongBitList(int capacity)
    {
        var internalCapacity = capacity / 8 + (capacity % 8 > 0 ? 1 : 0);
        _byteList = new List<byte>(internalCapacity);
    }
        
    public LongBitList(IEnumerable<bool> collection)
    {
        if (collection is ICollection<bool> c)
        {
            var count = c.Count;
            if (count == 0)
            {
                _byteList = new List<byte>(0);
            }
            else
            {
                var internalCapacity = count / 8 + (count % 8 > 0 ? 1 : 0);
                _byteList = new List<byte>(internalCapacity);
                _byteList.AddRange(Convert(c));
            }

            Count = count;
        }
        else
        {
            _byteList = new List<byte>(Convert(collection));
        }
    }

    public int Count { get; private set; }
    public int Capacity => _byteList.Capacity * 8;
    public bool IsReadOnly => false;
        
    public IEnumerator<bool> GetEnumerator()
    {
        return new BitEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(bool item)
    {
        var offset = GetOffset(Count);
        if (offset == 0)
            _byteList.Add(0);
        this[GetSubIndex(Count), offset] = item;
        Count++;
    }

    public void Clear()
    {
        _byteList.Clear();
        Count = 0;
    }

    // todo: improve
    public void CopyTo(bool[] array, int arrayIndex)
    {
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Number can't be less then zero.");
        if (arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Number can't be greater then array's upper bound.");
        if (array.Length < arrayIndex + Count)
            throw new ArgumentException("Destination array is not long enough. Check the destination index, length, and the array's lower bounds.", nameof(array));

        for (var i = 0; i < Count; i++)
        {
            array[arrayIndex + i] = this[GetSubIndex(i), GetOffset(i)];
        }
    }

    #region Can't implement

    [Obsolete("This method doesn't work", true)]
    public bool Contains(bool item)
    {
        return false;
    }

    [Obsolete("This method doesn't work", true)]
    public bool Remove(bool item)
    {
        return false;
    }
        
    [Obsolete("This method doesn't work", true)]
    public int IndexOf(bool item)
    {
        return -1;
    }

    #endregion

    public void Insert(int index, bool item)
    {
        if (index > Count || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the bounds of the List.");
        
        if (Count % 8 == 0)
            _byteList.Add(0);

        var subIndex = GetSubIndex(index);
        var offset = GetOffset(index);
        
        ShiftRightBitsToRight(subIndex, offset, ref item);

        for (var i = subIndex + 1; i < _byteList.Count; i++)
        {
            ShiftWholeByteToRight(i, ref item);
        }

        Count++;
    }

    public void RemoveAt(int index)
    {
        if (index > Count || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the bounds of the List.");
        
        var subIndex = GetSubIndex(index);
        var offset = GetOffset(index);
        
        var item = false; // fill with false if no previous bytes
        for (var i = _byteList.Count - 1; i > subIndex; i--)
        {
            ShiftWholeByteToLeft(i, ref item);
        }
        
        ShiftRightBitsToLeft(subIndex, offset, ref item);
        
        if (Count % 8 == 1)
            _byteList.RemoveAt(Count / 8);

        Count--;
    }

    public bool this[int index]
    {
        get => index < Count && index >= 0 ? this[GetSubIndex(index), GetOffset(index)] : throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the collection.");
        set => this[GetSubIndex(index), GetOffset(index)] = index < Count && index >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the collection.");
    }

    private bool this[int subIndex, int offset]
    {
        get => (_byteList[subIndex] & GetMaskByOffset(offset)) != 0;
        set
        {
            if (value)
            {
                _byteList[subIndex] |= GetMaskByOffset(offset);
            }
            else
            {
                _byteList[subIndex] &= GetInvertedMaskByOffset(offset);
            }
        }
    }

    #region Private

    private IEnumerable<byte> Convert(IEnumerable<bool> collection)
    {
        var counter = 0;
        var arr = new bool[8];
        foreach (var b in collection)
        {
            var offset = GetOffset(counter);
            arr[offset] = b;
            if (offset == 7)
            {
                yield return Convert(arr);
            }
            counter++;
        }

        if (GetOffset(counter) != 0)
        {
            yield return Convert(arr);
        }

        Count = counter;
    }

    private static byte Convert(bool[] arr)
    {
        byte result = 0;
            
        for (var offset = 0; offset < arr.Length; offset++)
        {
            var bit = arr[offset] ? 1 : 0;
            result |= (byte)(bit << offset);
        }

        return result;
    }

    private static int GetSubIndex(int index)
    {
        return index / 8;
    }

    private static int GetOffset(int index)
    {
        return index % 8;
    }

    private static byte GetMaskByOffset(int offset)
    {
        return (byte) (1 << offset);
    }

    private static byte GetLeftMaskExcluding(int offset)
    {
        const byte zero = 0;
        const byte one = 1;
        const byte two = 1 + 2;
        const byte three = 1 + 2 + 4;
        const byte four = 1 + 2 + 4 + 8;
        const byte five = 1 + 2 + 4 + 8 + 16;
        const byte six = 1 + 2 + 4 + 8 + 16 + 32;
        const byte seven = 1 + 2 + 4 + 8 + 16 + 32 + 64;
        
        return offset switch
        {
            0 => zero,
            1 => one,
            2 => two,
            3 => three,
            4 => four,
            5 => five,
            6 => six,
            7 => seven,
            _ => throw new ArgumentOutOfRangeException(nameof(offset), "Index must by between >=0 and <=7.")
        };
    }

    private static byte GetRightMaskIncluding(int offset)
    {
        const byte zero = byte.MaxValue;
        const byte one = byte.MaxValue - 1;
        const byte two = byte.MaxValue - 1 - 2;
        const byte three = byte.MaxValue - 1 - 2 - 4;
        const byte four = byte.MaxValue - 1 - 2 - 4 - 8;
        const byte five = byte.MaxValue - 1 - 2 - 4 - 8 - 16;
        const byte six = byte.MaxValue - 1 - 2 - 4 - 8 - 16 - 32;
        const byte seven = byte.MaxValue - 1 - 2 - 4 - 8 - 16 - 32 - 64;
        
        return offset switch
        {
            0 => zero,
            1 => one,
            2 => two,
            3 => three,
            4 => four,
            5 => five,
            6 => six,
            7 => seven,
            _ => throw new ArgumentOutOfRangeException(nameof(offset), "Index must by between >=0 and <=7.")
        };
    }

    private static byte GetRightMaskExcluding(int offset)
    {
        const byte zero = byte.MaxValue - 1;
        const byte one = byte.MaxValue - 1 - 2;
        const byte two = byte.MaxValue - 1 - 2 - 4;
        const byte three = byte.MaxValue - 1 - 2 - 4 - 8;
        const byte four = byte.MaxValue - 1 - 2 - 4 - 8 - 16;
        const byte five = byte.MaxValue - 1 - 2 - 4 - 8 - 16 - 32;
        const byte six = byte.MaxValue - 1 - 2 - 4 - 8 - 16 - 32 - 64;
        const byte seven = byte.MaxValue - 1 - 2 - 4 - 8 - 16 - 32 - 64 - 128;
        
        return offset switch
        {
            0 => zero,
            1 => one,
            2 => two,
            3 => three,
            4 => four,
            5 => five,
            6 => six,
            7 => seven,
            _ => throw new ArgumentOutOfRangeException(nameof(offset), "Index must by between >=0 and <=7.")
        };
    }

    private static byte GetInvertedMaskByOffset(int offset)
    {
        return (byte) (byte.MaxValue - GetMaskByOffset(offset));
    }

    private void ShiftRightBitsToLeft(int subIndex, int offset, ref bool value)
    {
        var toSave = this[subIndex, offset];
        
        var @byte = _byteList[subIndex];
        var leftMaskExcluding = GetLeftMaskExcluding(offset);
        var rightMaskExcluding = GetRightMaskExcluding(offset);

        var leftBits = @byte & leftMaskExcluding;
        var rightBits = @byte & rightMaskExcluding;

        var newRightBits = rightBits >> 1;
        var newByte = (byte) (leftBits | newRightBits);

        _byteList[subIndex] = newByte;

        this[subIndex, 7] = value;

        value = toSave;
    }

    private void ShiftWholeByteToLeft(int subIndex, ref bool value)
    {
        var toSave = this[subIndex, 0];
        _byteList[subIndex] = (byte) (_byteList[subIndex] >> 1);
        this[subIndex, 7] = value;
        value = toSave;
    }

    private void ShiftRightBitsToRight(int subIndex, int offset, ref bool value)
    {
        var toSave = this[subIndex, 7];

        var @byte = _byteList[subIndex];
        var leftMaskExcluding = GetLeftMaskExcluding(offset);
        var rightMaskIncluding = GetRightMaskIncluding(offset);

        var leftBits = @byte & leftMaskExcluding;
        var rightBits = @byte & rightMaskIncluding;

        var newRightBits = rightBits << 1;
        var newByte = (byte) (leftBits | newRightBits);

        _byteList[subIndex] = newByte;

        this[subIndex, offset] = value;

        value = toSave;
    }

    private void ShiftWholeByteToRight(int subIndex, ref bool value)
    {
        var toSave = this[subIndex, 7];
        _byteList[subIndex] = (byte) (_byteList[subIndex] << 1);
        this[subIndex, 0] = value;
        value = toSave;
    }
    
    private struct BitEnumerator : IEnumerator<bool>
    {
        private readonly LongBitList _bitList;
        private int _index;

        public BitEnumerator(LongBitList bitList)
        {
            _bitList = bitList;
            _index = -1;
        }
        
        public bool MoveNext()
        {
            var nextIndex = _index + 1;
            if (nextIndex >= _bitList.Count)
            {
                _index = _bitList.Count;
                return false;
            }
            _index = nextIndex;
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public bool Current
        {
            get
            {
                if (_index < 0)
                    throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
                if (_index >= _bitList.Count)
                    throw new InvalidOperationException("Enumeration already finished.");
                return _bitList[GetSubIndex(_index), GetOffset(_index)];
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }

    #endregion
}