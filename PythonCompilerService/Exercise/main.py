from tests import *
from solution import *

print("[Start exercise]")

solution = Solution()
tests = Tests()

try:
    tests.run(solution)
except ExerciseException as ex:
    print(f"Test failed: {ex}")
    print("[Exercise failed!]")
    raise ex
finally:
    print("[End exercise]")
