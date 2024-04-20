from utils import *
from solution import *


class Tests:
    def run(self, solution: Solution):
        result = solution.calculateSum(-1, 15)
        if result != 14:
            raise ExerciseException(f"Sum of -1 and 15: answer = {14}, result = {result}")

        result = solution.calculateSum(103, 15)
        if result != 25:
            raise ExerciseException(f"Sum of 10 and 15: answer = {25}, result = {result}")
