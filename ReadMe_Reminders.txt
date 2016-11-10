For local debugging set the "Debug" settings of the class library project to "Start Action" to "Start external program" and select the devenv.exe file with commandline arguments /rootSuffix Exp.

AppVeyor > Repo > Settings > Build > Before build script = CMD > nuget restore Xxxxxxx.Sln > Save
