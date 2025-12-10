Pre-requisites:
Powershell: 
-setx ApiKey "my_secret" 
-setx HuggingFaceApiKey "my_secret"

If there is no VS Enterprise (like in my case): run this to get UT COVERAGE: reportgenerator -reports:"C:\ProcessMonitor.API\coverage.cobertura.xml" -targetdir:"C:\ProcessMonitor.API\CoverageReport" -reporttypes:Html

