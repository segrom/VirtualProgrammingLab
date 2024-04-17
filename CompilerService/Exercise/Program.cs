// See https://aka.ms/new-console-template for more information

using Exercise;

Console.WriteLine("[Start Exercise]");
var solution = new Solution();
var tests = new Tests();

Console.WriteLine(tests.Run(solution) 
    ? "[Exercise Complete!]" 
    : "[Exercise Failed!]");

Console.WriteLine("[End Exercise]");
