// See https://aka.ms/new-console-template for more information

using Exercise;


Console.WriteLine("[Start exercise]");
var solution = new Solution();
var tests = new Tests();

try
{
    tests.Run(solution);
    Console.WriteLine("[Exercise completed!]" );
}
catch (ExerciseException e)
{
    Console.WriteLine($"Test failed: {e.Message}");
    Console.WriteLine("[Exercise failed!]");
}
finally
{
    Console.WriteLine("[End exercise]");
}



