#include <iostream>
#include "Tests.cpp"

int main() {
    std::cout << "[Start exercise]" << std::endl;

    Solution* solution = new Solution();
    Tests* tests = new Tests();

    try
    {
        tests->Run(solution);
        std::cout << "[Exercise completed!]" << std::endl;
        std::cout << "[End exercise]" << std::endl;
    }
    catch (ExerciseException* e)
    {
        std::cout << "Test failed: " << e->what() << std::endl;
        std::cout << "[Exercise failed!]" << std::endl;
        std::cout << "[End exercise]" << std::endl;
        throw;
    }
    return 0;
}
