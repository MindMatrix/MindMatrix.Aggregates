dotnet new classlib -n %1 -o src\%1
@del src\%1\Class1.cs >NUL
dotnet new xunit -n %1.Tests -o test\%1.Tests
@del test\%1.Tests\UnitTest1.cs >NUL
dotnet remove test\%1.Tests\%1.Tests.csproj package Microsoft.NET.Test.Sdk
dotnet remove test\%1.Tests\%1.Tests.csproj package xunit
dotnet remove test\%1.Tests\%1.Tests.csproj package xunit.runner.visualstudio
dotnet add test\%1.Tests\%1.Tests.csproj package Fixie
dotnet add test\%1.Tests\%1.Tests.csproj package Shouldly
dotnet add test\%1.Tests\%1.Tests.csproj reference src\%1


