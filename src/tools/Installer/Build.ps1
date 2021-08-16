Push-Location $PSScriptRoot

$env:Path += ";${env:ProgramFiles(x86)}\Inno Setup 6\;${env:ProgramFiles(x86)}\MSI Wrapper\"

ISCC.exe setup.iss
# MsiWrapperBatch.exe config=".\msiwrapper.xml"

Pop-Location