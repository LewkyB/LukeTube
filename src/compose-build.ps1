param (
    [Parameter(Mandatory=$True, HelpMessage="You have to choose a build type.")]
    [ValidateSet('telemetry', 'without-frontend', 'default', 'database')]
    [string]$DockerComposeBuildType,

    [Parameter(Mandatory=$True, HelpMessage="You have to choose up or down.")]
    [ValidateSet('up', 'down')]
    [string]$UpOrDown
)

$DockerComposeFile;

switch ($DockerComposeBuildType)
{
    'default' {$DockerComposeFile = '.\docker-compose.yml'}
    'telemetry' {$DockerComposeFile = '.\docker-compose-telemetry.yml'}
    'without-frontend' {$DockerComposeFile = '.\docker-compose-without-frontend.yml'}
    'database' {$DockerComposeFile = '.\docker-compose-database-only.yml'}
}

if ($UpOrDown -eq 'up')
{
    try {
        docker compose -f $DockerComposeFile down;

        if ($DockerComposeBuildType -ne 'database')
        {
            dotnet build .\LukeTube.sln;
        }

        docker compose -f $DockerComposeFile build;
        docker compose -f $DockerComposeFile up
    }
    catch {
        write-Host "$DockerComposeBuildType $UpOrDown has failed."
    }
}
elseif ($UpOrDown -eq 'down')
{
    try {
        docker compose -f $DockerComposeFile down;
    }
    catch {
        write-Host "$DockerComposeBuildType $UpOrDown has failed."
    }
}