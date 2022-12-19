param (
    [Parameter(Mandatory=$True, HelpMessage="You have to choose a build type.")]
    [ValidateSet('telemetry', 'without-frontend', 'default', 'database', 'telemetry-minimal')]
    [string]$DockerComposeBuildType,

    [Parameter(Mandatory=$True, HelpMessage="You have to choose up or down.")]
    [ValidateSet('up', 'down')]
    [string]$UpOrDown,

    [Parameter(Mandatory=$False, HelpMessage="You have to choose up or down.")]
    [ValidateSet('no-build')]
    [string]$Build
)

$DockerComposeFile;

switch ($DockerComposeBuildType)
{
    'default' {$DockerComposeFile = '.\docker-compose.yml'}
    'telemetry' {$DockerComposeFile = '.\docker-compose-telemetry.yml'}
    'telemetry-minimal' {$DockerComposeFile = '.\docker-compose-telemetry-minimal.yml'}
    'without-frontend' {$DockerComposeFile = '.\docker-compose-without-frontend.yml'}
    'database' {$DockerComposeFile = '.\docker-compose-database-cache.yml'}
}

if ($UpOrDown -eq 'up')
{
    try {
        docker compose -f $DockerComposeFile down;

        if ($DockerComposeBuildType -ne 'database')
        {
            dotnet build .\LukeTube.sln;
        }

        if ($Build -ne 'no-build')
        {
            docker compose -f $DockerComposeFile build;
        }

        docker compose -f $DockerComposeFile up -d
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