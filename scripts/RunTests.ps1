param([String]$ci="false") 

# Run the test coverage
$TestOutput = dotnet test --verbosity normal --collect:"XPlat Code Coverage" --settings:coverlet.runsettings /p:threshold=98 /p:thresholdType=line /p:thresholdStat=total /p:CollectCoverage=true

Write-Host $TestOutput

# ci variable is set in .github/workflows/truevote-api-version.yml. When run locally, it will fall through here, pass or fail
if ($ci -eq "true" -and $TestOutput -clike "*FAILED*") {
	Write-Host "Failed. Exiting"
	exit -1
}

# Find the generated GUID in the path
$TestReports = $TestOutput | Select-String coverage.cobertura.xml | ForEach-Object { $_.Line.Trim() }

# Replace backslashes with slashes. This helps it run on both Windows and Unix
$TestReports = $TestReports.Replace('\', '/')

# Parse off the path
$TestReportsPath = $TestReports.Substring(0, $TestReports.LastIndexOf('/'))

# Copy the coverage file up one directory
Copy-Item $TestReportsPath/*.* -Destination 'TrueVote.Api.Tests/TestResults'

# Generate the HTML Report
dotnet reportgenerator "-reports:$TestReports" "-targetdir:TrueVote.Api.Tests/coverage-html" "-reporttype:Html"
