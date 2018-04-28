using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

// justification for this app:  to cheap to buy licence for Visual Studio. Use msbuild to build instead.
// This app facilitates the initial creation of a console app.
namespace createNewConsoleApp
{
    class Program
    {
        const char  openBrace = '{',
                   closeBrace = '}';

        static void Main(String[] args)
        {
            AppSettingsReader asr = new AppSettingsReader();
            string rootFolder = string.Empty;
            GetConfigValue<string>(asr, "rootFolder", ref rootFolder, "");
            bool showSyntax = false;

            if (((args == null) || (args.Length == 0)) || (args[0] == "/?") || (args[0].ToLower() == "--h"))
            {
                showSyntax = true;
            }
            if (showSyntax)
            {
                Console.Out.WriteLine($"SYNTAX:  createNewConsoleApp  consoleAppName{Environment.NewLine}will set up a new solution consoleAppName, under {rootFolder}");
            }
            else
            {
                string childPath = Path.Combine(rootFolder, args[0]);
                if (Directory.Exists(childPath))
                {
                    Console.Out.WriteLine($"Folder {childPath} already exists. Solution for {args[0]} will not be created.");
                }
                else
                {
                    string csprojPath = Path.Combine(childPath, args[0]);
                    string propertiesPath = Path.Combine(csprojPath, "Properties");
                    CreateNewFolders(new string [] { rootFolder, csprojPath, propertiesPath} );
                    // we need three new guids: one each for solution, project and postSolution
                    Guid [] guids = new Guid[3];
                    for  (int i = 0; i < guids.Length; i++)
                    {
                    	guids[i] = Guid.NewGuid();
                    }
                    CreateNewSolution(childPath, args[0], guids);
                    CreateNewBuildProj(childPath, args[0]);
                    CreateNewProjectFiles(csprojPath, args[0], guids[1]);  // creates csproj, Program.cs and App.config
                    CreateNewAssemblyInfo(propertiesPath, args[0], guids[1]);
                    Console.Out.WriteLine($"Folder {childPath} and associated folders and files have been created.");
               }
           }
        }
        private static void CreateNewFolders(string [] newFolders)
        {
             foreach (string f in newFolders)
             {
                 Directory.CreateDirectory(f);
             }
        }

        private static void CreateNewSolution(string rootFolder, string appName, Guid [] guids)
        {
             string slnFile = $"{appName}.sln";
             string fPath = Path.Combine(rootFolder, slnFile);
             TextWriter tw = new StreamWriter(fPath, false);
             tw.Write($"{Environment.NewLine}Microsoft Visual Studio Solution File, Format Version 12.00{Environment.NewLine}" +
                 $"# Visual Studio 15{Environment.NewLine}" +
                 $"VisualStudioVersion = 15.0.27004.2009{Environment.NewLine}" +
                 $"MinimumVisualStudioVersion = 10.0.40219.1{Environment.NewLine}Project(\"{openBrace}");
             tw.Write(guids[0].ToString());
             tw.Write($"{closeBrace}\") = \"{appName}\", \"{appName}\\{appName}.csproj\", \"{openBrace}");
             tw.Write(guids[1].ToString());
             tw.WriteLine($"{closeBrace}\"");
             tw.WriteLine($"EndProject{Environment.NewLine}" +
                 $"Global{Environment.NewLine}" +
                 $"	GlobalSection(SolutionConfigurationPlatforms) = preSolution{Environment.NewLine}" +
                 $"		Debug|Any CPU = Debug|Any CPU{Environment.NewLine}" +
                 $"		Release|Any CPU = Release|Any CPU{Environment.NewLine}" +
                 $"	EndGlobalSection{Environment.NewLine}" +
                 $"	GlobalSection(ProjectConfigurationPlatforms) = postSolution{Environment.NewLine}");
             tw.WriteLine($"		{openBrace}{guids[1]}{closeBrace}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
             tw.WriteLine($"		{openBrace}{guids[1]}{closeBrace}.Debug|Any CPU.Build.0 = Debug|Any CPU");
             tw.WriteLine($"		{openBrace}{guids[1]}{closeBrace}.Release|Any CPU.ActiveCfg = Release|Any CPU");
             tw.WriteLine($"		{openBrace}{guids[1]}{closeBrace}.Release|Any CPU.Build.0 = Release|Any CPU");
             tw.WriteLine($"	EndGlobalSection{Environment.NewLine}" +
                 $"	GlobalSection(SolutionProperties) = preSolution{Environment.NewLine}" +
                 $"		HideSolutionNode = FALSE{Environment.NewLine}" +
                 $"	EndGlobalSection{Environment.NewLine}" +
                 $"	GlobalSection(ExtensibilityGlobals) = postSolution{Environment.NewLine}" +
                 $"		SolutionGuid = {guids[2]}{Environment.NewLine}" +
                 $"	EndGlobalSection{Environment.NewLine}" +
                 "EndGlobal");
             tw.Flush();
             tw.Close();        
        }
  
        private static void CreateNewBuildProj(string rootFolder, string appName)
        {
             string fPath = Path.Combine(rootFolder, "build.proj");
             TextWriter tw = new StreamWriter(fPath, false);
             tw.WriteLine($"<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" DefaultTargets=\"Build\">{Environment.NewLine}" +
                 $"  <!-- derived from https://blog.codeinside.eu/2010/11/24/howto-open-mstest-with-msbuild-2/ -->{Environment.NewLine}" +
                 $"  <PropertyGroup>{Environment.NewLine}" +
                 $"    <OutDir>$(MSBuildStartupDirectory)\\OutDir\\</OutDir>{Environment.NewLine}" +
                 $"    <SolutionProperties>{Environment.NewLine}" +
                 $"      OutDir=$(OutDir);{Environment.NewLine}" +
                 $"      Platform=Any CPU;{Environment.NewLine}" +
                 $"      Configuration=Release{Environment.NewLine}" +
                 $"    </SolutionProperties>{Environment.NewLine}" +
                 $"    <RunUnitTests>false</RunUnitTests>{Environment.NewLine}" +
                 $"    <ResultsFolder>TestResults</ResultsFolder>{Environment.NewLine}" +
                 $"    <ResultsFile>{appName}.trx</ResultsFile>");
             tw.WriteLine($"    <UnitTestTransformFile>{appName}\\CaptureUnitTestResults.xslt</UnitTestTransformFile>");
             tw.WriteLine($"  </PropertyGroup>{Environment.NewLine}" +
                 $"  <ItemGroup>{Environment.NewLine}" +
                 $"    <_FilesToTransform Include=\"\\$(ResultsFolder)\\$(ResultsFile)\"/>{Environment.NewLine}" +
                 $"  </ItemGroup>{Environment.NewLine}" +
                 $"  <ItemGroup>{Environment.NewLine}" +
                 $"    <Solution Include=\".\\{appName}.sln\">");

             tw.WriteLine($"      <Properties>{Environment.NewLine}" +
                 $"        $(SolutionProperties){Environment.NewLine}" +
                 $"      </Properties>{Environment.NewLine}" +
                 $"    </Solution>{Environment.NewLine}" +
                 $"  </ItemGroup>{Environment.NewLine}" +
                 $"  <Target Name=\"Build\">{Environment.NewLine}" +
                 $"    <MSBuild Projects=\"@(Solution)\"/>{Environment.NewLine}" +
                 $"    <Message Importance=\"high\" Text=\"RunUnitTests is $(RunUnitTests)\"/>{Environment.NewLine}" +
                 $"    <CallTarget Targets=\"RunTests\" ContinueOnError=\"false\" Condition=\"'$(RunUnitTests)' == 'true'\" />{Environment.NewLine}" +
                 $"  </Target>{Environment.NewLine}" +
                 $"  <Target Name=\"RunTests\" Label=\"Unit Tests\">{Environment.NewLine}" +
                 $"    <Delete Files=\"$(ResultsFolder)\\$(ResultsFile)\" />{Environment.NewLine}" +
                 $"    <Exec ContinueOnError=\"false\" Command='\"c:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\IDE\\mstest.exe\" /nologo /testcontainer:\"$(MSBuildStartupDirectory)\\OutDir\\{appName}Tests.dll\"  /resultsfile:\"$(ResultsFolder)\\$(ResultsFile)\"' >{Environment.NewLine}" +
                 $"      <Output TaskParameter=\"ExitCode\" PropertyName=\"ErrorCode\"/>{Environment.NewLine}" +
                 $"    </Exec>{Environment.NewLine}" +
                 $"    <Message Importance=\"high\" Text=\"$(ErrorCode)\"/>{Environment.NewLine}" +
                 $"    <CallTarget Targets=\"Transform\" ContinueOnError=\"false\" Condition=\"'$(ErrorCode)' == '0'\" />{Environment.NewLine}" +
                 $"  </Target>{Environment.NewLine}" +
                 $"  <Target Name=\"Transform\">{Environment.NewLine}" +
                 $"    <XslTransformation XslInputPath=\"$(UnitTestTransformFile)\" XmlInputPaths=\"$(ResultsFolder)\\$(ResultsFile)\" OutputPaths=\"$(OutDir){appName}.html\" />{Environment.NewLine}" +
                 $"{Environment.NewLine}" +
                 $"    <Message Importance=\"high\" Text=\"see $(OutDir){appName}.html\"/>{Environment.NewLine}" +
                 $"  </Target>{Environment.NewLine}" +
                 "</Project>");

             tw.Flush();
             tw.Close();
        }

        // creates csproj, Program.cs and App.config
        private static void CreateNewProjectFiles(string csprojPath, string appName, Guid  projectGuid)
        {
             string fPath = Path.Combine(csprojPath, $"{appName}.csproj");
             TextWriter tw = new StreamWriter(fPath, false);
             tw.WriteLine($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}" +
                 $"<Project ToolsVersion=\"15.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">{Environment.NewLine}" +
                 $"  <Import Project=\"$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props\" Condition=\"Exists('$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')\" />{Environment.NewLine}" +
                 $"  <PropertyGroup>{Environment.NewLine}" +
                 $"    <Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>{Environment.NewLine}" +
                 $"    <Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>{Environment.NewLine}" +
                 $"    <ProjectGuid>{openBrace}{projectGuid}{closeBrace}</ProjectGuid>");
             tw.WriteLine($"    <OutputType>Exe</OutputType>{Environment.NewLine}" +
                 $"    <RootNamespace>{appName}</RootNamespace>{Environment.NewLine}" +
                 $"    <AssemblyName>{appName}</AssemblyName>{Environment.NewLine}" +
                 $"    <TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>{Environment.NewLine}" +
                 $"    <FileAlignment>512</FileAlignment>{Environment.NewLine}" +
                 $"    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>{Environment.NewLine}" +
                 $"  </PropertyGroup>{Environment.NewLine}" +
                 $"  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">{Environment.NewLine}" +
                 $"    <PlatformTarget>AnyCPU</PlatformTarget>{Environment.NewLine}" +
                 $"    <DebugSymbols>true</DebugSymbols>{Environment.NewLine}" +
                 $"    <DebugType>full</DebugType>{Environment.NewLine}" +
                 $"    <Optimize>false</Optimize>{Environment.NewLine}" +
                 $"    <OutputPath>bin\\Debug\\</OutputPath>{Environment.NewLine}" +
                 $"    <DefineConstants>DEBUG;TRACE</DefineConstants>{Environment.NewLine}" +
                 $"    <ErrorReport>prompt</ErrorReport>{Environment.NewLine}" +
                 $"    <WarningLevel>4</WarningLevel>{Environment.NewLine}" +
                 $"  </PropertyGroup>{Environment.NewLine}" +
                 $"  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' \">{Environment.NewLine}" +
                 $"    <PlatformTarget>AnyCPU</PlatformTarget>{Environment.NewLine}" +
                 $"    <DebugType>pdbonly</DebugType>{Environment.NewLine}" +
                 $"    <Optimize>true</Optimize>{Environment.NewLine}" +
                 $"    <OutputPath>bin\\Release\\</OutputPath>{Environment.NewLine}" +
                 $"    <DefineConstants>TRACE</DefineConstants>{Environment.NewLine}" +
                 $"    <ErrorReport>prompt</ErrorReport>{Environment.NewLine}" +
                 $"    <WarningLevel>4</WarningLevel>{Environment.NewLine}" +
                 $"  </PropertyGroup>{Environment.NewLine}" +
                 $"  <ItemGroup>{Environment.NewLine}" +
                 $"    <Reference Include=\"System\" />{Environment.NewLine}" +
                 $"    <Reference Include=\"System.Core\" />{Environment.NewLine}" +
                 $"    <Reference Include=\"System.Xml.Linq\" />{Environment.NewLine}" +
                 $"    <Reference Include=\"System.IO\" />{Environment.NewLine}" +
                 $"    <Reference Include=\"System.Data.DataSetExtensions\" />{Environment.NewLine}" +
                 $"    <Reference Include=\"Microsoft.CSharp\" />{Environment.NewLine}" +
                 $"    <Reference Include=\"System.Data\" />{Environment.NewLine}" +
                 $"    <Reference Include=\"System.Net.Http\" />{Environment.NewLine}" +
                 $"    <Reference Include=\"System.Xml\" />{Environment.NewLine}" +
                 $"  </ItemGroup>{Environment.NewLine}" +
                 $"  <ItemGroup>{Environment.NewLine}" +
                 $"    <Compile Include=\"Program.cs\" />{Environment.NewLine}" +
                 $"    <Compile Include=\"Properties\\AssemblyInfo.cs\" />{Environment.NewLine}" +
                 $"  </ItemGroup>{Environment.NewLine}" +
                 $"  <ItemGroup>{Environment.NewLine}" +
                 $"    <None Include=\"App.config\" />{Environment.NewLine}" +
                 $"  </ItemGroup>{Environment.NewLine}" +
                 $"  <Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />{Environment.NewLine}" +
                 $"</Project>");
             tw.Flush();
             tw.Close();

             fPath = Path.Combine(csprojPath, "Program.cs");
             tw = new StreamWriter(fPath, false);

             tw.WriteLine($"using System;{Environment.NewLine}" +
                 $"using System.Collections.Generic;{Environment.NewLine}" +
                 $"using System.ComponentModel;{Environment.NewLine}" +
                 $"using System.Configuration;{Environment.NewLine}" +
                 $"using System.Diagnostics;{Environment.NewLine}" +
                 $"using System.Globalization;{Environment.NewLine}" +
                 $"using System.IO;{Environment.NewLine}" +
                 $"using System.Linq;");
             tw.WriteLine($"using System.Text;{Environment.NewLine}" +
                 $"using System.Threading.Tasks;{Environment.NewLine}" +
                 $"using Microsoft.Win32;{Environment.NewLine}" +
                 $"{Environment.NewLine}namespace {appName}");

             tw.WriteLine($"{openBrace}");
             tw.WriteLine($"    class Program{Environment.NewLine}    {openBrace}{Environment.NewLine}");
             tw.WriteLine($"        static void Main(String[] args){Environment.NewLine}        {openBrace}");
             tw.WriteLine($"        {closeBrace}{Environment.NewLine}    {closeBrace}");
             tw.WriteLine($"{closeBrace}");
             tw.Flush();
             tw.Close();

             // finally app.config
             fPath = Path.Combine(csprojPath, "App.config");
             tw = new StreamWriter(fPath, false, Encoding.UTF8);
             tw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
             tw.WriteLine($"<configuration>{Environment.NewLine}" +
                 $"    <startup>{Environment.NewLine}" +
                 $"        <supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.6.1\" />{Environment.NewLine}" +
                 $"    </startup>{Environment.NewLine}" +
                 "</configuration>");
             tw.Flush();
             tw.Close();
        }

        private static void CreateNewAssemblyInfo(string propertiesPath, string appName, Guid  projectGuid)
        {
             string fPath = Path.Combine(propertiesPath, "AssemblyInfo.cs");
             TextWriter tw = new StreamWriter(fPath, false, Encoding.Unicode);
             tw.WriteLine("using System.Reflection;");

             tw.WriteLine("using System.Runtime.CompilerServices;");
             tw.WriteLine("using System.Runtime.InteropServices;");
             tw.WriteLine("");

             tw.WriteLine("// General Information about an assembly is controlled through the following{Environment.NewLine}" +
                 $"// set of attributes. Change these attribute values to modify the information{Environment.NewLine}" +
                 $"// associated with an assembly.{Environment.NewLine}{Environment.NewLine}" +
                 $"[assembly: AssemblyTitle(\"{appName}\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyDescription(\"\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyConfiguration(\"\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyCompany(\"\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyProduct(\"{appName}\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyCopyright(\"Copyright ©  {DateTime.Now.Year}\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyTrademark(\"\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyCulture(\"\")]{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}" +
                 $"// Setting ComVisible to false makes the types in this assembly not visible{Environment.NewLine}" +
                 $"// to COM components.  If you need to access a type in this assembly from{Environment.NewLine}" +
                 $"// COM, set the ComVisible attribute to true on that type.{Environment.NewLine}" +
                 $"[assembly: ComVisible(false)]{Environment.NewLine}" +
                 $"// The following GUID is for the ID of the typelib if this project is exposed to COM{Environment.NewLine}" +
                 $"[assembly: Guid(\"{projectGuid}\")]{Environment.NewLine}{Environment.NewLine}" +
                 $"// Version information for an assembly consists of the following four values:{Environment.NewLine}" +
                 $"//{Environment.NewLine}" +
                 $"//      Major Version{Environment.NewLine}" +
                 $"//      Minor Version{Environment.NewLine}" +
                 $"//      Build Number{Environment.NewLine}" +
                 $"//      Revision{Environment.NewLine}" +
                 $"//{Environment.NewLine}" +
                 $"// You can specify all the values or you can default the Build and Revision Numbers{Environment.NewLine}" +
                 $"// by using the '*' as shown below:{Environment.NewLine}" +
                 $"// [assembly: AssemblyVersion(\"1.0.*\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyVersion(\"1.0.0.0\")]{Environment.NewLine}" +
                 $"[assembly: AssemblyFileVersion(\"1.0.0.0\")]");
             tw.Flush();
             tw.Close();
        }
     
        /// <summary>
        /// GetConfigValue
        /// </summary>
        /// <typeparam name="T">passed type</typeparam>
        /// <param name="appSettingsReader">System.Configuration.AppSettingsReader</param>
        /// <param name="keyName">string</param>
        /// <param name="keyValue">ref T</param>
        /// <param name="defaultValue">T</param>
        private static void GetConfigValue<T>(System.Configuration.AppSettingsReader appSettingsReader,
                                        string keyName, ref T keyValue, T defaultValue)
        {
            keyValue = defaultValue;
            // provide a default
            try
            {
                string tempS = (string)appSettingsReader.GetValue(keyName, typeof(System.String));
                if ((tempS != null) && (tempS.Trim().Length > 0))
                {
                    keyValue = (T)TypeDescriptor.GetConverter(keyValue.GetType()).ConvertFrom(tempS);
                }
                else
                    Debug.WriteLine("Registry failed to read value from " + keyName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());  // if key does not exist, not a problem. Caller must pre-assign values anyway
            }
        }
    }
}
