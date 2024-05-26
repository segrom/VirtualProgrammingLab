#pragma once
#include <exception>
#include <string>

class ExerciseException: public std::exception {
public:
    ExerciseException(const std::string& message): message{message}
    {}
    const char* what() const noexcept override
    {
        return message.c_str();
    }
private:
    std::string message;
};