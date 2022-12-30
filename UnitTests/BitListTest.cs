using ConsoleApp1;
using ConsoleApp1.BitLists;

namespace UnitTests;

public class BitListTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(12)]
    public void CtorByCapacityTest(int capacity)
    {
        var bitList = new ByteBitList(capacity);
        Assert.True(bitList.Capacity >= capacity);
    }
    
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true, false, true, false, true, false, true, false, true)]
    [InlineData(true, false, true, false, true, false, true, false)]
    public void CtorByCollectionTest(params bool[] collection)
    {
        var bitList = new ByteBitList(collection);
        Assert.Equal(collection.Length, bitList.Count);
        for (var i = 0; i < collection.Length; i++)
        {
            Assert.True(collection[i] == bitList[i]);
        }
    }
    
    [Fact]
    public void GetterOutOfRangeTest()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var bitList = new ByteBitList();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitList[1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitList[-1]);
    }
    
    [Fact]
    public void SetterOutOfRangeTest()
    {
        // ReSharper disable once CollectionNeverQueried.Local
        var bitList = new ByteBitList() ;
        Assert.Throws<ArgumentOutOfRangeException>(() => bitList[1] = true);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitList[-1] = true);
    }
    
    [Fact]
    public void SetterTest()
    {
        const int totalCount = 20;
        var bitList = new ByteBitList(Enumerable.Repeat(true, totalCount));
        var initialCapacity = bitList.Capacity;
        for (var i = 0; i < totalCount; i++)
        {
            bitList[i] = false;
            Assert.False(bitList[i]);
            for (var j = 0; j < totalCount; j++)
            {
                if (j == i)
                    continue;
                Assert.True(bitList[j]);
            }
            bitList[i] = true;
        }
        Assert.Equal(initialCapacity, bitList.Capacity);
        Assert.Equal(totalCount, bitList.Count);
    }
    
    [Theory]
    [InlineData(true, 3, false, false, false, false, false, false, false, true)]
    [InlineData(true, 7, false, false, false, false, false, false, false, true, false, true)]
    [InlineData(true, 8, false, false, false, false, false, false, false, true, false, true)]
    public void InsertTest(bool item, int index, params bool[] collection)
    {
        var bitList = new ByteBitList(collection);
        bitList.Insert(index, item);
        var list = collection.ToList();
        list.Insert(index, item);
        Assert.Equal(list.Count, bitList.Count);
        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(list[i], bitList[i]);
        }
    }

    [Fact]
    public void InsertOutOfRangeTest()
    {
        // ReSharper disable once CollectionNeverQueried.Local
        var bitList = new ByteBitList();
        bitList.Insert(0, true);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitList.Insert(2, true));
        Assert.Throws<ArgumentOutOfRangeException>(() => bitList.Insert(-1, true));
    }
    
    [Theory]
    [InlineData]
    [InlineData(true)]
    [InlineData(false, false, false, false, false, false, false, true)]
    [InlineData(false, false, false, false, false, false, false, true, false, true)]
    public void IteratorTest(params bool[] collection)
    {
        var bitList = new ByteBitList(collection);
        using var bitEnumerator = bitList.GetEnumerator();
        using var collectionEnumerator = collection.AsEnumerable().GetEnumerator();
        
        IteratorInternalTest(collectionEnumerator, bitEnumerator);
        collectionEnumerator.Reset();
        bitEnumerator.Reset();
        IteratorInternalTest(collectionEnumerator, bitEnumerator);
        
        void IteratorInternalTest(IEnumerator<bool> collectionEnumerator, IEnumerator<bool> bitEnumerator)
        {
            Assert.Throws<InvalidOperationException>(() => bitEnumerator.Current);
            while (collectionEnumerator.MoveNext())
            {
                Assert.True(bitEnumerator.MoveNext());
                Assert.Equal(collectionEnumerator.Current, bitEnumerator.Current);
            }

            Assert.False(bitEnumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => bitEnumerator.Current);
        }
    }
    
    [Theory]
    [InlineData(3, false, false, false, false, false, false, false, true)]
    [InlineData(7, false, false, false, false, false, false, false, true, false, true)]
    [InlineData(8, false, false, false, false, false, false, false, true, false, true)]
    public void RemoveAtTest(int index, params bool[] collection)
    {
        var bitList = new ByteBitList(collection);
        bitList.RemoveAt(index);
        var list = collection.ToList();
        list.RemoveAt(index);
        Assert.Equal(list.Count, bitList.Count);
        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(list[i], bitList[i]);
        }
    }

    [Fact]
    public void CopyToArrayIndexIsOutOfRange()
    {
        var arrayToCopy = new[] {true};
        var array = new bool[2];
        arrayToCopy.CopyTo(array, 5);
    }
}