namespace Exercise;

public class Tests
{
    public bool Run(Solution solution)
    {
        var result = solution.Sum(10, 55);
        if (result != 65) return false;
        
        result = solution.Sum(-2, 0);
        if (result != -2) return false;

        return true;
    }
}