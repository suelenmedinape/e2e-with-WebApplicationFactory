namespace rifa_csharp_tests;

public class UnitTest1
{
    /*public static int Add(int x, int y) =>
        x + y;

    [Fact]
    public void Good() =>
        Assert.Equal(4, Add(2, 2));

    [Fact]
    public void Bad() =>
        Assert.Equal(5, Add(2, 2));*/
    
    /*[Theory]
    [InlineData(2, 2, 4)]
    [InlineData(3, 2, 5)]
    [InlineData(10, 5, 15)]
    public void Soma_varios_casos(int a, int b, int esperado)
    {
        Assert.Equal(esperado, a + b);
    }*/

    public static IEnumerable<object[]> DadosSoma =>
        new List<object[]>
        {
            new object[] { 1, 2, 3 },
            new object[] { 5, 5, 10 }
        };

    [Theory]
    [MemberData(nameof(DadosSoma))]
    public void Soma_com_memberdata(int a, int b, int esperado)
    {
        Assert.Equal(esperado, a + b);
    }

}