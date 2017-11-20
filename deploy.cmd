%DEPLOYMENT_SOURCE%\.paket\paket.exe install
dotnet build %DEPLOYMENT_SOURCE%\html-to-fsharp.sln
dotnet publish %DEPLOYMENT_SOURCE%\src\HtmlToFSharp.Web\ -c Release
xcopy %DEPLOYMENT_SOURCE%\src\HtmlToFSharp.Web\bin\Release\net461\publish\* %DEPLOYMENT_TARGET% /Y