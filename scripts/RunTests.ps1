param([String]$ci="false") 

# Run the test coverage
$TestOutput = dotnet coverlet "TrueVote.Api.Tests/bin/Debug/net8.0/TrueVote.Api.Tests.dll" --target "dotnet" --targetargs "test --verbosity normal --no-build" --format cobertura --output TrueVote.Api.Tests/TestResults/ --threshold=100 --threshold-type=line --threshold-stat=total --exclude-by-file "**.g.cs"

Write-Host $TestOutput

# ci variable is set in .github/workflows/truevote-api-version.yml. When run locally, it will fall through here, pass or fail
if ($ci -eq "true" -and ($TestOutput -clike "*below the specified*" -or $TestOutput -clike "*FAILED*")) {
	Write-Host "Failed. Exiting"
	exit -1
}

# Generate the HTML Report
dotnet reportgenerator "-reports:TrueVote.Api.Tests/TestResults/coverage.cobertura.xml" "-targetdir:TrueVote.Api.Tests/coverage-html" "-reporttype:Html"
