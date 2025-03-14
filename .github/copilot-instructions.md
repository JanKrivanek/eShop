
# Instructions for tests generation

Generate comprehensive C# unit tests using MSTest for every public/protected/internal method in any provided public/protected/internal class.
The tests must cover the happy paths, edge cases, and exceptional scenarios, and should follow C# best practices including clear naming conventions and the Arrange-Act-Assert pattern.

## Requirements

1. For each public/protected/internal class:
    - Create a corresponding test class named "[ClassName]Tests".
    - Use meaningful namespace organization (e.g., "[SourceNamespace].UnitTests").
    - Ensure that fields in the test class are declared as readonly when possible.
2. If the file contains only interfaces or delegates, return an empty reply.
3. For every public/protected/internal method, analyze its signature and (if available) its comments to infer:
    - Expected inputs, outputs, and side effects.
    - Boundary conditions and error conditions (e.g., invalid parameters, exception triggers).
    - Scenarios in which dependencies could affect behavior.
4. If methods interact with external dependencies:
    - Assume dependencies are provided via dependency injection.
    - Use Moq to mock these dependencies.
    - Define expected behavior for the mocks based on the scenarios tested.
5. Each unit test should:
    - Follow the Arrange-Act-Assert pattern.
    - Have a descriptive name in the format: MethodName_Condition_ExpectedOutcome.
    - Include an XML documentation comment explaining the purpose of the test, the functional steps of the test and expected outcome.
    - Validate that the outcome (e.g., return value, thrown exception, side effect) matches what is expected.
6. Ensure your test code:
    - Includes all required using directives (e.g., using Microsoft.VisualStudio.TestTools.UnitTesting; using Moq;).
    - The using directives are sorted alphabetically.
    - Is fully self-contained, formatted as a complete C# file ready to compile.
    - Covers a variety of inputs (happy path, boundary, invalid) for each method.
    - Fields should be initialized in the constructor.
    - DOES NOT use setup methods (e.g., TestInitialize) unless absolutely necessary.
7. The result should include:
    - The required NuGet packages for the test code with their version.
    - The C# code
    - **DO NOT** add anything after the C# code.

## Notes

- Use meaningful test data and ensure all potential paths (including nulls or invalid data where applicable) are tested.
- If a method uses external services (e.g., a repository, API client), configure a mock with expected behavior.
- Each test method should clearly communicate its purpose in both its name and accompanying comments.

## Test code style

- Follow the code style of preexisting code.
- Comment the test to explain them.
- If possible indicate 'Arrange', 'Act', 'Assert' parts of the test.
- Prefer Data-driven unittests when needing to test multiple variants of parameters.