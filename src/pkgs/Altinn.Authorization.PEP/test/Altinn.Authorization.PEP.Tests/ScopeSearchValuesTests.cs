using Altinn.Authorization.Scopes;

namespace UnitTests;

public class ScopeSearchValuesTests
{
    [Fact]
    public void CannotBeEmpty()
    {
        Assert.Throws<ArgumentException>(() => ScopeSearchValues.Create([]));
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Index(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        for (int i = 0; i < items.Length; i++)
        {
            Assert.Equal(items[i], sut[i]);
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Count(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);
        
        Assert.Equal(items.Length, sut.Count);
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Enumerator(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);
        int index = 0;
        
        foreach (var item in sut)
        {
            Assert.Equal(items[index], item);
            index++;
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_Empty_False(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        Assert.False(sut.Check(string.Empty));
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_NotInSet_False(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        Assert.False(sut.Check("xyz"));
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_ExactString_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            Assert.True(sut.Check(item));
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_StartOfString_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            Assert.True(sut.Check($"{item} xyz"));
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_EndOfString_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            Assert.True(sut.Check($"xyz {item}"));
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_MiddleOfString_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            Assert.True(sut.Check($"xyz {item} zyx"));
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_MiddleOfString_Duplicated_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            Assert.True(sut.Check($"xyz {item} {item} zyx"));
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_Substring_False(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            Assert.False(sut.Check($"xyz{item} {item}{item} {item}xyz xyz{item}xyz"));
        }
    }

    public static TheoryData<string[]> ValidLists => new()
    {
        { ["abc"] },
        { ["def"] },
        { ["abc", "def"] },
        { ["abc", "abcdef", "def"] },
        { ["a", "b", "c", "d", "e", "f"] },
    };
}
