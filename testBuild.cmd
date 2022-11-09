dotnet restore VRCSTT.csproj
dotnet build VRCSTT.csproj -c Release --no-restore
dotnet publish VRCSTT.csproj -c Release --self-contained -r win-x64 --no-build -o TestBuild