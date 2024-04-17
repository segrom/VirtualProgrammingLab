namespace Exercise;

public class Tests
{
    // use ExerciseException
    public void Run(Solution solution)
    {
        var result = solution.Sum(10, 55);
        if (result != 65) throw new ExerciseException($"Sum of 10 and 55: answer = {65}, result = {result}");
        
        result = solution.Sum(-2, 0);
        if (result != -2) throw new ExerciseException($"Sum of -2 and 0: answer = {-1}, result = {result}");
    }
}