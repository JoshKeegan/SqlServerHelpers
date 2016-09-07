if($env:APPVEYOR_REPO_BRANCH -eq "release")
{
	$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'
	$version = [System.Reflection.Assembly]::LoadFile("$root\SqlServerHelpers\bin\Release\SqlServerHelpers.dll").GetName().Version
	$versionStr = "{0}.{1}.{2}.{3}" -f ($version.Major, $version.Minor, $version.Build, $version.Revision)
	
	Write-Host "Generating nuspec"
	nuget spec $root\SqlServerHelpers\bin\Release\SqlServerHelpers.dll
	cp $root\SqlServerHelpers\bin\Release\SqlServerHelpers.dll.nuspec $root\NuGet\SqlServerHelpers.nuspec

	Write-Host "Setting .nuspec version tag to $versionStr"

	$content = (Get-Content $root\NuGet\SqlServerHelpers.nuspec) 
	$content = $content -replace '\$version\$', $versionStr

	$content | Out-File $root\NuGet\SqlServerHelpers.compiled.nuspec

	& nuget pack $root\NuGet\SqlServerHelpers.compiled.nuspec
}
else
{
	Write-Host "Not packaging for nuget. To package & push to nuget, merge onto the release branch"
}