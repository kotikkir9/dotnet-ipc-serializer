public interface ICalculator
{
    int Add(int a, int b);
    int Subtract(int a, int b);
    int[] GetRandomNumbers();
}

public class Calculator : ICalculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Subtract(int a, int b)
    {
        return a - b;
    }

    public int[] GetRandomNumbers()
    {
        throw new NotImplementedException();
    }
}
