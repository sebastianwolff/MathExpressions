#Get Path to csproj
$path = "$PSScriptRoot\Expressions.csproj"

#Read csproj (XML)
$content = Get-Content $path -Raw -Encoding "UTF8";

$xml = [xml]($content)

#Retrieve Version Nodes
$assemblyVersion = $xml.Project.PropertyGroup.AssemblyVersion
$fileVersion = $xml.Project.PropertyGroup.FileVersion

#Split the Version Numbers
$avMajor, $avMinor, $avBuild,$avBuild2 = $assemblyVersion -split "\." 


#Increment Revision
$oldVersion = $assemblyVersion[0];

$avBuildNew  = ([int]$avBuild) 
$avBuild2New = ([int]$avBuild2[0]) + 1;
if($avBuild2New -ge 10000) {
    $avBuildNew  = $avBuildNew + 1 
    $avBuild2New = 100;
}

$newVerison = "$avMajor.$avMinor.$avBuildNew.$avBuild2New";
$contentNew = $content.Replace($oldVersion, $newVerison).Trim()
Set-Content -Path $path -Value $contentNew -Encoding "UTF8"
